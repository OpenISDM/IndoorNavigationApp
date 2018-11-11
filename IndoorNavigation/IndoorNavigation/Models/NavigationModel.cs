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
 *      NavigationModel.cs
 * 
 * Abstract:
 *      
 *      各模組使用到的模型
 *
 * Authors:
 * 
 *      Kenneth Tang, kenneth@gm.nssh.ntpc.edu.tw
 * 
 */

using System;

namespace IndoorNavigation.Models
{
    /// <summary>
    /// 做訊號處理使用的Beacon訊號物件
    /// </summary>
    public class BeaconSignal
    {
        public Guid UUID { get; set; }
        /// <summary>
        /// IBeacon Major field
        /// </summary>
        public int Major { get; set; }
        /// <summary>
        /// IBeacon Minor field
        /// </summary>
        public int Minor { get; set; }
        public int RSSI { get; set; }
    }

    /// <summary>
    /// 監聽到的Beacon物件
    /// </summary>
    public class BeaconSignalModel : BeaconSignal
    {
        public int TxPower { get; set; }
        public DateTime Timestamp { get; set; }
    }

    /// <summary>
    /// 前往下一個地點的資訊
    /// </summary>
    public class NextInstructionModel
    {
        /// <summary>
        /// 下一個地點
        /// </summary>
        public BeaconGroupModel NextPoint { get; set; }
        /// <summary>
        /// 前往下一個地點要轉向的角度
        /// </summary>
        public int Angle { get; set; }
    }
}
