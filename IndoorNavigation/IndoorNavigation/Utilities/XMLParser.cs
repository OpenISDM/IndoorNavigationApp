using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using IndoorNavigation.Models.NavigaionLayer;


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
            _navigationgraph._regions = new Dictionary<Guid, OneRegion>();
           // _navigationgraph._edges = new Dictionary<Guid, List<RegionEdge>>();
            _navigationgraph._navigraphs = new Dictionary<Guid, Navigraph>();

            EARTH_RADIUS = 6378137;
        }
        public NavigationGraph GetString(XmlDocument xmlDocument)
        {
            Console.WriteLine("In Parser");
            //XmlNode navigation_graph = xmldocument.SelectSingleNode("navigation_graph");
            /*
            XmlNode regions = xmldocument.SelectSingleNode("navigation_graph/regions");
            XmlNode navigraphs = xmldocument.SelectSingleNode("navigation_graph/navigraphs");

            XmlNodeList region = xmldocument.SelectNodes("navigation_graph/regions/region");
            XmlNodeList edgeInRegions = xmldocument.SelectNodes("navigation_graph/regions/edge");
            XmlNodeList navigraph = xmldocument.SelectNodes("navigation_graph/navigraphs/navigraph");
            
            // XmlNodeList edgeInNavigraph = xmldocument.SelectNodes("navigation_graph/navigraphs/navigraph/edge");
            */
           
            XmlElement elementInNavigationGraph = (XmlElement)xmlDocument.SelectSingleNode("navigation_graph");
            /*
            XmlElement elementInRegions = (XmlElement)regions;
            XmlElement elementInnavigraphs = (XmlElement)navigraphs;

            XmlElement elementInRegion = (XmlElement)region;
            //XmlElement elementInWaypoint = (XmlElement)waypoint;
            */
            
            _navigationgraph._country = elementInNavigationGraph.GetAttribute("country");
            _navigationgraph._cityCounty = elementInNavigationGraph.GetAttribute("city_county");
            _navigationgraph._industryService = elementInNavigationGraph.GetAttribute("industry_service");
            _navigationgraph._ownerOrganization = elementInNavigationGraph.GetAttribute("owner_organization");
            _navigationgraph._buildingName = elementInNavigationGraph.GetAttribute("building_name");

            XmlNodeList region = xmlDocument.SelectNodes("navigation_graph/regions/region");
            foreach (XmlNode regionNode in region)
            {
                OneRegion regionGet = new OneRegion();
                regionGet._waypointsByCategory = new Dictionary<CategoryType, List<Waypoint>>();
                XmlElement elementInRegion = (XmlElement)regionNode;
                regionGet._id = Guid.Parse(elementInRegion.GetAttribute("id"));
                Console.WriteLine("region id : " + regionGet._id);
                //
                regionGet._id = Guid.Parse(elementInRegion.GetAttribute("id"));
                Console.WriteLine("ips_type : " + regionGet._id);
                //
                regionGet._name = elementInRegion.GetAttribute("name");
                Console.WriteLine("region name : " + regionGet._name);
                regionGet._floor = Int32.Parse(elementInRegion.GetAttribute("floor"));
                Console.WriteLine("region_floor ; " + regionGet._floor);

                XmlNodeList waypoint = regionNode.SelectNodes("waypoint");

                /*

                int waypointNumber = 0;


                List<Waypoint> WaypointListGet = new List<Waypoint>();
                foreach (XmlNode waypoint_node in waypoint)
                {
                    Waypoint waypointGet = new Waypoint();
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

                    waypointGet._id = Guid.Parse(waypoint_id);
                    waypointGet._name = waypoint_name;

                    waypointGet._category = (CategoryType)Enum.Parse(typeof(CategoryType), waypoint_category, false);

                    WaypointListGet.Add(waypointGet);

                    waypointNumber++;
                }

                for (int i = 0; i < WaypointListGet.Count; i++)
                {
                    if (regionGet._waypointsByCategory.ContainsKey(WaypointListGet[i]._category))
                    {
                        regionGet._waypointsByCategory[WaypointListGet[i]._category].Add(WaypointListGet[i]);
                    }
                    else
                    {
                        regionGet._waypointsByCategory.Clear();
                        regionGet._waypointsByCategory.Add(WaypointListGet[i]._category, new List<Waypoint> { WaypointListGet[i] });
                    }
                }

                //regionGet._waypointsByCategory.Add(waypointGet._category, WaypointListGet.Add(waypointGet));
                _navigationgraph._regions.Add(regionGet._id, regionGet);

                regionNumber++;
                */
            }
            /*
            List<RegionNeighbor> regionNeighborList = new List<RegionNeighbor>();

             
            foreach (XmlNode edgeNode in edgeInRegions)
            {
                RegionEdge regionEdge = new RegionEdge();
                RegionNeighbor regionNeighbor = new RegionNeighbor();
                XmlElement elementEdgeInRegion = (XmlElement)edgeInRegions[edgeNumber];
                string edge_source_region = elementEdgeInRegion.GetAttribute("source_region");
                Console.WriteLine("edge source region : " + edge_source_region);
                string edge_source_portal_waypoint = elementEdgeInRegion.GetAttribute("source_portal_waypoint");
                Console.WriteLine("source_portal_waypoint : " + edge_source_portal_waypoint);
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

                regionEdge._sinkRegionID = Guid.Parse(edge_sink_region);
                regionEdge._sinkWaypointID = Guid.Parse(edge_sink_portal_waypoint);
                regionEdge._sourceWaypointID = Guid.Parse(edge_source_portal_waypoint);
                //regionEdge._direction
                regionEdge._direction = (CardinalDirection)Enum.Parse(typeof(CardinalDirection), edge_direction, false);
                regionEdge._connectionType = (ConnectionType)Enum.Parse(typeof(ConnectionType), edge_connection_type, false);
                regionEdge._distance = 10;

                if (_navigationgraph._edges.ContainsKey(Guid.Parse(edge_source_region)))
                {
                    //_navigationgraph
                    _navigationgraph._edges[Guid.Parse(edge_source_region)].Add(regionEdge);
                }
                else
                {
                    _navigationgraph._edges.Clear();
                    _navigationgraph._edges.Add(Guid.Parse(edge_source_region), new List<RegionEdge> { regionEdge });
                }

                regionNeighbor._id = Guid.Parse(edge_source_region);
                regionNeighborList.Add(regionNeighbor);
                if (_navigationgraph._regions.ContainsKey(Guid.Parse(edge_source_region)))
                {
                    //Console.WriteLine("Neighbor element : " + _navigationgraph._regions[Guid.Parse(edge_source_region)]._neighbors.Count);
                    if (_navigationgraph._regions[Guid.Parse(edge_source_region)]._neighbors == null)
                    {
                        _navigationgraph._regions[Guid.Parse(edge_source_region)]._neighbors = new List<RegionNeighbor>();
                        _navigationgraph._regions[Guid.Parse(edge_source_region)]._neighbors.Add(regionNeighbor);
                    }
                    else
                    {

                        _navigationgraph._regions[Guid.Parse(edge_source_region)]._neighbors.Add(regionNeighbor);
                    }


                }
                else
                {
                    Console.WriteLine("No Region Neighbor");
                    //_navigationgraph._regions[Guid.Parse(edge_source_region)]._neighbors.Clear();
                    //_navigationgraph._regions[Guid.Parse(edge_source_region)]._neighbors.Add
                }
                ////////////////////////////////////////////////////////////////////

                Guid tempRegionID = regionEdge._sinkRegionID;
                if (Int32.Parse(edge_bi_direction) == 2)
                {
                    regionEdge = new RegionEdge();
                    regionEdge._sinkRegionID = Guid.Parse(edge_source_region);
                    regionEdge._sinkWaypointID = Guid.Parse(edge_source_portal_waypoint);
                    regionEdge._sourceWaypointID = Guid.Parse(edge_sink_portal_waypoint);
                    regionEdge._distance = 10;
                    //_navigationgraph._edges.Add(Guid.Parse(edge_sink_region), regionEdge);

                    if (_navigationgraph._edges.ContainsKey(tempRegionID))
                    {
                        //_navigationgraph
                        _navigationgraph._edges[tempRegionID].Add(regionEdge);
                    }
                    else
                    {
                        _navigationgraph._edges.Clear();
                        _navigationgraph._edges.Add(tempRegionID, new List<RegionEdge> { regionEdge });
                    }


                    regionNeighbor._id = regionEdge._sinkRegionID;
                    
                    if (_navigationgraph._regions.ContainsKey(tempRegionID))
                    {
                        //Console.WriteLine("Neighbor element : " + _navigationgraph._regions[Guid.Parse(edge_source_region)]._neighbors.Count);
                        if (_navigationgraph._regions[tempRegionID]._neighbors == null)
                        {
                            _navigationgraph._regions[tempRegionID]._neighbors = new List<RegionNeighbor>();
                            _navigationgraph._regions[tempRegionID]._neighbors.Add(regionNeighbor);
                        }
                        else
                        {

                            _navigationgraph._regions[tempRegionID]._neighbors.Add(regionNeighbor);
                        }


                    }
                    else
                    {
                        Console.WriteLine("No Region Neighbor");
                        //_navigationgraph._regions[Guid.Parse(edge_source_region)]._neighbors.Clear();
                        //_navigationgraph._regions[Guid.Parse(edge_source_region)]._neighbors.Add
                    }
                    ////////////////////////////////////////////////////////////////////

                }



                ///////////////Add neighbor into region in Dictionary///////////////




                edgeNumber++;
            }




            string navigrap_numbers = elementInnavigraphs.GetAttribute("navigrap_numbers");
            Console.WriteLine("navigraphs number : " + navigrap_numbers);


            foreach (XmlNode navigraphNode in navigraph)
            {
                Navigraph navigraphGet = new Navigraph();
                navigraphGet._edges = new Dictionary<Guid, List<WaypointEdge>>();
                navigraphGet._waypoints = new Dictionary<Guid, Waypoint>();
                navigraphGet._beacons = new Dictionary<Guid, List<Guid>>();
                int waypointNumber = 0;
                XmlElement elementInNavigraph = (XmlElement)navigraph[navigraphNumber];
                string navigraph_region_id = elementInNavigraph.GetAttribute("region_id");
                Console.WriteLine("region_ID : " + navigraph_region_id);
                string navigraph_ips_type = elementInNavigraph.GetAttribute("ips_type");
                Console.WriteLine("navigraph ips type : " + navigraph_ips_type);
                string navigraph_waypoint_numbers = elementInNavigraph.GetAttribute("waypoint_numbers");
                Console.WriteLine("navigraph waypoint numbers : " + navigraph_waypoint_numbers);
                string navigraph_edge_numbers = elementInNavigraph.GetAttribute("edge_numbers");

                navigraphGet._id = Guid.Parse(navigraph_region_id);
                navigraphGet._IPSType = (IPSType)Enum.Parse(typeof(IPSType), navigraph_ips_type, false);

                XmlNodeList waypointInNavigraph = navigraphNode.SelectNodes("waypoint");


                foreach (XmlNode waypointNode in waypointInNavigraph)
                {
                    Waypoint waypointGet = new Waypoint();

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

                    waypointGet._id = Guid.Parse(waypoint_id);
                    waypointGet._name = waypoint_name;
                    waypointGet._type = (LocationType)Enum.Parse(typeof(LocationType), waypoint_type, true);
                    waypointGet._category = (CategoryType)Enum.Parse(typeof(CategoryType), waypoint_category, true);

                    navigraphGet._waypoints.Add(waypointGet._id, waypointGet);
                    waypointNumber++;
                }

                XmlNodeList edgeInNavigraph = navigraphNode.SelectNodes("edge");
                XmlNode beacons = navigraphNode.SelectSingleNode("beacons");
                XmlElement elementInbeacons = (XmlElement)beacons;

                edgeNumber = 0;
                foreach (XmlNode edgeNode in edgeInNavigraph)
                {
                    WaypointEdge waypointEdge = new WaypointEdge();
                    WaypointNeighbor waypointNeighborGet = new WaypointNeighbor();
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

                    waypointEdge._sinkWaypointID = Guid.Parse(edge_sink);
                    waypointEdge._distance = 10;
                    waypointEdge._connectionType = (ConnectionType)Enum.Parse(typeof(ConnectionType), edge_connection_type, false);
                    waypointEdge._direction = (CardinalDirection)Enum.Parse(typeof(CardinalDirection), edge_direction, false);

                    waypointNeighborGet._id = Guid.Parse(edge_source);

                    Guid tempWaypointID = waypointEdge._sinkWaypointID;

                    if (navigraphGet._edges.ContainsKey(Guid.Parse(edge_source)))
                    {
                        //_navigationgraph
                        navigraphGet._edges[Guid.Parse(edge_source)].Add(waypointEdge);
                    }
                    else
                    {
                        navigraphGet._edges.Clear();
                        navigraphGet._edges.Add(Guid.Parse(edge_source), new List<WaypointEdge> { waypointEdge });
                    }
                    //Guid tempWaypointID = waypointEdge._sinkWaypointID;

                    waypointNeighborGet._id = waypointEdge._sinkWaypointID;
                   
                    if (navigraphGet._edges.ContainsKey(Guid.Parse(edge_source)))
                    {
                        //Console.WriteLine("Neighbor element : " + _navigationgraph._regions[Guid.Parse(edge_source_region)]._neighbors.Count);
                        if (navigraphGet._waypoints[Guid.Parse(edge_source)]._neighbors == null)
                        {
                            navigraphGet._waypoints[Guid.Parse(edge_source)]._neighbors = new List<WaypointNeighbor>();
                            navigraphGet._waypoints[Guid.Parse(edge_source)]._neighbors.Add(waypointNeighborGet);
                        }
                        else
                        {
                            navigraphGet._waypoints[Guid.Parse(edge_source)]._neighbors.Add(waypointNeighborGet);
                        }

                    }
                    else
                    {
                        Console.WriteLine("No Region Neighbor");
                        //_navigationgraph._regions[Guid.Parse(edge_source_region)]._neighbors.Clear();
                        //_navigationgraph._regions[Guid.Parse(edge_source_region)]._neighbors.Add
                    }
                    ////////////////////////////////////////////////////////////////////

                    if (Int32.Parse(edge_bi_direction) == 2)
                    {
                        waypointEdge = new WaypointEdge();

                        waypointEdge._sinkWaypointID = navigraphGet._id;
                        waypointEdge._connectionType = (ConnectionType)Enum.Parse(typeof(ConnectionType), edge_connection_type, false);
                        waypointEdge._direction = (CardinalDirection)Enum.Parse(typeof(CardinalDirection), edge_bi_direction, false);
                        waypointEdge._distance = 10;
                        waypointNeighborGet._id = waypointEdge._sinkWaypointID;
                        
                        if (navigraphGet._edges.ContainsKey(tempWaypointID))
                        {
                            //_navigationgraph
                            navigraphGet._edges[tempWaypointID].Add(waypointEdge);
                        }                  
                        else
                        {
                            navigraphGet._edges.Clear();
                            navigraphGet._edges.Add(tempWaypointID, new List<WaypointEdge> { waypointEdge });
                        }

                        if (navigraphGet._edges.ContainsKey(tempWaypointID))
                        {
                            //Console.WriteLine("Neighbor element : " + _navigationgraph._regions[Guid.Parse(edge_source_region)]._neighbors.Count);
                            if (navigraphGet._waypoints[tempWaypointID]._neighbors == null)
                            {
                                navigraphGet._waypoints[tempWaypointID]._neighbors = new List<WaypointNeighbor>();
                                navigraphGet._waypoints[tempWaypointID]._neighbors.Add(waypointNeighborGet);
                            }
                            else
                            {
                                navigraphGet._waypoints[tempWaypointID]._neighbors.Add(waypointNeighborGet);
                            }

                        }
                        else
                        {
                            Console.WriteLine("No Region Neighbor");
                            //_navigationgraph._regions[Guid.Parse(edge_source_region)]._neighbors.Clear();
                            //_navigationgraph._regions[Guid.Parse(edge_source_region)]._neighbors.Add
                        }
                        ////////////////////////////////////////////////////////////////////

                    }

                    if (navigraphGet._waypoints.ContainsKey(Guid.Parse(edge_source)))
                    {
                        if (navigraphGet._waypoints[Guid.Parse(edge_source)]._neighbors.Count == 0)
                        {
                            navigraphGet._waypoints[Guid.Parse(edge_source)]._neighbors.Clear();
                        }
                        navigraphGet._waypoints[Guid.Parse(edge_source)]._neighbors.Add(waypointNeighborGet);
                    }
                    else
                    {
                        Console.WriteLine("No Edge Neighbor");
                        //_navigationgraph._regions[Guid.Parse(edge_source_region)]._neighbors.Clear();
                        //_navigationgraph._regions[Guid.Parse(edge_source_region)]._neighbors.Add
                    }

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
                        if (navigraphGet._beacons.ContainsKey(Guid.Parse(stringArray[i])))
                        {
                            navigraphGet._beacons[Guid.Parse(stringArray[i])].Add(Guid.Parse(beacon_uuid));
                        }
                        else
                        {
                            navigraphGet._beacons.Clear();
                            navigraphGet._beacons.Add(Guid.Parse(stringArray[i]), new List<Guid> { Guid.Parse(beacon_uuid) });
                        }

                    }

                    beaconNumber++;
                }
                _navigationgraph._navigraphs.Add(navigraphGet._id, navigraphGet);
                navigraphNumber++;
            }

            // string navigraph_region_id = elementInnavigraphs.GetAttribute("region_id");

            //xmlString.
            */
            return _navigationgraph;
        }
        //public double GetDistance(double lng1, double lat1, double lng2, double lat2)
        //{
        //    double radLat1 = Rad(lat1);
        //    double radLng1 = Rad(lng1);
        //    double radLat2 = Rad(lat2);
        //    double radLng2 = Rad(lng2);
        //    double a = radLat1 - radLat2;
        //    double b = radLng1 - radLng2;
        //    double result = 2 * Math.Asin(Math.Sqrt(Math.Pow(Math.Sin(a / 2), 2) + Math.Cos(radLat1) * Math.Cos(radLat2) * Math.Pow(Math.Sin(b / 2), 2))) * EARTH_RADIUS;
        //    return result;
        //}

        //private static double Rad(double d)
        //{
        //    return (double)d * Math.PI / 180d;
        //}

    }
}

