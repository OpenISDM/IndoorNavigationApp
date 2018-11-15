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
 *      The model would be used by each module
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
    /// The signal element for signal processing for LBeacon
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
    /// The element of monitoring LBeacon
    /// </summary>
    public class BeaconSignalModel : BeaconSignal
    {
        public int TxPower { get; set; }
        public DateTime Timestamp { get; set; }
    }

    /// <summary>
    /// The information of next location
    /// </summary>
    public class NextInstructionModel
    {
        /// <summary>
        /// Next location
        /// </summary>
        public BeaconGroupModel NextPoint { get; set; }
        /// <summary>
        /// The angle to turn to next location
        /// </summary>
        public int Angle { get; set; }
    }
}
