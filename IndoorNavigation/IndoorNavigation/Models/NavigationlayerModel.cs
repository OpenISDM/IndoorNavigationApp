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
 *      This file contains models which have Beacon's attribute and data of
 *      route planning. These models are used for waypoint-based navigation.
 * 
 * File Name:
 *
 *      NavigationlayerModel.cs
 *
 * Abstract:
 *
 *      TODO: Add the new abstract
 *
 * Authors:
 *
 *      
 *
 */

using System;
using System.Collections.Generic;
using System.Linq;
using GeoCoordinatePortable;

namespace IndoorNavigation.Models.NavigaionLayer
{
    public class NavigationModel
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
        /// The angle to turn to the next waypoint
        /// </summary>
        public int Angle { get; set; }
    }

    #region Navigraph

    /// <summary>
    /// The top level of the navigation graph within two-level hierarchy
    /// </summary>
    public class Navigraph
    {
        public string Name { get; set; }
        public List<Region> Regions { get; set; }
        public LocationConnectModel LocationConnect { get; set; }
    }

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
        public string Target { get; set; }
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

        /// <summary>
        /// Reference direction for the host waypoint
        /// </summary>
        public ReferenceDirection Direction { get; set; }
    }

    public enum ReferenceDirection
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

    /// <summary>
    /// The edge between the two waypoints
    /// </summary>
    public class LocationConnectModel : LocationConnect
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
}