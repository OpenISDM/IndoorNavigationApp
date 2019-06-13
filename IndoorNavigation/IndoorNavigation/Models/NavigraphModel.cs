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
 *      The Model of the navigation graph include beacon element, 
 *      beacon group element and the element for connecting two waypoints
 *
 * File Name:
 *
 *      NavigraphModel.cs
 *
 * Abstract:
 *
 *      
 *
 * Authors:
 *
 *      Kenneth Tang, kenneth@gm.nssh.ntpc.edu.tw
 *
 */

 // TODO: The following class of Beacon should be combined into NavigationGraph

using System;
using System.Collections.Generic;
using System.Linq;
using GeoCoordinatePortable;

namespace IndoorNavigation.Models
{
    /// <summary>
    /// The attribute of beacon
    /// </summary>
    public abstract class Beacon
    {
        /// <summary>
        /// Beacon UUID
        /// </summary>
        public Guid UUID { get; set; }
        /// <summary>
        /// IBeacon Major field
        /// </summary>
        public int Major { get; set; }
        /// <summary>
        /// IBeacon Minor field
        /// </summary>
        public int Minor { get; set; }
        /// <summary>
        /// Threshold RSSI level
        /// </summary>
        public int Threshold { get; set; }
        /// <summary>
        /// The floor on which the beacon is installed
        /// </summary>
        public virtual float Floor { get; set; }
        /// <summary>
        /// The types of Beacon
        /// </summary>
    }

    /// <summary>
    /// The attribute of iBeacon, including it threshold, floor of 
    /// installation and it location and Major, Minor. 
    /// </summary>
    public class IBeaconModel : Beacon, IIBeacon
    {
        /// <summary>
        /// iBeacon coordinates
        /// </summary>
        public GeoCoordinate IBeaconCoordinates { get; set; }
    }

    /// <summary>
    /// The attribute of LBeacon, including the rssi threshold, floor of 
    /// installation and direction of installation.
    /// </summary>
    public class LBeaconModel : Beacon, ILBeacon
    {
        /// <summary>
        /// Beacon's direction of installation
        /// Beacon's reffered coordinates where the arrow points to
        /// Not be used in this version
        /// </summary>
        public GeoCoordinate MarkCoordinates { get; set; }

        public override float Floor { get { return this.GetFloor(); } }
    }

    /// <summary>
    /// Beacon group containing multiple beacons that mark a single waypoint
    /// </summary>
    public abstract class BeaconGroup
    {
        /// <summary>
        /// Group id
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Name of the waypoint
        /// </summary>
        public string Name { get; set; }
    }

    /// <summary>
    /// A group of beacons that are used to mark a waypoint.
    /// </summary>
    public class WaypointModel : BeaconGroup, IBeaconGroupModel
    {
        /// <summary>
        /// Beacon's union
        /// </summary>
        public List<Beacon> Beacons { get; set; }

        /// <summary>
        /// Waypoint coordinates
        /// The coordinate pf a Beacon group is the centroid
        /// of the beacon group.
        /// </summary>
        public GeoCoordinate Coordinates
        {
            get
            {
                // Get all the LBeacon's coordinates in the group
                List<GeoCoordinate> _Coordinates =
                    Beacons.Select(c => c.GetCoordinates()).ToList();

                // Compute the average of the coordinates of all the LBeaocns 
                // in order to get the central coordinates.
                double TotalLatitude = 0; double TotalLongitude = 0;

                foreach (GeoCoordinate coordinate in _Coordinates)
                {
                    TotalLatitude += coordinate.Latitude;
                    TotalLongitude += coordinate.Longitude;
                }

                return new GeoCoordinate(
                    TotalLatitude / _Coordinates.Count(),
                    TotalLongitude / _Coordinates.Count());
            }
        }

        /// <summary>
        /// The floor where the waypoint is located
        /// </summary>
        public float Floor
        {
            get
            {
                if (Beacons.Count != 0)
                    return Beacons.First().Floor;

                return float.NaN;
            }
        }
    }

    /// <summary>
    /// A group of LBeacons that are used to mark a waypoint. This element is 
    /// used to store the offline navigation graph data on the phone.
    /// </summary>
    public class WaypointModelForNavigraphFile : BeaconGroup,
        IBeaconGroupModelForNavigraphFile
    {
        /// <summary>
        /// Beacon's union
        /// </summary>
        public List<Guid> Beacons { get; set; }
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
        /// ID of neighbor waypoint
        /// </summary>
        public Guid TargetWaypointId { get; set; }
    }

    /// <summary>
    /// Path connecting two waypoints
    /// </summary>
    public class LocationConnectModel : LocationConnect, ILocationConnectModel
    {
        /// <summary>
        /// Location A
        /// </summary>
        public WaypointModel SourceWaypoint { get; set; }
        /// <summary>
        /// Location B
        /// </summary>
        public WaypointModel TargetWaypoint { get; set; }
    }

}
