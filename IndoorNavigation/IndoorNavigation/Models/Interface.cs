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
 *      Interfa.cs
 * 
 * Abstract:
 *      
 *      Define the interface required to connect IOS projects and 
 *      Android project and map information.
 *
 * Authors:
 * 
 *      Kenneth Tang, kenneth@gm.nssh.ntpc.edu.tw
 * 
 */

using GeoCoordinatePortable;
using System;
using System.Collections.Generic;

namespace IndoorNavigation.Models
{
    #region Map information interface

    /// <summary>
    /// 保留給IBeacon資訊使用的介面
    /// </summary>
    public interface IIBeacon
    {
        /// <summary>
        /// IBeacon coordinate
        /// </summary>
        GeoCoordinate IBeaconCoordinate { get; set; }
    }

    /// <summary>
    /// LBeacon資訊
    /// </summary>
    public interface ILBeacon
    {
        /// <summary>
        /// Beacon 安裝方向
        /// Beacon 上的箭頭指向的參考座標
        /// 目前版本尚未使用
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
    /// 一個群體內有多個Beacon，用於儲存在手機上的離線地圖資料
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
    /// 一個道路連接兩個地點，用於儲存在手機上的離線地圖資料
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

    #endregion

    #region Interface for connecting IOS projects and Android projects

    /// <summary>
    /// 實作與beacon scan物件連接的介面
    /// 在IOS和Android專案各有一個Beacon scan物件
    /// Beacon scan功能必須使用系統提供的原生API
    /// </summary>
    public interface IBeaconScan
    {
        void Init(Action<List<BeaconSignalModel>> SendSignalFunction);
        void StartScan(List<Guid> BeaconsUUID);
        void StopScan();
        void Close();
    }

    #endregion
}
