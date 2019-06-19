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

        private int _currentNavigateStep;

        private List<Waypoint> _waypointsOnRoute = new List<Waypoint>();
        private Dictionary<Guid, List<Waypoint>> _waypointsOnWrongWay = new Dictionary<Guid, List<Waypoint>>();

        private Graph<Region, string> _regionGraph = 
                                        new Graph<Region, string>();
        private Graph<Waypoint, string> _subgraph = 
                                        new Graph<Waypoint, string>();

        public SessionEvent Event { get; private set; }

        public Waypoint _startWaypoint = new Waypoint();
        public Waypoint _finalWaypoint = new Waypoint();

        private Thread _navigateThread;

        public Session(Navigraph graph,
                       Guid startWaypointID,
                       Guid finalWaypointID,
                       int[] avoid)
        {
            Event = new SessionEvent();

            //Read the xml file to get regions and edges information
            _regionGraph = graph.GetRegiongraph();
            //We only consider to one region situation,
            //therefore, we add different floors' waypoints in same regions
            _subgraph = graph.Regions[0].GetNavigationSubgraph(avoid);

            //Use the ID we get to search where the waypoints are
            _startWaypoint = _subgraph.Where(node =>
            node.Item.ID.Equals(startWaypointID)).Select(w => w.Item).First();

            _finalWaypoint = _subgraph.Where(node => 
            node.Item.ID.Equals(finalWaypointID)).Select(w => w.Item).First();

            //Get the best route through dijkstra
            _currentNavigateStep = 0;
            GetPath(_startWaypoint, _finalWaypoint, _subgraph);

            _IPSClient = new WaypointClient();
            _IPSClient.Event.EventHandler = new EventHandler(CheckArrivedWaypoint);
            Console.WriteLine("--------Constructor-------------------");
            Console.WriteLine("source " + _startWaypoint.ID);
            Console.WriteLine("destination " + _finalWaypoint.ID);
            Console.WriteLine("---------------------------");
            Console.WriteLine("Routing path:");
            foreach (Waypoint waypoint in _waypointsOnRoute)
            {
                Console.WriteLine("waypoint " +  waypoint.ID);
            }
            Console.WriteLine("---------------------------");
            foreach (Waypoint waypoint in _waypointsOnRoute)
            {
                Console.WriteLine("waypoint "+ waypoint.ID);
                for (int i = 0; i < _waypointsOnWrongWay[waypoint.ID].Count; i++) {
                    Console.WriteLine("possible wrong [" +  _waypointsOnWrongWay[waypoint.ID][i].ID + "]");
                }
            }

            _navigateThread = new Thread(() => invokeIPSWork(_currentNavigateStep));
            _navigateThread.Start();


            Console.WriteLine("---------Finish of contructor------------------");
            StartNavigate(_currentNavigateStep);
        }

        public void StartNavigate(int currentStep) {
            List<Waypoint> monitorWaypointList = new List<Waypoint>();
            monitorWaypointList.Add(_waypointsOnRoute[currentStep]);
            for (int i = 0; i < _waypointsOnWrongWay[_waypointsOnRoute[currentStep].ID].Count; i++)
            {
                monitorWaypointList.Add(_waypointsOnWrongWay[_waypointsOnRoute[currentStep].ID][i]);
            }

            foreach (Waypoint waypoing in monitorWaypointList)
            {
                Console.WriteLine("In Session's StartNavigate waypoints for monitoring: " + waypoing.ID);
            }
            _IPSClient.SetWaypointList(monitorWaypointList);

        }
        private void invokeIPSWork(int currentStep) {
            /*           
                        List<Waypoint> monitorWaypointList = new List<Waypoint>();
                        monitorWaypointList.Add(_waypointsOnRoute[currentStep]);
                        for(int i = 0; i < _waypointsOnWrongWay[_waypointsOnRoute[currentStep].ID].Count; i++) { 
                            monitorWaypointList.Add(_waypointsOnWrongWay[_waypointsOnRoute[currentStep].ID][i]);
                        }

                        foreach (Waypoint waypoing in monitorWaypointList) {
                            Console.WriteLine("waypoints for monitoring: " + waypoing.ID);
                        }
                       _IPSClient.SetWaypointList(monitorWaypointList);
              */
            Console.WriteLine("---- invokeIPSWork ----");
            while (true)
            {
                Thread.Sleep(1000);
                _IPSClient.SignalProcessing();
                Console.WriteLine("thread for IPSClient.SignalProcessing....");   
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

            //Put the correct waypoint into List all_correct_waaypoint
            //Put all correct waypoints and their neighbors in list
            //AllNoneWrongWaypoint
            for (int i = 0; i < path.Count(); i++)
            {
                _waypointsOnRoute.Add(subgraph[path.ToList()[i]].Item);
               
                List<Waypoint> tempWrongWaypointList = new List<Waypoint>();
                if (i - 1 >= 0)
                {
                    for (int j = 0; j < _waypointsOnRoute[i-1].Neighbors.Count; j++)
                    {
                        if(!subgraph[path.ToList()[i]].Item.ID.Equals(_waypointsOnRoute[i-1].Neighbors[j].TargetWaypointUUID))
                        {
                            tempWrongWaypointList.Add(new Waypoint { ID = _waypointsOnRoute[i-1].Neighbors[j].TargetWaypointUUID });
                            
                        }
                    }
                }
                _waypointsOnWrongWay.Add(_waypointsOnRoute[i].ID, tempWrongWaypointList);
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

            //NavigationInstruction is a structure that contains five
            //elements that need to be passed to the main and UI
            NavigationInstruction navigationInstruction =
                                            new NavigationInstruction();
            //getWaypoint is used to check where the current way point is in
            //which place of the Allcorrectwaypoint

            if (currentWaypoint == _finalWaypoint)
            {
                Console.WriteLine("---- [case: arrived destination] .... ");
                Event.OnEventCall(new NavigationEventArgs
                {
                    Result = NavigationResult.Arrival
                });
            }
            else if (_waypointsOnRoute[_currentNavigateStep].Equals(currentWaypoint))
            {
                Console.WriteLine("---- [case: arrived waypoint] .... ");

                Console.WriteLine("current waypoint: " + currentWaypoint.ID);
                Console.WriteLine("next waypoint: " + _waypointsOnRoute[_currentNavigateStep+1].ID);

                navigationInstruction.CurrentWaypoint = currentWaypoint;
                navigationInstruction.NextWaypoint =
                                        _waypointsOnRoute[_currentNavigateStep+1];

                
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
                    //getwaypoint=0 means that we are now in the starting
                    // point, we do not have the previous waypoint 
                    //therefore, we give the first point a direction called
                    // FirstDirection
                    
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
                navigationInstruction.Progress =
                (double)Math.Round(
                    (decimal)_currentNavigateStep / _waypointsOnRoute.Count, 3);

                // Raise event to notify the UI/main thread with the result
                Event.OnEventCall(new NavigationEventArgs
                {
                    Result = NavigationResult.Run,
                    NextInstruction = navigationInstruction
                });
                Console.WriteLine("After raising event from Session");

                _currentNavigateStep++;
                StartNavigate(_currentNavigateStep);
            }
            /*
            else if (_waypointsOnWrongWay[_waypointsOnRoute[_currentNavigateStep].ID].Contains(currentWaypoint) == false)
            {
                Console.WriteLine("---- [case: wrong waypoint] .... ");
                Event.OnEventCall(new NavigationEventArgs
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

                Event.OnEventCall(new NavigationEventArgs
                {
                    Result = NavigationResult.Run,
                    NextInstruction = new NavigationInstruction
                    {
                        NextWaypoint = _waypointsOnRoute[1],
                        Distance = Navigraph.
                        GetDistance(_subgraph,
                                    currentWaypoint,
                                    _waypointsOnRoute[1]),
                        Progress = 0,
                        Direction = TurnDirection.FirstDirection
                    }
                });
            }
            */
            Console.WriteLine("<< CheckArrivedWaypoint ");
        }

        public enum NavigationResult
        {
            Run = 0,
            AdjustRoute,
            Arrival,
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
    }

    public class SessionEvent
    {
        public event EventHandler SessionResultHandler;

        public void OnEventCall(EventArgs args)
        {
            SessionResultHandler?.Invoke(this, args);
        }
    }
}
