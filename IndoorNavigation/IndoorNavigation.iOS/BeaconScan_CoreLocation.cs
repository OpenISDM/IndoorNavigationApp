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
 *
 *      
 * Version:
 *
 *      1.0.0, 20190605
 * 
 * File Name:
 *
 *      BeaconScan.cs
 *
 * Abstract:
 *
 *      This file used to read the beacon.
 *      We now have two ways to read beacon, one is CoreBluetooth and another is CoreLocation.
 *      However, they both have their own pros and cons. The advantage of core location is it can read IBeacon but its
 *      drawback is it can just read 1 time/s and cannot adjust, in addition, it needs to keep registering and cut the register. Or
 *      it needs to get all the interested guid first. So the effectively is really bad.
 *
 * Authors:
 *
 *      Eric Lee, ericlee@iis.sinica.edu.tw
 *
 */
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CoreLocation;
using Foundation;
using IndoorNavigation.iOS;
using IndoorNavigation.Models;
using UIKit;
using Xamarin.Forms;
using CoreBluetooth;

[assembly: Xamarin.Forms.Dependency(typeof(BeaconScan))]
namespace IndoorNavigation.iOS
{
    public class CoreLocationBeaconScan : LBeaconScan
    {
        protected CLLocationManager _locationManager;
        private List<CLBeaconRegion> beaconsRegion;


        //private readonly CBCentralManager _manager = new CBCentralManager();

        private int _rssiThreshold = -40;

        public NavigationEvent _event { get; private set; }

        public CoreLocationBeaconScan()
        {
            _locationManager = new CLLocationManager();

            _event = new NavigationEvent();

            if (UIDevice.CurrentDevice.CheckSystemVersion(8, 0))
            {
                _locationManager.RequestWhenInUseAuthorization();
            }
            _locationManager.DidRangeBeacons += DiscoveredPeripheral;
        }

        public void StartScan()
        {
            Console.WriteLine("Start LBeacon");
            List<Guid> beaconUUID = new List<Guid>();

            beaconUUID.Add(new Guid("FDA50693-A4E2-4FB1-AFCF-C6EB07640003"));
            beaconUUID.Add(new Guid("FDA50693-A4E2-4FB1-AFCF-C6EB07640004"));
            beaconUUID.Add(new Guid("FDA50693-A4E2-4FB1-AFCF-C6EB07640005"));
            beaconUUID.Add(new Guid("FDA50693-A4E2-4FB1-AFCF-C6EB07640006"));

            beaconUUID.Add(new Guid("FDA50693-A4E2-4FB1-AFCF-C6EB07640007"));
            beaconUUID.Add(new Guid("FDA50693-A4E2-4FB1-AFCF-C6EB07640008"));
            beaconUUID.Add(new Guid("FDA50693-A4E2-4FB1-AFCF-C6EB07640001"));
            beaconUUID.Add(new Guid("FDA50693-A4E2-4FB1-AFCF-C6EB07640002"));
            beaconUUID.Add(new Guid("FDA50693-A4E2-4FB1-AFCF-C6EB07640009"));
            beaconUUID.Add(new Guid("FDA50693-A4E2-4FB1-AFCF-C6EB07640010"));
            beaconUUID.Add(new Guid("FDA50693-A4E2-4FB1-AFCF-C6EB07640011"));
            beaconUUID.Add(new Guid("FDA50693-A4E2-4FB1-AFCF-C6EB07640012"));
            beaconUUID.Add(new Guid("FDA50693-A4E2-4FB1-AFCF-C6EB07640013"));
            beaconUUID.Add(new Guid("FDA50693-A4E2-4FB1-AFCF-C6EB07640014"));
            beaconUUID.Add(new Guid("FDA50693-A4E2-4FB1-AFCF-C6EB07640015"));
            beaconUUID.Add(new Guid("FDA50693-A4E2-4FB1-AFCF-C6EB07640016"));
            beaconUUID.Add(new Guid("FDA50693-A4E2-4FB1-AFCF-C6EB07640017"));
            beaconUUID.Add(new Guid("FDA50693-A4E2-4FB1-AFCF-C6EB07640018"));
            beaconUUID.Add(new Guid("FDA50693-A4E2-4FB1-AFCF-C6EB07640019"));
            beaconUUID.Add(new Guid("FDA50693-A4E2-4FB1-AFCF-C6EB07640020"));
            beaconUUID.Add(new Guid("FDA50693-A4E2-4FB1-AFCF-C6EB07640021"));
            beaconUUID.Add(new Guid("FDA50693-A4E2-4FB1-AFCF-C6EB07640022"));
            beaconUUID.Add(new Guid("FDA50693-A4E2-4FB1-AFCF-C6EB07640023"));
            beaconUUID.Add(new Guid("FDA50693-A4E2-4FB1-AFCF-C6EB07640024"));
            beaconUUID.Add(new Guid("FDA50693-A4E2-4FB1-AFCF-C6EB07640025"));
            beaconUUID.Add(new Guid("FDA50693-A4E2-4FB1-AFCF-C6EB07640026"));
            beaconsRegion = new List<CLBeaconRegion>();
            var UUIDObjects =
                beaconUUID.Select(c => new NSUuid(c.ToString()));
            beaconsRegion.AddRange(
                UUIDObjects.Select(c => new CLBeaconRegion(c, c.AsString())));

    
            foreach (CLBeaconRegion beaconRegion in beaconsRegion)
                _locationManager.StartRangingBeacons(beaconRegion);
        }

        public void StopScan()
        {
            if (beaconsRegion != null)
                foreach (CLBeaconRegion beaconRegion in beaconsRegion)
                    _locationManager.StopRangingBeacons(beaconRegion);
        }

        public void Close() {
         
        }

        public void Dispose()
        {
            Console.WriteLine("In Dispose");
            //this._manager.DiscoveredPeripheral -= this.DiscoveredPeripheral;
            //this._manager.UpdatedState -= this.UpdatedState;
        }

        private void DiscoveredPeripheral(object sender, CLRegionBeaconsRangedEventArgs args)
        {
            if (args.Beacons.Length != 0)
            {
                
                List<BeaconSignalModel> signals = args.Beacons.Select(c =>
                    new BeaconSignalModel
                    {
                        UUID = Guid.Parse(c.ProximityUuid.AsString()),
                        Major = c.Major.Int32Value,
                        Minor = c.Minor.Int32Value,
                        RSSI = (int)c.Rssi,
                    }).ToList();
                int i = 0;


                _event.OnEventCall(new BeaconScanEventArgs
                {
                    _signals = signals
                });
            }
        }
    }
}
