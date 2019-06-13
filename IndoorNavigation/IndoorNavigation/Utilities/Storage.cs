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
 *      This static class contains all the static functions that provide 
 *      methods for loading and saving data on the phone, such as loading a 
 *      waypoint-based navigation graph.
 *      
 * Version:
 *
 *      1.0.0-beta.1, 20190521
 * 
 * File Name:
 *
 *      Storage.cs
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
 *      Paul Chang, paulchang@iis.sinica.edu.tw
 *      
 */

using System;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using IndoorNavigation.Models.NavigaionLayer;
using System.Diagnostics;
using System.Net;

namespace IndoorNavigation.Modules.Utilities
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

        public static bool DownloadNavigraph(string URL, string navigraphName)
        {
            string filePath = Path.Combine(navigraphFolder, navigraphName);

            try
            {
                if (!Directory.Exists(navigraphFolder))
                    Directory.CreateDirectory(navigraphFolder);

                using (WebClient webClient = new WebClient())
                    webClient.DownloadFileAsync(new Uri(URL), filePath);

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// This method returns the name of all the locations.
        /// </summary>
        /// <returns></returns>
        public static string[] GetAllNavigraphs()
        {
            // Check the folder of navigation graph if it is exist
            if (!Directory.Exists(navigraphFolder))
                Directory.CreateDirectory(navigraphFolder);

            return Directory.GetFiles(navigraphFolder)
                .Select(path => Path.GetFileName(path))
                .OrderBy(file => file).ToArray();
        }

        /// <summary>
        /// Loads the navigraph XML from the specified file name.
        /// </summary>
        /// <returns>The navigraph object.</returns>
        /// <param name="FileName">File name.</param>
        public static Navigraph LoadNavigraphXML(string FileName)
        {
            string filePath = Path.Combine(navigraphFolder, FileName);

            if (!File.Exists(filePath))
                throw new FileNotFoundException();

            var xmlString = File.ReadAllText(filePath);
            StringReader stringReader = new StringReader(xmlString);

            XmlSerializer xmlSerializer = new XmlSerializer(typeof(Navigraph));
            XmlTextReader xmlReader = new XmlTextReader(stringReader);

            Navigraph navigraph;
            try
            {
                navigraph = (Navigraph)xmlSerializer.Deserialize(xmlReader);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                throw new Exception();
            }
            finally
            {
                if (xmlReader != null)
                {
                    xmlReader.Close();
                }
                if (stringReader != null)
                {
                    stringReader.Close();
                }
            }

            return navigraph;
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
        /// <param name="FileName"></param>
        /// <param name="NavigraphDatas"></param>
        /// <returns></returns>
        public static bool SaveNavigraphInformation(
            string FileName, string NavigraphDatas)
        {
            string filePath = Path.Combine(navigraphFolder, FileName);
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
        /// <param name="GraphName"></param>
        public static void DeleteNavigraph(string GraphName)
        {
            string filePath = Path.Combine(navigraphFolder, GraphName);

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
            foreach (string place in GetAllNavigraphs())
                DeleteNavigraph(place);
        }
    }
}
