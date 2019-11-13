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
 *      This class is used to select the route specified by the starting
 *      point, destination, and user preferences. When in navigation,
 *      the class will give the next waypoint, and when the user is in
 *      the wrong way, the class will re-route.
 *      
 * Version:
 *
 *      1.0.0, 201911123eee3333e
 * 
 * File Name:
 *
 *      IPSModules.cs
 *
 * Abstract:
 *
 *      Waypoint-based navigator is a mobile Bluetooth navigation application
 *      that runs on smart phones. It is structed to support anywhere 
 *      navigation. Indoors in areas covered by different indoor positioning 
 *      system (IPS) and outdoors covered by GPS. In particilar, it can rely
 *      on BeDIS (Building/environment Data and Information System) for
 *      indoor positioning. Using this IPS, the navigator does not need to 
 *      continuously monitor its own position, since the IPS broadcast to the 
 *      navigator the location of each waypoint. 
 *      This version makes use of Xamarin.Forms, which is a complete 
 *      cross-platform UI tookit that runs on both iOS and Android.
 *      
 * Authors:
 *
 *      Eric Lee, ericlee@iis.sinica.edu.tw
 *     
 *
 */

using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using Dijkstra.NET.Model;
using Dijkstra.NET.Extensions;
using IndoorNavigation.Models.NavigaionLayer;
using IndoorNavigation.Models;
using IndoorNavigation.Modules.IPSClients;
using System.Diagnostics.Contracts;

delegate void _addInterestedBeacon(Guid regionGuid, List<Guid> waypointGuids);
delegate void _runClient();

namespace IndoorNavigation.Modules
{
    public class IPSModules : IDisposable
    {
        private IIPSClient _IPSClient;
        private List<IIPSClient> _multipleClient;
        private IIPSClient _waypointClient;
        private IIPSClient _ibeaconCLient;
        private bool haveLBeacon;
        private bool haveIBeacon;
        private bool haveGPS;
        private IPSModules _IPSModules;
        private List<WaypointBeaconsMapping> _monitorLBeaconGuid;
        private List<WaypointBeaconsMapping> _monitorIBeaconGuid;
        public NavigationEvent _event { get; private set; }
        NavigationGraph _navigationGraph;
        private _addInterestedBeacon _lbeacon;
        private _addInterestedBeacon _ibeacon;
        private _addInterestedBeacon _gps;
        private int _firstStep = -1;
        private bool _isKeepDetection;

        public IPSModules(NavigationGraph navigationGraph)
        {
            _IPSClient = new WaypointClient();
            _ibeaconCLient = new IBeaconClient();
            _waypointClient = new WaypointClient();
            _multipleClient = new List<IIPSClient>();
            _event = new NavigationEvent();
            _navigationGraph = navigationGraph;
            haveGPS = false;
            haveIBeacon = false;
            haveLBeacon = false;
            _monitorLBeaconGuid = new List<WaypointBeaconsMapping>();
            _monitorIBeaconGuid = new List<WaypointBeaconsMapping>();
            _isKeepDetection = true;
            _lbeacon = new _addInterestedBeacon(ADDLBeacon);
            _ibeacon = new _addInterestedBeacon(ADDIBeacon);
            _gps = new _addInterestedBeacon(ADDGPS);
        }

