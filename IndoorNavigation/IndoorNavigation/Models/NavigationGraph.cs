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
 *      This model contains nest class: Navigraph class and also contains
 *      classes of Waypoint, Neighbor and Edge/Connection which as 
 *      the XML element.
 *      Navigraph class in a nest class that defines the structure of
 *      navigation sub-graphs of a region, and this class also has the method
 *      to get RegionGraph.
 *      
 * Version:
 *
 *      1.0.0, 20190605
 * 
 * File Name:
 *
 *      NavigationGraph.cs
 *
 * Abstract:
 *
 *      Waypoint-based navigator is a mobile Bluetooth navigation application
 *      that runs on smart phones. It is structed to support
 *      anywhere navigation. Indoors in areas covered by different
 *      indoor positioning system (IPS) and outdoors covered by GPS.
 *      In particilar, it can rely on BeDIS (Building/environment Data and
 *      Information System) for indoor positioning. 
 *      Using this IPS, the navigator does not need to continuously monitor
 *      its own position, since the IPS broadcast to the navigator the
 *      location of each waypoint.
 *      This version makes use of Xamarin.Forms, which is a complete 
 *      cross-platform UI tookit that runs on both iOS and Android.
 *
 * Authors:
 *
 *      Paul Chang, paulchang@iis.sinica.edu.tw
 *      Eric Lee, ericlee@iis.sinica.edu.tw
 *      Chun Yu Lai, chunyu1202@gmail.com
 *
 */
using System;
using System.Collections.Generic;
using System.Linq;
using Dijkstra.NET.Model;
using GeoCoordinatePortable;
using System.Xml.Serialization;
using System.Xml;

namespace IndoorNavigation.Models.NavigaionLayer
{

    public enum LocationType
    {
        landmark = 0,
        junction_branch,
        midpath,
        terminal_destination,
        portal
    }

    public enum CardinalDirection
    {
        North = 0,
        Northeast,
        East,
        Southeast,
        South,
        Southwest,
        West,
        Northwest,
        Up,
        Down
    }

    public enum TurnDirection
    {
        FirstDirection = -1, // Exception: used only in the first step
        Forward = 0,
        Forward_Right,
        Right,
        Backward_Right,
        Backward,
        Backward_Left,
        Left,
        Forward_Left,
        Up,
        Down
    }

    public enum IPSType
    {
        LBeacon = 0,
        iBeacon,
        GPS
    }

    public enum DirectionalConnection
    {
        OneWay = 1,
        BiDirection = 2
    }

    public enum ConnectionType
    {
        NormalHallway = 0,
        Stair,
        Elevator,
        Escalator
    }

    public enum CategoryType
    {
        Others = 0,
        Clinics,
        Cashier,
        Exit,
        ExaminationRoom,
        Pharmacy,
        ConvenienceStore,
        Bathroom,
        BloodCollectionCounter
    }

    public class NavigationGraph
    {
        private string _country;
        private string _cityCounty;
        private string _industryService;
        private string _ownerOrganization;
        private string _buildingName;

        //Guid is region's Guid
        private Dictionary<Guid, Region> _regions;

        //public Dictionary<Guid, List<RegionEdge>> _edges { get; set; }
        private Dictionary<Tuple<Guid,Guid>, List<RegionEdge>> _edges { get; set;}

        //Guid is region's Guid
        private Dictionary<Guid, Navigraph> _navigraphs { get; set; }

