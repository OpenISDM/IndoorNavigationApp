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
 *      Chun Yu Lai, chunyu1202@gmail.com
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
        private IIPSClient _ipsClient;

        private int _currentNavigateStep;

        private List<Waypoint> _waypointsOnRoute = new List<Waypoint>();
        private Dictionary<Guid, List<Waypoint>> _waypointsOnWrongWay = new Dictionary<Guid, List<Waypoint>>();

        private Graph<Region, string> _regionGraph = 
                                        new Graph<Region, string>();
        private Graph<Waypoint, string> _subgraph = 
                                        new Graph<Waypoint, string>();

        public NavigationEvent _event { get; private set; }

        private Waypoint _startWaypoint = new Waypoint();
        private Waypoint _finalWaypoint = new Waypoint();

        private Thread _navigateThread;

        public Session(Navigraph graph,
                      // Guid startWaypointID,
                       Guid finalWaypointID,
                       int[] avoid)
        {
            _event = new NavigationEvent();

            //Read the xml file to get regions and edges information
            _regionGraph = graph.GetRegiongraph();
            //We only consider to one region situation,
            //therefore, we add different floors' waypoints in same regions
            _subgraph = graph.Regions[0].GetNavigationSubgraph(avoid);

            //Use the ID we get to search where the waypoints are
            _finalWaypoint = _subgraph.Where(node => 
            node.Item.ID.Equals(finalWaypointID)).Select(w => w.Item).First();

            _ipsClient = new WaypointClient();
            _ipsClient._event._eventHandler += new EventHandler(CheckArrivedWaypoint);

            _navigateThread = new Thread(() => InvokeIPSWork());
            _navigateThread.Start();

            _currentNavigateStep = -1;
            StartNavigation(_currentNavigateStep);
        }

        public void StartNavigation(int currentStep) {
            List<Waypoint> monitorWaypointList = new List<Waypoint>();

            if (currentStep == -1)
            {
                // Detection of starting waypoing:
                // Use all Waypoints of the NavigationGraph to detect the starting waypoint
                monitorWaypointList = _subgraph.Select(waypoint => waypoint.Item).ToList();
                foreach (Waypoint waypoint in monitorWaypointList) {
                    Console.WriteLine("all waypoints: " + waypoint.ID);
                }
            }
            else {
                monitorWaypointList.Add(_waypointsOnRoute[currentStep]);
                for (int i = 0; i < _waypointsOnWrongWay[_waypointsOnRoute[currentStep].ID].Count; i++)
                {
                    monitorWaypointList.Add(_waypointsOnWrongWay[_waypointsOnRoute[currentStep].ID][i]);
                }

                foreach (Waypoint waypoing in monitorWaypointList)
                {
                    Console.WriteLine("Monitoring waypoints: " + waypoing.ID);
                }
            }
           
            _ipsClient.SetWaypointList(monitorWaypointList);
        }

        private void InvokeIPSWork() {
            Console.WriteLine("---- invokeIPSWork ----");
            while (true)
            {
                Thread.Sleep(1000);
                _ipsClient.DetectWaypoints();
            }
        }

        private void GetPath(Waypoint startWaypoint, 
                             Waypoint finalWaypoint,
                             Graph<Waypoint, string> subgraph)
        {
            //Get the keys of the start way point and destination waypoint
            //and throw the two keys into dijkstra to get the best route
            var startPoingKey = subgraph
                .Where(waypoint => waypoint.Item.ID
                .Equals(startWaypoint.ID)).Select(c => c.Key).First();
            var endPointKey = subgraph
                .Where(waypoint => waypoint.Item.ID
                .Equals(finalWaypoint.ID)).Select(c => c.Key).First();

            var path = subgraph.Dijkstra(startPoingKey,
                                           endPointKey).GetPath();

            for (int i = 0; i < path.Count(); i++)
            {
                _waypointsOnRoute.Add(subgraph[path.ToList()[i]].Item);
               
                List<Waypoint> tempWrongWaypointList = new List<Waypoint>();

                // Construct the two-step possible wrong-way waypoints
                Console.WriteLine("construct: correct waypoint: " + _waypointsOnRoute[i].ID);
                if (i - 1 >= 0)
                {
                    for (int j = 0; j < _waypointsOnRoute[i-1].Neighbors.Count; j++)
                    {
                        if(!subgraph[path.ToList()[i]].Item.ID.
                            Equals(_waypointsOnRoute[i-1].Neighbors[j].TargetWaypointUUID))
                        { 
                            Waypoint oneStepWrongWaypoint =
                                subgraph.Where(waypoint => waypoint.Item.ID.
                                Equals(_waypointsOnRoute[i - 1].Neighbors[j].TargetWaypointUUID)).
                                Select(n => n.Item).First();

                            Console.WriteLine("construct: one step wrong waypoint: " +
                                              oneStepWrongWaypoint.ID);

                            for (int m = 0; m < oneStepWrongWaypoint.Neighbors.Count; m++) {
                                Waypoint twoStepWrongWaypoint =
                                    subgraph.Where(waypoint => waypoint.Item.ID.
                                    Equals(oneStepWrongWaypoint.Neighbors[m].TargetWaypointUUID)).
                                    Select(n => n.Item).
                                    First();

                                if (!twoStepWrongWaypoint.ID.Equals(oneStepWrongWaypoint.ID) &&
                                    !twoStepWrongWaypoint.ID.Equals(_waypointsOnRoute[i-1].ID))
                                {
                                    Console.WriteLine("construct: two step wrong waypoint: " +
                                                      twoStepWrongWaypoint.ID);

                                    tempWrongWaypointList.Add(twoStepWrongWaypoint);
                                }
                            }
                        }
                    }
                }
                _waypointsOnWrongWay.Add(_waypointsOnRoute[i].ID, tempWrongWaypointList);
            }

            foreach (Waypoint waypoint in _waypointsOnRoute)
            {
                Console.WriteLine("correct waypoint:" + waypoint.ID);
                for (int i = 0; i < _waypointsOnWrongWay[waypoint.ID].Count; i++) {
                    Console.WriteLine("possible wrong-way waypoint: {0}",
                                      _waypointsOnWrongWay[waypoint.ID][i].ID);
                }
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
            Waypoint currentWaypoint = _subgraph.Where(node =>
            node.Item.ID.Equals((args as WayPointSignalEventArgs).CurrentWaypoint.ID)).
            Select(w => w.Item).First();

            if (_currentNavigateStep == -1)
            {
                // Detection of starting waypoing:
                // Detected the waypoint most closed to user.
                _startWaypoint = currentWaypoint;

                _currentNavigateStep = 0;
                GetPath(_startWaypoint, _finalWaypoint, _subgraph);
                StartNavigation(_currentNavigateStep);
            }
            else
            {

                //NavigationInstruction is a structure that contains five
                //elements that need to be passed to the main and UI
                NavigationInstruction navigationInstruction =
                                                new NavigationInstruction();
                //getWaypoint is used to check where the current way point is in
                //which place of the Allcorrectwaypoint

                if (currentWaypoint == _finalWaypoint)
                {
                    Console.WriteLine("---- [case: arrived destination] .... ");
                    _event.OnEventCall(new NavigationEventArgs
                    {
                        Result = NavigationResult.Arrival
                    });
                }
                else if (_waypointsOnRoute[_currentNavigateStep].Equals(currentWaypoint))
                {
                    Console.WriteLine("---- [case: arrived waypoint] .... ");

                    Console.WriteLine("current waypoint: " + currentWaypoint.ID);
                    Console.WriteLine("next waypoint: " + _waypointsOnRoute[_currentNavigateStep + 1].ID);

                    navigationInstruction.CurrentWaypoint = currentWaypoint;
                    navigationInstruction.NextWaypoint =
                                            _waypointsOnRoute[_currentNavigateStep + 1];


                    //If the floor information between the current waypoint and
                    //next are different, it means that the users need to change
                    //the floor, therefore, we can determine the users need to 
                    //go up or down by compare which floor is higher

                    if (currentWaypoint.Floor !=
                                    _waypointsOnRoute[_currentNavigateStep + 1].Floor)
                    {
                        Console.WriteLine("different floor case");
                        if (currentWaypoint.Floor >
                                    _waypointsOnRoute[_currentNavigateStep + 1].Floor)
                        {
                            navigationInstruction.Direction = TurnDirection.Down;
                        }
                        else
                        {
                            navigationInstruction.Direction = TurnDirection.Up;
                        }
                        navigationInstruction.Distance = 0;
                        Console.WriteLine("end of different floor case");
                    }
                    //If the fllor information between current way point and next
                    // way point are the same, we need to tell the user go
                    //straight or turn direction, therefore, we need to have
                    //three informations, previous, current and next waypoints
                    else
                    {
                        Console.WriteLine("same floor case");
                        navigationInstruction.Distance =
                        Navigraph.GetDistance(_subgraph,
                                              currentWaypoint,
                                              _waypointsOnRoute[_currentNavigateStep + 1]);
                 
                        if (_currentNavigateStep == 0)
                        {
                            navigationInstruction.Direction =
                                                    TurnDirection.FirstDirection;
                        }
                        else
                        {
                            navigationInstruction.Direction =
                                Navigraph.GetTurnDirection(
                                               _waypointsOnRoute[_currentNavigateStep - 1],
                                                currentWaypoint,
                                               _waypointsOnRoute[_currentNavigateStep + 1]);
                        }
                        Console.WriteLine("end of same floor case");
                    }

                    //Get the progress
                    Console.WriteLine("calculate progress: {0}/{1}",
                                      _currentNavigateStep,
                                      _waypointsOnRoute.Count);

                    navigationInstruction.Progress =
                        (double)Math.Round(100 * ((decimal)_currentNavigateStep /
                                           (_waypointsOnRoute.Count - 1)), 3);

                    // Raise event to notify the UI/main thread with the result
                    _event.OnEventCall(new NavigationEventArgs
                    {
                        Result = NavigationResult.Run,
                        NextInstruction = navigationInstruction
                    });
                    Console.WriteLine("After raising event from Session");

                    _currentNavigateStep++;
                    StartNavigation(_currentNavigateStep);
                }
                else
                {
                    Console.WriteLine("arrived waypoint ID:" + currentWaypoint.ID);
                    Console.WriteLine("possible wrong waypoint:");

                    bool isWrongWaypoint = false;

                    for (int i = 0;
                         i < _waypointsOnWrongWay[_waypointsOnRoute[_currentNavigateStep].ID].Count;
                         i++) {

                        Console.WriteLine("waypoing ID:" +
                                          _waypointsOnWrongWay[_waypointsOnRoute[_currentNavigateStep].ID][i].ID);

                        if (currentWaypoint.ID.
                            Equals(_waypointsOnWrongWay[_waypointsOnRoute[_currentNavigateStep].ID][i].ID)){

                            isWrongWaypoint = true;
                            break;

                        }
                    }
                    if (isWrongWaypoint)
                    {
                        Console.WriteLine("---- [case: wrong waypoint] .... ");
                        _event.OnEventCall(new NavigationEventArgs
                        {
                            Result = NavigationResult.AdjustRoute
                        });

                        //If the waypoint is wrong, we initial the correct waypoint
                        // and its neighbors and rerun the GetPath function and reget
                        //the navigationInstruction
                        _waypointsOnRoute = new List<Waypoint>();
                        _waypointsOnWrongWay = new Dictionary<Guid, List<Waypoint>>();
                        GetPath(currentWaypoint, _finalWaypoint, _subgraph);

                        _currentNavigateStep = 0;
                        _event.OnEventCall(new NavigationEventArgs
                        {
                            Result = NavigationResult.Run,
                            NextInstruction = new NavigationInstruction
                            {
                                CurrentWaypoint = currentWaypoint,
                                NextWaypoint = _waypointsOnRoute[_currentNavigateStep + 1],
                                Distance = Navigraph.
                                GetDistance(_subgraph,
                                            currentWaypoint,
                                            _waypointsOnRoute[_currentNavigateStep + 1]),
                                Progress = 0,
                                Direction = TurnDirection.FirstDirection
                            }
                        });
                        StartNavigation(_currentNavigateStep);
                    }
                }
            }
            Console.WriteLine("<< CheckArrivedWaypoint ");
        }

        public enum NavigationResult
        {
            Run = 0,
            AdjustRoute,
            Arrival,
        }

        public class NavigationInstruction
        {
            /// <summary>
            /// The next waypoint within navigation path
            /// </summary>
            /// 
            public Waypoint CurrentWaypoint;

            public Waypoint NextWaypoint;

            /// <summary>
            /// The distance from current waypoint to NextWaypoint
            /// </summary>
            public double Distance;

            /// <summary>
            /// Progress of navigation.
            /// </summary>
            public double Progress;

            /// <summary>
            /// The direction to turn to the next waypoint using the enum type
            /// </summary>
            public TurnDirection Direction;
        }

        public class NavigationEventArgs : EventArgs
        {
            /// <summary>
            /// Status of navigation
            /// </summary>
            public NavigationResult Result { get; set; }

            /// <summary>
            /// Gets or sets the next instruction. It will send to the
            // ViewModel to update the UI instruction.
            /// </summary>
            public NavigationInstruction NextInstruction { get; set; }

        }
    }

}
