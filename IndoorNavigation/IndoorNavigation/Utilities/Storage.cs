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
using System.Reflection;
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
        internal static readonly string _firstDirectionInstuctionFolder
             = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FirstDirection");

        internal static readonly string _informationFolder
             = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Information");

        internal static readonly string _embeddedResourceReoute = "IndoorNavigation.Resources.";

        private static object _fileLock = new object();
        
        public static string[] GetAllNavigationGraphs()
        {
            // Check the folder of navigation graph if it is exist
            if (!Directory.Exists(_navigraphFolder))
                Directory.CreateDirectory(_navigraphFolder);
            if (!Directory.Exists(_firstDirectionInstuctionFolder))
                Directory.CreateDirectory(_firstDirectionInstuctionFolder);
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
            if (xmlString == "") { 
                DeleteNavigationGraph(FileName);
            }
            StringReader stringReader = new StringReader(xmlString);
            XmlDocument document = new XmlDocument();
            document.Load(filePath);
            //XMLParser xmlParser = new XMLParser();
            //NavigationGraph navigationGraph =  xmlParser.GetString(document);
            NavigationGraph navigationGraph = new NavigationGraph(document);

            return navigationGraph;
        }

        public static FirstDirectionInstruction LoadFirstDirectionXML(string FileName)
        {
            string filePath = Path.Combine(_firstDirectionInstuctionFolder, FileName);

            var xmlString = File.ReadAllText(filePath);
            if (xmlString == "")
            {
                DeleteFirstDirectionXML(FileName);
            }

            StringReader stringReader = new StringReader(xmlString);
            XmlDocument document = new XmlDocument();
            document.Load(filePath);

            FirstDirectionInstruction firstDirectionInstruction = new FirstDirectionInstruction(document);

            return firstDirectionInstruction;

        }

        public static XMLInformation LoadInformationML(string FileName)
        {
            string filePath = Path.Combine(_informationFolder, FileName);

            var xmlString = File.ReadAllText(filePath);
            if (xmlString == "")
            {
                DeleteFirstDirectionXML(FileName);
            }

            StringReader stringReader = new StringReader(xmlString);
            XmlDocument document = new XmlDocument();
            document.Load(filePath);

            XMLInformation xmlInformation  = new XMLInformation(document);

            return xmlInformation;

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

        public static void DeleteFirstDirectionXML(string fileName)
        {
            string filePath_en = Path.Combine(_firstDirectionInstuctionFolder, fileName + "_en-US.xml");
            string filePath_zh = Path.Combine(_firstDirectionInstuctionFolder, fileName + "_zh.xml");
            if (!Directory.Exists(_firstDirectionInstuctionFolder))
                Directory.CreateDirectory(_firstDirectionInstuctionFolder);

            lock (_fileLock)
            {
                File.Delete(filePath_en);
                File.Delete(filePath_zh);
            }
                //File.Delete(filePath);
        }

        public static void DeleteInformationML(string fileName)
        {
            string filePath_en = Path.Combine(_informationFolder, fileName + "_info_en-US.xml");
            string filePath_zh = Path.Combine(_informationFolder, fileName + "_info_zh.xml");
            if (!Directory.Exists(_informationFolder))
                Directory.CreateDirectory(_informationFolder);

            lock (_fileLock)
            {
                File.Delete(filePath_en);
                File.Delete(filePath_zh);
            }
               // File.Delete(filePath);
        }

        public static void DeleteAllNavigationGraph()
        {
            foreach (string place in GetAllNavigationGraphs())
                DeleteNavigationGraph(place);
        }

        public static void DeleteAllFirstDirectionXML()
        {
            foreach(string place in GetAllNavigationGraphs())
            {
                DeleteFirstDirectionXML(place);
            }
        }

        public static void DeleteAllInformationXML()
        {
            foreach (string place in GetAllNavigationGraphs())
            {
                DeleteInformationML(place);
            }
        }

        public static void GenerateFileRoute(string fileName, string readingPath)
        {
            string sourceNavigationData = Path.Combine(_embeddedResourceReoute  + readingPath + "."+ readingPath + ".xml");
            string sinkNavigationData = Path.Combine(NavigraphStorage._navigraphFolder, 
                                            fileName);
            string sourceFirstDirectionData_en = Path.Combine(_embeddedResourceReoute + readingPath + "."+ readingPath + "_en-US.xml");
            string sourceFirstDirectionData_zh = Path.Combine(_embeddedResourceReoute + readingPath + "." + readingPath + "_zh.xml");
            string sinkFirstDirectionData_en = Path.Combine(NavigraphStorage._firstDirectionInstuctionFolder , fileName + "_en-US.xml");
            string sinkFirstDirectionData_zh = Path.Combine(NavigraphStorage._firstDirectionInstuctionFolder , fileName + "_zh.xml");

            string sourceInformation_en = Path.Combine(_embeddedResourceReoute + readingPath + "." + readingPath + "_info_en-US.xml");
            string sourceInformation_zh = Path.Combine(_embeddedResourceReoute + readingPath + "." + readingPath + "_info_zh.xml");
            string sinkInformation_en = Path.Combine(NavigraphStorage._informationFolder, fileName + "_info_en-US.xml");
            string sinkInformation_zh = Path.Combine(NavigraphStorage._informationFolder, fileName + "_info_zh.xml");

            Console.WriteLine("sourceInformation_en : " + sourceInformation_en);
            Console.WriteLine("sinkInformation_en : " + sinkInformation_en);


            try
            {
                if (!Directory.Exists(NavigraphStorage._navigraphFolder))
                    Directory.CreateDirectory(
                        NavigraphStorage._navigraphFolder);

                if (!Directory.Exists(NavigraphStorage._firstDirectionInstuctionFolder))
                    Directory.CreateDirectory(
                        NavigraphStorage._firstDirectionInstuctionFolder);

                if (!Directory.Exists(NavigraphStorage._informationFolder))
                    Directory.CreateDirectory(
                        NavigraphStorage._informationFolder);

                Storing(sourceNavigationData, sinkNavigationData);
                Storing(sourceFirstDirectionData_en, sinkFirstDirectionData_en);
                Storing(sourceFirstDirectionData_zh, sinkFirstDirectionData_zh);
                Storing(sourceInformation_en, sinkInformation_en);
                Storing(sourceInformation_zh, sinkInformation_zh);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public static void Storing(string sourceRoute, string sinkRoute)
        {
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(sourceRoute))
            {
                StreamReader tr = new StreamReader(stream);
                string fileContents = tr.ReadToEnd();
                File.WriteAllText(sinkRoute, fileContents.ToString());
            }
        }


    }
}
