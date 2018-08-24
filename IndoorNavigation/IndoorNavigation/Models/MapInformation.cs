using System;
using System.Collections.Generic;
using System.Linq;
using GeoCoordinatePortable;

namespace IndoorNavigation.Models
{
    /// <summary>
    /// Beacon group
    /// </summary>
    public abstract class BeaconGroup
    {
        /// <summary>
        /// Group id
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// ex: Police station
        /// </summary>
        public string Name { get; set; }
    }

    /// <summary>
    /// Location connect
    /// </summary>
    public abstract class LocationConnect
    {
        /// <summary>
        /// Is it a two-way road?
        /// </summary>
        public bool IsTwoWay { get; set; }
    }

    /// <summary>
    /// 表示要監聽的Beacon。 包含 監聽的門檻值、安裝樓層、Beacon安裝方向
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
        /// Beacon 安裝方向
        /// Beacon 上的箭頭指向的參考座標
        /// </summary>
        public GeoCoordinate MarkCoordinate { get; set; }
    }

    /// <summary>
    /// 一個Beacon群體，此群體視為一個地點
    /// </summary>
    public class BeaconGroupModel : BeaconGroup, IBeaconGroupModel
    {
        /// <summary>
        /// Beacon 集合
        /// </summary>
        public List<BeaconModel> Beacons { get; set; }

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
    /// 一個Beacon群體，此群體視為一個地點，此物件用於離線地圖資料
    /// </summary>
    public class BeaconGroupModelForMapFile : BeaconGroup, 
        IBeaconGroupModelForMapFile
    {
        /// <summary>
        /// Beacon 集合
        /// </summary>
        public List<Guid> Beacons { get; set; }
    }

    /// <summary>
    /// 連接兩個地點的道路
    /// Pay attention to direction
    /// </summary>
    public class LocationConnectModel : LocationConnect, ILocationConnectModel
    {
        /// <summary>
        /// 地點A
        /// </summary>
        public BeaconGroupModel BeaconA { get; set; }
        /// <summary>
        /// 地點B
        /// </summary>
        public BeaconGroupModel BeaconB { get; set; }
    }

    /// <summary>
    /// 連接兩個地點的道路，此物件用於離線地圖資料
    /// Pay attention to direction
    /// </summary>
    public class LocationConnectModelForMapFile : LocationConnect, 
        ILocationConnectModelForMapFile
    {
        /// <summary>
        /// 地點A
        /// </summary>
        public Guid BeaconA { get; set; }
        /// <summary>
        /// 地點B
        /// </summary>
        public Guid BeaconB { get; set; }
    }
}
