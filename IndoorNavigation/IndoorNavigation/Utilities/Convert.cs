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
 *      This class provides conversion methods for some modules
 *      
 * Version:
 *
 *      1.0.0-beta.1, 20190319
 * 
 * File Name:
 *
 *      Convert.cs
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
 *      
 */

using System;
using System.Collections.Generic;
using System.Linq;
using GeoCoordinatePortable;
using IndoorNavigation.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace IndoorNavigation
{
    /// <summary>
    /// Data conversion tool for indoor navigation
    /// </summary>
    public static partial class Convert
    {
        /// <summary>
        /// Expanded function
        /// Acquire the coordinates from the Beacon
        /// </summary>
        /// <param name="beacon"></param>
        /// <returns></returns>
        public static GeoCoordinate GetCoordinates(this
            Beacon beacon)
        {
            if (beacon.GetType() == typeof(LBeaconModel))
            {
                // Combine coordinate Hex data from UUID
                string[] idShards =
                    (beacon as LBeaconModel).UUID.ToString().Split('-');
                string latHexStr = idShards[2] + idShards[3];
                string lonHexStr = idShards[4].Substring(4, 8);

                // Convert coordinate hex data to coordinates
                float longitude = HexToFloat(lonHexStr);
                float latitude = HexToFloat(latHexStr);

                return new GeoCoordinate(latitude, longitude);
            }
            else if (beacon.GetType() == typeof(IBeaconModel))
            {
                return (beacon as IBeaconModel).IBeaconCoordinates;
            }

            throw new ArgumentException("Unrecognized Beacon type.");
        }

        /// <summary>
        /// Expanded function
        /// Acquire the floor from the LBeacon
        /// </summary>
        /// <param name="UUID"></param>
        /// <returns></returns>
        public static float GetFloor(this LBeaconModel LBeacon)
        {
            string[] idShards = LBeacon.UUID.ToString().Split('-');
            string floorHexStr = idShards[0];
            return HexToFloat(floorHexStr);
        }

        /// <summary>
        /// Convert hex string content to the float.
        /// e.g. 0xff20f342 -> 121.564445F
        /// </summary>
        /// <param name="Hex"></param>
        /// <returns></returns>
        private static float HexToFloat(string Hex)
        {
            // Hex string content to a byte array.
            byte[] Bytes = new byte[4];
            Bytes[0] = System.Convert.ToByte(Hex.Substring(0, 2), 16);
            Bytes[1] = System.Convert.ToByte(Hex.Substring(2, 2), 16);
            Bytes[2] = System.Convert.ToByte(Hex.Substring(4, 2), 16);
            Bytes[3] = System.Convert.ToByte(Hex.Substring(6, 2), 16);

            // byte array to a float.
            return BitConverter.ToSingle(Bytes, 0);
        }

        /// <summary>
        /// Expanded function
        /// Convert the navigation graph infomation(JSON file) to Beacon List
        /// </summary>
        /// <param name="JsonString"></param>
        /// <returns></returns>
        public static List<Beacon> ToBeacons(this string JsonString)
        {
            List<Beacon> beacons = new List<Beacon>();

            try
            {
                JObject json =
                    JsonConvert.DeserializeObject<JObject>(JsonString);

                // Aquire information of LBeacon
                List<LBeaconModel> lBeacons =
                    JsonConvert.DeserializeObject<List<LBeaconModel>>
                    (json["lBeacons"].ToString());

                // Aquire information of iBeacon
                List<IBeaconModel> iBeacons =
                    JsonConvert.DeserializeObject<List<IBeaconModel>>
                    (json["iBeacons"].ToString());

                beacons.AddRange(lBeacons);
                beacons.AddRange(iBeacons);

                return beacons;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Expanded function
        /// Convert BeaconGroupModelForNavigraphFiles to BeaconGroupModels
        /// </summary>
        /// <param name="BeaconGroups"></param>
        /// <param name="Beacons"></param>
        /// <returns></returns>
        public static List<WaypointModel> ToBeaconGroup(
            this List<WaypointModelForNavigraphFile> BeaconGroups,
            Dictionary<Guid,Beacon> Beacons)
        {
            return BeaconGroups.Select(BeaconGroup => new WaypointModel
            {
                Id = BeaconGroup.Id,
                Name = BeaconGroup.Name,
                Beacons = Beacons.Values.Where(Beacon =>
                BeaconGroup.Beacons.Contains(Beacon.UUID)).ToList()
            }).ToList();
        }

    }
}
