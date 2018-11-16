/*
 * Copyright (c) 2018 Academia Sinica, Institude of Information Science
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
 * File Name:
 *
 *      Storage.cs
 *
 * Abstract:
 *
 *      This provides the methods for cell phone's storage
 *
 * Authors:
 *
 *      Kenneth Tang, kenneth@gm.nssh.ntpc.edu.tw
 *
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using IndoorNavigation.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace IndoorNavigation.Modules
{
    /// <summary>
    /// It porvides the fast method to load and save data in local storage
    /// </summary>
    public static class MapStorage
    {
        private static readonly string mapFolder =
            Path.Combine(Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData), "Maps");

        private static object fileLock = new object();

        /// <summary>
        /// Return all the name of the locations.
        /// </summary>
        /// <returns></returns>
        public static string[] GetAllPlace()
        {
            // Check the folder of map if it is exist
            if (!Directory.Exists(mapFolder))
                Directory.CreateDirectory(mapFolder);

            return Directory.GetFiles(mapFolder)
                .Select(path => Path.GetFileName(path))
                .OrderBy(file => file).ToArray();
        }

        /// <summary>
        /// Load the map
        /// </summary>
        /// <param name="Place"></param>
        /// <returns></returns>
        public static bool LoadMap(string Place)
        {
            try
            {
                // Convert corresponding data of LBeacon
                JObject data =
                    JsonConvert.DeserializeObject<JObject>(LoadFile(Place));

                // Convert the JSON to element 
                string beaconJson = data["Beacon"].ToString();
                Utility.Beacons =
                    beaconJson.ToBeacons().ToDictionary(beacon =>beacon.UUID);
                Utility.BeaconGroups = JsonConvert.DeserializeObject
                    <List<BeaconGroupModelForMapFile>>
                    (data["BeaconGroup"].ToString())
                    .ToBeaconGroup(Utility.Beacons);
                Utility.LocationConnects = JsonConvert.DeserializeObject
                    <List<LocationConnectModelForMapFile>>
                    (data["LocationConnect"].ToString())
                    .ToLocationConnect(Utility.BeaconGroups);

                // Initialize path plannig and the data for setting map
                Utility.Route = new Navigation.RoutePlan(
                    Utility.BeaconGroups,
                    Utility.LocationConnects);

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Return specific information of the location
        /// </summary>
        /// <param name="FileName"></param>
        /// <returns></returns>
        private static string LoadFile(string FileName)
        {
            string filePath = Path.Combine(mapFolder, FileName);

            // Check the folder of map if it is exist
            if (!Directory.Exists(mapFolder))
            {
                Directory.CreateDirectory(mapFolder);
                return string.Empty;
            }

            // Check the file of map if it is exist
            if (!File.Exists(filePath))
                return string.Empty;

            lock(fileLock)
                return File.ReadAllText(filePath);
        }

        /// <summary>
        /// Store the map information of a location
        /// </summary>
        /// <param name="Place"></param>
        /// <param name="MapDatas"></param>
        /// <returns></returns>
        public static bool SaveMapInformation(string Place,string MapDatas)
        {
            string filePath = Path.Combine(mapFolder, Place);
            try
            {
                // Check the folder of map if it is exist
                if (!Directory.Exists(mapFolder))
                    Directory.CreateDirectory(mapFolder);

                // Write map information
                lock (fileLock)
                    File.WriteAllText(filePath, MapDatas);

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Delete specific map information
        /// </summary>
        /// <param name="Place"></param>
        public static void DeleteMap(string Place)
        {
            string filePath = Path.Combine(mapFolder, Place);

            // Check the folder of map if it is exist
            if (!Directory.Exists(mapFolder))
                Directory.CreateDirectory(mapFolder);

            lock (fileLock)
                File.Delete(filePath);
        }

        /// <summary>
        /// Delete all map information
        /// </summary>
        public static void DeleteAllMap()
        {
            foreach (string place in GetAllPlace())
                DeleteMap(place);
        }
    }
}
