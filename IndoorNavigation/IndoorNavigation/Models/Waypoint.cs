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
 *      Waypoint.cs
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
 *      Eric Lee, ericlee@iis.sinica.edu.tw
 */
using System;
using System.Collections.Generic;
using IndoorNavigation.Models.NavigaionLayer;

namespace IndoorNavigation.Models
{   
    public class Waypoint
    {
        public Guid _id { get; set; }
        public string _name { get; set; }
        public LocationType _type { get; set; }
        public CategoryType _category { get; set; }
        public List<Guid> _neighbors { get; set; }

        // We should save lon/lat inforamtion for calculating distance of
        // WaypointEdge later while parsing <edge> in XML
        public double _lon { get; set; }
        public double _lat { get; set; }
    }

    public class PortalWaypoints
    {
        public Guid _portalWaypoint1 { get; set; }
        public Guid _portalWaypoint2 { get; set; }
    }

    public class GroupWaypoint
    {
        public List<RegionWaypointPoint> _regionsAndWaypoints;
        public waypointDecisionOrIgnore _decisionOrIgnore;
    }
}
