using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GeoCoordinatePortable;

namespace IndoorNavigation.Models
{
    /// <summary>
    /// Beacon Model
    /// </summary>
    public class BeaconModel
    {
        /// <summary>
        /// Beacon UUID
        /// </summary>
        public Guid UUID { get; set; }
        /// <summary>
        /// Threshold (RSSI)
        /// </summary>
        public int Threshold { get; set; }
        /// <summary>
        /// Beacon installed floor
        /// </summary>
        public int Floor { get; set; }
        /// <summary>
        /// Beacon 上的箭頭指向的參考座標
        /// </summary>
        public GeoCoordinate MarkCoordinate { get; set; }
    }

    /// <summary>
    /// Beacon group model
    /// </summary>
    public class BeaconGroupModel
    {
        /// <summary>
        /// Group id
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Police station
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Beacons
        /// </summary>
        public List<BeaconModel> Beacons =
            new List<BeaconModel>();

        /// <summary>
        /// Group coordinate
        /// </summary>
        public GeoCoordinate Coordinate
        {
            get
            {
                // 取得群組內所有Beacon的座標
                List<GeoCoordinate> Coordinates = 
                    Beacons
                    .Select(c => Convert.ToCoordinate(c.UUID))
                    .ToList();

                // 將群組內所有Beacon座標取平均，計算群組中心座標
                double TotalLatitude = 0; double TotalLongitude = 0;

                foreach (GeoCoordinate Coordinate in Coordinates)
                {
                    TotalLatitude += Coordinate.Latitude;
                    TotalLongitude += Coordinate.Longitude;
                }

                return new GeoCoordinate(
                    TotalLatitude / Coordinates.Count(), 
                    TotalLongitude / Coordinates.Count());
            }
        }
    }

    /// <summary>
    /// Location connect model
    /// Pay attention to direction
    /// </summary>
    public class LocationConnectModel
    {
        /// <summary>
        /// Location A
        /// </summary>
        public BeaconGroupModel BeaconA { get; set; }
        /// <summary>
        /// Location B
        /// </summary>
        public BeaconGroupModel BeaconB { get; set; }
        /// <summary>
        /// Is it a two-way road?
        /// </summary>
        public bool IsTwoWay { get; set; }
    }
}
