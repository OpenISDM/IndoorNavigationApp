﻿/*
 * Copyright (c) 2019 Academia Sinica, Institude of Information Science
 *
 * License:
 *      GPL 3.0 : The content of this file is subject to the terms and
 *      conditions defined in file 'COPYING.txt', which is part of this source
 *      code package.
 *
 * Project Name:
 *
 *      IndoorNavigation
 *
 * File Description:
 * 
 *      This class is used to select the route specified by the starting
 *      point, destination, and user preferences. When in navigation,
 *      the class will give the next waypoint, and when the user is in
 *      the wrong way, the class will re-route.
 *      
 * Version:
 *
 *      1.0.0, 20190605
 * 
 * File Name:
 *
 *      Session.cs
 *
 * Abstract:
 *
 *      Waypoint-based navigator is a mobile Bluetooth navigation application
 *      that runs on smart phones. It is structed to support anywhere 
 *      navigation. Indoors in areas covered by different indoor positioning 
 *      system (IPS) and outdoors covered by GPS. In particilar, it can rely
 *      on BeDIS (Building/environment Data and Information System) for
 *      indoor positioning. Using this IPS, the navigator does not need to 
 *      continuously monitor its own position, since the IPS broadcast to the 
 *      navigator the location of each waypoint. 
 *      This version makes use of Xamarin.Forms, which is a complete 
 *      cross-platform UI tookit that runs on both iOS and Android.
 *
 * Authors:
 *
 *      Eric Lee, ericlee@iis.sinica.edu.tw
 *      Chun-Yu Lai, chunyu1202@gmail.com
 *
 */

using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using Dijkstra.NET.Model;
using Dijkstra.NET.Extensions;
using IndoorNavigation.Models.NavigaionLayer;
using IndoorNavigation.Models;
using IndoorNavigation.Modules.IPSClients;

namespace IndoorNavigation.Modules
{
    public class Session
    {
        private IIPSClient _IPSClient;

        private int _nextWaypointStep;

        private List<RegionWaypointPoint> _waypointsOnRoute = new List<RegionWaypointPoint>();
        private Dictionary<RegionWaypointPoint, List<RegionWaypointPoint>>             _waypointsOnWrongWay = new Dictionary<RegionWaypointPoint, List<RegionWaypointPoint>>();
        
        private Graph<Guid, string> _graphRegionGraph = new Graph<Guid, string>();

        private NavigationGraph _navigationGraph; 
        private Guid _sourceRegionID;
        private Guid _destinationRegionID;
        private Guid _destinationWaypointID;

        private Thread _waypointDetectionThread;
        private Thread _navigationControllerThread;

        private bool _isKeepDetection;
        private Guid _currentRegionID = new Guid();
        private Guid _currentWaypointID = new Guid();

        private ConnectionType[] _avoidConnectionTypes;

        private ManualResetEventSlim _nextWaypointEvent = new ManualResetEventSlim(false);

        public NavigationEvent _event { get; private set; }
        private Dictionary<Guid, Region> _regiongraphs = new Dictionary<Guid, Region>();

        public Session(NavigationGraph navigationGraph,
                       Guid sourceRegionID,
                       Guid destinationRegionID,
                       Guid destinationWaypointID,
                       ConnectionType[] avoidConnectionTypes)
        {
            _event = new NavigationEvent();

            _navigationGraph = navigationGraph;

            _sourceRegionID = sourceRegionID;
            _destinationRegionID = destinationRegionID;
            _destinationWaypointID = destinationWaypointID;

            _avoidConnectionTypes = avoidConnectionTypes;

            // construct region graph (across regions) which we can use to generate route
            _graphRegionGraph = navigationGraph.GenerateRegionGraph(avoidConnectionTypes);
            _regiongraphs = _navigationGraph.GetRegions();
            _nextWaypointStep = -1;
            _isKeepDetection = true;
            /*
            _IPSClient = new WaypointClient();
            _IPSClient._event._eventHandler += new EventHandler(CheckArrivedWaypoint);
            */
            _waypointDetectionThread = new Thread(() => InvokeIPSWork());
            _waypointDetectionThread.Start();

            _navigationControllerThread = new Thread(() => NavigatorProgram());
            _navigationControllerThread.Start();
        }

