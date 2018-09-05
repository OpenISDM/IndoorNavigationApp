using GeoCoordinatePortable;
using System;
using System.Collections.Generic;

namespace IndoorNavigation.Models
{
    public interface IIBeacon
    {
        /// <summary>
        /// IBeacon Major field
        /// </summary>
        int Major { get; set; }
        /// <summary>
        /// IBeacon Minor field
        /// </summary>
        int Minor { get; set; }
        /// <summary>
        /// IBeacon coordinate
        /// </summary>
        GeoCoordinate IBeaconCoordinate { get; set; }
    }

    public interface ILBeacon
    {
        /// <summary>
        /// Beacon 安裝方向
        /// Beacon 上的箭頭指向的參考座標
        /// </summary>
        GeoCoordinate MarkCoordinate { get; set; }
    }

    /// <summary>
    /// 一個群體內有多個Beacon
    /// </summary>
    public interface IBeaconGroupModel
    {
        /// <summary>
        /// Beacon 集合
        /// </summary>
        List<Beacon> Beacons { get; set; }
        /// <summary>
        /// 群組中心點座標
        /// </summary>
        GeoCoordinate Coordinate { get; }
    }

    /// <summary>
    /// 一個群體內有多個Beacon，用於離線地圖資料
    /// </summary>
    public interface IBeaconGroupModelForMapFile
    {
        /// <summary>
        /// Beacon 集合
        /// </summary>
        List<Guid> Beacons { get; set; }
    }

    /// <summary>
    /// 一個道路連接兩個地點
    /// </summary>
    public interface ILocationConnectModel
    {
        /// <summary>
        /// 地點A
        /// </summary>
        BeaconGroupModel BeaconA { get; set; }
        /// <summary>
        /// 地點B
        /// </summary>
        BeaconGroupModel BeaconB { get; set; }
    }

    /// <summary>
    /// 一個道路連接兩個地點，用於離線地圖資料
    /// </summary>
    public interface ILocationConnectModelForMapFile
    {
        /// <summary>
        /// 地點A
        /// </summary>
        Guid BeaconA { get; set; }
        /// <summary>
        /// 地點B
        /// </summary>
        Guid BeaconB { get; set; }
    }
}
