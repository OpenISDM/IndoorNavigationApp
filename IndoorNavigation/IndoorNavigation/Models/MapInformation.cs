/*
 * Copyright (c) 2018 Academia Sinica, Institude of Information Science
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
 * File Name:
 * 
 *      MapInformation.cs
 * 
 * Abstract:
 *      
 *      地圖上的物件，這些物件包含:Beacon物件、Beacon群組物件、用來記錄連接兩個位置的物件
 *
 * Authors:
 * 
 *      Kenneth Tang, kenneth@gm.nssh.ntpc.edu.tw
 * 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using GeoCoordinatePortable;

namespace IndoorNavigation.Models
{
    /// <summary>
    /// Beacon
    /// </summary>
    public abstract class Beacon
    {
        /// <summary>
        /// Beacon UUID
        /// </summary>
        public Guid UUID { get; set; }
        /// <summary>
        /// IBeacon Major field
        /// </summary>
        public int Major { get; set; }
        /// <summary>
        /// IBeacon Minor field
        /// </summary>
        public int Minor { get; set; }
        /// <summary>
        /// Threshold (RSSI)
        /// </summary>
        public int Threshold { get; set; }
        /// <summary>
        /// Beacon installed floor
        /// </summary>
        public virtual float Floor { get; set; }
    }

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
    /// 表示要監聽的IBeacon。 包含 監聽的門檻值、安裝樓層、Beacon安裝位置、
    /// Major、Minor
    /// </summary>
    public class IBeaconModel : Beacon, IIBeacon
    {
        /// <summary>
        /// IBeacon coordinate
        /// </summary>
        public GeoCoordinate IBeaconCoordinate { get; set; }
    }

    /// <summary>
    /// 表示要監聽的LBeacon。 包含 監聽的門檻值、安裝樓層、Beacon安裝方向
    /// </summary>
    public class LBeaconModel : Beacon, ILBeacon
    {
        /// <summary>
        /// Beacon 安裝方向
        /// Beacon 上的箭頭指向的參考座標
        /// 目前版本尚未使用
        /// </summary>
        public GeoCoordinate MarkCoordinate { get; set; }

        public override float Floor { get { return this.GetFloor(); } }
    }

    /// <summary>
    /// 一個Beacon群體，此群體視為一個地點
    /// </summary>
    public class BeaconGroupModel : BeaconGroup, IBeaconGroupModel
    {
        /// <summary>
        /// Beacon 集合
        /// </summary>
        public List<Beacon> Beacons { get; set; }

        /// <summary>
        /// Group coordinate
        /// 假如這個群組有2顆Beacon並排，兩顆Beacon中間的位置為Beacon群組的座標
        /// </summary>
        public GeoCoordinate Coordinate
        {
            get
            {
                // 取得群組內所有Beacon的座標
                List<GeoCoordinate> Coordinates =
                    Beacons.Select(c => c.GetCoordinate()).ToList();

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
    /// 一個Beacon群體，此群體視為一個地點，此物件用於儲存在手機上的離線地圖資料
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
    /// 連接兩個地點的道路，此物件用於儲存在手機上的離線地圖資料
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
