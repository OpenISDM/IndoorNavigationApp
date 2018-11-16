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
 *      MapInformation.cs
 *
 * Abstract:
 *
 *      The elements on the map: Beacon element, Beacon group element and the
 *      element for connecting two locations
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
    /// Beacon
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
        /// Threshold (RSSI)
        /// </summary>
        public int Threshold { get; set; }
        /// <summary>
        /// Beacon installed floor
        /// </summary>
        public virtual float Floor { get; set; }
    }

    /// <summary>
    /// Beacon group
    /// </summary>
    public abstract class BeaconGroup
    {
        /// <summary>
        /// Group id
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// ex: Police station
        /// </summary>
        public string Name { get; set; }
    }

    /// <summary>
    /// Location connect
    /// </summary>
    public abstract class LocationConnect
    {
        /// <summary>
        /// Is it a two-way road?
        /// </summary>
        public bool IsTwoWay { get; set; }
    }

    /// <summary>
    /// The monitor of LBeacon, including the threshold, floor of installation
    /// and the location.
    /// Major、Minor
    /// </summary>
    public class IBeaconModel : Beacon, IIBeacon
    {
        /// <summary>
        /// IBeacon coordinate
        /// </summary>
        public GeoCoordinate IBeaconCoordinate { get; set; }
    }

    /// <summary>
    /// The monitor of LBeacon, including the threshold, floor of installation
    /// and direction of installation.
    /// </summary>
    public class LBeaconModel : Beacon, ILBeacon
    {
        /// <summary>
        /// Beacon's direction of installation
        /// Beacon's reffered coordinate where the arrow points to
        /// Not be used in this version
        /// </summary>
        public GeoCoordinate MarkCoordinate { get; set; }

        public override float Floor { get { return this.GetFloor(); } }
    }

    /// <summary>
    /// A group of beacons that is regarded as a location
    /// </summary>
    public class BeaconGroupModel : BeaconGroup, IBeaconGroupModel
    {
        /// <summary>
        /// Beacon's union
        /// </summary>
        public List<Beacon> Beacons { get; set; }

        /// <summary>
        /// Group coordinate
        /// Assume there are two parallel Lbeacons, the central coordinate are
        /// regarded as the group's coordinate.
        /// </summary>
        public GeoCoordinate Coordinate
        {
            get
            {
                // Get all the LBeacon's coordinate in the group
                List<GeoCoordinate> Coordinates =
                    Beacons.Select(c => c.GetCoordinate()).ToList();

                // Compute the average of the coordinate of all the LBeaocns 
                // in order to get the central coordinate.
                double TotalLatitude = 0; double TotalLongitude = 0;

                foreach (GeoCoordinate Coordinate in Coordinates)
                {
                    TotalLatitude += Coordinate.Latitude;
                    TotalLongitude += Coordinate.Longitude;
                }

                return new GeoCoordinate(
                    TotalLatitude / Coordinates.Count(),
                    TotalLongitude / Coordinates.Count());
            }
        }
    }

    /// <summary>
    /// A group of LBeacon which is regarded as a location. This element is 
    /// used to store the map data when the network connection is down.
    /// </summary>
    public class BeaconGroupModelForMapFile : BeaconGroup,
        IBeaconGroupModelForMapFile
    {
        /// <summary>
        /// Beacon's union
        /// </summary>
        public List<Guid> Beacons { get; set; }
    }

    /// <summary>
    /// Path connecting two nodes
    /// Pay attention to direction
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
    /// A path connects two nodes. It is used to store the map data in the
    /// phone when the Internet is unconnected.
    /// Pay attention to direction
    /// </summary>
    public class LocationConnectModelForMapFile : LocationConnect,
        ILocationConnectModelForMapFile
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