        private void NavigatorProgram(){
            _nextWaypointStep = -1;
            _currentRegionID = _sourceRegionID;
            _currentWaypointID = new Guid();
            RegionWaypointPoint checkWrongRegionWaypoint = new RegionWaypointPoint();
            checkWrongRegionWaypoint._regionID = _currentRegionID;
            checkWrongRegionWaypoint._waypointID = _currentWaypointID;

            NavigateToNextWaypoint(_currentRegionID, _nextWaypointStep);

            while (true == _isKeepDetection &&
                   !(_currentRegionID.Equals(_destinationRegionID) &&
                     _currentWaypointID.Equals(_destinationWaypointID)))
            {

                Console.WriteLine("Continue to navigate to next step, current location {0}/{1}",
                                  _currentRegionID, _currentWaypointID);

                _nextWaypointEvent.Wait();

                if (_currentRegionID.Equals(_destinationRegionID) &&
                    _currentWaypointID.Equals(_destinationWaypointID)) {
                    Console.WriteLine("Arrived destination! {0}/{1}",
                                      _destinationRegionID,
                                      _destinationWaypointID);

                    _isKeepDetection = false;
                    _IPSClient.Stop();
                    break;
                }

                if (_nextWaypointStep == -1)
                {
                    Console.WriteLine("Detected start waypoint: " + _currentWaypointID);
                    // Detection of starting waypoing:
                    // Detected the waypoint most closed to user.
                    GenerateRoute(_currentRegionID,
                                  _currentWaypointID,
                                  _destinationRegionID,
                                  _destinationWaypointID);

                    _nextWaypointStep++;
                    //_currentRegionID = _waypointsOnRoute[_nextWaypointStep]._regionID;
                    Guid _nextRegionID = _waypointsOnRoute[_nextWaypointStep]._regionID;
                    NavigateToNextWaypoint(_nextRegionID, _nextWaypointStep);
                }
                else if (_currentRegionID.Equals(
                         _waypointsOnRoute[_nextWaypointStep]._regionID) &&
                         _currentWaypointID.Equals(
                         _waypointsOnRoute[_nextWaypointStep]._waypointID))
                {
                    Console.WriteLine("Arrived region/waypoint: {0}/{1}",
                                      _currentRegionID,
                                      _currentWaypointID);
                    _nextWaypointStep++;
                    Guid _nextRegionID = _waypointsOnRoute[_nextWaypointStep]._regionID;
                   // _currentRegionID = _waypointsOnRoute[_nextWaypointStep]._regionID;
                    NavigateToNextWaypoint(_nextRegionID, _nextWaypointStep);
                }
                else if (_nextWaypointStep >= 1 && _waypointsOnWrongWay[_waypointsOnRoute[_nextWaypointStep - 1]].Contains(checkWrongRegionWaypoint) == false)
                {
                    Console.WriteLine("In Program Wrong, going to Re-calculate the route");
                    _nextWaypointStep = 0;

                    GenerateRoute(
                                    _currentRegionID,
                                    _currentWaypointID,
                                    _destinationRegionID,
                                    _destinationWaypointID);

                    Console.WriteLine("Finish Construct New Route");

                    Guid previousRegionID = new Guid();
                    Guid previousWaypointID = new Guid();

                    _event.OnEventCall(new NavigationEventArgs
                    {
                        _result = NavigationResult.Run,
                        _nextInstruction = new NavigationInstruction
                        {
                            _currentWaypointName = _navigationGraph.GetWaypointNameInRegion(_currentRegionID,
                                                                 _currentWaypointID),
                            _nextWaypointName = _navigationGraph.GetWaypointNameInRegion(_waypointsOnRoute[1]._regionID, _waypointsOnRoute[1]._waypointID),
                            _progress = 0,
                            _information = _navigationGraph.GetInstructionInformation(
                                _nextWaypointStep,
                                _currentRegionID,
                                _currentWaypointID,
                                previousRegionID,
                                previousWaypointID,
                                _waypointsOnRoute[_nextWaypointStep + 1]._regionID,
                                _waypointsOnRoute[_nextWaypointStep + 1]._waypointID,
                                _avoidConnectionTypes
                                )
                        }
                    });

                    _nextWaypointStep++;
                    Guid _nextRegionID = _waypointsOnRoute[_nextWaypointStep]._regionID;
                    //_currentRegionID = _waypointsOnRoute[_nextWaypointStep]._regionID;
                    NavigateToNextWaypoint(_nextRegionID, _nextWaypointStep);
                }
                _nextWaypointEvent.Reset();
            }
        }

