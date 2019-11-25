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
 * 
 *     
 *      
 * Version:
 *
 *      1.0.0, 201911123
 * 
 * File Name:
 *
 *      XMLInformation.cs
 *
 * Abstract:
 *
 *      This file used to read the XML information, includes, the building name
 *      the waypoint name and the region name. Because we have several languages, therefore,
 *      we need this file to help to read all kinds of languages
 *      
 * Authors:
 *
 *      Eric Lee, ericlee@iis.sinica.edu.tw
 *     
 *
 */

using System;
using System.Collections.Generic;
using System.Xml;
using Xamarin.Forms;

namespace IndoorNavigation.Models.NavigaionLayer
{
    public class XMLInformation
    {
        private Dictionary<Guid, string> returnWaypointName;
        private Dictionary<Guid, string> returnRegionName;
        private string _buildingName;
        public XMLInformation(XmlDocument fileName)
        {
            XmlNode buildingName =fileName.SelectSingleNode("navigation_graph");
            XmlElement buildingElement = (XmlElement)buildingName;
            _buildingName = buildingElement.GetAttribute("building_name");

            XmlNodeList xmlRegion = fileName.SelectNodes("navigation_graph/regions/region");
            XmlNodeList xmlWaypoint = fileName.SelectNodes("navigation_graph/waypoints/waypoint");
            returnWaypointName = new Dictionary<Guid, string>();
            returnRegionName = new Dictionary<Guid, string>();
            foreach (XmlNode xmlNode in xmlRegion)
            {
                string name = "";
                Guid RegionGuid = new Guid();
                XmlElement xmlElement = (XmlElement)xmlNode;
                name = xmlElement.GetAttribute("name").ToString();
                RegionGuid = new Guid(xmlElement.GetAttribute("id"));
                returnRegionName.Add(RegionGuid,name);
            }

            foreach (XmlNode xmlNode in xmlWaypoint)
            {
                string name = "";
                Guid WaypointGuid = new Guid();
                XmlElement xmlElement = (XmlElement)xmlNode;
                name = xmlElement.GetAttribute("name").ToString();
                WaypointGuid = new Guid(xmlElement.GetAttribute("id"));
                returnWaypointName.Add(WaypointGuid, name);
            }
        }

        public string GiveRegionName(Guid guid)
        {
            return returnRegionName[guid];
        }

        public string GiveWaypointName(Guid guid)
        {
            return returnWaypointName[guid];
        }
        public string GiveGraphName()
        {
            return _buildingName;
        }
    }
}

