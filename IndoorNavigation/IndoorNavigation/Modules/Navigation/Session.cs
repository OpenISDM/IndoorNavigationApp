/*
 * Copyright (c) 2018 Academia Sinica, Institude of Information Science
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
 *      This class is used to select the route specified by the starting point,
 *      destination, and user preferences. When in navigation, the class will 
 *      give the next waypoint, and when the user is in the wrong way, the 
 *      class will re-route.
 *      
 * Version:
 *
 *      1.0.0-beta.1, 20190521
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
 *      system (IPS) and outdoors covered by GPS. In particilar, it can rely on 
 *      BeDIS (Building/environment Data and Information System) for indoor 
 *      positioning. Using this IPS, the navigator does not need to 
 *      continuously monitor its own position, since the IPS broadcast to the 
 *      navigator the location of each waypoint. 
 *      This version makes use of Xamarin.Forms, which is a complete 
 *      cross-platform UI tookit that runs on both iOS and Android.
 *
 * Authors:
 *
 *
 */
using System;
using System.Linq;
using System.Collections.Generic;
using Dijkstra.NET.Model;
using Dijkstra.NET.Extensions;
using IndoorNavigation.Models.NavigaionLayer;
using Region = IndoorNavigation.Models.NavigaionLayer.Region;

namespace IndoorNavigation.Modules
{
    public class Session
    {
        private List<Waypoint> _allCorrectWaypoint = new List<Waypoint>();

        private Graph<Region, string> _regionGraph = new Graph<Region, string>();
        private Graph<Waypoint, string> _subgraph = new Graph<Waypoint, string>();

        public List<Waypoint> AllCorrectWaypointGet = new List<Waypoint>();
        public List<Guid> AllNoneWrongWaypoint = new List<Guid>();

        public Waypoint finalwaypointInPath = new Waypoint();

        public Session(Navigraph graph, Waypoint startWaypoint, Waypoint finalWaypoint, List<int> avoid)
        {
            Console.WriteLine("Start Way point is" + startWaypoint.Name + "\n");
            Console.WriteLine("Final Way point is" + finalWaypoint.Name + "\n");
            //read the xml file to get regions and edges information
            _regionGraph = graph.GetRegiongraph();
            //we only consider to one region situation,
            //therefore, we add different floors' waypoints in same regions
            _subgraph = graph.Regions[0].SetNavigationSubgraph(avoid);
            //get the best route through dijkstra
            AllCorrectWaypointGet = GetPath(startWaypoint, finalWaypoint, _subgraph);
            finalwaypointInPath = finalWaypoint;
        }

        List<Waypoint> GetPath(
           Waypoint startWaypoint, Waypoint finalWaypoint, Graph<Waypoint, string> subgraph)
        {
            //get the keys of the start way point and destination waypoint and throw the two keys into dijkstra
            // to get the best route
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
                Console.WriteLine("Path " + i + ", " + subgraph[path.ToList()[i]].Item.Name);
            }
            //List<Waypoint> AllNeighborWaypoint = new List<Waypoint>();
            //put the correct waypoint into List all_correct_waaypoint
            for (int i = 0; i < path.Count(); i++)
            {
                _allCorrectWaypoint.Add(subgraph[path.ToList()[i]].Item);
            }

            //put all correct waypoints and their neighbors in list AllNoneWrongWaypoint
             for (int i = 0; i < _allCorrectWaypoint.Count; i++)
            {
                AllNoneWrongWaypoint.Add(_allCorrectWaypoint[i].ID);
                for (int j = 0; j < _allCorrectWaypoint[i].Neighbors.Count; j++)
                {
                    AllNoneWrongWaypoint.Add(_allCorrectWaypoint[i].Neighbors[j].TargetWaypointUUID);
                }

            }

            for (int i = 0; i < _allCorrectWaypoint.Count; i++)
            {
                Console.WriteLine("All correct Waypoint : " + _allCorrectWaypoint[i].Name + "\n");
            }
            //remove the elements that are repeated
            for (int i = 0; i < AllNoneWrongWaypoint.Count; i++)
            {
                for (int j = AllNoneWrongWaypoint.Count - 1; j > i; j--)
                {

                    if (AllNoneWrongWaypoint[i] == AllNoneWrongWaypoint[j])
                    {
                        AllNoneWrongWaypoint.RemoveAt(j);
                    }

                }
            }
            for (int i = 0; i < AllNoneWrongWaypoint.Count; i++)
            {
                Console.WriteLine("All Accept Guid : " + AllNoneWrongWaypoint[i] + "\n");
            }

            return _allCorrectWaypoint;
        }

