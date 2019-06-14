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
 *      
 *      
 * Version:
 *
 *      1.0.0, 201906
 * 
 * File Name:
 *
 *      IPSClient.cs
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
        // _beaconList must not be Waypoint List, but we uses it temporarily now.
        private List<Waypoint> _beaconList;
        private List<Waypoint> _waypointList;

        private object bufferLock = new object();
        private readonly EventHandler HBeaconScan;

        public Event Event { get; private set; }
        public List<BeaconSignalModel> beaconSignalBuffer = new List<BeaconSignalModel>();

        public WaypointClient()
        {
            Event = new Event();

            HBeaconScan = new EventHandler(HandleBeaconScan);
            Utility.BeaconScan.Event.BeaconScanEventHandler += HBeaconScan;
            _beaconList = null;
            _waypointList = null;
        }

        public void SetWaypointList(List<Waypoint> WaypointList)
        {
            if (WaypointList != null)
            {
                this._beaconList = WaypointList;
                this._waypointList = WaypointList;
                foreach (Waypoint monitorWaypoing in WaypointList) {
                    _beaconList.Add(new Waypoint { ID = monitorWaypoing.ID } );
                }
            }
            else
                throw new ArgumentException("Parameter cannot be null", "WaypointList");
        }

        public void SignalProcessing()
        {
            // Remove the obsolete data from buffer
            List<BeaconSignalModel> removeSignalBuffer =
                new List<BeaconSignalModel>();

            lock (bufferLock)
            {
                removeSignalBuffer.AddRange(
                    beaconSignalBuffer.Where(c =>
                    c.Timestamp < DateTime.Now.AddMilliseconds(-1000)));

                foreach (var obsoleteBeaconSignal in removeSignalBuffer)
                    beaconSignalBuffer.Remove(obsoleteBeaconSignal);

                foreach (BeaconSignalModel beacon in beaconSignalBuffer) {
                    Waypoint tempWaypoint = new Waypoint { ID = beacon.UUID };
                    if (_beaconList.Contains(tempWaypoint) && beacon.RSSI > -50) {
                        Event.OnEventCall(new WayPointSignalEventArgs{
                            CurrentWaypoint = tempWaypoint
                        }); ;
                    }
                }
            }
        }

        private void HandleBeaconScan(object sender, EventArgs e)
        {
            // Beacon signal filter, keeps the Beacon's signal recorded in
            // the graph
            IEnumerable<BeaconSignalModel> signals =
                (e as BeaconScanEventArgs).Signals
                .Where(signal => Utility.BeaconsDict.Values
                .Select(beacon => (beacon.UUID, beacon.Major, beacon.Minor))
                .Contains((signal.UUID, signal.Major, signal.Minor)));

            lock (bufferLock)
                beaconSignalBuffer.AddRange(signals);

        }

        public void Stop()
        {
            _beaconList = null;
            Utility.BeaconScan.Event.BeaconScanEventHandler -= HBeaconScan;
            beaconSignalBuffer = null;
            bufferLock = null;
        }

    }

    public class Event
    {
        public EventHandler EventHandler;

        public void OnEventCall(EventArgs e)
        {
            EventHandler?.Invoke(this, e);
        }
    }

    public class WayPointSignalEventArgs : EventArgs
    {
        public Waypoint CurrentWaypoint { get; set; }
    }

}
