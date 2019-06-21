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
 *      This file contains all the interfaces required by the application,
 *      such as the interface of IPSClient and the interface for 
 *      both iOS project and the Android project to allow the Xamarin.Forms 
 *      app to access the APIs on each platform.
 *      
 * Version:
 *
 *      1.0.0, 20190605
 * 
 * File Name:
 *
 *      Interface.cs
 *
 * Abstract:
 *
 *      Waypoint-based navigator is a mobile Bluetooth navigation application
 *      that runs on smart phones. It is structed to support anywhere 
 *      navigation indoors in areas covered by different indoor positioning 
 *      system (IPS) and outdoors covered by GPS.In particilar, it can rely on
 *      BeDIS (Building/environment Data and Information System) for indoor 
 *      positioning. This IPS provides a location beacon at every waypoint. The 
 *      beacon brocasts its own coordinates; Consequesntly, the navigator does 
 *      not need to continuously monitor its own position.
 *      This version makes use of Xamarin.Forms, which is a cross-platform UI 
 *      tookit that runs on both iOS and Android.
 *
 * Authors:
 *
 *      Kenneth Tang, kenneth@gm.nssh.ntpc.edu.tw
 *      Paul Chang, paulchang@iis.sinica.edu.tw
 *      m10717004@yuntech.edu.tw
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
    /// The interface reserved for iBeacon data
    /// </summary>
    public interface IIBeacon
    {
        /// <summary>
        /// This method returns its coordinate because ibeacon not
        /// have the coordinate information in UUID.
        /// </summary>
        GeoCoordinate IBeaconCoordinates { get; set; }
    }

    /// <summary>
    /// The interface of LBeacon
    /// </summary>
    public interface ILBeacon
    {
        /// <summary>
        /// The direction of Beacon's installation
        /// The reffered coordinate for the arrow on the Beacon
        /// Not used in this version
        /// </summary>
        GeoCoordinate MarkCoordinates { get; set; }
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
        GeoCoordinate Coordinates { get; }
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

    #region Interface for connecting both iOS project and Android project

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
        void Speak(string text, string language);
    }
    #endregion

    #region Interface for IPS Client
    public interface IIPSClient
    {
        Beacon SignalProcessing();
        void SetBeaconList(List<Beacon> BeaconList);
        void Stop();
    }
    #endregion
}
