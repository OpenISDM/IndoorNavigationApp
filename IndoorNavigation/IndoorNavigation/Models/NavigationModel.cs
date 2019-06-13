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
 *      This file contains models which have Beacon's attribute and data
 *      of route planning, these are used for navigator.
 * 
 * File Name:
 *
 *      NavigationModel.cs
 *
 * Abstract:
 *
 *      
 *
 * Authors:
 *
 *      Kenneth Tang, kenneth@gm.nssh.ntpc.edu.tw
 *      Paul Chang, paulchang@iis.sinica.edu.tw
 *
 */



using System;
using System.Collections.Generic;

// TODO: Should be moved
namespace IndoorNavigation.Models
{
    /// <summary>
    /// The signal element for signal processing for LBeacon
    /// </summary>
    public class BeaconSignal
    {
        public Guid UUID { get; set; }
        public int Major { get; set; }
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

        public BeaconSignalModel()
        {
            Timestamp = DateTime.Now;
        }
    }
}
