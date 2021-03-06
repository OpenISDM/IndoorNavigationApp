﻿/*
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
 *      Bo-Chen Huang, m10717004@yuntech.edu.tw
 *      Chun-Yu Lai, chunyu1202@gmail.com
 *
 */
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GeoCoordinatePortable;


namespace IndoorNavigation.Models
{
    #region Interface for connecting both iOS project and Android project

    public interface IBeaconScan
    {
        void StartScan();
        void StopScan();
        void Close();
        NavigationEvent _event { get; }
    }

    public interface LBeaconScan
    {
        void StartScan();
        void StopScan();
        void Close();
        NavigationEvent _event { get; }
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
        void DetectWaypoints();
        void SetWaypointList(List<WaypointBeaconsMapping> WaypointList);
        void Stop();

        NavigationEvent _event { get; }
    }

    public class WaypointBeaconsMapping
    {
        public RegionWaypointPoint _WaypointIDAndRegionID { get; set; }
        public List<Guid> _Beacons { get; set; }
        public Dictionary<Guid, int> _BeaconThreshold { get; set; }
    }
    #endregion
}
