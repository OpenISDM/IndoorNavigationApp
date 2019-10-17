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
 *      1.0.0, 20190822
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
 *      Wang-Chi Ho, h.wangchi.0970@gmail.com
 *
 */
using Android.Bluetooth;
using IndoorNavigation.Droid;
using IndoorNavigation.Models;
using System;
using System.Collections.Generic;
using System.Linq;

[assembly: Xamarin.Forms.Dependency(typeof(BeaconScanForIBeacon))]
namespace IndoorNavigation.Droid
{
    public class BeaconScanForIBeacon : Java.Lang.Object, BluetoothAdapter.ILeScanCallback, IBeaconScan
    {
        protected BluetoothAdapter _adapter;
        protected BluetoothManager _manager;
        private int _count = 0;
        private int _rssiThreshold = -80;

        public NavigationEvent _event { get; private set; }

        public BeaconScanForIBeacon()
        {
            _event = new NavigationEvent();
            var appContext = Android.App.Application.Context;
            this._manager = (BluetoothManager)appContext.GetSystemService("bluetooth");
            this._adapter = this._manager.Adapter;
        }

        public void StartScan()
        {
            if (!this._adapter.IsEnabled)
            {
                _adapter.Enable();
            }
            //_rssiThreshold = rssiOption;
            this._count = 0;
            this._adapter.StartLeScan(this);
        }

        public void StopScan()
        {
            this._adapter.StopLeScan(this);
        }

        public void OnLeScan(BluetoothDevice bleDevice, int rssi, byte[] scanRecord)
        {
            this._count = this._count + 1;
            if (rssi > _rssiThreshold && rssi < 0)
            {
                string tempUUID = BitConverter.ToString(scanRecord);
                string identifierUUID = ExtractBeaconUUID(tempUUID);
                Console.WriteLine("\n >> Find A Beacon[{0}] Name:{1}; Address:{2}; RSSI:{3}; Record:{4}\n", this._count, bleDevice, bleDevice.Address, rssi, identifierUUID);
                if (identifierUUID.Length >= 36)
                {
                    List<BeaconSignalModel> signals = new List<BeaconSignalModel>();
                    signals.Add(new BeaconSignalModel
                    {
                        UUID = new Guid(identifierUUID),
                        RSSI = rssi
                    });

                    _event.OnEventCall(new BeaconScanEventArgs
                    {
                        _signals = signals
                    });
                }
            }
        }

        public void Close()
        {
        }

        public void Dispose()
        {
            Console.WriteLine(">> Beacon Scan Dispose");
        }

        private void UpdatedState(object sender, EventArgs args)
        {
        }
       
        private string ExtractBeaconUUID(string stringAdvertisementSpecificData)
        {
            string[] parse = stringAdvertisementSpecificData.Split("-");

            if (parse.Count() < 60)
            {
                return stringAdvertisementSpecificData;
            }
            else
            {
                var parser = string.Format("00000000-0402-{0}{1}-0000-{2}{3}{4}{5}{6}{7}",
                                            parse[43], parse[42],
                                            parse[46], parse[47], parse[48], parse[49], parse[50], parse[51]);
                return parser.ToString();
            }
        }
    }
}