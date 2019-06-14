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
        private List<Waypoint> _beaconList = new List<Waypoint>();
        private List<Waypoint> _waypointList = new List<Waypoint>();

        private object bufferLock = new object();
        private readonly EventHandler HBeaconScan;

        public Event Event { get; private set; }
        public List<BeaconSignalModel> beaconSignalBuffer = new List<BeaconSignalModel>();

        public WaypointClient()
        {
            Event = new Event();

            HBeaconScan = new EventHandler(HandleBeaconScan);
            Utility.BeaconScan.Event.BeaconScanEventHandler += HBeaconScan;
            _beaconList = new List<Waypoint>();
            _waypointList = new List<Waypoint>();
        }

        public void SetWaypointList(List<Waypoint> WaypointList)
        {
            this._beaconList = new List<Waypoint>(); ;
            this._waypointList = WaypointList;
                
            for(int i = 0;i<WaypointList.Count;i++)
            {
                _beaconList.Add(new Waypoint { ID = WaypointList[i].ID });
            }

            List<Guid> tempBeaconGuid = new List<Guid>();
            for (int i = 0; i < WaypointList.Count; i++)
            {
                tempBeaconGuid.Add(WaypointList[i].ID);
            }
            Utility.BeaconScan.StopScan();
            Utility.BeaconScan.StartScan(tempBeaconGuid);

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

                // for testing
                /*
                List<BeaconSignalModel> signals = new List<BeaconSignalModel>();
                signals.Add(new BeaconSignalModel { UUID = Guid.Parse("00000018-0000-0000-3060-000000010700"), Major = 1, Minor = 0, RSSI = -30 });
                beaconSignalBuffer = signals;
                */

                foreach (BeaconSignalModel beacon in beaconSignalBuffer) {
                    Waypoint tempWaypoint = new Waypoint { ID = beacon.UUID };
                    // for testing
                    /*
                    Console.WriteLine("Detected waypoint: [" + tempWaypoint.ID + "] rssi=" + beacon.RSSI);
                    Event.OnEventCall(new WayPointSignalEventArgs
                    {
                        CurrentWaypoint = tempWaypoint
                    }); 
                    //
                    */
                    Console.WriteLine("Detected waypoint: [" + tempWaypoint.ID + "] rssi=" + beacon.RSSI);
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
