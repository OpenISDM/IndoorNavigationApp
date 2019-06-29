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
using IndoorNavigation.Models.NavigaionLayer;

namespace IndoorNavigation.Modules.Utilities
{
    public static class NavigraphStorage
    {
        internal static readonly string _navigraphFolder =
            Path.Combine(Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData),
                    "Navigraph");

        private static object _fileLock = new object();
        
        public static string[] GetAllNavigationGraphs()
        {
            // Check the folder of navigation graph if it is exist
            if (!Directory.Exists(_navigraphFolder))
                Directory.CreateDirectory(_navigraphFolder);

            return Directory.GetFiles(_navigraphFolder)
                .Select(path => Path.GetFileName(path))
                .OrderBy(file => file).ToArray();
        }

        public static NavigationGraph LoadNavigationGraphXML(string FileName)
        {
            string filePath = Path.Combine(_navigraphFolder, FileName);
            if (!File.Exists(filePath))
                throw new FileNotFoundException();

            var xmlString = File.ReadAllText(filePath);
            StringReader stringReader = new StringReader(xmlString);
            XmlDocument document = new XmlDocument();
            document.Load(filePath);
            //XMLParser xmlParser = new XMLParser();
            //NavigationGraph navigationGraph =  xmlParser.GetString(document);
            NavigationGraph navigationGraph = new NavigationGraph(document);

            return navigationGraph;
        }

        public static void DeleteNavigationGraph(string GraphName)
        {
            string filePath = Path.Combine(_navigraphFolder, GraphName);

            // Check the folder of navigraph if it is exist
            if (!Directory.Exists(_navigraphFolder))
                Directory.CreateDirectory(_navigraphFolder);

            lock (_fileLock)
                File.Delete(filePath);
        }

        public static void DeleteAllNavigationGraph()
        {
            foreach (string place in GetAllNavigationGraphs())
                DeleteNavigationGraph(place);
        }
    }
}