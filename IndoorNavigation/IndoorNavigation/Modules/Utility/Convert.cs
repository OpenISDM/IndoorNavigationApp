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
    /// <summary>
    /// Data conversion tool for indoor navigation
    /// </summary>
    public static partial class Convert
    {
        /// <summary>
        /// Convert UUID to a Coordinate object.
        /// </summary>
        /// <param name="UUID"></param>
        /// <returns></returns>
        public static GeoCoordinate GetCoordinate(this
            Beacon beacon)
        {
            if (beacon.GetType() == typeof(LBeaconModel))
            {
                // Combine coordinate Hex data from UUID
                string[] idShards =
                    (beacon as LBeaconModel).UUID.ToString().Split('-');
                string latHexStr = idShards[2] + idShards[3];
                string lonHexStr = idShards[4].Substring(4, 8);

                // Convert coordinate hex data to coordinate
                float longitude = HexToFloat(lonHexStr);
                float latitude = HexToFloat(latHexStr);

                return new GeoCoordinate(latitude, longitude);
            }
            else if (beacon.GetType() == typeof(IBeaconModel))
            {
                return (beacon as IBeaconModel).IBeaconCoordinate;
            }

            throw new ArgumentException("Unrecognized Beacon type.");
        }

        public static float GetFloor(Guid UUID)
        {
            string[] idShards = UUID.ToString().Split('-');
            string floorHexStr = idShards[0];
            return HexToFloat(floorHexStr);
        }

        /// <summary>
        /// 擴充功能
        /// 將記錄Beacons的Json字串轉換成Beacon list
        /// </summary>
        /// <param name="JsonString"></param>
        /// <returns></returns>
        public static List<Beacon> ToBeacons(this string JsonString)
        {
            List<Beacon> beacons = new List<Beacon>();

            try
            {
                JObject json = JsonConvert.DeserializeObject<JObject>(JsonString);

                // 取得lBeacon資料
                List<LBeaconModel> lBeacons = 
                    JsonConvert.DeserializeObject<List<LBeaconModel>>
                    (json["lBeacons"].ToString());

                // 取得iBeacon資料
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
        /// 擴充功能
        /// 從BeaconGroupModelForMapFiles 轉換成 BeaconGroupModels
        /// </summary>
        /// <param name="BeaconGroups"></param>
        /// <param name="Beacons"></param>
        /// <returns></returns>
        public static List<BeaconGroupModel> ToBeaconGroup(
            this List<BeaconGroupModelForMapFile> BeaconGroups,
            List<Beacon> Beacons)
        {
            return BeaconGroups.Select(BeaconGroup => new BeaconGroupModel
            {
                Id = BeaconGroup.Id,
                Name = BeaconGroup.Name,
                Beacons = Beacons.Where(Beacon =>
                BeaconGroup.Beacons.Contains(Beacon.UUID)).ToList()
            }).ToList();
        }

        /// <summary>
        /// 擴充功能
        /// 從 LocationConnectModelForMapFiles 轉換成 LocationConnectModels
        /// </summary>
        /// <param name="LocationConnects"></param>
        /// <param name="BeaconGroups"></param>
        /// <returns></returns>
        public static List<LocationConnectModel> ToLocationConnect(
            this List<LocationConnectModelForMapFile> LocationConnects,
            List<BeaconGroupModel> BeaconGroups)
        {
            return LocationConnects.Select(LocationConnect =>
            new LocationConnectModel
            {
                BeaconA = BeaconGroups.Where(BeaconGroup =>
                BeaconGroup.Id == LocationConnect.BeaconA).First(),
                BeaconB = BeaconGroups.Where(BeaconGroup =>
                BeaconGroup.Id == LocationConnect.BeaconB).First(),
                IsTwoWay = LocationConnect.IsTwoWay
            }).ToList();
        }

        /// <summary>
        /// Convert hex string content to a float.
        /// 0xff20f342 -> 121.564445F
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
    }
}
