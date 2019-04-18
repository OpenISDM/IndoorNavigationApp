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
 *      This file contains definitions of attributes for computing routes,
 *      including waypoint objects, turn directions between waypoints,
 *      with their distances.
 * 
 * File Name:
 *
 *      NavigationlayerModel.cs
 *
 * Abstract:
 *
 *      Waypoint-based navigator is a mobile Bluetooth navigation application that 
 *      runs on smart phones. It is structed to support anywhere navigation. Indoors 
 *      in areas covered by different indoor positioning system (IPS) and outdoors
 *      covered by GPS. In particilar, it can rely on BeDIS 
 *      (Building/environment Data and Information System) for indoor positioning. 
 *      Using this IPS, the navigator does not need to continuously monitor its own 
 *      position, since the IPS broadcast to the navigator the location of each waypoint.
 *      This version makes use of Xamarin.Forms, which is a complete cross-platform 
 *      UI tookit that runs on both iOS and Android.
 *
 * Authors:
 *
 *      
 *
 */

using System;
using System.Collections.Generic;
using System.Linq;
using Dijkstra.NET.Model;
using GeoCoordinatePortable;
using Dijkstra.NET.Extensions;

namespace IndoorNavigation.Models.NavigaionLayer
{
    public enum CardinalDirection
    {
        North = 0,
        Northeast,
        East,
        Southeast,
        South,
        Southwest,
        West,
        Northwest
    }

    public enum TurnDirection
    {
        FirstDirection = -1, // used in first step within navigation
        Forward = 0,
        Forward_Right,
        Right,
        Backward_Right,
        Backward,
        Backward_Left,
        Left,
        Forward_Left,
    }

    /// <summary>
    /// The top level of the navigation graph within two-level hierarchy
    /// </summary>
    public class Navigraph
    {
        public string Name { get; private set; }
        public List<Region> Regions { get; private set; }
        public List<Edge> Edges { get; private set; }

        private const double thresholdOfDistance = 10;
        private Graph<Waypoint, string> navigraph = 
            new Graph<Waypoint, string>();

        /// <summary>
        /// Initializes a new instance of the Navigraph class
        /// </summary>
        public Navigraph(string name, List<Region> regions, List<Edge> edges)
        {
            Name = name;
            Regions = regions;
            Edges = edges;

            // Add all the waypoints into navigraph
            IEnumerable<Waypoint> listOfAllWaypoints = 
                    regions.SelectMany(region => region.Waypoints);
            foreach (Waypoint waypoint in listOfAllWaypoints)
            {
                navigraph.AddNode(waypoint);
            }

            // Set the navigraph which used by Dijkstra algorithm
            foreach (Edge edge in edges)
            {
                int distance = System.Convert.ToInt32(edge.Distance);

                // Get two connected location's key value
                uint beaconAKey = navigraph.Where(waypoint =>
                        waypoint.Item.UUID == edge.SourceWaypoint.UUID)
                        .Select(waypoint => waypoint.Key).First();
                uint beaconBKey = navigraph.Where(waypoint =>
                        waypoint.Item.UUID == edge.TargetWaypoint.UUID)
                        .Select(waypoint => waypoint.Key).First();

                // Connect to the each waypoint
                navigraph.Connect(beaconAKey, beaconBKey,
                        distance, string.Empty);
            }
        }

        public Queue<NavigationInstruction> GetPath(Waypoint startWaypoint, Waypoint destinationWaypoint)
        {
            Queue<NavigationInstruction> returnedPathQueue = 
                    new Queue<NavigationInstruction>();

            // Find where is the start/destination waypoint and find its key
            uint startKey = navigraph
                    .Where(WaypointList => WaypointList.Item.UUID
                    .Equals(startWaypoint.UUID)).Select(c => c.Key).First();
            uint destinationKey = navigraph
                    .Where(WaypointList => WaypointList.Item.UUID
                    .Equals(destinationWaypoint.UUID)).Select(c => c.Key).First();

            var path = navigraph.Dijkstra(startKey, destinationKey).GetPath();
            for (int i = 0; i < path.Count() - 1; i++)
            {
                // Get both current waypoint and next waypoint which within path
                Waypoint _currentWaypoint = navigraph[path.ToList()[i]].Item;
                Waypoint _nextWaypoint = navigraph[path.ToList()[i]].Item;

                if (i == 0)
                {
                    returnedPathQueue.Enqueue(new NavigationInstruction
                    {
                        NextWaypoint = _nextWaypoint,
                        WrongwayWaypointList = GetWrongwayWaypoints(null, 
                                                        _currentWaypoint,
                                                        _nextWaypoint,
                                                        thresholdOfDistance),
                        Direction = TurnDirection.FirstDirection
                    });
                }
                else
                {
                    Waypoint _previousWaypoint = navigraph[path.ToList()[i - 1]].Item;

                    returnedPathQueue.Enqueue(new NavigationInstruction
                    {
                        NextWaypoint = _nextWaypoint,
                        WrongwayWaypointList = GetWrongwayWaypoints(
                                                     _previousWaypoint, 
                                                     _currentWaypoint, 
                                                     _nextWaypoint,
                                                     thresholdOfDistance),
                        Direction = GetTurnDirection(_previousWaypoint,
                                                     _currentWaypoint,
                                                     _nextWaypoint)
                    });
                }
            }

            return returnedPathQueue;
        }

