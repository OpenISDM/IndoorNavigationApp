using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GeoCoordinatePortable;

namespace IndoorNavigation.Models
{
    public class Beacon
    {
        public Guid UUID { get; set; }
        public int RSSI { get; set; }
        public int Floor { get; set; }
        public GeoCoordinate MarkCoordinate { get; set; }
    }

    public class BeaconGroup
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public List<Beacon> Locations =
            new List<Beacon>();

        public GeoCoordinate Coordinate
        {
            get
            {
                List<GeoCoordinate> Coordinates = Locations.Select(c => Convert.ToCoordinate(c.UUID)).ToList();

                double TotalLatitude = 0; double TotalLongitude = 0;

                foreach (GeoCoordinate Coordinate in Coordinates)
                {
                    TotalLatitude += Coordinate.Latitude;
                    TotalLongitude += Coordinate.Longitude;
                }
                return new GeoCoordinate(TotalLatitude / Coordinates.Count(), TotalLongitude / Coordinates.Count());
            }
        }
    }

    public class LocationAssociation
    {
        public BeaconGroup BeaconA { get; set; }
        public BeaconGroup BeaconB { get; set; }
    }
}
