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
 * File Name:
 *
 *      NavigraphInformation.cs
 *
 * Abstract:
 *
 *      The elements of the navigation graph include beacon element, 
 *      beacon group element and the element for connecting two waypoints
 *
 * Authors:
 *
 *      Kenneth Tang, kenneth@gm.nssh.ntpc.edu.tw
 *
 */

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
    /// Location connect
    /// </summary>
    public abstract class LocationConnect
    {
        /// <summary>
        /// Is it a two ways path?
        /// </summary>
        public bool IsTwoWay { get; set; }
    }

    /// <summary>
    /// The attribute of LBeacon, including it threshold, floor of installation
    /// and it location and Major, Minor
    /// </summary>
    public class IBeaconModel : Beacon, IIBeacon
    {
        /// <summary>
        /// LBeacon coordinates
        /// </summary>
        public GeoCoordinates IBeaconCoordinates { get; set; }
    }

    /// <summary>
    /// The monitor of LBeacon, including the rssi threshold, floor of installation
    /// and direction of installation.
    /// </summary>
    public class LBeaconModel : Beacon, ILBeacon
    {
        /// <summary>
        /// Beacon's direction of installation
        /// Beacon's reffered coordinates where the arrow points to
        /// Not be used in this version
        /// </summary>
        public GeoCoordinates MarkCoordinates { get; set; }

        public override float Floor { get { return this.GetFloor(); } }
    }

    /// <summary>
    /// A group of beacons that are used to mark a waypoint
    /// </summary>
    public class BeaconGroupModel : BeaconGroup, IBeaconGroupModel
    {
        /// <summary>
        /// Beacon's union
        /// </summary>
        public List<Beacon> Beacons { get; set; }

        /// <summary>
        /// Group coordinates
        /// The coordinate pf a Beacon group is the centroid
        /// of the beacon group
        /// </summary>
        public GeoCoordinates Coordinates
        {
            get
            {
                // Get all the LBeacon's coordinates in the group
                List<GeoCoordinates> coordinates =
                    Beacons.Select(c => c.GetCoordinates()).ToList();

                // Compute the average of the coordinates of all the LBeaocns 
                // in order to get the central coordinates.
                double TotalLatitude = 0; double TotalLongitude = 0;

                foreach (GeoCoordinates _coordinate in coordinates)
                {
                    TotalLatitude += _coordinate.Latitude;
                    TotalLongitude += _coordinate.Longitude;
                }

                return new GeoCoordinates(
                    TotalLatitude / coordinates.Count(),
                    TotalLongitude / coordinates.Count());
            }
        }
    }

    /// <summary>
    /// A group of LBeacons that are used to mark a waypoint. This element is 
    /// used to store the offline navigation graph data on the phone.
    /// </summary>
    public class BeaconGroupModelForMapFile : BeaconGroup,
        IBeaconGroupModelForNavigraphFile
    {
        /// <summary>
        /// Beacon's union
        /// </summary>
        public List<Guid> Beacons { get; set; }
    }

    /// <summary>
    /// Path connecting two waypoints
    /// Consider one way or two ways
    /// </summary>
    public class LocationConnectModel : LocationConnect, ILocationConnectModel
    {
        /// <summary>
        /// Location A
        /// </summary>
        public BeaconGroupModel BeaconA { get; set; }
        /// <summary>
        /// Location B
        /// </summary>
        public BeaconGroupModel BeaconB { get; set; }
    }

    /// <summary>
    /// A path connects two waypoints. The file is used to store
    /// the navigation graph data in the phone.
    /// Consider one way or two ways
    /// </summary>
    public class LocationConnectModelForMapFile : LocationConnect,
        ILocationConnectModelForNavigraphFile
    {
        /// <summary>
        /// Location A
        /// </summary>
        public Guid BeaconA { get; set; }
        /// <summary>
        /// Location B
        /// </summary>
        public Guid BeaconB { get; set; }
    }
}
