using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using IndoorNavigation.Models.NavigaionLayer;
using System.Linq;
using Dijkstra.NET.Model;
using GeoCoordinatePortable;
using System.Xml.Serialization;
using IndoorNavigation.Models;

namespace IndoorNavigation.Modules
{
    public class XMLParser
    {
        public NavigationGraph _navigationgraph;
        private double EARTH_RADIUS;
        public XMLParser()
        {
            Console.WriteLine("Construct Parser");
            _navigationgraph = new NavigationGraph();
            EARTH_RADIUS = 6378137;
        }
        public NavigationGraph GetString(XmlDocument xmldocument)
        {
            Console.WriteLine("In Parser");

            _navigationgraph._industryService = "hospital";
            _navigationgraph._regions = new Dictionary<Guid, OneRegion>();

            OneRegion region = new OneRegion();
            Guid guidRegion = new Guid("00000000-0000-0000-0000-000000000001");
            region._id = guidRegion;
            region._name = "2F";
            region._floor = 2;
            region._waypointsByCategory = new Dictionary<CategoryType, List<Waypoint>>();

            List<Waypoint> tempWaypointList = new List<Waypoint>();
            Waypoint waypoint = new Waypoint();
            waypoint._id = new Guid("00000018-0000-0000-2460-000000005900");
            waypoint._name = "Test";
            waypoint._category = CategoryType.Clinics;
            tempWaypointList.Add(waypoint);
            region._waypointsByCategory.Add(CategoryType.Clinics, tempWaypointList);

            _navigationgraph._regions.Add(guidRegion, region);

            Console.WriteLine("end Parser");
            /*
            XmlNode navigation_graph = xmldocument.SelectSingleNode("navigation_graph");
            XmlNode regions = xmldocument.SelectSingleNode("navigation_graph/regions");
            XmlNode navigraphs = xmldocument.SelectSingleNode("navigation_graph/navigraphs");

            XmlNodeList region = xmldocument.SelectNodes("navigation_graph/regions/region");
            XmlNodeList edgeInRegions = xmldocument.SelectNodes("navigation_graph/regions/edge");
            XmlNodeList navigraph = xmldocument.SelectNodes("navigation_graph/navigraphs/navigraph");
            // XmlNodeList edgeInNavigraph = xmldocument.SelectNodes("navigation_graph/navigraphs/navigraph/edge");


            XmlElement elementInNavigationGraph = (XmlElement)navigation_graph;
            XmlElement elementInRegions = (XmlElement)regions;
            XmlElement elementInnavigraphs = (XmlElement)navigraphs;

            //XmlElement elementInRegion = (XmlElement)region;
            //XmlElement elementInWaypoint = (XmlElement)waypoint;

            int regionNumber = 0;
            int edgeNumber = 0;
            int navigraphNumber = 0;
            int stringListNumber = 0;
            string[] stringArray;
            

            string country = elementInNavigationGraph.GetAttribute("country");
            string city = elementInNavigationGraph.GetAttribute("city_country");
            string industry_type = elementInNavigationGraph.GetAttribute("industry_service");
            string owner_organization = elementInNavigationGraph.GetAttribute("owner_organization");
            string name_navigation_graph = elementInNavigationGraph.GetAttribute("building_name");

            //_navigationgraph._country = country;
            //_navigationgraph._city_country = city;
            //_navigationgraph._industry_service = industry_type;
            //_navigationgraph._owner_organization = owner_organization;
            //_navigationgraph._building_name = name_navigation_graph;
            
            int region_number = Int32.Parse(elementInRegions.GetAttribute("region_numbers"));
            Console.WriteLine("region_numbers : " + region_number);
            string edge_numbers = elementInRegions.GetAttribute("edge_numbers");
            Console.WriteLine("edge_numbers : " + edge_numbers);

            
           
            foreach (XmlNode region_node in region)
            {
                //Region 
                XmlElement elementInRegion = (XmlElement)region[regionNumber];
                string region_id = elementInRegion.GetAttribute("id");
                Console.WriteLine("region id : " + region_id);
                string region_name = elementInRegion.GetAttribute("name");
                Console.WriteLine("region name : " + region_name);
                string region_floor = elementInRegion.GetAttribute("floor");
                Console.WriteLine("region_floor ; " + region_floor);

                XmlNodeList waypoint = region_node.SelectNodes("waypoint");
                int waypointNumber = 0;

                

                foreach (XmlNode waypoint_node in waypoint)
                {

                    XmlElement elementInWaypoint = (XmlElement)waypoint[waypointNumber];
                    string waypoint_id = elementInWaypoint.GetAttribute("id");
                    Console.WriteLine("waypoint id : " + waypoint_id);
                    string waypoint_name = elementInWaypoint.GetAttribute("name");
                    Console.WriteLine("waypoint name : " + waypoint_name);
                    string waypoint_type = elementInWaypoint.GetAttribute("type");
                    Console.WriteLine("waypoint type : " + waypoint_type);
                    string waypoint_lat = elementInWaypoint.GetAttribute("lat");
                    Console.WriteLine("waypoint lat : " + waypoint_lat);
                    string waypoint_lon = elementInWaypoint.GetAttribute("lon");
                    Console.WriteLine("waypoint lon : " + waypoint_lon);
                    string waypoint_category = elementInWaypoint.GetAttribute("category");
                    Console.WriteLine("waypoint category : " + waypoint_category);
                    waypointNumber++;
                }
                regionNumber++;
            }

            foreach (XmlNode edgeNode in edgeInRegions)
            {
                XmlElement elementEdgeInRegion = (XmlElement)edgeInRegions[edgeNumber];
                string edge_source_region = elementEdgeInRegion.GetAttribute("source_region");
                Console.WriteLine("edge source region : " + edge_source_region);
                string sedge_ource_portal_waypoint = elementEdgeInRegion.GetAttribute("source_portal_waypoint");
                Console.WriteLine("source_portal_waypoint : " + sedge_ource_portal_waypoint);
                string edge_sink_region = elementEdgeInRegion.GetAttribute("sink_region");
                Console.WriteLine("sink_region : " + edge_sink_region);
                string edge_sink_portal_waypoint = elementEdgeInRegion.GetAttribute("sink_portal_waypoint");
                Console.WriteLine("sink_portal_waypoint : " + edge_sink_portal_waypoint);
                string edge_direction = elementEdgeInRegion.GetAttribute("direction");
                Console.WriteLine("direction : " + edge_direction);
                string edge_bi_direction = elementEdgeInRegion.GetAttribute("bi_direction");
                Console.WriteLine("bi_direction : " + edge_bi_direction);
                string edge_connection_type = elementEdgeInRegion.GetAttribute("connection_type");
                Console.WriteLine("connection_type : " + edge_connection_type);
                edgeNumber++;
            }


            string navigrap_numbers = elementInnavigraphs.GetAttribute("navigrap_numbers");
            Console.WriteLine("navigraphs number : " + navigrap_numbers);

            foreach (XmlNode navigraphNode in navigraph)
            {
                int waypointNumber = 0;
                XmlElement elementInNavigraph = (XmlElement)navigraph[navigraphNumber];
                string navigraph_region_id = elementInNavigraph.GetAttribute("region_id");
                Console.WriteLine("region_ID : " + navigraph_region_id);
                string navigraph_ips_type = elementInNavigraph.GetAttribute("ips_type");
                Console.WriteLine("navigraph ips type : " + navigraph_ips_type);
                string navigraph_waypoint_numbers = elementInNavigraph.GetAttribute("waypoint_numbers");
                Console.WriteLine("navigraph waypoint numbers : " + navigraph_waypoint_numbers);
                string navigraph_edge_numbers = elementInNavigraph.GetAttribute("edge_numbers");

                XmlNodeList waypointInNavigraph = navigraphNode.SelectNodes("waypoint");
                foreach (XmlNode waypointNode in waypointInNavigraph)
                {
                    XmlElement elementInWaypoint = (XmlElement)waypointInNavigraph[waypointNumber];
                    string waypoint_id = elementInWaypoint.GetAttribute("id");
                    Console.WriteLine("Navigraph waypoint id : " + waypoint_id);
                    string waypoint_name = elementInWaypoint.GetAttribute("name");
                    Console.WriteLine("Navigraph waypoint name : " + waypoint_name);
                    string waypoint_type = elementInWaypoint.GetAttribute("type");
                    Console.WriteLine("Navigraph waypoint type : " + waypoint_type);
                    string waypoint_lat = elementInWaypoint.GetAttribute("lat");
                    Console.WriteLine("Navigraph waypoint lat : " + waypoint_lat);
                    string waypoint_lon = elementInWaypoint.GetAttribute("lon");
                    Console.WriteLine("Navigraph waypoint lon : " + waypoint_lon);
                    string waypoint_category = elementInWaypoint.GetAttribute("category");
                    Console.WriteLine("Navigraph waypoint category : " + waypoint_category);
                    waypointNumber++;
                }

                XmlNodeList edgeInNavigraph = navigraphNode.SelectNodes("edge");
                XmlNode beacons = navigraphNode.SelectSingleNode("beacons");
                XmlElement elementInbeacons = (XmlElement)beacons;

                edgeNumber = 0;
                foreach (XmlNode edgeNode in edgeInNavigraph)
                {
                    XmlElement elementInedgeInNavigraph = (XmlElement)edgeInNavigraph[edgeNumber];
                    string edge_source = elementInedgeInNavigraph.GetAttribute("source");
                    Console.WriteLine("element edge source : " + edge_source);
                    string edge_sink = elementInedgeInNavigraph.GetAttribute("sink");
                    Console.WriteLine("element edge sink : " + edge_sink);
                    string edge_direction = elementInedgeInNavigraph.GetAttribute("direction");
                    Console.WriteLine("element edge direction : " + edge_direction);
                    string edge_bi_direction = elementInedgeInNavigraph.GetAttribute("bi_direction");
                    Console.WriteLine("element bi_direction : " + edge_bi_direction);
                    string edge_connection_type = elementInedgeInNavigraph.GetAttribute("connection_type");
                    Console.WriteLine("element connection type : " + edge_connection_type);
                    edgeNumber++;
                }

                string beacon_number = elementInbeacons.GetAttribute("beacon_numbers");
                XmlNodeList beacon = beacons.SelectNodes("beacon");
                int beaconNumber = 0;

                foreach (XmlNode beaconNode in beacon)
                {
                    XmlElement elementInBeacon = (XmlElement)beacon[beaconNumber];
                    string beacon_uuid = elementInBeacon.GetAttribute("uuid");
                    Console.WriteLine("uuid : " + beacon_uuid);
                    string waypoint_ids = elementInBeacon.GetAttribute("waypoint_ids");
                    Console.WriteLine("waypoint_ids : " + waypoint_ids);
                    stringArray = waypoint_ids.Split(';');
                    for (int i = 0; i < stringArray.Length; i++)
                    {
                        Console.WriteLine("waypoint id split check : " + stringArray[i]);
                    }
                    beaconNumber++;
                }



                navigraphNumber++;
            }

            // string navigraph_region_id = elementInnavigraphs.GetAttribute("region_id");

            //xmlString.
            */
            return _navigationgraph;
        }
        public double GetDistance(double lng1, double lat1, double lng2, double lat2)
        {
            double radLat1 = Rad(lat1);
            double radLng1 = Rad(lng1);
            double radLat2 = Rad(lat2);
            double radLng2 = Rad(lng2);
            double a = radLat1 - radLat2;
            double b = radLng1 - radLng2;
            double result = 2 * Math.Asin(Math.Sqrt(Math.Pow(Math.Sin(a / 2), 2) + Math.Cos(radLat1) * Math.Cos(radLat2) * Math.Pow(Math.Sin(b / 2), 2))) * EARTH_RADIUS;
            return result;
        }

        private static double Rad(double d)
        {
            return (double)d * Math.PI / 180d;
        }
        
    }
}
