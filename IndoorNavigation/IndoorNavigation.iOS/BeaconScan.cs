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

        public NavigationEvent _Event { get; private set; }

        public BeaconScan()
        {
            _Event = new NavigationEvent();
            this.manager.DiscoveredPeripheral += this.DiscoveredPeripheral;
            this.manager.UpdatedState += this.UpdatedState;
            Console.WriteLine("In BeaconScan constructor: CBCentralManager stata =" + this.manager.State);
        }

        public void StartScan() {
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

        public void StopScan()
        {
            this.manager.StopScan();
            Console.WriteLine("Scanning stopped");
        }

        public void Close() {
        }

        public void Dispose()
        {
            this.manager.DiscoveredPeripheral -= this.DiscoveredPeripheral;
            this.manager.UpdatedState -= this.UpdatedState;
        }

        private void DiscoveredPeripheral(object sender, CBDiscoveredPeripheralEventArgs args)
        {
            /*
            Sample of AdvertisementData data:
            2019-06-17 13:31:52.209 IndoorNavigation.iOS[904:5335527] detected 7A8B3CF6-48C9-61FB-C100-9A6AFF29053D AdvertisementData = {
                kCBAdvDataIsConnectable = 0;
                kCBAdvDataManufacturerData = <0f000215 00000018 00000000 24600000 00002300 00020000 ce>;
            } rssi = -42
            */
            int rssiThreshold = -45;
            if ((args as CBDiscoveredPeripheralEventArgs).RSSI.Int32Value > rssiThreshold &&
                (args as CBDiscoveredPeripheralEventArgs).RSSI.Int32Value < 0)
            {
                string bufferUUID = " ";
                string identifierUUID = "";

                var tempUUID = (args as CBDiscoveredPeripheralEventArgs).AdvertisementData.ValueForKey((NSString)"kCBAdvDataManufacturerData");
                if (tempUUID != null)
                {
                    bufferUUID = tempUUID.ToString();
                    identifierUUID = extractBeaconUUID(bufferUUID);

                    if (identifierUUID.Length >= 36)
                    {
                        List<BeaconSignalModel> signals = new List<BeaconSignalModel>();

                        signals.Add(new BeaconSignalModel
                        {
                            UUID = new Guid(identifierUUID),
                            RSSI = (args as CBDiscoveredPeripheralEventArgs).RSSI.Int32Value
                        });

                        _Event.OnEventCall(new BeaconScanEventArgs
                        {
                            Signals = signals
                        });
                    }
                }

            }   
            
        }

        private void UpdatedState(object sender, EventArgs args)
        {
           
        }

        private string extractBeaconUUID(string stringAdvertisementSpecificData)
        {
            string[] parse = stringAdvertisementSpecificData.Split(" ");
        
            if(parse.Count()<6)
            {
                return stringAdvertisementSpecificData;
            }
            else
            {
                var parser = parse[1] + "-" +
                             parse[2].Substring(0,4) + "-" +
                             parse[2].Substring(4,4) + "-" +
                             parse[3].Substring(0,4) + "-" +
                             parse[3].Substring(4,4) + parse[4];
                return parser.ToString();
            }
        }
    }
}
