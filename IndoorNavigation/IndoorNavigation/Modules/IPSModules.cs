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
 * 
 *     
 *      
 * Version:
 *
 *      1.0.0, 201911123
 * 
 * File Name:
 *
 *      IPSModules.cs
 *
 * Abstract:
 *
 *      This file is used to adjust the IPS. We now have LBeacon and IBeacon. This file
 *      is the intermedium of Session.cs and the different kinds of IPSClient. This file also will
 *      get the interested beacon Guid and pass to IPS client. When the event handler get the
 *      interested waypoint and region, this file will also pass the intersted waypint and region
 *      to Session.
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

        //TODO : if add new client, here needs to add a new function, like the format below
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
            //TODO: If add new IPSClient, here needs to add
        }
        // private static void Detect()
        public IPSType GetIPSType(Guid regionGuid)
        {
            IPSType regionIPSType = new IPSType();
            regionIPSType =
                   _navigationGraph.GetRegionIPSType(regionGuid);
            return regionIPSType;
        }

        //TODO : If add new client, here needs to add new function just likes below.
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
            CleanAllMappingBeaconList();
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
                //TODO;f add new client, here needs to add
            }
        }

        public void HaveBeaconAllFalse()
        {
            haveLBeacon = false;
            haveIBeacon = false;
            haveGPS = false;
            //TODO: if add new client, here needs to add
        }

        public void CleanAllMappingBeaconList()
        {
            _monitorIBeaconGuid = new List<WaypointBeaconsMapping>();
            _monitorLBeaconGuid = new List<WaypointBeaconsMapping>();
            //TODO: if add new client, here needs to add
        }
        //Qw set the possible interested beacon Guid to each client
        public void SetMonitorBeaconList()
        {
            if (haveLBeacon == true)
            {
                _IPSClient.SetWaypointList(_monitorLBeaconGuid);
            }
            if (haveIBeacon==true)
            {
                for(int i = 0;i<_monitorIBeaconGuid.Count();i++)
                {
                    for(int j = 0;j<_monitorIBeaconGuid[i]._Beacons.Count();j++)
                    {
                        Console.WriteLine("Ibeacon Guid : " + _monitorIBeaconGuid[i]._Beacons[j].ToString());
                        Console.WriteLine("IBeacon Threshold : " + _monitorIBeaconGuid[i]._BeaconThreshold[_monitorIBeaconGuid[i]._Beacons[j]].ToString());
                    }
                    
                }
                _IPSClient.SetWaypointList(_monitorIBeaconGuid);
            }
            if (haveGPS==true)
            {

            }
            //TODO:if add new client, here need to add.
        }

        //At first, wee need to open all the client because the user does not know where they are. When we know where they are,
        // we can then close all the client
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
        //Start beacon Scanning, TODO: If add new IPSClient, here needs to add new Detection
        public void OpenBeconScanning()
        {
            _IPSClient.DetectWaypoints();
            _ibeaconCLient.DetectWaypoints();
            _waypointClient.DetectWaypoints();
        }

        //Close All the event, TODO : if add new IPSCLient, remember here needs to modify
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
