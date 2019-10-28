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
 *      1.0.0, 20190719
 * 
 * File Name:
 *
 *      BeaconScanForIBeacon.cs
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
 *      Eric Lee, ericlee@iis.sinica.edu.tw
 *
 */

using System;
using CoreBluetooth;
using System.Linq;
using Foundation;
using IndoorNavigation.Models;
using System.Collections.Generic;
using IndoorNavigation.iOS;

[assembly: Xamarin.Forms.Dependency(typeof(BeaconScanForIBeacon))]
namespace IndoorNavigation.iOS
{
    public class BeaconScanForIBeacon : IBeaconScan
    {
        private readonly CBCentralManager _manager = new CBCentralManager();

        private int _rssiThreshold = -100;

        public NavigationEvent _event { get; private set; }
        public BeaconScanForIBeacon()
        {
            _event = new NavigationEvent();
            this._manager.DiscoveredPeripheral += this.DiscoveredPeripheral;
            this._manager.UpdatedState += this.UpdatedState;
            Console.WriteLine("In New BeaconScan constructor: CBCentralManager stata =" +
                              this._manager.State);
        }

        public void StartScan()
        {
            Console.WriteLine("Start Ibeacon ");
            if (CBCentralManagerState.PoweredOn == this._manager.State)
            {
                var uuids = new CBUUID[0];
                PeripheralScanningOptions options = new PeripheralScanningOptions();
                options.AllowDuplicatesKey = true;

                this._manager.ScanForPeripherals(uuids, options);
            }
        }

        private void DiscoveredPeripheral(object sender, CBDiscoveredPeripheralEventArgs args)
        {
            if ((args as CBDiscoveredPeripheralEventArgs).RSSI.Int32Value > _rssiThreshold &&
                (args as CBDiscoveredPeripheralEventArgs).RSSI.Int32Value < 0)
            {
                var tempUUID = (args as CBDiscoveredPeripheralEventArgs).AdvertisementData
                               .ValueForKey((NSString)"kCBAdvDataServiceData");
               
                if (tempUUID != null)
                {
                    string bufferUUID = tempUUID.ToString();
                    string identifierUUID = ExtractBeaconUUID(bufferUUID);
                    if (identifierUUID.Length == 36 && Guid.TryParse(identifierUUID, out Guid guid) == true)
                    {
                        List<BeaconSignalModel> signals = new List<BeaconSignalModel>();

                        signals.Add(new BeaconSignalModel
                        {
                            UUID = new Guid(identifierUUID),
                            RSSI = (args as CBDiscoveredPeripheralEventArgs).RSSI.Int32Value
                        });

                        _event.OnEventCall(new BeaconScanEventArgs
                        {
                            _signals = signals
                        });
                    }
                }
            }
        }

        private void UpdatedState(object sender, EventArgs args)
        {

        }

        public void StopScan()
        {
            this._manager.StopScan();
        }

        public void Close()
        {
        }

        public void Dispose()
        {
            Console.WriteLine("In Dispose");
            this._manager.DiscoveredPeripheral -= this.DiscoveredPeripheral;
            this._manager.UpdatedState -= this.UpdatedState;
        }

        private string ExtractBeaconUUID(string stringAdvertisementSpecificData)
        {
            if(stringAdvertisementSpecificData.Length==35||stringAdvertisementSpecificData.Length==56)
            {
                string[] parse = stringAdvertisementSpecificData.Split(" ");
                Console.WriteLine("In IBeacon Parser");
                for (int i = 0; i < parse.Count(); i++)
                {
                    Console.WriteLine("Parse : " + parse[i]);
                }
                if (parse.Count() < 8)
                {
                    return stringAdvertisementSpecificData;
                }
                else if(parse[6].Substring(1, 6) == "length")
                {
                    var parser = "00000000" + "-" +
                                 "0402" + "-" + parse[4] + "-" +
                                  "0000" + "-" +
                                  parse[11].Substring(6, 12);
                    Console.WriteLine("parser : " + parser);
                    return parser.ToString();
                }
                else
                {
                    var parser = "00000000" + "-" +
                                 "0402" + "-" + parse[4] + "-" +
                                  "0000" + "-" +
                                  parse[6].Substring(5, 4) +
                                  parse[7].Substring(0, 8);
                    Console.WriteLine("parser : " + parser);
                    return parser.ToString();
                }
            }
            else
            {
                return "";
            }
            
        }

    }
}

