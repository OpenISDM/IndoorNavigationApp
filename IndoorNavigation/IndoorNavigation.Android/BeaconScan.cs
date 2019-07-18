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
 *      such as the interface of IPSClient and the interface for the 
 *      Android project to allow the Xamarin.Forms app to access the APIs. 
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
 *      Wang-Chi Ho, h.wangchi.0970@gmail.com
 *
 */
using IndoorNavigation.Droid;
using IndoorNavigation.Models;
using Plugin.BLE;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using Plugin.BLE.Abstractions.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

[assembly: Xamarin.Forms.Dependency(typeof(BeaconScan))]
namespace IndoorNavigation.Droid
{
    public class BeaconScan : IBeaconScan
    {
        IBluetoothLE _bluetooth = CrossBluetoothLE.Current;
        IAdapter _bleAdapter = CrossBluetoothLE.Current.Adapter;

        private int _rssiThreshold = -50;

        public NavigationEvent _event { get; private set; }

        public BeaconScan()
        {
            _event = new NavigationEvent();
            this._bleAdapter.DeviceDiscovered += this.DeviceDiscovered;

            Console.WriteLine("In BeaconScan constructor: IAdapter state = " +
                              this._bluetooth.State);
            this._bleAdapter.ScanTimeout = 1500;
        }

        public void Close()
        {
        }

        private void DeviceDiscovered(object sender, DeviceEventArgs args)
        {
            Console.WriteLine("Find A Beacon Id:{0}; RSSI:{1}", (args as DeviceEventArgs).Device.Id, (args as DeviceEventArgs).Device.Rssi);
            if ((args as DeviceEventArgs).Device.Rssi > _rssiThreshold &&
                (args as DeviceEventArgs).Device.Rssi < 0)
            {
                var tempUUID = (string)null;
                foreach (AdvertisementRecord record in (args as DeviceEventArgs).Device.AdvertisementRecords)
                {
                    if (record.Type == AdvertisementRecordType.ManufacturerSpecificData)
                    {
                        /* Sample of ManufacturerSpecificData data:
                         * Ade = {
                         *    [
                         *      Type ManufacturerSpecificData; 
                         *      Data 0F-00-02-15-00-00-00-18-00-00-00-00-66-60-00-00-00-01-19-00-00-02-00-00-CE
                         *    ]
                         *  }
                         */
                        tempUUID = string.Format("{0}", record.Data.ToHexString());
                        break;
                    }
                }

                if (tempUUID != null)
                {
                    string bufferUUID = tempUUID.ToString();
                    string identifierUUID = ExtractBeaconUUID(bufferUUID);
                    Console.WriteLine("Find Beacon Id:{0}; RSSI:{1}", identifierUUID, (args as DeviceEventArgs).Device.Rssi);

                    if (identifierUUID.Length >= 36)
                    {
                        List<BeaconSignalModel> signals = new List<BeaconSignalModel>();
                        signals.Add(new BeaconSignalModel
                        {
                            UUID = new Guid(identifierUUID),
                            RSSI = (args as DeviceEventArgs).Device.Rssi
                        });

                        _event.OnEventCall(new BeaconScanEventArgs
                        {
                            _signals = signals
                        });
                    }

                }
            }

        }

        public void Dispose()
        {
            Console.WriteLine(">> Beacon Scan Dispose");
            this._bleAdapter.DeviceDiscovered -= this.DeviceDiscovered;
        }

        private string ExtractBeaconUUID(string stringAdvertisementSpecificData)
        {
            string[] parse = stringAdvertisementSpecificData.Split("-");

            if (parse.Count() < 20)
            {
                return stringAdvertisementSpecificData;
            }
            else
            {
                var parser = string.Format("{0}{1}{2}{3}-{4}{5}-{6}{7}-{8}{9}-{10}{11}{12}{13}{14}{15}",
                                            parse[4], parse[5], parse[6], parse[7],
                                            parse[8], parse[9],
                                            parse[10], parse[11],
                                            parse[12], parse[13],
                                            parse[14], parse[15], parse[16], parse[17], parse[18], parse[19]);
                return parser.ToString();
            }
        }

        public void StartScan()
        {
            if (BluetoothState.On == this._bluetooth.State)
            {
                Console.WriteLine(">> Beacon Scan Start");
                Guid[] uuid = { };
                this._bleAdapter.StartScanningForDevicesAsync(uuid, null, true, default);
            }
        }

        public void StopScan()
        {
            Console.WriteLine(">> Beacon Scan Stop");
            this._bleAdapter.StopScanningForDevicesAsync();
        }

        private void UpdatedState(object sender, EventArgs args)
        {

        }

    }
}