        /// <summary>
        /// Gets the distance from source waypoint to target waypoint
        /// </summary>
        public double GetDistance(Waypoint sourceWaypoint, Waypoint targetWaypoint)
        {
            // Find where is the source/target waypoint and find its key
            uint sourceKey = navigraph
                            .Where(WaypointList => WaypointList.Item.UUID
                            .Equals(sourceWaypoint.UUID)).Select(c => c.Key).First();
            uint targetKey = navigraph
                            .Where(WaypointList => WaypointList.Item.UUID
                            .Equals(targetWaypoint.UUID)).Select(c => c.Key).First();

            // Returns the distance of the path
            return navigraph.Dijkstra(sourceKey, targetKey).Distance;
        }

        /// <summary>
        /// Gets the turning direction using three waypoints
        /// </summary>
        public TurnDirection GetTurnDirection(Waypoint previousWaypoint,
                                              Waypoint currentWaypoint,
                                              Waypoint nextWaypoint)
        {
            // Find the cardinal direction to the next waypoint
            CardinalDirection currentDirection = previousWaypoint.Neighbors
                                            .Where(neighbors => neighbors.TargetWaypointUUID.Equals(currentWaypoint.UUID))
                                            .Select(c => c.Direction).First();
            CardinalDirection nextDirection = currentWaypoint.Neighbors
                                            .Where(neighbors => neighbors.TargetWaypointUUID.Equals(nextWaypoint.UUID))
                                            .Select(c => c.Direction).First();

            // Calculate the turning direction by cardinal direction
            int nextTurnDirection = (int)nextDirection - (int)currentDirection;
            nextTurnDirection = nextTurnDirection < 0 ? nextTurnDirection + 8 : nextTurnDirection;

            return (TurnDirection)nextTurnDirection;
        }

        /// <summary>
        /// Get the wrongway waypoint for each waypoint within path
        /// </summary>
        public List<Waypoint> GetWrongwayWaypoints(Waypoint previousWaypoint,
                                                   Waypoint currentWaypoint,
                                                   Waypoint nextWaypoint,
                                                   double thresholdDistance)
        {
            List<Waypoint> wrongwayWaypointList = new List<Waypoint>();

            // Find the branch that is both connect with current waypoint
            // but not connect with next waypoint
            IEnumerable<Waypoint> adjacentWaypoints = (
                                from edge in Edges
                                where edge.SourceWaypoint.Equals(currentWaypoint.UUID) &&
                                      !edge.TargetWaypoint.UUID.Equals(nextWaypoint.UUID)
                                select edge.TargetWaypoint).
                                Concat(from edge in Edges
                                       where edge.TargetWaypoint.Equals(currentWaypoint.UUID) &&
                                             !edge.SourceWaypoint.UUID.Equals(nextWaypoint.UUID)
                                       select edge.SourceWaypoint);

            if (previousWaypoint != null)
            {
                // Filter the waypoint which is the previous waypoint and add it
                // into wrongwayWaypointList
                adjacentWaypoints = adjacentWaypoints.Where(waypoint => !waypoint.UUID.Equals(previousWaypoint.UUID));
                wrongwayWaypointList.Add(previousWaypoint);
            }

            for (int i = 0; i < adjacentWaypoints.Count(); i++)
            {
                if (currentWaypoint.Coordinates.GetDistanceTo(adjacentWaypoints.ElementAt(i).Coordinates) >= thresholdDistance)
                {
                    wrongwayWaypointList.Add(adjacentWaypoints.ElementAt(i));
                    continue;
                }

                IEnumerable<Waypoint> connectedWaypoints = GetConnectedWaypoints(
                                            adjacentWaypoints.ElementAt(i),
                                            previousWaypoint,
                                            currentWaypoint,
                                            nextWaypoint);

                if (connectedWaypoints.Any())
                {
                    // Remove the connected waypoints that included in 
                    // the adjacentWaypoints list
                    adjacentWaypoints = adjacentWaypoints.Concat(
                                        from waypoint in connectedWaypoints
                                        where !adjacentWaypoints.Any(adjWaypoint => adjWaypoint.UUID.Equals(waypoint.UUID))
                                        select waypoint);
                }
            }

            return wrongwayWaypointList.Distinct().ToList();
        }

