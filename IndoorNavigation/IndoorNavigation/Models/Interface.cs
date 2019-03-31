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
 *      This file contain the definition of the interface required to connect 
 *      Android/iOS project and navigation graph information.
 *
 * File Name:
 *
 *      Interface.cs
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

using GeoCoordinatePortable;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IndoorNavigation.Models
{
    #region Interface of navigation graph attribute

    /// <summary>
    /// The interface reserved for IBeacon data
    /// </summary>
    public interface IIBeacon
    {
        /// <summary>
        /// IBeacon coordinate
        /// </summary>
        GeoCoordinates IBeaconCoordinates { get; set; }
    }

    /// <summary>
    /// LBeacon資訊
    /// </summary>
    public interface ILBeacon
    {
        /// <summary>
        /// The direction of Beacon's installation
        /// The reffered coordinate for the arrow on the Beacon
        /// Not used in this version
        /// </summary>
        GeoCoordinates MarkCoordinates { get; set; }
    }

    /// <summary>
    /// There are multiple Beacons in a group
    /// All beacons in the group mark a single waypoint
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
        GeoCoordinates Coordinates { get; }
        /// <summary>
        /// The floor of the group
        /// </summary>
        float Floor { get; }
    }

    /// <summary>
    /// There are multiple Beacons in a group, each group mark a waypoint
    /// The file is used to store the navigation graph data in the phone
    /// </summary>
    public interface IBeaconGroupModelForNavigraphFile
    {
        /// <summary>
        /// Beacon's union
        /// </summary>
        List<Guid> Beacons { get; set; }
    }

    /// <summary>
    /// A path connects two waypoints
    /// </summary>
    public interface ILocationConnectModel
    {
        /// <summary>
        /// Location A
        /// </summary>
        WaypointModel SourceWaypoint { get; set; }
        /// <summary>
        /// Location B
        /// </summary>
        WaypointModel TargetWaypoint { get; set; }
    }

    #endregion

    #region Interface for connecting iOS project and Android project

    /// <summary>
    /// The interface with beacon scan module
    /// There is one Beacon scan module for each version, IOS and Android.
    /// Beacon scan module has to use the original API provided by system
    /// </summary>
    public interface IBeaconScan
    {
        void StartScan(List<Guid> BeaconsUUID);
        void StopScan();
        void Close();
        BeaconScanEvent Event { get; }
    }

    public interface IQrCodeDecoder
    {
        Task<string> ScanAsync();
    }

    public interface ITextToSpeech
    {
        void Speak(string text);
    }
    #endregion

    #region Interface for navigation algorithm
    public interface INavigationAlgorithm
    {
        void Work();
        void StopNavigation();
        ISignalProcessingAlgorithm CreateSignalProcessingAlgorithm();
        bool IsReachingDestination { get; }
    }
    #endregion

    #region Interface for signal processing algorithm
    public interface ISignalProcessingAlgorithm
    {
        void SignalProcessing();
    }
    #endregion
}
