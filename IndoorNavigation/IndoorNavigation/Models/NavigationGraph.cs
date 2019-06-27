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
        public string _country { get; set; }
        public string _cityCounty { get; set; }
        public string _industryService { get; set; }
        public string _ownerOrganization { get; set; }
        public string _buildingName { get; set; }

        //Guid is region's Guid
        public Dictionary<Guid, OneRegion> _regions { get; set; }

        //Guid is Source Region's Guid
        public Dictionary<Guid, RegionEdge> _edges { get; set; }

        //Guid is region's Guid
        public Dictionary<Guid, Navigraph> _navigraphs { get; set; }
    }

    public class OneRegion
    {
        public Guid _id { get; set; }
        public string _name { get; set; }
        public int _floor { get; set; }
        public List<RegionNeighbor> _neighbors { get; set; }
        public Dictionary<CategoryType, List<Waypoint>> _waypointsByCategory { get; set; }
    }

    public class RegionNeighbor
    {
        public Guid _id { get; set; }
    }

    public class RegionEdge
    {
        public Guid _sinkRegionID { get; set; }
        public Guid _sourceWaypointID { get; set; }
        public Guid _sinkWaypointID { get; set; }
        public double _distance { get; set; }
        public CardinalDirection _direction { get; set; }
        public ConnectionType _connectionType { get; set; }
    }

    public class Navigraph
    {
        public Guid _id { get; set; }
        public IPSType _IPSType { get; set; }

        //Guid is waypoint's Guid
        public Dictionary<Guid, Waypoint> _waypoints { get; set; }

        //Guid is source waypoint's Guid
        public Dictionary<Guid, WaypointEdge> _edges { get; set; }

        //Guid is waypoint's Guid
        public Dictionary<Guid, List<Guid>> _beacons { get; set; }
    }

    public class Waypoint
    {
        public Guid _id { get; set; }
        public string _name { get; set; }
        public string _type { get; set; }
        public CategoryType _category { get; set; }
        public List<WaypointNeighbor> _neighbors { get; set; }
    }

    public class WaypointNeighbor
    {
        public Guid _id { get; set; }
    }

    public class WaypointEdge
    {
        public Guid _sinkWaypointID { get; set; }
        public CardinalDirection _direction { get; set; }
        public ConnectionType _connectionType { get; set; }
        public double _distance { get; set; }
    }
}
