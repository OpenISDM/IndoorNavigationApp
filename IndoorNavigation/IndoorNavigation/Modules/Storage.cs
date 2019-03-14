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
 *
 *      Classes in this file provides the methods for storage on the phone
 *
 * File Name:
 *
 *      Storage.cs
 *
 * Abstract:
 *
 *      
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
    /// This class provides the fast method to load and save data in local 
    /// storage.
    /// </summary>
    public static class NavigraphStorage
    {
        internal static readonly string navigraphFolder =
            Path.Combine(Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData),
                    "Navigraph");

        private static object fileLock = new object();

        /// <summary>
        /// This method returns the name of all the locations.
        /// </summary>
        /// <returns></returns>
        public static string[] GetAllPlace()
        {
            // Check the folder of navigation graph if it is exist
            if (!Directory.Exists(navigraphFolder))
                Directory.CreateDirectory(navigraphFolder);

            return Directory.GetFiles(navigraphFolder)
                .Select(path => Path.GetFileName(path))
                .OrderBy(file => file).ToArray();
        }

        /// <summary>
        /// This method loads the navigation graph
        /// </summary>
        /// <param name="Place"></param>
        /// <returns></returns>
        public static bool LoadNavigraph(string Place)
        {
            try
            {
                // Convert corresponding data of LBeacon
                JObject data =
                    JsonConvert.DeserializeObject<JObject>(LoadFile(Place));

                // Convert the JSON to element 
                string beaconJson = data["Beacon"].ToString();
                Utility.BeaconsDict =
                    beaconJson.ToBeacons().ToDictionary(beacon =>beacon.UUID);
                Utility.Waypoints = JsonConvert.DeserializeObject
                    <List<BeaconGroupModelForNavigraphFile>>
                    (data["BeaconGroup"].ToString())
                    .ToBeaconGroup(Utility.BeaconsDict);
                Utility.LocationConnects = JsonConvert.DeserializeObject
                    <List<LocationConnectModelForNavigraphFile>>
                    (data["LocationConnect"].ToString())
                    .ToLocationConnect(Utility.Waypoints);

                // Initialize path planning and the data for setting navigraph
                Utility.WaypointRoute = new Navigation.WaypointRoutePlan(
                    Utility.Waypoints,
                    Utility.LocationConnects);

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Return specific information of the navigation graph
        /// </summary>
        /// <param name="FileName"></param>
        /// <returns></returns>
        private static string LoadFile(string FileName)
        {
            string filePath = Path.Combine(navigraphFolder, FileName);

            // Check the folder of navigraph if it is exist
            if (!Directory.Exists(navigraphFolder))
            {
                Directory.CreateDirectory(navigraphFolder);
                return string.Empty;
            }

            // Check the file of navigraph if it is exist
            if (!File.Exists(filePath))
                return string.Empty;

            lock(fileLock)
                return File.ReadAllText(filePath);
        }

        /// <summary>
        /// Store the navigation graph information of a location
        /// e.g., First floor of a building
        /// </summary>
        /// <param name="Place"></param>
        /// <param name="NavigraphDatas"></param>
        /// <returns></returns>
        public static bool SaveNavigraphInformation(
            string Place, string NavigraphDatas)
        {
            string filePath = Path.Combine(navigraphFolder, Place);
            try
            {
                // Check the folder of navigraph if it is exist
                if (!Directory.Exists(navigraphFolder))
                    Directory.CreateDirectory(navigraphFolder);

                // Write navigraph information
                lock (fileLock)
                    File.WriteAllText(filePath, NavigraphDatas);

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Delete specific navigation graph information
        /// </summary>
        /// <param name="Place"></param>
        public static void DeleteNavigraph(string Place)
        {
            string filePath = Path.Combine(navigraphFolder, Place);

            // Check the folder of navigraph if it is exist
            if (!Directory.Exists(navigraphFolder))
                Directory.CreateDirectory(navigraphFolder);

            lock (fileLock)
                File.Delete(filePath);
        }

        /// <summary>
        /// Delete all navigation graph information
        /// </summary>
        public static void DeleteAllNavigraph()
        {
            foreach (string place in GetAllPlace())
                DeleteNavigraph(place);
        }
    }
}
