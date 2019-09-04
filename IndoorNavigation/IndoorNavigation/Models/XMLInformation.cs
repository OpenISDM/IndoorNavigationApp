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
        public XMLInformation(XmlDocument fileName)
        {
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
    }
}