        public NavigationGraph(XmlDocument xmlDocument) {
            Console.WriteLine(">> NavigationGraph");

            // Read all attributes of <navigation_graph> tag
            XmlElement elementInNavigationGraph =
                (XmlElement)xmlDocument.SelectSingleNode("navigation_graph");
            _country = elementInNavigationGraph.GetAttribute("country");
            _cityCounty = elementInNavigationGraph.GetAttribute("city_county");
            _industryService = elementInNavigationGraph.GetAttribute("industry_service");
            _ownerOrganization = elementInNavigationGraph.GetAttribute("owner_organization");
            _buildingName = elementInNavigationGraph.GetAttribute("building_name");

            // Read all <region> blocks within <regions>
            XmlNodeList xmlRegion = xmlDocument.SelectNodes("navigation_graph/regions/region");
            foreach (XmlNode regionNode in xmlRegion)
            {
                Region region = new Region();

                // Read all attributes of each region
                XmlElement xmlElement = (XmlElement)regionNode;
                region._id = Guid.Parse(xmlElement.GetAttribute("id"));
                Console.WriteLine("id : " + region._id);

                region._IPSType =
                    (IPSType)Enum.Parse(typeof(IPSType),
                                        xmlElement.GetAttribute("ips_type"),
                                        false);
                Console.WriteLine("ips_type : " + region._IPSType);

                region._name = xmlElement.GetAttribute("name");
                Console.WriteLine("name : " + region._name);

                region._floor = Int32.Parse(xmlElement.GetAttribute("floor"));
                Console.WriteLine("floor : " + region._floor);

                // Read all <waypoint> within <region>
                XmlNodeList xmlWaypoint = regionNode.SelectNodes("waypoint");
                foreach (XmlNode waypointNode in xmlWaypoint)
                {
                    Waypoint waypoint = new Waypoint();

                    //Read all attributes of each waypint
                    XmlElement xmlWaypointElement = (XmlElement)waypointNode;
                    waypoint._id = Guid.Parse(xmlWaypointElement.GetAttribute("id"));
                    Console.WriteLine("id : " + waypoint._id);

                    waypoint._name = xmlWaypointElement.GetAttribute("name");
                    Console.WriteLine("name : " + waypoint._name);

                    waypoint._type =
                        (LocationType)Enum.Parse(typeof(LocationType),
                                                 xmlWaypointElement.GetAttribute("type"),
                                                 false);
                    Console.WriteLine("type : " + waypoint._type);

                    waypoint._category =
                        (CategoryType)Enum.Parse(typeof(CategoryType),
                                                 xmlWaypointElement.GetAttribute("category"),
                                                 false);
                    Console.WriteLine("category : " + waypoint._category);
                }

            }
            
            // Read all <edge> block within <regions>
            XmlNodeList xmlRegionEdge = xmlDocument.SelectNodes("navigation_graph/regions/edge");
            foreach (XmlNode regionEdgeNode in xmlRegionEdge)
            {
                RegionEdge regionEdge = new RegionEdge();

                // Read all attributes of each edge
                XmlElement xmlElement = (XmlElement)regionEdgeNode;
                regionEdge._region1 = Guid.Parse(xmlElement.GetAttribute("region1"));
                Console.WriteLine("region1 : " + regionEdge._region1);

                regionEdge._waypoint1 = Guid.Parse(xmlElement.GetAttribute("waypoint1"));
                Console.WriteLine("waypoint1 : " + regionEdge._waypoint1);

                regionEdge._region2 = Guid.Parse(xmlElement.GetAttribute("region2"));
                Console.WriteLine("region2 : " + regionEdge._region1);

                regionEdge._waypoint2 = Guid.Parse(xmlElement.GetAttribute("waypoint2"));
                Console.WriteLine("waypoint2 : " + regionEdge._waypoint2);

                regionEdge._biDirection = 
                    (DirectionalConnection)Enum.Parse(typeof(DirectionalConnection),
                                                      xmlElement.GetAttribute("bi_direction"),
                                                      false);
                Console.WriteLine("bi_direction : " + regionEdge._biDirection);

                regionEdge._source = 0;
                if (!String.IsNullOrEmpty(xmlElement.GetAttribute("source")))
                {
                    regionEdge._source = Int32.Parse(xmlElement.GetAttribute("source"));
                }
                Console.WriteLine("source : " + regionEdge._source);

                regionEdge._direction =
                    (CardinalDirection)Enum.Parse(typeof(CardinalDirection),
                                                  xmlElement.GetAttribute("direction"),
                                                  false);
                Console.WriteLine("direction : " + regionEdge._direction);

                regionEdge._connectionType =
                    (ConnectionType)Enum.Parse(typeof(ConnectionType),
                                               xmlElement.GetAttribute("connection_type"),
                                               false);
                Console.WriteLine("connection_type : " + regionEdge._connectionType);

            }

            // Read all <navigraph> blocks within <navigraphs>
            XmlNodeList xmlNavigraph = xmlDocument.SelectNodes("navigation_graph/navigraphs/navigraph");
            foreach (XmlNode navigraphNode in xmlNavigraph)
            {
                Navigraph navigraph = new Navigraph();

                // Read all attributes of each navigraph
                XmlElement xmlElement = (XmlElement)navigraphNode;
                navigraph._regionID = Guid.Parse(xmlElement.GetAttribute("region_id"));
                Console.WriteLine("region_id : " + navigraph._regionID);

                // Read all <waypoint> within <navigraph>
                XmlNodeList xmlWaypoint = navigraphNode.SelectNodes("waypoint");
                foreach (XmlNode waypointNode in xmlWaypoint)
                {
                    Waypoint waypoint = new Waypoint();

                    //Read all attributes of each waypint
                    XmlElement xmlWaypointElement = (XmlElement)waypointNode;
                    waypoint._id = Guid.Parse(xmlWaypointElement.GetAttribute("id"));
                    Console.WriteLine("id : " + waypoint._id);

                    waypoint._name = xmlWaypointElement.GetAttribute("name");
                    Console.WriteLine("name : " + waypoint._name);

                    waypoint._type =
                        (LocationType)Enum.Parse(typeof(LocationType),
                                                 xmlWaypointElement.GetAttribute("type"),
                                                 false);
                    Console.WriteLine("type : " + waypoint._type);

                    waypoint._category =
                        (CategoryType)Enum.Parse(typeof(CategoryType),
                                                 xmlWaypointElement.GetAttribute("category"),
                                                 false);
                    Console.WriteLine("category : " + waypoint._category);
                }

                // Read all <edge> block within <navigraph>
                XmlNodeList xmlWaypointEdge = navigraphNode.SelectNodes("edge");
                foreach (XmlNode waypointEdgeNode in xmlWaypointEdge)
                {
                    WaypointEdge waypointEdge = new WaypointEdge();

                    // Read all attributes of each edge
                    XmlElement xmlEdgeElement = (XmlElement)waypointEdgeNode;
                    waypointEdge._node1 = Guid.Parse(xmlEdgeElement.GetAttribute("node1"));
                    Console.WriteLine("node1 : " + waypointEdge._node1);

                    waypointEdge._node2 = Guid.Parse(xmlEdgeElement.GetAttribute("node2"));
                    Console.WriteLine("node2 : " + waypointEdge._node1);

                    waypointEdge._biDirection =
                        (DirectionalConnection)Enum.Parse(typeof(DirectionalConnection),
                                                          xmlEdgeElement.GetAttribute("bi_direction"),
                                                          false);
                    Console.WriteLine("bi_direction : " + waypointEdge._biDirection);

                    waypointEdge._source = 0;
                    if (!String.IsNullOrEmpty(xmlEdgeElement.GetAttribute("source")))
                    {
                        waypointEdge._source = Int32.Parse(xmlEdgeElement.GetAttribute("source"));
                    }
                    Console.WriteLine("source : " + waypointEdge._source);

                    waypointEdge._direction =
                        (CardinalDirection)Enum.Parse(typeof(CardinalDirection),
                                                      xmlEdgeElement.GetAttribute("direction"),
                                                      false);
                    Console.WriteLine("direction : " + waypointEdge._direction);

                    waypointEdge._connectionType =
                        (ConnectionType)Enum.Parse(typeof(ConnectionType),
                                                   xmlEdgeElement.GetAttribute("connection_type"),
                                                   false);
                    Console.WriteLine("connection_type : " + waypointEdge._connectionType);

                }

                // Read all <beacon> block within <navigraph/beacons>
                XmlNodeList xmlBeacon = navigraphNode.SelectNodes("beacons/beacon");
                foreach (XmlNode beaconNode in xmlBeacon)
                {
                    // Read all attributes of each beacon
                    XmlElement xmlBeaconElement = (XmlElement)beaconNode;
                    Console.WriteLine("uuid : " + Guid.Parse(xmlBeaconElement.GetAttribute("uuid")));

                    Console.WriteLine("waypoint_ids : " + xmlBeaconElement.GetAttribute("waypoint_ids"));

                }
            }

            Console.WriteLine("<< NavigationGraph");
        }

