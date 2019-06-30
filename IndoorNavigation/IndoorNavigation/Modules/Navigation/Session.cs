/*
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
        private Dictionary<Guid, List<Waypoint>> _waypointsOnWrongWay =
                new Dictionary<Guid, List<Waypoint>>();
        
        private Graph<Guid, string> _graphRegionGraph = new Graph<Guid, string>();
        
        public NavigationEvent _event { get; private set; }

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

            _nextWaypointStep = -1;
            _isKeepDetection = true;

            _IPSClient = new WaypointClient();
            _IPSClient._event._eventHandler += new EventHandler(CheckArrivedWaypoint);

            _waypointDetectionThread = new Thread(() => InvokeIPSWork());
            _waypointDetectionThread.Start();

            _navigationControllerThread = new Thread(() => NavigatorProgram());
            _navigationControllerThread.Start();
        }

        private void NavigatorProgram(){
            Guid sourceWaypointID = new Guid();

            _nextWaypointStep = -1;
            _currentRegionID = _sourceRegionID;
            _currentWaypointID = new Guid();

            NavigateToNextWaypoint(_nextWaypointStep);

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
                    break;
                }

                if (_nextWaypointStep == -1)
                {
                    Console.WriteLine("Detected start waypoint: " + _currentWaypointID);
                    // Detection of starting waypoing:
                    // Detected the waypoint most closed to user.
                    sourceWaypointID = _currentWaypointID;

                    GenerateRoute(_sourceRegionID,
                                  sourceWaypointID,
                                  _destinationRegionID,
                                  _destinationWaypointID);

                    _nextWaypointStep++;
                    NavigateToNextWaypoint(_nextWaypointStep);
                }/*
                else if (_currentWaypointID.Equals(_waypointsOnRoute[_nextWaypointStep]))
                {
                    Console.WriteLine("Arrived waypoint: " + _currentWaypoint._id);
                    _nextWaypointStep++;
                    NavigateToNextWaypoint(_nextWaypointStep);
                }
                else {
                    for (int i = 0; 
                         i < _waypointsOnWrongWay[_waypointsOnRoute[_nextWaypointStep]._id].Count; 
                         i++) {

                        if (_currentWaypoint._id.Equals(_waypointsOnWrongWay[_waypointsOnRoute[_nextWaypointStep]._id][i]._id)){

                            Console.WriteLine("At wrong waypoint: " + _currentWaypoint._id);
                            sourceWaypointID = _currentWaypoint._id;
                           // GenerateRoute(startWaypoint, _finalWaypoint, _subgraph);

                            _event.OnEventCall(new NavigationEventArgs
                            {
                                _result = NavigationResult.Run,
                                _nextInstruction = new NavigationInstruction
                                {
                                    _currentWaypointName = startWaypoint,
                                    _nextWaypointName = _waypointsOnRoute[1],
                                   
                                    Distance =
                                        Subgraph.GetDistance(_subgraph,
                                                              startWaypoint,
                                                              _waypointsOnRoute[1]),
                                    _progress = 0,
                                    _direction = TurnDirection.FirstDirection
                                }
                            });

                            _nextWaypointStep = 0;
                            NavigateToNextWaypoint(_nextWaypointStep);

                            break;
                        }
                    }
                }*/
                _nextWaypointEvent.Reset();
            }
        }

        private void NavigateToNextWaypoint(int nextStep) {
            List<WaypointBeaconsMapping> monitorWaypointList =
                new List<WaypointBeaconsMapping>();

            if (nextStep == -1)
            {
                // Detection of starting waypoing:
                // Use all Waypoints of the NavigationGraph to detect the starting waypoint
                List<Guid> waypointIDs =
                    _navigationGraph.GetAllWaypointIDInOneRegion(_sourceRegionID);
                foreach (Guid waypointID in waypointIDs)
                {
                    List<Guid> beaconIDs =
                        _navigationGraph.GetAllBeaconIDInOneWaypointOfRegion(_sourceRegionID,
                                                                               waypointID);
                    monitorWaypointList.Add(new WaypointBeaconsMapping
                    {
                        _WaypointID = waypointID,
                        _Beacons = beaconIDs
                    });
                }
                foreach (WaypointBeaconsMapping mappingItem in monitorWaypointList) {
                    Console.WriteLine("monitor waypoint={0}", mappingItem._WaypointID);
                    foreach (Guid beaconID in mappingItem._Beacons) {
                        Console.WriteLine("beacon={0}", beaconID);
                    }
                }
            }
            else
            {
                /*
                monitorWaypointList.Add(_waypointsOnRoute[nextStep]);
                for (int i = 0; i < _waypointsOnWrongWay[_waypointsOnRoute[nextStep]._id].Count; i++)
                {
                    monitorWaypointList.Add(_waypointsOnWrongWay[_waypointsOnRoute[nextStep]._id][i]);
                }

                foreach (Waypoint waypoing in monitorWaypointList)
                {
                    Console.WriteLine("Monitoring waypoints: " + waypoing._id);
                }*/
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

                    for (int j = 0; j < pathWaypoints.Count(); j++)
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

            Guid currentWaypointID = (args as WaypointSignalEventArgs)._detectedWaypointID;
            
            //NavigationInstruction is a structure that contains five
            //elements that need to be passed to the main and UI
            NavigationInstruction navigationInstruction = 
                new NavigationInstruction();

            if (_nextWaypointStep == -1)
            {
                _currentWaypointID = currentWaypointID;
                _nextWaypointEvent.Set();
                
                if (_currentRegionID.Equals(_destinationRegionID) &&
                    _currentWaypointID.Equals(_destinationWaypointID))
                {
                    Console.WriteLine("---- [case: arrived destination] .... ");

                    _event.OnEventCall(new NavigationEventArgs
                    {
                        _result = NavigationResult.Arrival
                    });
                }
            }
            else
            {
                /*
                if (currentWaypointID.Equals(_destinationWaypointID))
                {
                    Console.WriteLine("---- [case: arrived destination] .... ");

                    _event.OnEventCall(new NavigationEventArgs
                    {
                        _result = NavigationResult.Arrival
                    });
                }
                else if (_waypointsOnRoute[_nextWaypointStep].Equals(currentWaypointID))
                {
                    Console.WriteLine("---- [case: arrived waypoint] .... ");

                    Console.WriteLine("current waypoint: " + currentWaypointID);
                    Console.WriteLine("next waypoint: " + _waypointsOnRoute[_nextWaypointStep + 1]._id);
                    navigationInstruction._currentWaypointName = currentWaypoint;
                    navigationInstruction._nextWaypointName =
                        _waypointsOnRoute[_nextWaypointStep + 1];
                        
                    //If the floor information between the current waypoint and
                    //next are different, it means that the users need to change
                    //the floor, therefore, we can determine the users need to 
                    //go up or down by compare which floor is higher
                    
                    if (currentWaypoint.Floor !=
                        _waypointsOnRoute[_nextWaypointStep + 1].Floor)
                    {

                        if (currentWaypoint.Floor >
                            _waypointsOnRoute[_nextWaypointStep + 1].Floor)
                        {
                            navigationInstruction.Direction = TurnDirection.Down;
                        }
                        else
                        {
                            navigationInstruction.Direction = TurnDirection.Up;
                        }
                        navigationInstruction.Distance = 0;

                    }
                    //If the fllor information between current way point and next
                    // way point are the same, we need to tell the user go
                    //straight or turn direction, therefore, we need to have
                    //three informations, previous, current and next waypoints
                    else
                    {
                    
                        navigationInstruction._distance = 10;
                    
                        Subgraph.GetDistance(_subgraph,
                                              currentWaypoint,
                                              _waypointsOnRoute[_nextWaypointStep + 1]);
                        if (_nextWaypointStep == 0)
                        {
                            navigationInstruction._direction =
                                TurnDirection.FirstDirection;
                        }
                        else
                        {
                            navigationInstruction._direction = TurnDirection.FirstDirection;
                                Subgraph.GetTurnDirection(
                                    _waypointsOnRoute[_nextWaypointStep - 1],
                                    currentWaypoint,
                                    _waypointsOnRoute[_nextWaypointStep + 1]);
                         }
                       
                    }

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
                else
                {
                    /*
                    Console.WriteLine("arrived waypoint ID:" + currentWaypointID);
                    Console.WriteLine("possible wrong waypoint:");

                    for (int i = 0;
                         i < _waypointsOnWrongWay[_waypointsOnRoute[_nextWaypointStep]._id].Count;
                         i++)
                    {

                        Console.WriteLine("waypoing ID:" +
                                          _waypointsOnWrongWay[_waypointsOnRoute[_nextWaypointStep]
                                          ._id][i]._id);

                        if (currentWaypointID.
                            Equals(_waypointsOnWrongWay[_waypointsOnRoute[_nextWaypointStep]._id][i]._id))
                        {

                            Console.WriteLine("---- [case: wrong waypoint] .... ");
                            _event.OnEventCall(new NavigationEventArgs
                            {
                                _result = NavigationResult.AdjustRoute
                            });

                            break;
                        }
                    }
                }*/
                
                _currentWaypointID = currentWaypointID;
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

            public double _distance;

            public double _progress;

            public TurnDirection _direction;
        }

        public class NavigationEventArgs : EventArgs
        {
            public NavigationResult _result { get; set; }

            public NavigationInstruction _nextInstruction { get; set; }

        }
    }

}
