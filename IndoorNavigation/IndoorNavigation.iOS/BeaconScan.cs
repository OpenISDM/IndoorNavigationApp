using System;
using CoreBluetooth;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Linq;
using Foundation;
using IndoorNavigation.Models;
using System.Collections.Generic;
using IndoorNavigation.iOS;
using System.IO;

//namespace Bluetooth

[assembly: Xamarin.Forms.Dependency(typeof(BeaconScan))]
namespace IndoorNavigation.iOS
{
    public class BeaconScan : IBeaconScan
    {
        private readonly CBCentralManager manager = new CBCentralManager();

        public EventHandler DiscoveredDevice;
        public EventHandler StateChanged;

        public BeaconScanEvent Event { get; private set; }

        public void StartScan(List<Guid> BeaconsUUID) {
            Console.WriteLine("Scanning started: CBCentralManager state = " + this.manager.State);
            
            if (CBCentralManagerState.PoweredOn == this.manager.State)
            {
                /*
                var uuids = string.IsNullOrEmpty(serviceUuid)
                    ? new CBUUID[0]
                    : new[] { CBUUID.FromString(serviceUuid) };
                    */
                var uuids = new CBUUID[0];
                PeripheralScanningOptions options = new PeripheralScanningOptions();
                options.AllowDuplicatesKey = true;

                this.manager.ScanForPeripherals(uuids, options);
                

                //await Task.Delay(scanDuration);
                //this.StopScan();
            }
        }
        public void Close() {
        }

        public BeaconScan()
        {
            Event = new BeaconScanEvent();
            this.manager.DiscoveredPeripheral += this.DiscoveredPeripheral;
            this.manager.UpdatedState += this.UpdatedState;
            Console.WriteLine("In BeaconScan constructor: CBCentralManager stata =" + this.manager.State);
        }

        public void Dispose()
        {
            this.manager.DiscoveredPeripheral -= this.DiscoveredPeripheral;
            this.manager.UpdatedState -= this.UpdatedState;
        }

        public void StopScan()
        {
            this.manager.StopScan();
            Console.WriteLine("Scanning stopped");
        }

        private void DiscoveredPeripheral(object sender, CBDiscoveredPeripheralEventArgs args)
        {
            /* var device = $"{args.Peripheral.Name} - {args.Peripheral.Identifier?.Description}";
             Debug.WriteLine($"Discovered {device}");
             this.DiscoveredDevice?.Invoke(sender, args.Peripheral);
             */
            if ((args as CBDiscoveredPeripheralEventArgs).RSSI.Int32Value > -50)
            {
                string bufferUUID = " ";
                string identifierUUID = "";
         //       Console.WriteLine("detected " + (args as CBDiscoveredPeripheralEventArgs).Peripheral.Identifier + " Services = " + (args as CBDiscoveredPeripheralEventArgs).Peripheral.Services + " rssi = " + (args as CBDiscoveredPeripheralEventArgs).RSSI);
         //       Console.WriteLine("UUID is" + (args as CBDiscoveredPeripheralEventArgs).AdvertisementData.ValueForKey((NSString)"kCBAdvDataManufacturerData"));
                var tempUUID = (args as CBDiscoveredPeripheralEventArgs).AdvertisementData.ValueForKey((NSString)"kCBAdvDataManufacturerData");
                //Console.WriteLine("UUUUUUUU " + tempUUID);
                if (tempUUID != null)
                {
                    bufferUUID = tempUUID.ToString();
                    identifierUUID = readfile(bufferUUID);

                    //Console.WriteLine("WWWWWWW" + bufferUUID);
                    if (identifierUUID.Length >= 36)
                    {
                        List<BeaconSignalModel> signals = new List<BeaconSignalModel>();

                        signals.Add(new BeaconSignalModel
                        {
                            UUID = new Guid(identifierUUID),
                            RSSI = (args as CBDiscoveredPeripheralEventArgs).RSSI.Int32Value
                        });

                        Event.OnEventCall(new BeaconScanEventArgs
                        {
                            Signals = signals
                        });
                    }
                }

            }   
            
        }

        private void UpdatedState(object sender, EventArgs args)
        {
            /*
            Debug.WriteLine($"State = {this.manager.State}");
            this.StateChanged?.Invoke(sender, this.manager.State);
            */
        }
        private string readfile(string parseUUID)
        {
            ////string[] stringSeparators = new string[] { "\r\n" };
            //string[] parser = parseUUID.Split(stringSeparators, StringSplitOptions.None);
            ///var parser = parseUUID.Split(Environment.NewLine.ToString());

           
            string[] parse = parseUUID.Split(" ");
        /*
            for(int i = 0;i<parse.Count();i++)
            {
                Console.WriteLine("parser test " + parse[i]);
            }
            Console.WriteLine("range : " + parse.Count());
        */
            if(parse.Count()<6)
            {
                return parseUUID;
            }
            else
            {
                var parser = parse[1] + "-"+ parse[2].Substring(0,4)+"-"+parse[2].Substring(4,4)+"-"
                     + parse[3].Substring(0,4) + "-" + parse[3].Substring(4,4) + parse[4];
                Console.WriteLine("result" + parser);
                return parser.ToString();
            }


        }
    }
}

    /*
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
            for (int i = 0; i < BeaconsUUID.Count; i++) {
                Console.WriteLine("in BeaconScan's StartScan : " + BeaconsUUID[i]);
            }

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
            Console.WriteLine(">> HandleDidRangeBeacons e.Beacons.Length = " + e.Beacons.Length);
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
                Console.WriteLine("after raising event in HandleDigRangeBeacons");
            }
        }

        public void Close()
        {
            StopScan();
            locationManager.Dispose();
        }
    }
}*/