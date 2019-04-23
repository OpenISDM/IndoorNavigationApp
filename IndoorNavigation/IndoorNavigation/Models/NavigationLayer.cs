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
 *      NavigationLayer.cs
 *
 * Abstract:
 *
 *      Waypoint-based navigator is a mobile Bluetooth navigation application
 *      that runs on smart phones. It is structed to support
 *      anywhere navigation. Indoors in areas covered by different
 *      indoor positioning system (IPS) and outdoors covered by GPS.
 *      In particilar, it can rely on BeDIS (Building/environment Data and
 *      Information System) for indoor positioning. 
 *      Using this IPS, the navigator does not need to continuously monitor
 *      its own position, since the IPS broadcast to the navigator the
 *      location of each waypoint.
 *      This version makes use of Xamarin.Forms, which is a complete 
 *      cross-platform UI tookit that runs on both iOS and Android.
 *
 * Authors:
 *
 *      Paul Chang, paulchang@iis.sinica.edu.tw
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
        FirstDirection = -1, // Exception: used only in the first step
        Forward = 0,
        Forward_Right,
        Right,
        Backward_Right,
        Backward,
        Backward_Left,
        Left,
        Forward_Left,
    }

    public enum IPSType
    {
        LBeacon = 0,
        iBeacon,
        GPS
    }

    public enum ConnectionType
    {
        NormalHallway = 0,
        Stair,
        Elevator,
        Escalator
    }

    /// <summary>
    /// The top level of the navigation graph within two-level hierarchy
    /// </summary>
    public class Navigraph
    {
        /// <summary>
        /// Gets or sets the name
        /// e.g. Taipei City Hall, NTHU...etc
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the regions of entire Navigraph
        /// </summary>
        public List<Region> Regions { get; set; }

        /// <summary>
        /// Gets or sets the edges that connection between regions,
        /// e.g. stair, elevator...etc
        /// </summary>
        public List<Edge> Edges { get; set; }

        private const double thresholdOfDistance = 10; // 10 meters
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

            /*
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
            */
        }

        public Queue<NavigationInstruction> GetPath(Waypoint startWaypoint,
                                                Waypoint destinationWaypoint)
        {
            Queue<NavigationInstruction> returnedPathQueue =
                    new Queue<NavigationInstruction>();

            /*
            // Find where is the start/destination waypoint and find its key
            uint startKey = navigraph
                    .Where(WaypointList => WaypointList.Item.UUID
                    .Equals(startWaypoint.UUID)).Select(c => c.Key).First();
            uint destinationKey = navigraph
                    .Where(WaypointList => WaypointList.Item.UUID
                    .Equals(destinationWaypoint.UUID))
                    .Select(c => c.Key).First();

            var path = navigraph.Dijkstra(startKey, destinationKey).GetPath();
            for (int i = 0; i < path.Count() - 1; i++)
            {
                // Get both current waypoint and next waypoint within the path
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
                    Waypoint _previousWaypoint = navigraph[path.ToList()[i - 1]]
                                                 .Item;

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
            */
        }

        /// <summary>
        /// Gets the distance from source waypoint to target waypoint
        /// </summary>
        public double GetDistance(Waypoint sourceWaypoint,
                                  Waypoint targetWaypoint)
        {
            /*
            // Find where is the source/target waypoint and find its key
            uint sourceKey = navigraph
                            .Where(WaypointList => WaypointList.Item.UUID
                            .Equals(sourceWaypoint.UUID)).Select(c => c.Key)
                            .First();
            uint targetKey = navigraph
                            .Where(WaypointList => WaypointList.Item.UUID
                            .Equals(targetWaypoint.UUID)).Select(c => c.Key)
                            .First();

            // Returns the distance of the path
            return navigraph.Dijkstra(sourceKey, targetKey).Distance;
            */
        }

        /// <summary>
        /// Gets the turning direction using three waypoints
        /// e.g. my previous design was to convert the direction to 
        /// a human-readable instruction. When the user is walking down 
        /// the hallway facing east, which his/her next waypoint is in south,
        /// the function converts the instruction to "Turn right on ...".
        /// </summary>
        public TurnDirection GetTurnDirection(Waypoint previousWaypoint,
                                              Waypoint currentWaypoint,
                                              Waypoint nextWaypoint)
        {
            // Find the cardinal direction to the next waypoint
            CardinalDirection currentDirection = previousWaypoint.Neighbors
                                            .Where(neighbors =>
                                                neighbors.TargetWaypointUUID
                                                .Equals(currentWaypoint.UUID))
                                            .Select(c => c.Direction).First();
            CardinalDirection nextDirection = currentWaypoint.Neighbors
                                            .Where(neighbors =>
                                                neighbors.TargetWaypointUUID
                                                .Equals(nextWaypoint.UUID))
                                            .Select(c => c.Direction).First();

            // Calculate the turning direction by cardinal direction
            int nextTurnDirection = (int)nextDirection - (int)currentDirection;
            nextTurnDirection = nextTurnDirection < 0 ?
                                nextTurnDirection + 8 : nextTurnDirection;

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
            /*
            List<Waypoint> wrongwayWaypointList = new List<Waypoint>();

            // Find the branch that is connect with current waypoint
            // but not connect with the next target waypoint (within the step).
            IEnumerable<Waypoint> adjacentWaypoints = 
                                from edge in Edges
                                where edge.SourceWaypoint
                                          .Equals(currentWaypoint.UUID) &&
                                      !edge.TargetWaypoint.UUID
                                           .Equals(nextWaypoint.UUID)
                                select edge.TargetWaypoint;

            if (previousWaypoint != null)
            {
                // Filters the previous waypoint within the instruction steps 
                // and add it into wrongwayWaypointList.
                adjacentWaypoints = adjacentWaypoints.Where(waypoint =>
                                !waypoint.UUID.Equals(previousWaypoint.UUID));

                wrongwayWaypointList.Add(previousWaypoint);
            }

            for (int i = 0; i < adjacentWaypoints.Count(); i++)
            {
                if (currentWaypoint.Coordinates.GetDistanceTo(
                    adjacentWaypoints.ElementAt(i).Coordinates) >=
                    thresholdDistance)
                {
                    wrongwayWaypointList.Add(adjacentWaypoints.ElementAt(i));
                    continue;
                }

                IEnumerable<Waypoint> connectedWaypoints =
                                        GetConnectedWaypoints(
                                            adjacentWaypoints.ElementAt(i),
                                            previousWaypoint,
                                            currentWaypoint,
                                            nextWaypoint);

                if (connectedWaypoints.Any())
                {
                    // Remove the duplicated waypoints that are already 
                    // included in the adjacentWaypoints list
                    adjacentWaypoints = adjacentWaypoints.Concat(
                                        from waypoint in connectedWaypoints
                                        where !adjacentWaypoints
                                            .Any(adjWaypoint =>
                                                 adjWaypoint.UUID
                                                 .Equals(waypoint.UUID))
                                        select waypoint);
                }
            }

            // returns a list after removing duplicated elements in the list
            return wrongwayWaypointList.Distinct().ToList();
            */           
        }

        /// <summary>
        /// Get the neighbor waypoints which are connect with the first-layer
        /// waypoints, which are the source waypoint's neighbors.
        /// </summary>
        private IEnumerable<Waypoint> GetConnectedWaypoints(
                                                   Waypoint adjacentWaypoint,
                                                   Waypoint previousWaypoint,
                                                   Waypoint currentWaypoint,
                                                   Waypoint nextWaypoint)
        {
            /*
            IEnumerable<Waypoint> connectedList = (from edge in Edges
                                                   where edge.SourceWaypoint
                                                             .Equals(adjacentWaypoint.UUID) &&

                                                         !edge.TargetWaypoint.UUID
                                                              .Equals(currentWaypoint.UUID) &&

                                                         !edge.TargetWaypoint.UUID
                                                              .Equals(nextWaypoint.UUID)
                                                   select edge.TargetWaypoint);

            if (previousWaypoint != null)
            {
                // Filters the previous waypoint within the instruction steps
                connectedList = connectedList.Where(waypoint =>
                                !waypoint.UUID.Equals(previousWaypoint.UUID));
            }

            return connectedList;
            */           
        }
    }

    #region Navigraph parameters

    /// <summary>
    /// The second level of the navigation graph within two-level hierarchy
    /// </summary>
    public class Region
    {
        /// <summary>
        /// Gets or sets the name of Region.
        /// e.g. 1F of NTUH
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The list of waypoint objects (nodes)
        /// </summary>
        public List<Waypoint> Waypoints { get; set; }

        /// <summary>
        /// Connection between waypoints (edges)
        /// </summary>
        public List<Edge> Edges { get; set; }

        /// <summary>
        /// The navigation subgraph
        /// </summary>
        public Graph<Waypoint, string> NavigationSubgraph = 
                new Graph<Waypoint, string>();

        /// <summary>
        /// Initializes a navigation subgraph of the Region and combine all the
        /// waypoints and edges to NavigationSubgraph
        /// </summary>
        public void SetNavigationSubgraph()
        {
            // Add all the waypoints of each region into region graph
            foreach (Waypoint waypoint in Waypoints)
            {
                NavigationSubgraph.AddNode(waypoint);
            }

            // Set each path into region graph
            foreach (Edge edge in Edges)
            {
                // Get the distance of two locations which in centimeter
                int distance = System.Convert.ToInt32(edge.Distance);

                // Get two connected waypoints's key value
                uint sourceWaypointKey = NavigationSubgraph.Where(waypoint =>
                        waypoint.Item.UUID.Equals(edge.SourceWaypoint.UUID))
                        .Select(waypoint => waypoint.Key).First();
                uint targetWaypointKey = NavigationSubgraph.Where(waypoint =>
                        waypoint.Item.UUID.Equals(edge.TargetWaypoint.UUID))
                        .Select(waypoint => waypoint.Key).First();

                // Connect the waypoints
                NavigationSubgraph.Connect(sourceWaypointKey, targetWaypointKey,
                        distance, string.Empty);
            }
        }
    }

    /// <summary>
    /// The model of waypoint
    /// </summary>
    public class Waypoint
    {
        /// <summary>
        /// Universal Unique Identifier of waypoint
        /// </summary>
        public Guid UUID { get; set; }

        /// <summary>
        /// Friendly name of waypoint
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Use is it to identify whether to switch the corressponding
        /// IPSClient using the enum type
        /// </summary>
        public IPSType IPSClientType { get; set; }

        /// <summary>
        /// Information for navigationlayer. Whether it has a landmark or null
        /// </summary>
        public string Landmark { get; set; }

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
    /// Represents the connected neighbor waypoints in a navigation graph
    /// </summary>
    public class Neighbor
    {
        /// <summary>
        /// UUID of the neighbor waypoint
        /// </summary>
        public Guid TargetWaypointUUID { get; set; }

        /// <summary>
        /// Indicates the name of the target which is facing
        /// </summary>
        public string TargetName { get; set; }

        /// <summary>
        /// Cardinal direction for the host waypoint using the enum type
        /// </summary>
        public CardinalDirection Direction { get; set; }
    }

    /// <summary>
    /// The edge between the two waypoints
    /// </summary>
    public class Edge
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
        public double Distance { get; set; }

        /// <summary>
        /// Cardinal direction for the host waypoint using the enum type
        /// </summary>
        public CardinalDirection Direction { get; set; }

        /// <summary>
        /// The connection type using the enum type
        /// e.g. normal hallway, stair, elevator...etc
        /// </summary>
        public ConnectionType Connection { get; set; }

        /// <summary>
        /// Other informations regarding the edge
        /// e.g. wheelchair support
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
        /// The direction to turn to the next waypoint using the enum type
        /// </summary>
        public TurnDirection Direction { get; set; }
    }

}