using System;
using System.Collections.Generic;
using System.Linq;
using Dijkstra.NET.Model;
using GeoCoordinatePortable;
using System.Xml.Serialization;
using System.Xml;

namespace IndoorNavigation.Models.NavigaionLayer
{
    public class NavigationStructure
    {
        public string _country { get; set; }
        public string _cityCountry { get; set;}
        public string _industryService { get; set; }
        public string _ownerOrganization { get; set; }
        public string _buildingName { get; set; }

        //Guid is region's Guid
        public Dictionary<Guid, Region> _regions { get; set; }

        //Guid is Source Region's Guid
        public Dictionary<Guid, RegionEdge> _edges { get; set; }

        //Guid is region's Guid
        public Dictionary<Guid, Navigraph> _navigraphs { get; set; }
    }

    public struct RegionNeighbors
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
        public string ReferenceDirection
        {
            get { return _direction.ToString(); }
            set
            {
                if (string.IsNullOrEmpty(value) ||
                    !Enum.GetNames(typeof(CardinalDirection)).Contains(value))
                {
                    //Direction = CardinalDirection.NoDirection;
                }
                else
                {

                    _direction = (CardinalDirection)Enum.Parse(typeof(CardinalDirection), value);
                }
            }
        }


        public ConnectionType _connectionType { get; set; }
        public string _connectiontype
        {
            get { return _connectionType.ToString(); }
            set
            {
                if (string.IsNullOrEmpty(value) ||
                    !Enum.GetNames(typeof(ConnectionType)).Contains(value))
                {
                    //Connection = ConnectionType.NoConnectionType;
                }
                else
                {
                    _connectionType = (ConnectionType)Enum.Parse(typeof(ConnectionType), value);
                }
            }
        }
    }

    public class Navigraph
    {
        public Guid _id;
        public IPSType _IPSClientType { get; set; }
        [XmlElement("IPStype")]
        public string _IPStype
        {
            get { return _IPSClientType.ToString(); }
            set
            {
                if (string.IsNullOrEmpty(value) || !Enum.GetNames(typeof(IPSType)).Contains(value))
                {
                    //IPSClientType = IPSType.NoIPSType;
                }
                else
                {
                    _IPSClientType = (IPSType)Enum.Parse(typeof(IPSType), value);
                }
            }
        }

        //Guid is waypoint's Guid
        Dictionary<Guid, Waypoint> _waypoints;

        //Guid is source waypoint's Guid
        Dictionary<Guid, WaypointEdge> _edges;

        //Guid is waypoint's Guid
        Dictionary<Guid, List<Guid>> _beacons;
    }

    /*
    public class Waypoint
    {
        public Guid _id;
        public string _name;
        public string _type;
        public CategoryType _category { get; set; }
        public string CategoryType
        {
            get { return _category.ToString(); }
            set
            {
                if (string.IsNullOrEmpty(value) || !Enum.GetNames(typeof(CategoryType)).Contains(value))
                {
                    //Direction = CardinalDirection.NoDirection;
                }
                else
                {

                    _category = (CategoryType)Enum.Parse(typeof(CategoryType), value);
                }
            }
        }
        public List<WaypointNeighbors> _neighbors;
    }
    */

    public struct WaypointNeighbors
    {
        public Guid _id;
    }

    public struct WaypointEdge
    {
        public Guid _sinkWaypointID;

        public CardinalDirection _direction { get; set; }
        public string ReferenceDirection
        {
            get { return _direction.ToString(); }
            set
            {
                if (string.IsNullOrEmpty(value) ||
                    !Enum.GetNames(typeof(CardinalDirection)).Contains(value))
                {
                    //Direction = CardinalDirection.NoDirection;
                }
                else
                {

                    _direction = (CardinalDirection)Enum.Parse(typeof(CardinalDirection), value);
                }
            }
        }

        public ConnectionType _connectionType { get; set; }
        public string _connectiontype
        {
            get { return _connectionType.ToString(); }
            set
            {
                if (string.IsNullOrEmpty(value) ||
                    !Enum.GetNames(typeof(ConnectionType)).Contains(value))
                {
                    //Connection = ConnectionType.NoConnectionType;
                }
                else
                {
                    _connectionType = (ConnectionType)Enum.Parse(typeof(ConnectionType), value);
                }
            }
        }

        public double _distance;
    }
}
