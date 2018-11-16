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
 *      Interfa.cs
 *
 * Abstract:
 *
 *      Define the interface required to connect IOS projects and
 *      Android project and map information.
 *
 * Authors:
 *
 *      Kenneth Tang, kenneth@gm.nssh.ntpc.edu.tw
 *
 */

using GeoCoordinatePortable;
using System;
using System.Collections.Generic;

namespace IndoorNavigation.Models
{
    #region Map information interface

    /// <summary>
    /// The interface reserved for IBeacon data
    /// </summary>
    public interface IIBeacon
    {
        /// <summary>
        /// IBeacon coordinate
        /// </summary>
        GeoCoordinate IBeaconCoordinate { get; set; }
    }

    /// <summary>
    /// LBeacon資訊
    /// </summary>
    public interface ILBeacon
    {
        /// <summary>
        /// The direction of Beacon's installation
        /// The reffered coordinate for the arrow on the Beacon
        /// Not be used for this version
        /// </summary>
        GeoCoordinate MarkCoordinate { get; set; }
    }

    /// <summary>
    /// There are multiple Beacons in a group
    /// </summary>
    public interface IBeaconGroupModel
    {
        /// <summary>
        /// Beacon's union
        /// </summary>
        List<Beacon> Beacons { get; set; }
        /// <summary>
        /// The center coordinate of the group
        /// </summary>
        GeoCoordinate Coordinate { get; }
    }

    /// <summary>
    /// There are multiple Beacons in a group. It is used to store the map 
    /// data in the phone when the Internet is unconnected.
    /// </summary>
    public interface IBeaconGroupModelForMapFile
    {
        /// <summary>
        /// Beacon's union
        /// </summary>
        List<Guid> Beacons { get; set; }
    }

    /// <summary>
    /// A path connects two nodes
    /// </summary>
    public interface ILocationConnectModel
    {
        /// <summary>
        /// Location A
        /// </summary>
        BeaconGroupModel BeaconA { get; set; }
        /// <summary>
        /// Location B
        /// </summary>
        BeaconGroupModel BeaconB { get; set; }
    }

    /// <summary>
    /// A path connects two nodes. It is used to store the map data in the
    /// phone when the Internet is unconnected.
    /// </summary>
    public interface ILocationConnectModelForMapFile
    {
        /// <summary>
        /// Location A
        /// </summary>
        Guid BeaconA { get; set; }
        /// <summary>
        /// Location B
        /// </summary>
        Guid BeaconB { get; set; }
    }

    #endregion

    #region Interface for connecting IOS projects and Android projects

    /// <summary>
    /// The interface with beacon scan module
    /// There is one Beacon scan module for each version, IOS and Android.
    /// Beacon scan module has to use the original API provided by system
    /// </summary>
    public interface IBeaconScan
    {
        void Init(Action<List<BeaconSignalModel>> SendSignalFunction);
        void StartScan(List<Guid> BeaconsUUID);
        void StopScan();
        void Close();
    }

    #endregion
}
