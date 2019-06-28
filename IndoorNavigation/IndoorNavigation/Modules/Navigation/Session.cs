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

        private List<Waypoint> _waypointsOnRoute = new List<Waypoint>();
        private Dictionary<Guid, List<Waypoint>> _waypointsOnWrongWay =
            new Dictionary<Guid, List<Waypoint>>();
        /*
        private Graph<Waypoint, string> _subgraph = 
                                        new Graph<Waypoint, string>();
                                        */

        public NavigationEvent _event { get; private set; }

        private Waypoint _finalWaypoint = new Waypoint();

        private Thread _waypointDetectionThread;
        private Thread _navigationControllerThread;

        private bool _isKeepDetection;
        private Waypoint _currentWaypoint = new Waypoint();
        private ManualResetEventSlim _nextWaypointEvent = new ManualResetEventSlim(false);

        public Session(NavigationGraph graph,
                       Guid finalRegionID,
                       Guid finalWaypointID,
                       int[] avoid)
        {
            _event = new NavigationEvent();

          //_subgraph = graph.Regions[0].GetNavigationSubgraph(avoid);

            //Use the ID we get to search where the waypoints are
       /*     _finalWaypoint = _subgraph.Where(node => 
            node.Item.ID.Equals(finalWaypointID)).Select(w => w.Item).First();
            */
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

            Waypoint startWaypoint = new Waypoint();

            NavigateToNextWaypoint(_nextWaypointStep);

            while (true == _isKeepDetection &&
                   !_currentWaypoint._id.Equals(_finalWaypoint._id)) {
                Console.WriteLine("Continue to navigate to next step, _currentWaypoint ID: {0}, _finalWaypoint ID: {1}",
                                  _currentWaypoint._id, _finalWaypoint._id);

                _nextWaypointEvent.Wait();

                if (_currentWaypoint._id.Equals(_finalWaypoint._id)) {
                    Console.WriteLine("Arrived destination!");

                    break;
                }

                if (_nextWaypointStep == -1)
                {
                    Console.WriteLine("Detected start waypoint: " + _currentWaypoint._id);
                    // Detection of starting waypoing:
                    // Detected the waypoint most closed to user.
                    startWaypoint = _currentWaypoint;

                   // GenerateRoute(startWaypoint, _finalWaypoint, _subgraph);
                    _nextWaypointStep++;
                    NavigateToNextWaypoint(_nextWaypointStep);
                }
                else if (_currentWaypoint.Equals(_waypointsOnRoute[_nextWaypointStep]))
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
                            startWaypoint = _currentWaypoint;
                           // GenerateRoute(startWaypoint, _finalWaypoint, _subgraph);

                            _event.OnEventCall(new NavigationEventArgs
                            {
                                _result = NavigationResult.Run,
                                _nextInstruction = new NavigationInstruction
                                {
                                    _currentWaypoint = startWaypoint,
                                    _nextWaypoint = _waypointsOnRoute[1],
                                    /*
                                    Distance =
                                        Subgraph.GetDistance(_subgraph,
                                                              startWaypoint,
                                                              _waypointsOnRoute[1]),
                                                              */
                                    _progress = 0,
                                    _direction = TurnDirection.FirstDirection
                                }
                            });

                            _nextWaypointStep = 0;
                            NavigateToNextWaypoint(_nextWaypointStep);

                            break;
                        }
                    }
                }
                _nextWaypointEvent.Reset();
            }
        }

        private void NavigateToNextWaypoint(int nextStep) {
            List<WaypointBeaconsMapping> monitorWaypointList = new List<WaypointBeaconsMapping>();

            if (nextStep == -1)
            {
                // Detection of starting waypoing:
                // Use all Waypoints of the NavigationGraph to detect the starting waypoint
                /*
                monitorWaypointList = _subgraph.Select(waypoint => waypoint.Item).ToList();
                foreach (Waypoint waypoint in monitorWaypointList) {
                    Console.WriteLine("all waypoints: " + waypoint._id);
                }
                */
            }
            else {
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

        private void GenerateRoute(Waypoint startWaypoint, 
                                   Waypoint finalWaypoint,
                                   Graph<Waypoint, string> subgraph)
        {
            //Get the keys of the start way point and destination waypoint
            //and throw the two keys into dijkstra to get the best route
            var startPoingKey = subgraph
                .Where(waypoint => waypoint.Item._id
                .Equals(startWaypoint._id)).Select(c => c.Key).First();
            var endPointKey = subgraph
                .Where(waypoint => waypoint.Item._id
                .Equals(finalWaypoint._id)).Select(c => c.Key).First();

            var path = subgraph.Dijkstra(startPoingKey,
                                           endPointKey).GetPath();

            _waypointsOnRoute = new List<Waypoint>();
            _waypointsOnWrongWay = new Dictionary<Guid, List<Waypoint>>();
            for (int i = 0; i < path.Count(); i++)
            {
                _waypointsOnRoute.Add(subgraph[path.ToList()[i]].Item);
               
                List<Waypoint> tempWrongWaypointList = new List<Waypoint>();

                // Construct the two-step possible wrong-way waypoints
                Console.WriteLine("GenerateRoute: correct waypoint: " + _waypointsOnRoute[i]._id);
                if (i - 1 >= 0)
                {
                    /*
                    for (int j = 0; j < _waypointsOnRoute[i-1].Neighbors.Count; j++)
                    {
                        if(!subgraph[path.ToList()[i]].Item.ID.
                            Equals(_waypointsOnRoute[i-1].Neighbors[j].TargetWaypointUUID))
                        { 
                            Waypoint oneStepWrongWaypoint =
                                subgraph.Where(waypoint => waypoint.Item.ID.
                                Equals(_waypointsOnRoute[i - 1].Neighbors[j].TargetWaypointUUID)).
                                Select(n => n.Item).First();

                            Console.WriteLine("GenerateRoute: one step wrong waypoint: " +
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
                                    Console.WriteLine("GenerateRoute: two step wrong waypoint: " +
                                                      twoStepWrongWaypoint.ID);

                                    tempWrongWaypointList.Add(twoStepWrongWaypoint);
                                }
                            }
                        }
                    }
                    */
                }
                _waypointsOnWrongWay.Add(_waypointsOnRoute[i]._id, tempWrongWaypointList);
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
            Waypoint currentWaypoint = new Waypoint();
            /*_subgraph.Where(node =>
            node.Item._id.Equals((args as WayPointSignalEventArgs).CurrentWaypoint._id)).
            Select(w => w.Item).First();
            */
            //NavigationInstruction is a structure that contains five
            //elements that need to be passed to the main and UI
            NavigationInstruction navigationInstruction = 
                new NavigationInstruction();

            if (_nextWaypointStep == -1)
            {
                _currentWaypoint = currentWaypoint;
                _nextWaypointEvent.Set();

                if (currentWaypoint == _finalWaypoint)
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
                if (currentWaypoint == _finalWaypoint)
                {
                    Console.WriteLine("---- [case: arrived destination] .... ");

                    _event.OnEventCall(new NavigationEventArgs
                    {
                        _result = NavigationResult.Arrival
                    });
                }
                else if (_waypointsOnRoute[_nextWaypointStep].Equals(currentWaypoint))
                {
                    Console.WriteLine("---- [case: arrived waypoint] .... ");

                    Console.WriteLine("current waypoint: " + currentWaypoint._id);
                    Console.WriteLine("next waypoint: " + _waypointsOnRoute[_nextWaypointStep + 1]._id);

                    navigationInstruction._currentWaypoint = currentWaypoint;
                    navigationInstruction._nextWaypoint =
                        _waypointsOnRoute[_nextWaypointStep + 1];

                    //If the floor information between the current waypoint and
                    //next are different, it means that the users need to change
                    //the floor, therefore, we can determine the users need to 
                    //go up or down by compare which floor is higher
                    /*
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

                        navigationInstruction.Distance =
                        Subgraph.GetDistance(_subgraph,
                                              currentWaypoint,
                                              _waypointsOnRoute[_nextWaypointStep + 1]);

                        if (_nextWaypointStep == 0)
                        {
                            navigationInstruction.Direction =
                                TurnDirection.FirstDirection;
                        }
                        else
                        {
                            navigationInstruction.Direction =
                                Subgraph.GetTurnDirection(
                                    _waypointsOnRoute[_nextWaypointStep - 1],
                                    currentWaypoint,
                                    _waypointsOnRoute[_nextWaypointStep + 1]);
                        }

                    }*/

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
                    Console.WriteLine("arrived waypoint ID:" + currentWaypoint._id);
                    Console.WriteLine("possible wrong waypoint:");

                    for (int i = 0;
                         i < _waypointsOnWrongWay[_waypointsOnRoute[_nextWaypointStep]._id].Count;
                         i++)
                    {

                        Console.WriteLine("waypoing ID:" +
                                          _waypointsOnWrongWay[_waypointsOnRoute[_nextWaypointStep]
                                          ._id][i]._id);

                        if (currentWaypoint._id.
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
                }

                _currentWaypoint = currentWaypoint;
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
            public Waypoint _currentWaypoint;

            public Waypoint _nextWaypoint;

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