        private void NavigateToNextWaypoint(Guid regionID, int nextStep) {
            List<WaypointBeaconsMapping> monitorWaypointList =
                new List<WaypointBeaconsMapping>();

            if (nextStep == -1)
            {
                // Detection of starting waypoing:
                // Use all Waypoints of the NavigationGraph to detect the starting waypoint
                IPSType nextRegionIPSType =
                   _navigationGraph.GetRegionIPSType(regionID);

                if (IPSType.LBeacon == nextRegionIPSType)
                {
                    _IPSClient = new WaypointClient();
                    _IPSClient._event._eventHandler += new EventHandler(CheckArrivedWaypoint);
                }
                else if(IPSType.iBeacon == nextRegionIPSType)
                {
                    _IPSClient = new IBeaconClient();
                    _IPSClient._event._eventHandler += new EventHandler(CheckArrivedWaypoint);
                }

                List<Guid> waypointIDs =
                    _navigationGraph.GetAllWaypointIDInOneRegion(regionID);
                foreach (Guid waypointID in waypointIDs)
                {
                    RegionWaypointPoint tempRegionWaypointInInitial = new RegionWaypointPoint();
                    tempRegionWaypointInInitial._waypointID = waypointID;
                    tempRegionWaypointInInitial._regionID = regionID;
                    List<Guid> beaconIDs = new List<Guid>();
                    if (IPSType.LBeacon == nextRegionIPSType ||
                        IPSType.iBeacon == nextRegionIPSType)
                    {
                        beaconIDs = _navigationGraph
                                    .GetAllBeaconIDInOneWaypointOfRegion(regionID,
                                                                         waypointID);
                    }
                    monitorWaypointList.Add(new WaypointBeaconsMapping
                    {
                        _WaypointIDAndRegionID = tempRegionWaypointInInitial,
                        _Beacons = beaconIDs
                    });
                }
                foreach (WaypointBeaconsMapping mappingItem in monitorWaypointList) {
                    Console.WriteLine("monitor waypoint={0}", mappingItem._WaypointIDAndRegionID._waypointID);
                    foreach (Guid beaconID in mappingItem._Beacons) {
                        Console.WriteLine("beacon={0}", beaconID);
                    }
                }
            }
            else
            {
                RegionWaypointPoint checkPoint = _waypointsOnRoute[nextStep];
                IPSType nextRegionIPSType =
                    _navigationGraph.GetRegionIPSType(checkPoint._regionID);
                if (!nextRegionIPSType.Equals(_navigationGraph.GetRegionIPSType(_currentRegionID)))
                {
                    _IPSClient.Stop();
                    _IPSClient._event._eventHandler -= new EventHandler(CheckArrivedWaypoint);

                    if (IPSType.LBeacon == nextRegionIPSType)
                    {
                        _IPSClient = new WaypointClient();
                        _IPSClient._event._eventHandler += new EventHandler(CheckArrivedWaypoint);
                    }
                    else if (IPSType.iBeacon == nextRegionIPSType)
                    {
                        _IPSClient = new IBeaconClient();
                        _IPSClient._event._eventHandler += new EventHandler(CheckArrivedWaypoint);
                    }

                }

                List<Guid> beaconIDs = new List<Guid>();

                if (IPSType.LBeacon == nextRegionIPSType ||
                   IPSType.iBeacon == nextRegionIPSType) {

                    beaconIDs =
                        _navigationGraph
                        .GetAllBeaconIDInOneWaypointOfRegion(checkPoint._regionID,
                                                             checkPoint._waypointID);
                }

                RegionWaypointPoint tempRegionWaypointoutInInitial = new RegionWaypointPoint();
                tempRegionWaypointoutInInitial._waypointID = checkPoint._waypointID;
                tempRegionWaypointoutInInitial._regionID = regionID;

                monitorWaypointList.Add(new WaypointBeaconsMapping
                {
                    _WaypointIDAndRegionID = tempRegionWaypointoutInInitial,
                    _Beacons = beaconIDs
                });


                List<Guid> WrongbeaconIDs = new List<Guid>();
                WaypointBeaconsMapping waypointBeacons = new WaypointBeaconsMapping();
                //Add possible wrong beacons' into monitorWaypointList that will give to IPSClient.
                if (_nextWaypointStep >= 1)
                {
                    waypointBeacons._Beacons = new List<Guid>();

                    if (_waypointsOnWrongWay[_waypointsOnRoute[_nextWaypointStep - 1]] != null)
                    {
                        Guid tempGuid = new Guid();
                        foreach (RegionWaypointPoint items in _waypointsOnWrongWay[_waypointsOnRoute[_nextWaypointStep - 1]])
                        {
                            for (int j = 0; j < _waypointsOnWrongWay[_waypointsOnRoute[_nextWaypointStep - 1]].Count(); j++)
                            {
                                if (_waypointsOnWrongWay[_waypointsOnRoute[_nextWaypointStep - 1]][j]._waypointID == items._waypointID)
                                {
                                    tempGuid = _waypointsOnWrongWay[_waypointsOnRoute[_nextWaypointStep - 1]][j]._regionID;
                                }
                            }

                            WrongbeaconIDs = _navigationGraph
                                        .GetAllBeaconIDInOneWaypointOfRegion(tempGuid, items._waypointID);

                            RegionWaypointPoint tempRegionWaypointInWrongWay = new RegionWaypointPoint();
                            tempRegionWaypointInWrongWay._waypointID = items._waypointID;
                            tempRegionWaypointInWrongWay._regionID = tempGuid;
                            Console.WriteLine("waypoint ID : " + items._waypointID);
                            foreach (Guid guid in WrongbeaconIDs)
                            {
                                Console.WriteLine("related guid of waypoint ID : " + guid);
                            }
                          
                            monitorWaypointList.Add(new WaypointBeaconsMapping
                            {
                                _WaypointIDAndRegionID = tempRegionWaypointInWrongWay,
                                _Beacons = WrongbeaconIDs
                            });
                        }
                    }
                }

                //Print All beacons' Guid that will give to IPSClients
                foreach (WaypointBeaconsMapping waypointBeaconsMapping in monitorWaypointList)
                {
                    Console.WriteLine("Check mapping Current WaypointID: " + _currentWaypointID);
                    Console.WriteLine("Expected next Step : " + _waypointsOnRoute[_nextWaypointStep]._waypointID);
                    
                    foreach (Guid guids in waypointBeaconsMapping._Beacons)
                    {
                        Console.WriteLine("ALL Interested Guid : " + guids);
                    }
                }
            }

            _IPSClient.SetWaypointList(monitorWaypointList);
        }

