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
using Dijkstra.NET.Model;
using Dijkstra.NET.Extensions;
using IndoorNavigation.Models.NavigaionLayer;
using IndoorNavigation.Models;

namespace IndoorNavigation.Modules
{
    public class Session
    {
        //_allCorrectWaypoint used to store all correct Waypoints
        private List<Waypoint> _allCorrectWaypoint = new List<Waypoint>();

        private Graph<Region, string> _regionGraph = 
                                        new Graph<Region, string>();
        private Graph<Waypoint, string> _subgraph = 
                                        new Graph<Waypoint, string>();

        public Event Event { get; private set; }

        //_allNoneWrongWaypoint used to store all correct Waypoints and their
        //neighbors
        public List<Guid> _allNoneWrongWaypoint = new List<Guid>();
        //Destination
        public Waypoint _finalWaypoint = new Waypoint();

        public Session(Navigraph graph,
                       Guid startWaypointID,
                       Guid finalWaypointID,
                       int[] avoid)
        {
            Event = new Event();

            //Read the xml file to get regions and edges information
            _regionGraph = graph.GetRegiongraph();
            //We only consider to one region situation,
            //therefore, we add different floors' waypoints in same regions
            _subgraph = graph.Regions[0].GetNavigationSubgraph(avoid);

            //Use the ID we get to search where the waypoints are
            Waypoint startWaypoint = _subgraph.Where(node =>
            node.Item.ID.Equals(startWaypointID)).Select(w => w.Item).First();

            _finalWaypoint = _subgraph.Where(node => 
            node.Item.ID.Equals(finalWaypointID)).Select(w => w.Item).First();

            //Get the best route through dijkstra
            _allCorrectWaypoint = GetPath(startWaypoint,
                                        _finalWaypoint, _subgraph);
        }

        private List<Waypoint> GetPath(
           Waypoint startWaypoint, Waypoint finalWaypoint,
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
                _allCorrectWaypoint.Add(subgraph[path.ToList()[i]].Item);
                _allNoneWrongWaypoint.Add(_allCorrectWaypoint[i].ID);
                for (int j = 0;j<_allCorrectWaypoint[i].Neighbors.Count;j++)
                {
                    _allNoneWrongWaypoint.Add(
                    _allCorrectWaypoint[i].Neighbors[j].TargetWaypointUUID);
                }
            }

            //Remove the elements that are repeated in _allNoneWrongWaypoint
            for (int i = 0; i < _allNoneWrongWaypoint.Count; i++)
            {
                for (int j = _allNoneWrongWaypoint.Count - 1; j > i; j--)
                {
                    if (_allNoneWrongWaypoint[i] == _allNoneWrongWaypoint[j])
                    {
                        _allNoneWrongWaypoint.RemoveAt(j);
                    }
                }
            }
            return _allCorrectWaypoint;
        }

        //In this function we get the currentwaypoint and determine whether
        //the users are in the right path or not.
        //And we return a structure called navigationInstruction that 
        //contains four elements that Navigation main and UI need.
        //Moreover, if the users are not on the right path, we reroute and 
        //tell users the new path.
        public void DetectRoute(object sender, EventArgs args)
        {
            Waypoint currentWaypoint = _subgraph.Where(node =>
            node.Item.ID.Equals((args as WaypointScanEventArgs).WaypointID)).
            Select(w => w.Item).First();

            //NavigationInstruction is a structure that contains five
            //elements that need to be passed to the main and UI
            NavigationInstruction navigationInstruction =
                                            new NavigationInstruction();
            //getWaypoint is used to check where the current way point is in
            //which place of the Allcorrectwaypoint
            int getWaypoint = 0;

            if (currentWaypoint == _finalWaypoint)
            {
                Event.OnEventCall(new NavigationEventArgs
                {
                    Result = NavigationResult.Arrival
                });
            }
            else if (_allCorrectWaypoint.Contains(currentWaypoint))
            {
                //Check where the current point is in the AllcorrectWaypoint,
                //we need this informationto get the direction, the progress
                //and the next way point
                for (int i = 0; i < _allCorrectWaypoint.Count; i++)
                {
                    if (_allCorrectWaypoint[i].ID == currentWaypoint.ID)
                    {
                        getWaypoint = i;
                        break;
                    }
                }
                //Get nextwaypoint
                navigationInstruction.NextWaypoint =
                                        _allCorrectWaypoint[getWaypoint + 1];

                //If the floor information between the current waypoint and
                //next are different, it means that the users need to change
                //the floor, therefore, we can determine the users need to 
                //go up or down by compare which floor is higher
                if (currentWaypoint.Floor !=
                                _allCorrectWaypoint[getWaypoint + 1].Floor)
                {
                    if (currentWaypoint.Floor >
                                _allCorrectWaypoint[getWaypoint + 1].Floor)
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
                    Navigraph.GetDistance(_subgraph,
                                          currentWaypoint,
                                        _allCorrectWaypoint[getWaypoint + 1]);
                    //getwaypoint=0 means that we are now in the starting
                    // point, we do not have the previous waypoint 
                    //therefore, we give the first point a direction called
                    // FirstDirection
                    if (getWaypoint == 0)
                    {
                        navigationInstruction.Direction =
                                                TurnDirection.FirstDirection;
                    }
                    else
                    {
                        navigationInstruction.Direction =
                            Navigraph.GetTurnDirection(
                                           _allCorrectWaypoint[getWaypoint-1],
                                            currentWaypoint,
                                          _allCorrectWaypoint[getWaypoint+1]);
                    }
                }

                //Get the progress
                navigationInstruction.Progress =
                (double)Math.Round(
                    (decimal)getWaypoint / _allCorrectWaypoint.Count, 3);

                // Raise event to notify the UI/main thread with the result
                Event.OnEventCall(new NavigationEventArgs
                {
                    Result = NavigationResult.Run,
                    NextInstruction = navigationInstruction
                });
            }
            else if (_allNoneWrongWaypoint
                    .Contains(currentWaypoint.ID) == false)
            {
                Event.OnEventCall(new NavigationEventArgs
                {
                    Result = NavigationResult.AdjustRoute
                });

                //If the waypoint is wrong, we initial the correct waypoint
                // and its neighbors and rerun the GetPath function and reget
                //the navigationInstruction
                _allCorrectWaypoint = new List<Waypoint>();
                _allNoneWrongWaypoint = new List<Guid>();
                _allCorrectWaypoint = GetPath(currentWaypoint,
                                              _finalWaypoint,
                                              _subgraph);

                Event.OnEventCall(new NavigationEventArgs
                {
                    Result = NavigationResult.Run,
                    NextInstruction = new NavigationInstruction
                    {
                        NextWaypoint = _allCorrectWaypoint[1],
                        Distance = Navigraph.
                        GetDistance(_subgraph,
                                    currentWaypoint,
                                    _allCorrectWaypoint[1]),
                        Progress = 0,
                        Direction = TurnDirection.FirstDirection
                    }
                });
            }
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

    public class Event
    {
        public event EventHandler SessionResultHandler;

        public void OnEventCall(EventArgs args)
        {
            SessionResultHandler?.Invoke(this, args);
        }
    }
}