        public NavigationInstruction DetectRoute(Waypoint currentwaypoint)
        {
            //returnInformation is a structure that contains five elements that need to be passed to the main and UI
            NavigationInstruction navigationInstruction = new NavigationInstruction();
            //getWaypoint is used to check where the current way point is in the Allcorrectwaypoint
            int getWaypoint = 0;

            Console.WriteLine("current Way Point ; " + currentwaypoint.Name);

            if (_allCorrectWaypoint.Contains(currentwaypoint))
            {
                //ReturnInformation.Status status = new ReturnInformation.Status();
                navigationInstruction.Result = NavigationResult.Run;
                //check where the current point is in the AllcorrectWaypoint, we need this information
                //to get the direction, the progress and the next way point
                for (int i = 0; i < _allCorrectWaypoint.Count; i++)
                {
                    if (_allCorrectWaypoint[i].ID == currentwaypoint.ID)
                    {
                        getWaypoint = i;
                        break;
                    }
                }
                //get nextwaypoint
                navigationInstruction.NextWaypoint = _allCorrectWaypoint[getWaypoint + 1];
                //if the floor information is different, it means that the users need to change the floor
                //therefore, we can detemine the users need to go up or down by compare which floor is bigger
                if (currentwaypoint.Floor != _allCorrectWaypoint[getWaypoint + 1].Floor)
                {
                    if (currentwaypoint.Floor > _allCorrectWaypoint[getWaypoint + 1].Floor)
                    {
                        navigationInstruction.Direction = TurnDirection.Down;
                    }
                    else
                    {
                        navigationInstruction.Direction = TurnDirection.Up;
                    }
                    navigationInstruction.Distance = 0;
                }
                //if the fllor information between current way point and next way point are same,
                //we need to tell the use go straight or turn direction, therefore, we need to 
                //have three informations, previous, current and next waypoints
                else
                {
                    Console.WriteLine("Nextwaypoint : " + navigationInstruction.NextWaypoint.Name);
                    uint sourceKey = _subgraph
                                    .Where(WaypointList => WaypointList.Item.ID
                                    .Equals(_allCorrectWaypoint[getWaypoint].ID)).Select(c => c.Key)
                                     .First();
                    uint targetKey = _subgraph
                                    .Where(WaypointList => WaypointList.Item.ID
                                    .Equals(_allCorrectWaypoint[getWaypoint + 1].ID)).Select(c => c.Key)
                                    .First();
                    navigationInstruction.Distance = _subgraph.Dijkstra(sourceKey, targetKey).Distance;
                    Console.WriteLine("Distance : " + navigationInstruction.Distance);
                    Console.WriteLine("Get way Point : " + getWaypoint);
                    Console.WriteLine("all correct way point count : " + _allCorrectWaypoint.Count);
                    //getwaypoint=0 means that we are now in the starting point, we do not have the previous waypoint 
                    //information, we give the previous waypoint 0
                    if (getWaypoint == 0)
                    {
                        CardinalDirection currentDirection = 0;
                        CardinalDirection nextDirection = _allCorrectWaypoint[getWaypoint].Neighbors
                                                        .Where(neighbors =>
                                                            neighbors.TargetWaypointUUID
                                                            .Equals(_allCorrectWaypoint[getWaypoint + 1].ID))
                                                        .Select(c => c.Direction).First();

                        // Calculate the turning direction by cardinal direction
                        int nextTurnDirection = (int)nextDirection - (int)currentDirection;
                        nextTurnDirection = nextTurnDirection < 0 ?
                                            nextTurnDirection + 8 : nextTurnDirection;
                        navigationInstruction.Direction = (TurnDirection)nextTurnDirection;
                    }
                    else
                    {
                        CardinalDirection currentDirection = _allCorrectWaypoint[getWaypoint - 1].Neighbors
                                                .Where(neighbors =>
                                                    neighbors.TargetWaypointUUID
                                                    .Equals(_allCorrectWaypoint[getWaypoint].ID))
                                                .Select(c => c.Direction).First();
                        CardinalDirection nextDirection = _allCorrectWaypoint[getWaypoint].Neighbors
                                                        .Where(neighbors =>
                                                            neighbors.TargetWaypointUUID
                                                            .Equals(_allCorrectWaypoint[getWaypoint + 1].ID))
                                                        .Select(c => c.Direction).First();

                        int nextTurnDirection = (int)nextDirection - (int)currentDirection;
                        nextTurnDirection = nextTurnDirection < 0 ?
                                            nextTurnDirection + 8 : nextTurnDirection;
                        navigationInstruction.Direction = (TurnDirection)nextTurnDirection;
                        Console.WriteLine("Direction : " + nextTurnDirection + nextDirection + currentDirection);
                    }
                }
                navigationInstruction.Progress = (double)Math.Round((decimal)getWaypoint / _allCorrectWaypoint.Count, 3);
            }
            else if (AllNoneWrongWaypoint.Contains(currentwaypoint.ID) == false)
            {
                //if the waypoint is wrong, we initial the correct waypoint and its neighbors
                //and rerun the GetPath function and reget the navigationInstruction
                _allCorrectWaypoint = new List<Waypoint>();
                AllNoneWrongWaypoint = new List<Guid>();
                _allCorrectWaypoint = GetPath(currentwaypoint, finalwaypointInPath, _subgraph);
                for (int i = 0; i < _allCorrectWaypoint.Count; i++)
                {
                    Console.WriteLine("Renew Route : " + _allCorrectWaypoint[i].Name);
                }
                Console.WriteLine("out of this loop");
                navigationInstruction = DetectRoute(currentwaypoint);
                navigationInstruction.Result = NavigationResult.AdjustRoute;
                Console.WriteLine("check");
            }

            return navigationInstruction;
        }

        public enum NavigationResult
        {
            Run = 0,
            AdjustRoute,
            Contonue,
        }

        public class NavigationInstruction : EventArgs
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
            /// Result of navigation
            /// </summary>
            public NavigationResult Result;

            /// <summary>
            /// The direction to turn to the next waypoint using the enum type
            /// </summary>
            public TurnDirection Direction;
        }
    }
}