        private void InvokeIPSWork() {
            Console.WriteLine("---- InvokeIPSWork ----");
            while (true == _isKeepDetection)
            {
                Thread.Sleep(1000);
                _IPSClient.DetectWaypoints();
            }
        }

        private void GenerateRoute(Guid sourceRegionID,
                                   Guid sourceWaypointID,
                                   Guid destinationRegionID,
                                   Guid destinationWaypointID)
        {
            // generate path between regions (from sourceRegionID to destnationRegionID)
            uint region1Key = _graphRegionGraph
                              .Where(node => node.Item.Equals(sourceRegionID))
                              .Select(node => node.Key).First();
            uint region2Key = _graphRegionGraph
                              .Where(node => node.Item.Equals(destinationRegionID))
                              .Select(node => node.Key).First();

            var pathRegions = _graphRegionGraph.Dijkstra(region1Key, region2Key).GetPath();

            if (0 == pathRegions.Count())
            {
                Console.WriteLine("No path. Need to change avoid connection type");
                return;
            }

            // store the generate Dijkstra path across regions
            List<Guid> regionsOnRoute = new List<Guid>();
            for (int i = 0; i < pathRegions.Count(); i++)
            {
                regionsOnRoute.Add(_graphRegionGraph[pathRegions.ToList()[i]].Item);
            }

            // generate the path of the region/waypoint checkpoints across regions
            _waypointsOnRoute = new List<RegionWaypointPoint>();
            _waypointsOnRoute.Add(new RegionWaypointPoint
            {
                _regionID = sourceRegionID,
                _waypointID = sourceWaypointID
            });

            for(int i = 0; i < _waypointsOnRoute.Count(); i++)
            {
                RegionWaypointPoint checkPoint = _waypointsOnRoute[i];
                Console.WriteLine("check index = {0}, count = {1}, region {2} waypoint {3}",
                                  i,
                                  _waypointsOnRoute.Count(),
                                  checkPoint._regionID,
                                  checkPoint._waypointID);
                if (regionsOnRoute.IndexOf(checkPoint._regionID) + 1 <
                    regionsOnRoute.Count())
                {
                    LocationType waypointType =
                        _navigationGraph.GetWaypointTypeInRegion(checkPoint._regionID,
                                                                 checkPoint._waypointID);
                    
                    Guid nextRegionID =
                        regionsOnRoute[regionsOnRoute.IndexOf(checkPoint._regionID) + 1];

                    PortalWaypoints portalWaypoints =
                        _navigationGraph.GetPortalWaypoints(checkPoint._regionID,
                                                            checkPoint._waypointID,
                                                            nextRegionID,
                                                            _avoidConnectionTypes);

                    if (LocationType.portal != waypointType)
                    {
                        _waypointsOnRoute.Add(new RegionWaypointPoint
                        {
                            _regionID = checkPoint._regionID,
                            _waypointID = portalWaypoints._portalWaypoint1
                        });
                    }
                    else if (LocationType.portal == waypointType)
                    {
                        if (!checkPoint._waypointID.Equals(portalWaypoints._portalWaypoint1))
                        {
                            _waypointsOnRoute.Add(new RegionWaypointPoint
                            {
                                _regionID = checkPoint._regionID,
                                _waypointID = portalWaypoints._portalWaypoint1
                            });
                        }
                        else
                        {
                            _waypointsOnRoute.Add(new RegionWaypointPoint
                            {
                                _regionID = nextRegionID,
                                _waypointID = portalWaypoints._portalWaypoint2
                            });
                        }
                    }
                }   
            }
            int indexLastCheckPoint = _waypointsOnRoute.Count() - 1;
            if (!(_destinationRegionID.
                Equals(_waypointsOnRoute[indexLastCheckPoint]._regionID) &&
                _destinationWaypointID.
                Equals(_waypointsOnRoute[indexLastCheckPoint]._waypointID)))
            {
                _waypointsOnRoute.Add(new RegionWaypointPoint
                {
                    _regionID = _destinationRegionID,
                    _waypointID = _destinationWaypointID
                });
            }

            foreach (RegionWaypointPoint checkPoint in _waypointsOnRoute)
            {
                Console.WriteLine("region-graph region/waypoint = {0}/{1}",
                                  checkPoint._regionID,
                                  checkPoint._waypointID);
            }

            // fill in all the path between waypoints in the same region / navigraph
            for (int i = 0; i < _waypointsOnRoute.Count() - 1; i++)
            {
                RegionWaypointPoint currentCheckPoint = _waypointsOnRoute[i];
                RegionWaypointPoint nextCheckPoint = _waypointsOnRoute[i + 1];

                if (currentCheckPoint._regionID.Equals(nextCheckPoint._regionID))
                {
                    Graph<Guid, string> _graphNavigraph =
                        _navigationGraph.GenerateNavigraph(currentCheckPoint._regionID,
                                                           _avoidConnectionTypes);

                    // generate path between two waypoints in the same region / navigraph
                    uint waypoint1Key = _graphNavigraph
                                        .Where(node => node.Item
                                               .Equals(currentCheckPoint._waypointID))
                                        .Select(node => node.Key).First();
                    uint waypoint2Key = _graphNavigraph
                                        .Where(node => node.Item
                                               .Equals(nextCheckPoint._waypointID))
                                        .Select(node => node.Key).First();

                    var pathWaypoints =
                        _graphNavigraph.Dijkstra(waypoint1Key, waypoint2Key).GetPath();

                    for (int j = pathWaypoints.Count()-1; j >0; j--)
                    {
                        if (j != 0 && j != pathWaypoints.Count() - 1)
                        {
                            _waypointsOnRoute.Insert(i + 1, new RegionWaypointPoint
                            {
                                _regionID = currentCheckPoint._regionID,
                                _waypointID = _graphNavigraph[pathWaypoints.ToList()[j]].Item
                            });
                        }
                    }
                }
            }

            // display the resulted full path of region/waypoint between source and destination
            foreach (RegionWaypointPoint checkPoint in _waypointsOnRoute)
            {
                Console.WriteLine("full-path region/waypoint = {0}/{1}",
                                  checkPoint._regionID,
                                  checkPoint._waypointID);
            }

            int nextStep = 1;
            _waypointsOnWrongWay = new Dictionary<RegionWaypointPoint, List<RegionWaypointPoint>>();
            Region tempRegion = new Region();
            List<Guid> neighborGuid = new List<Guid>();

            //For each waypoint in _waypointsOnRoute, decide their wrong waypoint.
            foreach (RegionWaypointPoint locationRegionWaypoint in _waypointsOnRoute)
            {
                RegionWaypointPoint tempRegionWaypoint = new RegionWaypointPoint();

                //Get the neighbor of all wapoint in _waypointOnRoute.
                neighborGuid = new List<Guid>();
                neighborGuid = _navigationGraph.GetNeighbor(locationRegionWaypoint._regionID, locationRegionWaypoint._waypointID);

                tempRegion = _regiongraphs[locationRegionWaypoint._regionID];

                LocationType locationType =
                        _navigationGraph.GetWaypointTypeInRegion(locationRegionWaypoint._regionID,
                                                                 locationRegionWaypoint._waypointID);
                //If the waypoints are portal, we need to get its related portal waypoints in other regions.
                if (locationType.ToString() == "portal")
                {
                    foreach (Guid regionNeighborGuid in tempRegion._neighbors)
                    {
                        RegionWaypointPoint portalWaypointRegionGuid = new RegionWaypointPoint();
                        portalWaypointRegionGuid = _navigationGraph.GiveNeighborWaypointInNeighborRegion(
                                        locationRegionWaypoint._regionID,
                                        locationRegionWaypoint._waypointID,
                                        regionNeighborGuid);
                        if (portalWaypointRegionGuid._waypointID != Guid.Empty)
                        {
                            if (_waypointsOnRoute.Count() > nextStep)
                            {
                                if (_waypointsOnRoute[nextStep]._waypointID != portalWaypointRegionGuid._waypointID)
                                {
                                    if (!_waypointsOnWrongWay.Keys.Contains(locationRegionWaypoint))
                                    {
                                        tempRegionWaypoint = new RegionWaypointPoint();
                                        tempRegionWaypoint._waypointID = portalWaypointRegionGuid._waypointID;
                                        tempRegionWaypoint._regionID = portalWaypointRegionGuid._regionID;
                                        _waypointsOnWrongWay.Add(locationRegionWaypoint, new List<RegionWaypointPoint> { tempRegionWaypoint });
                                    }
                                    else
                                    {
                                        tempRegionWaypoint = new RegionWaypointPoint();
                                        tempRegionWaypoint._regionID = portalWaypointRegionGuid._regionID;
                                        tempRegionWaypoint._waypointID = portalWaypointRegionGuid._waypointID;
                                        if (_waypointsOnWrongWay[locationRegionWaypoint].Contains(tempRegionWaypoint) == true)
                                        {
                                            _waypointsOnWrongWay[locationRegionWaypoint].Add(tempRegionWaypoint);
                                        }
                                    }
                                }
                            }

                        }
                    }
                }
                //Get the waypoint neighbor's Guids and add them in _waypointsOnWrongWay except next Waypoint Guid.
                //We know just consider One-Step Wrong Way.
                foreach (Guid guid in neighborGuid)
                {
                    if (_waypointsOnRoute.Count() > nextStep)
                    {
                        if (_waypointsOnRoute[nextStep]._waypointID != guid)
                        {
                            double distanceBetweenCurrentAndNeighbor = _navigationGraph.StraightDistanceBetweenWaypoints(
                                       locationRegionWaypoint._regionID,
                                       locationRegionWaypoint._waypointID,
                                       guid);
                            double distanceBetweenNextAndNeighbor = 0;
                            //If current region == next region, we can get get the straight distance of the neighbors of current waypoint and next waypoint.
                            //If current region != next region, we just consider the distance between cuurent and its neighbers, therefore, we give distanceBetweenNextAndNeighbor
                            //the same value of distanceBetweenCurrentAndNeighbor.
                            if (locationRegionWaypoint._regionID == _waypointsOnRoute[nextStep]._regionID)
                            {
                                distanceBetweenNextAndNeighbor = _navigationGraph.StraightDistanceBetweenWaypoints(
                                       locationRegionWaypoint._regionID,
                                       _waypointsOnRoute[nextStep]._waypointID,
                                       guid);
                            }
                            else
                            {
                                distanceBetweenNextAndNeighbor = distanceBetweenCurrentAndNeighbor;
                            }
                            //If the distance of current and its neighbors and the distance between next and current's neighbors
                            //are far enough, we add them into _waypointOnWrongWay, else if the distance between current and its neighbors
                            //are too close, we need to find one more step.
                            if (distanceBetweenCurrentAndNeighbor >= 2 && distanceBetweenNextAndNeighbor>=2)
                            {
                                if (!_waypointsOnWrongWay.Keys.Contains(locationRegionWaypoint))
                                {
                                    tempRegionWaypoint = new RegionWaypointPoint();
                                    tempRegionWaypoint._regionID = locationRegionWaypoint._regionID;
                                    tempRegionWaypoint._waypointID = guid;
                                    _waypointsOnWrongWay.Add(locationRegionWaypoint, new List<RegionWaypointPoint> { tempRegionWaypoint });
                                }
                                else
                                {
                                    tempRegionWaypoint = new RegionWaypointPoint();
                                    tempRegionWaypoint._regionID = locationRegionWaypoint._regionID;
                                    tempRegionWaypoint._waypointID = guid;
                                    _waypointsOnWrongWay[locationRegionWaypoint].Add(tempRegionWaypoint);
                                }
                            }
                            else if(distanceBetweenCurrentAndNeighbor < 2/* && distanceBetweenNextAndNeighbor < 2*/)
                            {
                                List<Guid> nearWaypointNeighbor = new List<Guid>();
                                nearWaypointNeighbor = _navigationGraph.GetNeighbor(locationRegionWaypoint._regionID, guid);

                                LocationType currentType =
                                    _navigationGraph.GetWaypointTypeInRegion(locationRegionWaypoint._regionID,
                                                                             guid);
                                Region nearWaypointRegion = new Region();
                                nearWaypointRegion = _regiongraphs[locationRegionWaypoint._regionID];

                                if (currentType.ToString() == "portal")
                                {
                                    foreach (Guid nearWaypointRegionGuid in nearWaypointRegion._neighbors)
                                    {
                                        RegionWaypointPoint portalNearWaypointRegionGuid = new RegionWaypointPoint();
                                        portalNearWaypointRegionGuid = _navigationGraph.GiveNeighborWaypointInNeighborRegion(
                                                        locationRegionWaypoint._regionID,
                                                        guid,
                                                        nearWaypointRegionGuid);
                                        if (portalNearWaypointRegionGuid._waypointID != Guid.Empty)
                                        {
                                            if (_waypointsOnRoute.Count() > nextStep)
                                            {
                                                if (_waypointsOnRoute[nextStep]._waypointID != portalNearWaypointRegionGuid._waypointID)
                                                {
                                                    if (!_waypointsOnWrongWay.Keys.Contains(locationRegionWaypoint))
                                                    {
                                                        tempRegionWaypoint = new RegionWaypointPoint();
                                                        tempRegionWaypoint._waypointID = portalNearWaypointRegionGuid._waypointID;
                                                        tempRegionWaypoint._regionID = portalNearWaypointRegionGuid._regionID;
                                                        _waypointsOnWrongWay.Add(locationRegionWaypoint, new List<RegionWaypointPoint> { portalNearWaypointRegionGuid });

                                                    }
                                                    else
                                                    {
                                                        tempRegionWaypoint = new RegionWaypointPoint();
                                                        tempRegionWaypoint._regionID = portalNearWaypointRegionGuid._regionID;
                                                        tempRegionWaypoint._waypointID = portalNearWaypointRegionGuid._waypointID;
                                                        if (_waypointsOnWrongWay[locationRegionWaypoint].Contains(portalNearWaypointRegionGuid) == true)
                                                        {
                                                            _waypointsOnWrongWay[locationRegionWaypoint].Add(portalNearWaypointRegionGuid);

                                                        }
                                                    }
                                                }
                                            }
                                        }        

                                    }
                                }

                                List<Guid> nearNonePortalWaypoint = new List<Guid>();

                                nearNonePortalWaypoint = _navigationGraph.GetNeighbor(locationRegionWaypoint._regionID, guid);
                              
                                foreach (Guid nearWaypointofSameRegion in nearNonePortalWaypoint)
                                {
                                    if (_waypointsOnRoute.Count() > nextStep)
                                    {
                                        double distanceBetweenCurrentAndNearNeighbor = _navigationGraph.StraightDistanceBetweenWaypoints(
                                                                                        locationRegionWaypoint._regionID,
                                                                                        locationRegionWaypoint._waypointID,
                                                                                        nearWaypointofSameRegion);

                                        double distanceBetweenNextAndNearNeighbor = 0;
                                        if (locationRegionWaypoint._regionID==_waypointsOnRoute[nextStep]._regionID)
                                        {
                                            distanceBetweenNextAndNearNeighbor = _navigationGraph.StraightDistanceBetweenWaypoints(
                                                                                        locationRegionWaypoint._regionID,
                                                                                        locationRegionWaypoint._waypointID,
                                                                                        nearWaypointofSameRegion);
                                        }
                                        else
                                        {
                                            distanceBetweenNextAndNearNeighbor = distanceBetweenCurrentAndNearNeighbor;
                                        }
                                        
                                        if (_waypointsOnRoute[nextStep]._waypointID != nearWaypointofSameRegion &&
                                            nearWaypointofSameRegion!=guid&&
                                            distanceBetweenCurrentAndNearNeighbor>=2&&
                                            distanceBetweenNextAndNearNeighbor>=2)
                                        {
                                            if (!_waypointsOnWrongWay.Keys.Contains(locationRegionWaypoint))
                                            {
                                                tempRegionWaypoint = new RegionWaypointPoint();
                                                tempRegionWaypoint._regionID = locationRegionWaypoint._regionID;
                                                tempRegionWaypoint._waypointID = nearWaypointofSameRegion;
                                                _waypointsOnWrongWay.Add(locationRegionWaypoint, new List<RegionWaypointPoint> { tempRegionWaypoint });
                                            }
                                            else
                                            {
                                                tempRegionWaypoint = new RegionWaypointPoint();
                                                tempRegionWaypoint._regionID = locationRegionWaypoint._regionID;
                                                tempRegionWaypoint._waypointID = nearWaypointofSameRegion;
                                                _waypointsOnWrongWay[locationRegionWaypoint].Add(tempRegionWaypoint);
                                            }
                                        }
                                        else
                                        {
                                            if (!_waypointsOnWrongWay.Keys.Contains(locationRegionWaypoint))
                                            {
                                                _waypointsOnWrongWay.Add(locationRegionWaypoint, new List<RegionWaypointPoint> { });
                                            }
                                        }
                                    }
                                }
                            }
                            
                        }
                        else
                        {
                            if (!_waypointsOnWrongWay.Keys.Contains(locationRegionWaypoint))
                            {
                                _waypointsOnWrongWay.Add(locationRegionWaypoint, new List<RegionWaypointPoint> { });
                            }
                        }
                    }
                }
                nextStep++;
            }

            //Print All Possible Wrong Way
            foreach (KeyValuePair<RegionWaypointPoint, List<RegionWaypointPoint>> item in _waypointsOnWrongWay)
            {
                Console.WriteLine("Region ID : " + item.Key._regionID);
                Console.WriteLine("Waypoint ID : " + item.Key._waypointID);
                Console.WriteLine("All possible Wrong : ");
                foreach (RegionWaypointPoint items in item.Value)
                {
                    Console.WriteLine("Region Guid : " + items._regionID);
                    Console.WriteLine("Waypoint Guid : " + items._waypointID);
                }
                Console.WriteLine("\n");
            }
        }