        public void ADDIBeacon(Guid regionGuid, List<Guid> waypointGuids)
        {
            IPSType ipsType = _navigationGraph.GetRegionIPSType(regionGuid);
           
            if (ipsType == IPSType.iBeacon)
            {
                haveIBeacon = true;
                List<WaypointBeaconsMapping> tempIBeaconMapping = new List<WaypointBeaconsMapping>();
                tempIBeaconMapping = FindTheMappingOfWaypointAndItsBeacon(regionGuid, waypointGuids);
                _monitorIBeaconGuid.AddRange(tempIBeaconMapping);
            }
        }
        public void ADDLBeacon(Guid regionGuid, List<Guid> waypointGuids)
        {
            IPSType ipsType = _navigationGraph.GetRegionIPSType(regionGuid);
            if (ipsType == IPSType.LBeacon)
            {
                haveLBeacon = true;
                List<WaypointBeaconsMapping> tempLBeaconMapping = new List<WaypointBeaconsMapping>();
                tempLBeaconMapping = FindTheMappingOfWaypointAndItsBeacon(regionGuid, waypointGuids);
                _monitorLBeaconGuid.AddRange(tempLBeaconMapping);
            }
        }
        public void ADDGPS(Guid regionGuid, List<Guid>waypointGuids)
        {
            IPSType ipsType = _navigationGraph.GetRegionIPSType(regionGuid);
            if (ipsType == IPSType.GPS)
            {
                haveGPS = true; 
            }
        }
        public List<WaypointBeaconsMapping> FindTheMappingOfWaypointAndItsBeacon(Guid regionGuid, List<Guid> waypoints)
        {
            List<WaypointBeaconsMapping> waypointBeaconsMappings = new List<WaypointBeaconsMapping>();
            foreach(Guid waypointID in waypoints)
            {
                RegionWaypointPoint regionWaypointPoint = new RegionWaypointPoint();
                regionWaypointPoint._regionID = regionGuid;
                regionWaypointPoint._waypointID = waypointID;
                List<Guid> beaconIDs = new List<Guid>();
                beaconIDs = _navigationGraph.GetAllBeaconIDInOneWaypointOfRegion(regionGuid, waypointID);
                Dictionary<Guid, int> beaconThresholdMapping = new Dictionary<Guid, int>();
                for(int i = 0; i < beaconIDs.Count(); i++)
                {
                    beaconThresholdMapping.Add(beaconIDs[i], _navigationGraph.GetBeaconRSSIThreshold(regionGuid, beaconIDs[i]));
                }
                waypointBeaconsMappings.Add(new WaypointBeaconsMapping
                {
                    _WaypointIDAndRegionID = regionWaypointPoint,
                    _Beacons = beaconIDs,
                    _BeaconThreshold = beaconThresholdMapping
                });
            }
            return waypointBeaconsMappings;
        }
        public void AtStarting_ReadALLIPSType(List<Guid> allRegionGuid)
        {
            foreach (Guid regionGuid in allRegionGuid)
            {
                List<Guid> waypointGuids = new List<Guid>();
                waypointGuids = _navigationGraph.GetAllWaypointIDInOneRegion(regionGuid);
                _lbeacon(regionGuid, waypointGuids);
                _ibeacon(regionGuid, waypointGuids);
                _gps(regionGuid, waypointGuids);
            }

            StartAllExistClient();
        }
        public void AddNextWaypointInterestedGuid(Guid regionGuid, Guid waypointGuid)
        {
            _lbeacon(regionGuid, new List<Guid> { waypointGuid });
            _ibeacon(regionGuid, new List<Guid> { waypointGuid });
            _gps(regionGuid, new List<Guid> { waypointGuid });
        }
        public void AddNextNextWaypointInterestedGuid(Guid regionGuid, Guid waypointGuid)
        {
            _lbeacon(regionGuid, new List<Guid> { waypointGuid });
            _ibeacon(regionGuid, new List<Guid> { waypointGuid });
            _gps(regionGuid, new List<Guid> { waypointGuid });
        }

        public void AddWrongWaypointInterestedGuid(Guid regionGuid, Guid waypointGuid)
        {
            _lbeacon(regionGuid, new List<Guid> { waypointGuid });
            _ibeacon(regionGuid, new List<Guid> { waypointGuid });
            _gps(regionGuid, new List<Guid> { waypointGuid });
        }

        // In this fuction, we deal the initial location situation.
        // We do not know where the users are, therefore, we open all
        // types of Clients that the map have. 
        public void StartAllExistClient()
        {
            if (haveLBeacon == true)
            {
                _waypointClient = new WaypointClient();
                _waypointClient._event._eventHandler += new EventHandler(PassMatchedWaypointAndRegionToSession);
                _waypointClient.SetWaypointList(_monitorLBeaconGuid);
            }
            if(haveIBeacon == true)
            {
                _ibeaconCLient = new IBeaconClient();
                _ibeaconCLient._event._eventHandler += new EventHandler(PassMatchedWaypointAndRegionToSession);
                _ibeaconCLient.SetWaypointList(_monitorIBeaconGuid);
            }
            if(haveGPS == true)
            {

            }
        }
        // private static void Detect()
        public IPSType GetIPSType(Guid regionGuid)
        {
            IPSType regionIPSType = new IPSType();
            regionIPSType =
                   _navigationGraph.GetRegionIPSType(regionGuid);
            return regionIPSType;
        }