        /// <summary>
        /// Get the waypoints which are connect with
        /// </summary>
        private IEnumerable<Waypoint> GetConnectedWaypoints(
                                                   Waypoint adjacentWaypoint,
                                                   Waypoint previousWaypoint,
                                                   Waypoint currentWaypoint,
                                                   Waypoint nextWaypoint)
        {
            IEnumerable<Waypoint> connectedList = (from edge in Edges
                                where edge.SourceWaypoint.Equals(adjacentWaypoint.UUID) &&
                                      !edge.TargetWaypoint.UUID.Equals(currentWaypoint.UUID) &&
                                      !edge.TargetWaypoint.UUID.Equals(nextWaypoint.UUID)
                                select edge.TargetWaypoint).
                                Concat(from edge in Edges
                                       where edge.TargetWaypoint.Equals(adjacentWaypoint.UUID) &&
                                             !edge.SourceWaypoint.UUID.Equals(currentWaypoint.UUID) &&
                                             !edge.SourceWaypoint.UUID.Equals(nextWaypoint.UUID)
                                       select edge.SourceWaypoint);

            if (previousWaypoint != null)
            {
                //filter the waypoint which is the previous waypoint
                connectedList = connectedList.Where(waypoint => !waypoint.UUID.Equals(previousWaypoint.UUID));
            }

            return connectedList;
        }
    }

    #region Navigraph parameters

    /// <summary>
    /// The second level of the navigation graph within two-level hierarchy
    /// </summary>
    public class Region
    {
        public string Name { get; set; }
        public List<Waypoint> Waypoints { get; set; }
    }

    /// <summary>
    /// The model of waypoint.
    /// </summary>
    public class Waypoint
    {
        /// <summary>
        /// Friendly name of waypoint
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Information for navigation layer. Whether it has a landmark or null
        /// </summary>
        public string Landmark { get; set; }

        /// <summary>
        /// Universal Unique Identifier of waypoint
        /// </summary>
        public Guid UUID { get; set; }

        /// <summary>
        /// The coordinates of a waypoint
        /// </summary>
        public GeoCoordinate Coordinates { get; set; }

        /// <summary>
        /// The floor where the waypoint is located
        /// </summary>
        public int Floor { get; set; }

        /// <summary>
        /// Neighbors of the waypoint
        /// </summary>
        public List<Neighbor> Neighbors { get; set; }
    }

    /// <summary>
    /// Location connect
    /// </summary>
    public abstract class LocationConnect
    {
        /// <summary>
        /// Indicates the name of the target which is facing
        /// </summary>
        public string TargetName { get; set; }

        /// <summary>
        /// Cardinal direction for the host waypoint
        /// </summary>
        public CardinalDirection Direction { get; set; }
    }

    /// <summary>
    /// Represents the connected neighbor waypoints in a navigation graph
    /// </summary>
    public class Neighbor : LocationConnect
    {
        /// <summary>
        /// ID of the neighbor waypoint
        /// </summary>
        public Guid TargetWaypointUUID { get; set; }        
    }

    /// <summary>
    /// The edge between the two waypoints
    /// </summary>
    public class Edge : LocationConnect
    {
        /// <summary>
        /// Location A
        /// </summary>
        public Waypoint SourceWaypoint { get; set; }
        /// <summary>
        /// Location B
        /// </summary>
        public Waypoint TargetWaypoint { get; set; }

        /// <summary>
        /// Gets or sets the distance
        /// </summary>
        public double Distance
        {
            get => SourceWaypoint.Coordinates
                        .GetDistanceTo(TargetWaypoint.Coordinates);
        }

        /// <summary>
        /// Other informations regarding the edge, e.g. wheelchair support
        /// </summary>
        public string OtherInformations { get; set; }
    }

    #endregion

    /// <summary>
    /// Instruction of next location to be delivered at the next waypoint
    /// </summary>
    public class NavigationInstruction
    {
        /// <summary>
        /// The next waypoint within navigation path
        /// </summary>
        public Waypoint NextWaypoint { get; set; }
        /// <summary>
        /// The List of "wrong way" waypoint of the next location
        /// </summary>
        public List<Waypoint> WrongwayWaypointList { get; set; }

        /// <summary>
        /// The direction to turn to the next waypoint
        /// </summary>
        public TurnDirection Direction { get; set; }
    }

}