        public string GetIndustryServer() {
            return _industryService;
        }

        public Dictionary<Guid, Region> GetResions() {
            return _regions;
        }

        public class Navigraph
        {
            public Guid _regionID { get; set; }

            //Guid is waypoint's Guid
            public Dictionary<Guid, Waypoint> _waypoints { get; set; }

            public Dictionary<Tuple<Guid, Guid>, WaypointEdge> _edges { get; set; }

            //Guid is waypoint's Guid
            public Dictionary<Guid, List<Guid>> _beacons { get; set; }
        }

        public struct RegionEdge
        {
            //public Tuple<Guid, Guid> edge;
            public Guid _region1 { get; set; }
            public Guid _region2 { get; set; }
            public Guid _waypoint1 { get; set; }
            public Guid _waypoint2 { get; set; }
            public DirectionalConnection _biDirection { get; set; }
            public int _source { get; set; }
            public double _distance { get; set; }
            public CardinalDirection _direction { get; set; }
            public ConnectionType _connectionType { get; set; }
        }

        public struct WaypointEdge
        {
            public Guid _node1 { get; set; }
            public Guid _node2 { get; set; }
            public DirectionalConnection _biDirection { get; set; }
            public int _source { get; set; }
            public CardinalDirection _direction { get; set; }
            public ConnectionType _connectionType { get; set; }
            public double _distance { get; set; }
        }
    }
}
