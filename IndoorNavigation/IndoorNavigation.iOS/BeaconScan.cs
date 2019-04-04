using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CoreLocation;
using Foundation;
using IndoorNavigation.iOS;
using IndoorNavigation.Models;
using UIKit;

[assembly: Xamarin.Forms.Dependency(typeof(BeaconScan))]
namespace IndoorNavigation.iOS
{
    class BeaconScan : IBeaconScan
    {
        protected static CLLocationManager locationManager;
        private static List<CLBeaconRegion> beaconsRegion;
        public BeaconScanEvent Event { get; private set; }

        public BeaconScan()
        {
            Event = new BeaconScanEvent();
            locationManager = new CLLocationManager();
            // iOS 8.0以上需要開啟定位權限
            if (UIDevice.CurrentDevice.CheckSystemVersion(8, 0))
            {
                locationManager.RequestWhenInUseAuthorization();
            }

            locationManager.DidRangeBeacons += HandleDidRangeBeacons;
        }

        public void StartScan(List<Guid> BeaconsUUID)
        {
            // 把要監聽的Beacon uuid轉換成系統API需要的物件
            beaconsRegion = new List<CLBeaconRegion>();
            var UUIDObjects = 
                BeaconsUUID.Select(c => new NSUuid(c.ToString()));
            beaconsRegion.AddRange(
                UUIDObjects.Select(c => new CLBeaconRegion(c, c.AsString())));

            // 開始監聽beacon廣播
            foreach (CLBeaconRegion beaconRegion in beaconsRegion)
                locationManager.StartRangingBeacons(beaconRegion);
        }

        public void StopScan()
        {
            // 停止監聽所有beacon廣播
            if (beaconsRegion != null)
                foreach (CLBeaconRegion beaconRegion in beaconsRegion)
                    locationManager.StopRangingBeacons(beaconRegion);
        }

        private void HandleDidRangeBeacons(object sender, 
            CLRegionBeaconsRangedEventArgs e)
        {
            if (e.Beacons.Length != 0)
            {
                // 發送Beacon訊號強度和其它資訊到訊號分析模組
                List<BeaconSignalModel> signals = e.Beacons.Select(c => 
                    new BeaconSignalModel {
                        UUID = Guid.Parse(c.ProximityUuid.AsString()),
                        Major = c.Major.Int32Value,
                        Minor = c.Minor.Int32Value,
                        RSSI = (int)c.Rssi,
                    }).ToList();

                Event.OnEventCall(new BeaconScanEventArgs
                {
                    Signals = signals
                });
            }
        }

        public void Close()
        {
            StopScan();
            locationManager.Dispose();
        }
    }
}