        public void IsLBeaconType()
        {
            _IPSClient = new WaypointClient();
           // _IPSClient.SetWaypointList(_monitorLBeaconGuid);
            _IPSClient._event._eventHandler += new EventHandler(PassMatchedWaypointAndRegionToSession);
            haveLBeacon = true;
        }
        public void IsIBeaconType()
        {
            _IPSClient = new IBeaconClient();
            //_IPSClient.SetWaypointList(_monitorIBeaconGuid);
            _IPSClient._event._eventHandler += new EventHandler(PassMatchedWaypointAndRegionToSession);
            haveIBeacon = true;
        }

        public void IsGPSType()
        {
            
        }
        //This function gets the matched waypoint and region from each Clients, then pass the waypoint and region information to Session
        public void PassMatchedWaypointAndRegionToSession(object sender, EventArgs args)
        {
            RegionWaypointPoint matchedWaypointAndRegion = new RegionWaypointPoint();
            matchedWaypointAndRegion._waypointID = (args as WaypointSignalEventArgs)._detectedRegionWaypoint._waypointID;
            matchedWaypointAndRegion._regionID = (args as WaypointSignalEventArgs)._detectedRegionWaypoint._regionID;
            _event.OnEventCall(new WaypointSignalEventArgs
            {
                _detectedRegionWaypoint = matchedWaypointAndRegion
            });
            return;
        }
        public void CompareToCurrentAndNextIPSType(Guid currentRegionGuid, Guid nextRegionGuid, int firstStep)
        {
            IPSType currentIPSType = _navigationGraph.GetRegionIPSType(currentRegionGuid);
            IPSType nextIPSType = _navigationGraph.GetRegionIPSType(nextRegionGuid);
            if (!nextIPSType.Equals(currentIPSType)||firstStep==_firstStep)
            {
                HaveBeaconAllFalse();
                _IPSClient.Stop();
                _IPSClient._event._eventHandler -= new EventHandler(PassMatchedWaypointAndRegionToSession);
                OpenCurrentIPSClient(currentIPSType);
            }
        }

        public void OpenCurrentIPSClient(IPSType currentIPSType)
        {
            switch (currentIPSType)
            {
                case IPSType.LBeacon:
                    IsLBeaconType();
                    break;
                case IPSType.iBeacon:
                    IsIBeaconType();
                    break;
                case IPSType.GPS:
                    IsGPSType();
                    break;
            }
        }

        public void HaveBeaconAllFalse()
        {
            haveLBeacon = false;
            haveIBeacon = false;
            haveGPS = false;
        }

        public void SetMonitorBeaconList()
        {
            if (haveLBeacon == true)
            {
                _IPSClient.SetWaypointList(_monitorLBeaconGuid);
            }
            else if (haveIBeacon==true)
            {
                _IPSClient.SetWaypointList(_monitorIBeaconGuid);
            }
            else if (haveGPS==true)
            {

            }
        }

        public void CloseStartAllExistClient()
        {
            if (haveLBeacon == true)
            {
                _waypointClient.Stop();
                _monitorLBeaconGuid = new List<WaypointBeaconsMapping>();
                _waypointClient._event._eventHandler -= new EventHandler(PassMatchedWaypointAndRegionToSession);
                
            }
            if (haveIBeacon == true)
            {
                _ibeaconCLient.Stop();
                _monitorIBeaconGuid = new List<WaypointBeaconsMapping>();
                _ibeaconCLient._event._eventHandler -= new EventHandler(PassMatchedWaypointAndRegionToSession);
            }
            if (haveGPS == true)
            {

            }
        }

        public void OpenBeconScanning()
        {
            _IPSClient.DetectWaypoints();
            _ibeaconCLient.DetectWaypoints();
            _waypointClient.DetectWaypoints();
        }

        public void Close()
        {
            _lbeacon -= new _addInterestedBeacon(ADDIBeacon);
            _ibeacon -= new _addInterestedBeacon(ADDLBeacon);
            _gps -= new _addInterestedBeacon(ADDGPS);
            _monitorIBeaconGuid = new List<WaypointBeaconsMapping>();
            _monitorLBeaconGuid = new List<WaypointBeaconsMapping>();
            _IPSClient._event._eventHandler -= new EventHandler(PassMatchedWaypointAndRegionToSession);
            haveGPS = false;
            haveIBeacon = false;
            haveLBeacon = false;
        }
        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
