﻿/*
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

        //_allNoneWrongWaypoint used to store all correct Waypoints and their neighbors
        public List<Guid> _allNoneWrongWaypoint = new List<Guid>();

        public Waypoint finalwaypointInPath = new Waypoint();

        public Session(Navigraph graph, Waypoint startWaypoint, Waypoint finalWaypoint, List<int> avoid)
        {
            Console.WriteLine("Start Way point is" + startWaypoint.Name + "\n");
            Console.WriteLine("Final Way point is" + finalWaypoint.Name + "\n");
            //Read the xml file to get regions and edges information
            _regionGraph = graph.GetRegiongraph();
            //We only consider to one region situation,
            //therefore, we add different floors' waypoints in same regions
            _subgraph = graph.Regions[0].SetNavigationSubgraph(avoid);
            //Get the best route through dijkstra
            _allCorrectWaypoint = GetPath(startWaypoint, finalWaypoint, _subgraph);
            finalwaypointInPath = finalWaypoint;
        }

        List<Waypoint> GetPath(
           Waypoint startWaypoint, Waypoint finalWaypoint, Graph<Waypoint, string> subgraph)
        {
            //Get the keys of the start way point and destination waypoint and throw the two keys into dijkstra
            //to get the best route
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

            //Put the correct waypoint into List all_correct_waaypoint
            ////Put all correct waypoints and their neighbors in list AllNoneWrongWaypoint
            for (int i = 0; i < path.Count(); i++)
            {
                _allCorrectWaypoint.Add(subgraph[path.ToList()[i]].Item);
                _allNoneWrongWaypoint.Add(_allCorrectWaypoint[i].ID);
                for (int j = 0; j < _allCorrectWaypoint[i].Neighbors.Count; j++)
                {
                    _allNoneWrongWaypoint.Add(_allCorrectWaypoint[i].Neighbors[j].TargetWaypointUUID);
                }
            }
                   
            for (int i = 0; i < _allCorrectWaypoint.Count; i++)
            {
                Console.WriteLine("All correct Waypoint : " + _allCorrectWaypoint[i].Name + "\n");
            }

            //Remove the elements that are repeated
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

            for (int i = 0; i < _allNoneWrongWaypoint.Count; i++)
            {
                Console.WriteLine("All Accept Guid : " + _allNoneWrongWaypoint[i] + "\n");
            }

            return _allCorrectWaypoint;
        }

        public NavigationInstruction DetectRoute(Waypoint currentwaypoint)
        {
            //NavigationInstruction is a structure that contains five elements that need to be passed to the main and UI
            NavigationInstruction navigationInstruction = new NavigationInstruction();
            //getWaypoint is used to check where the current way point is in which place of the Allcorrectwaypoint
            int getWaypoint = 0;
        
            if(currentwaypoint==finalwaypointInPath)
            {
                navigationInstruction.Result = NavigationResult.Arrival;
                Console.WriteLine("Arrive");
            }
            else if (_allCorrectWaypoint.Contains(currentwaypoint))
            {
                //ReturnInformation.Status status = new ReturnInformation.Status();
                navigationInstruction.Result = NavigationResult.Run;
                //Check where the current point is in the AllcorrectWaypoint, we need this information
                //to get the direction, the progress and the next way point
                for (int i = 0; i < _allCorrectWaypoint.Count; i++)
                {
                    if (_allCorrectWaypoint[i].ID == currentwaypoint.ID)
                    {
                        getWaypoint = i;
                        break;
                    }
                }
                //Get nextwaypoint
                navigationInstruction.NextWaypoint = _allCorrectWaypoint[getWaypoint + 1];
                //If the floor information between the current waypoint and next is different, it means that the users need to change the floor
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
                //If the fllor information between current way point and next way point are same,
                //we need to tell the use go straight or turn direction, therefore, we need to 
                //have three informations, previous, current and next waypoints
                else
                {
                    navigationInstruction.Distance =  Navigraph.GetDistance(_subgraph,currentwaypoint, _allCorrectWaypoint[getWaypoint+1]);
                    //getwaypoint=0 means that we are now in the starting point, we do not have the previous waypoint 
                    if (getWaypoint == 0)
                    {
                        navigationInstruction.Direction = TurnDirection.FirstDirection;
                    }
                    else
                    {
                        navigationInstruction.Direction = Navigraph.GetTurnDirection(_allCorrectWaypoint[getWaypoint  -1], currentwaypoint, _allCorrectWaypoint[getWaypoint + 1]);
                    }
                }
                navigationInstruction.Progress = (double)Math.Round((decimal)getWaypoint / _allCorrectWaypoint.Count, 3);
            }
            else if (_allNoneWrongWaypoint.Contains(currentwaypoint.ID) == false)
            {
                //if the waypoint is wrong, we initial the correct waypoint and its neighbors
                //and rerun the GetPath function and reget the navigationInstruction
                _allCorrectWaypoint = new List<Waypoint>();
                _allNoneWrongWaypoint = new List<Guid>();
                _allCorrectWaypoint = GetPath(currentwaypoint, finalwaypointInPath, _subgraph);
                for (int i = 0; i < _allCorrectWaypoint.Count; i++)
                {
                    Console.WriteLine("Renew Route : " + _allCorrectWaypoint[i].Name);
                }

                navigationInstruction = DetectRoute(currentwaypoint);
                navigationInstruction.Result = NavigationResult.AdjustRoute;
            }

            return navigationInstruction;
        }

        public enum NavigationResult
        {
            Run = 0,
            AdjustRoute,
            Arrival,
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