        //In this function we get the currentwaypoint and determine whether
        //the users are in the right path or not.
        //And we return a structure called navigationInstruction that 
        //contains four elements that Navigation main and UI need.
        //Moreover, if the users are not on the right path, we reroute and 
        //tell users the new path.
        public void CheckArrivedWaypoint(object sender, EventArgs args)
        {
            Console.WriteLine(">> CheckArrivedWaypoint ");

            _currentWaypointID = (args as WaypointSignalEventArgs)._detectedRegionWaypoint._waypointID;
            _currentRegionID = (args as WaypointSignalEventArgs)._detectedRegionWaypoint._regionID;
            Console.WriteLine("CheckArrived currentWaypoint : " + _currentWaypointID);
            Console.WriteLine("CheckArrived currentRegion : " + _currentRegionID);
            RegionWaypointPoint detectWrongWay = new RegionWaypointPoint();
            detectWrongWay._waypointID = _currentWaypointID;
            detectWrongWay._regionID = _currentRegionID;

            //NavigationInstruction is a structure that contains five
            //elements that need to be passed to the main and UI
            NavigationInstruction navigationInstruction = 
                new NavigationInstruction();

            if (_nextWaypointStep == -1)
            {                
                if (_currentRegionID.Equals(_destinationRegionID) &&
                    _currentWaypointID.Equals(_destinationWaypointID))
                {
                    Console.WriteLine("---- [case: arrived destination] .... ");

                    _event.OnEventCall(new NavigationEventArgs
                    {
                        _result = NavigationResult.Arrival
                    });
                }
                _nextWaypointEvent.Set();

            }
            else
            {
                if (_currentRegionID.Equals(_destinationRegionID) &&
                    _currentWaypointID.Equals(_destinationWaypointID))
                {
                    Console.WriteLine("---- [case: arrived destination] .... ");

                    _event.OnEventCall(new NavigationEventArgs
                    {
                        _result = NavigationResult.Arrival
                    });
                }
                else if (_currentRegionID.Equals(
                             _waypointsOnRoute[_nextWaypointStep]._regionID) &&
                         _currentWaypointID.Equals(
                             _waypointsOnRoute[_nextWaypointStep]._waypointID))
                {
                    Console.WriteLine("---- [case: arrived waypoint] .... ");

                    Console.WriteLine("current region/waypoint: {0}/{1}",
                                      _currentRegionID,
                                      _currentWaypointID);
                    Console.WriteLine("next region/waypoint: {0}/{1}",
                                      _waypointsOnRoute[_nextWaypointStep + 1]._regionID,
                                      _waypointsOnRoute[_nextWaypointStep + 1]._waypointID);
                    navigationInstruction._currentWaypointName =
                        _navigationGraph.GetWaypointNameInRegion(_currentRegionID,
                                                                 _currentWaypointID);
                    navigationInstruction._nextWaypointName =
                        _navigationGraph.GetWaypointNameInRegion(
                            _waypointsOnRoute[_nextWaypointStep + 1]._regionID,
                            _waypointsOnRoute[_nextWaypointStep + 1]._waypointID);

                    Guid previousRegionID = new Guid();
                    Guid previousWaypointID = new Guid();
                    if (_nextWaypointStep - 1 >= 0) {
                        previousRegionID =
                            _waypointsOnRoute[_nextWaypointStep - 1]._regionID;
                        previousWaypointID =
                            _waypointsOnRoute[_nextWaypointStep - 1]._waypointID;
                    }

                    navigationInstruction._information =
                        _navigationGraph
                        .GetInstructionInformation(
                            _nextWaypointStep,
                            _currentRegionID,
                            _currentWaypointID,
                            previousRegionID,
                            previousWaypointID,
                            _waypointsOnRoute[_nextWaypointStep + 1]._regionID,
                            _waypointsOnRoute[_nextWaypointStep + 1]._waypointID,
                            _avoidConnectionTypes);
                    
                    //Get the progress
                    Console.WriteLine("calculate progress: {0}/{1}",
                                      _nextWaypointStep,
                                      _waypointsOnRoute.Count);

                    navigationInstruction._progress =
                        (double)Math.Round(100 * ((decimal)_nextWaypointStep /
                                           (_waypointsOnRoute.Count - 1)), 3);

                    // Raise event to notify the UI/main thread with the result
                    _event.OnEventCall(new NavigationEventArgs
                    {
                        _result = NavigationResult.Run,
                        _nextInstruction = navigationInstruction
                    });
                }
                else if (_nextWaypointStep >= 1 &&
                    _waypointsOnWrongWay[_waypointsOnRoute[_nextWaypointStep - 1]].Contains(detectWrongWay) == false)
                {
                    Console.WriteLine("---- [case: wrong waypoint] .... ");
                    _event.OnEventCall(new NavigationEventArgs
                    {
                        _result = NavigationResult.AdjustRoute
                    });
                    Console.WriteLine("Adjust Route");
                }
                
                _nextWaypointEvent.Set();
            }

            Console.WriteLine("<< CheckArrivedWaypoint ");
        }

        public void CloseSession()
        {
            _isKeepDetection = false;
            _IPSClient.Stop();
            _nextWaypointEvent.Dispose();
            _waypointDetectionThread.Abort();
            _navigationControllerThread.Abort();
        }

        

        public enum NavigationResult
        {
            Run = 0,
            AdjustRoute,
            Arrival,
        }

        public class NavigationInstruction
        {
            public string _currentWaypointName;

            public string _nextWaypointName;

            public double _progress;

            public InstructionInformation _information { get; set; }
        }

        public class NavigationEventArgs : EventArgs
        {
            public NavigationResult _result { get; set; }

            public NavigationInstruction _nextInstruction { get; set; }

        }
    }

}
