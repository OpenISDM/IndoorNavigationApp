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
 *      1.0.0, 20190605
 * 
 * File Name:
 *
 *      BeaconScan.cs
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
 *      Chun-Yu Lai, chunyu1202@gmail.com
 *
 */
using System;
using CoreBluetooth;
using System.Linq;
using Foundation;
using IndoorNavigation.Models;
using System.Collections.Generic;
using IndoorNavigation.iOS;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

[assembly: Xamarin.Forms.Dependency(typeof(BeaconScan))]
namespace IndoorNavigation.iOS
{
    public class BeaconScan : LBeaconScan
    {
        private readonly CBCentralManager _manager = new CBCentralManager();

        private int _rssiThreshold = -100;

        public NavigationEvent _event { get; private set; }

        public BeaconScan()
        {


            _event = new NavigationEvent();
            this._manager.DiscoveredPeripheral += this.DiscoveredPeripheral;
            this._manager.UpdatedState += this.UpdatedState;
            Console.WriteLine("In BeaconScan constructor: CBCentralManager stata =" +
                              this._manager.State);
        }

        public void StartScan()
        {
            Console.WriteLine("Start LBeacon");
            //_rssiThreshold = rssiOption;
            if (CBCentralManagerState.PoweredOn == this._manager.State)
            {
                var uuids = new CBUUID[0];
                PeripheralScanningOptions options = new PeripheralScanningOptions();
                options.AllowDuplicatesKey = true;

                this._manager.ScanForPeripherals(uuids, options);
            }
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

        private void DiscoveredPeripheral(object sender, CBDiscoveredPeripheralEventArgs args)
        {
            if ((args as CBDiscoveredPeripheralEventArgs).RSSI.Int32Value > _rssiThreshold &&
                (args as CBDiscoveredPeripheralEventArgs).RSSI.Int32Value < 0)
            {
                Console.WriteLine("Check UUID : " + (args as CBDiscoveredPeripheralEventArgs).AdvertisementData);
                //var data = Data(bytes: manufacturerData.bytes, count: Int(manufacturerData.length))
         
                var tempUUID = (args as CBDiscoveredPeripheralEventArgs).AdvertisementData
                               .ValueForKey((NSString)"kCBAdvDataManufacturerData");
           
          
                



                


                if (tempUUID != null)
                {
                    var arr = (NSData)(args as CBDiscoveredPeripheralEventArgs).AdvertisementData.ObjectForKey((NSString)"kCBAdvDataManufacturerData");
                    byte[] result = new byte[arr.Length];
                    Marshal.Copy(arr.Bytes, result, 0, (int)arr.Length);
                    var token = BitConverter.ToString(result).Replace("-", "");
                    
                    string bufferUUID = token.ToString();
                    string identifierUUID = ExtractBeaconUUID(bufferUUID);
                    
                    if (identifierUUID.Length == 36&&Guid.TryParse(identifierUUID,out Guid guid)==true)
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

        private string ExtractBeaconUUID(string stringAdvertisementSpecificData)
        {
            if(stringAdvertisementSpecificData.Length==50)
            {
                var parser = stringAdvertisementSpecificData.Substring(8, 8) + "-" +
                    stringAdvertisementSpecificData.Substring(16, 4) + "-" +
                    stringAdvertisementSpecificData.Substring(20, 4) + "-" +
                    stringAdvertisementSpecificData.Substring(24, 4) + "-" +
                    stringAdvertisementSpecificData.Substring(28, 12);
                return parser;
            }
            else
            {
                return stringAdvertisementSpecificData;
            }
        }
    }
}
