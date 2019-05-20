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
using System.Xml.Serialization;
using System.Xml;

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
    [XmlRoot("Navigraph")]
    public class Navigraph
    {
        /// <summary>
        /// Gets or sets the name
        /// e.g. Taipei City Hall, NTHU...etc
        /// </summary>
        [XmlElement("Name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the regions of entire Navigraph
        /// </summary>
        [XmlArray("Regions")]
        [XmlArrayItem("Region", typeof(Region))]
        public List<Region> Regions { get; set; }

        /// <summary>
        /// Gets or sets the edges that connection between regions,
        /// e.g. stair, elevator...etc
        /// </summary>
        [XmlArray("Edges")]
        [XmlArrayItem("Edge", typeof(Edge))]
        public List<Edge> Edges { get; set; }


        #region Public methods

        /// <summary>
        /// Get the instance of region graph, nodes in it are regions and edges
        /// are pathways linking the regions.
        /// </summary>
        public Graph<Region, string> GetRegiongraph()
        {
            Graph<Region, string> regionGraph =
                    new Graph<Region, string>();

            foreach (Region region in Regions)
            {
                regionGraph.AddNode(region);
            }

            foreach (Edge edge in Edges)
            {
                Region sourceRegion = Regions.First(region =>
                        region.Waypoints.Any(waypoint => 
                        waypoint.ID.Equals(edge.SourceWaypointUUID)));
                Region targetRegion = Regions.First(region =>
                        region.Waypoints.Any(waypoint =>
                        waypoint.ID.Equals(edge.TargetWaypointUUID)));

                uint sourceKey = regionGraph
                        .Where(region => region.Item.Waypoints
                        .Equals(sourceRegion.Waypoints))
                        .Select(c => c.Key).First();
                uint targetKey = regionGraph
                        .Where(region => region.Item.Waypoints
                        .Equals(targetRegion.Waypoints))
                        .Select(c => c.Key).First();

                regionGraph.Connect(sourceKey, targetKey,
                        (int)edge.Distance, string.Empty);
            }


            return regionGraph;
        }

        /// <summary>
        /// Gets the distance from source waypoint to target waypoint
        /// </summary>
        public static double GetDistance(
                                    Graph<Waypoint, string> connectionGraph,
                                    Waypoint sourceWaypoint,
                                    Waypoint targetWaypoint)
        {
            // Find where is the source/target waypoint and find its key
            uint sourceKey = connectionGraph
                            .Where(WaypointList => WaypointList.Item.ID
                            .Equals(sourceWaypoint.ID)).Select(c => c.Key)
                            .First();
            uint targetKey = connectionGraph
                            .Where(WaypointList => WaypointList.Item.ID
                            .Equals(targetWaypoint.ID)).Select(c => c.Key)
                            .First();

            // Returns the distance of the path
            return connectionGraph.Dijkstra(sourceKey, targetKey).Distance;
        }

        /// <summary>
        /// Gets the turning direction using three waypoints
        /// e.g. This method was to convert the direction to 
        /// a human-readable instruction. When the user is walking down 
        /// the hallway facing east, which his/her next waypoint is in south,
        /// the function converts the instruction to "Turn right on ...".
        /// </summary>
        public static TurnDirection GetTurnDirection(
                                                Waypoint previousWaypoint,
                                                Waypoint currentWaypoint,
                                                Waypoint nextWaypoint)
        {
            // Find the cardinal direction to the next waypoint
            CardinalDirection currentDirection = previousWaypoint.Neighbors
                                            .Where(neighbors =>
                                                neighbors.TargetWaypointUUID
                                                .Equals(currentWaypoint.ID))
                                            .Select(c => c.Direction).First();
            CardinalDirection nextDirection = currentWaypoint.Neighbors
                                            .Where(neighbors =>
                                                neighbors.TargetWaypointUUID
                                                .Equals(nextWaypoint.ID))
                                            .Select(c => c.Direction).First();

            // Calculate the turning direction by cardinal direction
            int nextTurnDirection = (int)nextDirection - (int)currentDirection;
            nextTurnDirection = nextTurnDirection < 0 ?
                                nextTurnDirection + 8 : nextTurnDirection;

            return (TurnDirection)nextTurnDirection;
        }

        #endregion
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
        [XmlElement("Name")]
        public string Name { get; set; }

        /// <summary>
        /// The list of waypoint objects (nodes)
        /// </summary>
        [XmlArray("Waypoints")]
        [XmlArrayItem("Waypoint", typeof(Waypoint))]
        public List<Waypoint> Waypoints { get; set; }

        /// <summary>
        /// Connection between waypoints (edges)
        /// </summary>
        [XmlArray("Edges")]
        [XmlArrayItem("Edge", typeof(Edge))]
        public List<Edge> Edges { get; set; }

        /// <summary>
        /// The navigation subgraph
        /// </summary>
        [XmlIgnore]
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
                        waypoint.Item.ID.Equals(edge.SourceWaypoint.ID))
                        .Select(waypoint => waypoint.Key).First();
                uint targetWaypointKey = NavigationSubgraph.Where(waypoint =>
                        waypoint.Item.ID.Equals(edge.TargetWaypoint.ID))
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
        [XmlElement("UUID")]
        public Guid ID { get; set; }

        /// <summary>
        /// Friendly name of waypoint
        /// </summary>
        [XmlElement("Name")]
        public string Name { get; set; }

        /// <summary>
        /// Use is it to identify whether to switch the corressponding
        /// IPSClient using the enum type.
        /// </summary>
        [XmlIgnore]
        public IPSType IPSClientType { get; set; }
        [XmlElement("IPStype")]
        public string IPStype
        {
            get { return IPSClientType.ToString(); }
            set
            {
                if (string.IsNullOrEmpty(value) || !Enum.GetNames(typeof(IPSType)).Contains(value))
                {
                    //IPSClientType = IPSType.NoIPSType;
                }
                else
                {
                    IPSClientType = (IPSType)Enum.Parse(typeof(IPSType), value);
                }
            }
        }

        /// <summary>
        /// Message used to instruct the user to face a direction known to the
        /// naivgator.
        /// </summary>
        [XmlElement("Landmark")]
        public string OrientationInstruction { get; set; }

        /// <summary>
        /// The coordinates of a waypoint
        /// </summary>
        [XmlIgnore]
        public GeoCoordinate Coordinates { get; set; }
        private double lat;
        [XmlElement("Lat")]
        public string Lat
        {
            get => lat.ToString();
            set => lat = double.Parse(value);
        }
        [XmlElement("Lon")]
        public string Lon
        {
            get => Coordinates.Longitude.ToString();
            set => Coordinates = new GeoCoordinate(lat, double.Parse(value));
        }

        /// <summary>
        /// The floor where the waypoint is located
        /// </summary>
        [XmlElement("Floor")]
        public int Floor { get; set; }

        /// <summary>
        /// Neighbors of the waypoint
        /// </summary>
        [XmlArray("Neighbors")]
        [XmlArrayItem("Neighbor", typeof(Neighbor))]
        public List<Neighbor> Neighbors { get; set; }
    }

    /// <summary>
    /// Represents the connected neighbor waypoints in a navigation graph
    /// </summary>
    public struct Neighbor
    {
        /// <summary>
        /// UUID of the neighbor waypoint
        /// </summary>
        [XmlElement("TargetWaypointUUID")]
        public Guid TargetWaypointUUID { get; set; }

        /// <summary>
        /// Indicates the name of the target which is facing
        /// </summary>
        [XmlElement("TargetName")]
        public string TargetName { get; set; }

        /// <summary>
        /// Cardinal direction for the host waypoint using the enum type
        /// </summary>
        [XmlElement("Direction")]
        public CardinalDirection Direction { get; set; }
    }

    /// <summary>
    /// The edge/connection between the two waypoints(Location A -> Location B)
    /// </summary>
    public class Edge
    {
        /// <summary>
        /// Location A
        /// </summary>
        [XmlIgnore]
        public Waypoint SourceWaypoint { get; set; }
        [XmlElement("SourceWaypointUUID")]
        public Guid SourceWaypointUUID { get; set; }

        /// <summary>
        /// Location B
        /// </summary>
        [XmlIgnore]
        public Waypoint TargetWaypoint { get; set; }
        [XmlElement("TargetWaypointUUID")]
        public Guid TargetWaypointUUID { get; set; }

        /// <summary>
        /// Gets or sets the distance
        /// </summary>
        [XmlElement("Distance")]
        public double Distance { get; set; }

        /// <summary>
        /// Cardinal direction for the host waypoint using the enum type
        /// </summary>
        [XmlIgnore]
        public CardinalDirection Direction { get; set; }
        [XmlElement("ReferenceDirection")]
        public string ReferenceDirection
        {
            get { return Direction.ToString(); }
            set
            {
                if (string.IsNullOrEmpty(value) || !Enum.GetNames(typeof(CardinalDirection)).Contains(value))
                {
                    //Direction = CardinalDirection.NoDirection;
                }
                else
                {
                    Direction = (CardinalDirection)Enum.Parse(typeof(CardinalDirection), value);
                }
            }
        }

        /// <summary>
        /// The connection type using the enum type
        /// e.g. normal hallway, stair, elevator...etc
        /// </summary>
        [XmlIgnore]
        public ConnectionType ConnectionType { get; set; }
        [XmlElement("Connectiontype")]
        public string Connectiontype
        {
            get { return ConnectionType.ToString(); }
            set
            {
                if (string.IsNullOrEmpty(value) || !Enum.GetNames(typeof(ConnectionType)).Contains(value))
                {
                    //Connection = ConnectionType.NoConnectionType;
                }
                else
                {
                    ConnectionType = (ConnectionType)Enum.Parse(typeof(ConnectionType), value);
                }
            }
        }

        /// <summary>
        /// Other informations regarding the edge
        /// e.g. wheelchair support
        /// </summary>
        [XmlElement("OtherInformations")]
        public string OtherInformations { get; set; }
    }

    #endregion
}