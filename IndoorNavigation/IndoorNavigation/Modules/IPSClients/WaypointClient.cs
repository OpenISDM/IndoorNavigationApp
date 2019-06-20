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
 *      
 *      
 * Version:
 *
 *      1.0.0, 201906
 * 
 * File Name:
 *
 *      WaypointClient.cs
 *
 * Abstract:
 *
 *      Waypoint-based navigator is a mobile Bluetooth navigation application
 *      that runs on smart phones. It is structed to support anywhere 
 *      navigation. Indoors in areas covered by different indoor positioning 
 *      system (IPS) and outdoors covered by GPS. In particilar, it can rely on 
 *      BeDIS (Building/environment Data and Information System) for indoor 
 *      positioning. Using this IPS, the navigator does not need to 
 *      continuously monitor its own position, since the IPS broadcast to the 
 *      navigator the location of each waypoint. 
 *      This version makes use of Xamarin.Forms, which is a complete 
 *      cross-platform UI tookit that runs on both iOS and Android.
 *
 * Authors:
 *
 *      Kenneth Tang, kenneth@gm.nssh.ntpc.edu.tw
 *      Paul Chang, paulchang@iis.sinica.edu.tw
 *      m10717004@yuntech.edu.tw
 *      Chun Yu Lai, chunyu1202@gmail.com
 *
 */
using System;
using System.Linq;
using System.Collections.Generic;
using IndoorNavigation.Models;
using IndoorNavigation.Models.NavigaionLayer;
using IndoorNavigation.Modules;

namespace IndoorNavigation.Modules.IPSClients
{
    class WaypointClient : IIPSClient
    {
        private List<Waypoint> _waypointList = new List<Waypoint>();

        private object _bufferLock = new object();
        private readonly EventHandler _HBeaconScan;

        public NavigationEvent _Event { get; private set; }
        private List<BeaconSignalModel> _beaconSignalBuffer = new List<BeaconSignalModel>();

        public WaypointClient()
        {
            _Event = new NavigationEvent();

            _HBeaconScan = new EventHandler(HandleBeaconScan);
            Utility.BeaconScan._Event._EventHandler += _HBeaconScan;
            _waypointList = new List<Waypoint>();

        }

        public void SetWaypointList(List<Waypoint> WaypointList)
        {
            this._waypointList = WaypointList;

            Utility.BeaconScan.StartScan();
        }

        public void SignalProcessing()
        {
            Console.WriteLine(">> In SignalProcessing");

            // Remove the obsolete data from buffer
            List<BeaconSignalModel> removeSignalBuffer =
                new List<BeaconSignalModel>();

            lock (_bufferLock)
            {
                removeSignalBuffer.AddRange(
                    _beaconSignalBuffer.Where(c =>
                    c.Timestamp < DateTime.Now.AddMilliseconds(-1000)));

                foreach (var obsoleteBeaconSignal in removeSignalBuffer)
                    _beaconSignalBuffer.Remove(obsoleteBeaconSignal);

                foreach (BeaconSignalModel beacon in _beaconSignalBuffer)
                {
                    for (int i = 0; i < _waypointList.Count; i++) {

                        for (int j = 0; j < _waypointList[i].Beacons.Count; j++) {
                            if (beacon.UUID.Equals(_waypointList[i].Beacons[j].UUID)) {
                                Console.WriteLine("Matched waypoint:" +
                                                  _waypointList[i].ID +
                                                  " by detected Beacon:" +
                                                  beacon.UUID);

                                _Event.OnEventCall(new WayPointSignalEventArgs
                                {
                                    CurrentWaypoint = _waypointList[i]
                                }) ;
                                return;
                            }
                        }
                    }
                }
            }
            Console.WriteLine("<< In SignalProcessing");
        }

        private void HandleBeaconScan(object sender, EventArgs e)
        {
            IEnumerable<BeaconSignalModel> signals =
                (e as BeaconScanEventArgs)._signals;

            foreach (BeaconSignalModel signal in signals)
            {
                Console.WriteLine("Detected Beacon UUID : " + signal.UUID + " RSSI = " + signal.RSSI);
            }

            lock (_bufferLock)
                _beaconSignalBuffer.AddRange(signals);

        }

        public void Stop()
        {
            Utility.BeaconScan._Event._EventHandler -= _HBeaconScan;
            _beaconSignalBuffer = null;
            _bufferLock = null;
        }

    }

    public class WayPointSignalEventArgs : EventArgs
    {
        public Waypoint CurrentWaypoint { get; set; }
    }

}
