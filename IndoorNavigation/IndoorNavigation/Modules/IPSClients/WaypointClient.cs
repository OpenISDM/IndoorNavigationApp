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
 *      1.0.0, 20190719
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
 *      Bo-Chen Huang, m10717004@yuntech.edu.tw
 *      Chun-Yu Lai, chunyu1202@gmail.com
 *      Eric Lee, ericlee@iis.sinica.edu.tw
 *
 */
using System;
using System.Linq;
using System.Collections.Generic;
using IndoorNavigation.Models;
using Xamarin.Forms;

namespace IndoorNavigation.Modules.IPSClients
{
    class WaypointClient : IIPSClient
    {
        private List<WaypointBeaconsMapping> _waypointBeaconsList = new List<WaypointBeaconsMapping>();

        private object _bufferLock = new object();
        private readonly EventHandler _beaconScanEventHandler;

        public NavigationEvent _event { get; private set; }
        private List<BeaconSignalModel> _beaconSignalBuffer = new List<BeaconSignalModel>();


        public WaypointClient()
        {
            _event = new NavigationEvent();
            Utility._lbeaconScan = DependencyService.Get<LBeaconScan>();
            _beaconScanEventHandler = new EventHandler(HandleBeaconScan);
            Utility._lbeaconScan._event._eventHandler += _beaconScanEventHandler;
            _waypointBeaconsList = new List<WaypointBeaconsMapping>();

        }

        public void SetWaypointList(List<WaypointBeaconsMapping> waypointBeaconsList)
        {
            int rssiOption = -40;

            if (Application.Current.Properties.ContainsKey("StrongRssi"))
            {
                if ((bool)Application.Current.Properties["StrongRssi"] == true)
                {
                    rssiOption = -70;
                }
                else if ((bool)Application.Current.Properties["WeakRssi"] == true)
                {
                    rssiOption = -40;
                }
                else if ((bool)Application.Current.Properties["MediumRssi"] == true)
                {
                    rssiOption = -55;
                }
            }

            this._waypointBeaconsList = waypointBeaconsList;
            Utility._lbeaconScan.StartScan(rssiOption);
        }

        public void DetectWaypoints()
        {
            Console.WriteLine(">> In DetectWaypoints");
            //Utility._beaconScan.StartScan();
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

                //BeaconSignalModel beaconSignalModel = new BeaconSignalModel();
                //beaconSignalModel.UUID = new Guid("00000016-0000-0001-9500-000000014500");
                //_beaconSignalBuffer.Add(beaconSignalModel);

                foreach (BeaconSignalModel beacon in _beaconSignalBuffer)
                {
                    foreach (WaypointBeaconsMapping waypointBeaconsMapping in _waypointBeaconsList)
                    {
                        foreach (Guid beaconGuid in waypointBeaconsMapping._Beacons)
                        {
                            if (beacon.UUID.Equals(beaconGuid))
                            {
                                Console.WriteLine("Matched waypoint: {0} by detected Beacon {1}",
                                waypointBeaconsMapping._WaypointIDAndRegionID._waypointID,
                                beaconGuid);
                                _event.OnEventCall(new WaypointSignalEventArgs
                                {
                                    _detectedRegionWaypoint = waypointBeaconsMapping._WaypointIDAndRegionID
                                });
                                return;
                            }
                        }
                    }
                }
            }

            Console.WriteLine("<< In DetectWaypoints");
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
            Utility._lbeaconScan.StopScan();
            _beaconSignalBuffer.Clear();
            _waypointBeaconsList.Clear();
            Utility._lbeaconScan._event._eventHandler -= _beaconScanEventHandler;
        }
    }

    public class WaypointSignalEventArgs : EventArgs
    {
        public RegionWaypointPoint _detectedRegionWaypoint { get; set; }
    }

}
