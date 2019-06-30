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
 *      This file contains all the interfaces required by the application,
 *      such as the interface of IPSClient and the interface for 
 *      both iOS project and the Android project to allow the Xamarin.Forms 
 *      app to access the APIs on each platform.
 *      
 * Version:
 *
 *      1.0.0, 20190629
 * 
 * File Name:
 *
 *      Region.cs
 *
 * Abstract:
 *
 *      Waypoint-based navigator is a mobile Bluetooth navigation application
 *      that runs on smart phones. It is structed to support anywhere 
 *      navigation indoors in areas covered by different indoor positioning 
 *      system (IPS) and outdoors covered by GPS.In particilar, it can rely on
 *      BeDIS (Building/environment Data and Information System) for indoor 
 *      positioning. This IPS provides a location beacon at every waypoint. The 
 *      beacon brocasts its own coordinates; Consequesntly, the navigator does 
 *      not need to continuously monitor its own position.
 *      This version makes use of Xamarin.Forms, which is a cross-platform UI 
 *      tookit that runs on both iOS and Android.
 *
 * Authors:
 *
 *      Chun-Yu Lai, chunyu1202@gmail.com
 *
 */
using System;
using System.Collections.Generic;
using IndoorNavigation.Models.NavigaionLayer;

namespace IndoorNavigation.Models
{
    public class Region
    {
        public Guid _id { get; set; }
        public IPSType _IPSType { get; set; }
        public string _name { get; set; }
        public int _floor { get; set; }
        public List<Guid> _neighbors { get; set; }
        public Dictionary<CategoryType, List<Waypoint>> _waypointsByCategory { get; set; }
    }

    public class RegionWaypointPoint
    {
        public Guid _regionID { get; set; }
        public Guid _waypointID { get; set; }
    }
}
