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
using IndoorNavigation.Models.NavigaionLayer;
using IndoorNavigation.Models;
using IndoorNavigation.Modules.IPSClients;


delegate void _addInterestedBeacon(Guid regionGuid, List<Guid> waypointGuids);
delegate void _runClient();

namespace IndoorNavigation.Modules
{
    public class IPSModules : IDisposable
    {

        private Dictionary<IPSType, IIPSClient> _multipleClient;
        private Dictionary<IPSType, bool> _haveIPSKind;

        private List<WaypointBeaconsMapping> _monitorLBeaconGuid;
        private List<WaypointBeaconsMapping> _monitorIBeaconGuid;
        public NavigationEvent _event { get; private set; }
        NavigationGraph _navigationGraph;
        private _addInterestedBeacon _lbeacon;
        private _addInterestedBeacon _ibeacon;
        private _addInterestedBeacon _gps;
        private int _firstStep = -1;

        public IPSModules(NavigationGraph navigationGraph)
        {
            _multipleClient = new Dictionary<IPSType, IIPSClient>();

            _multipleClient.Add(IPSType.LBeacon, new WaypointClient());
            _multipleClient.Add(IPSType.iBeacon, new IBeaconClient());

            _haveIPSKind = new Dictionary<IPSType, bool>();

            _haveIPSKind.Add(IPSType.LBeacon, false);
            _haveIPSKind.Add(IPSType.iBeacon, false);
            _haveIPSKind.Add(IPSType.GPS, false);

            _event = new NavigationEvent();
            _navigationGraph = navigationGraph;

            _monitorLBeaconGuid = new List<WaypointBeaconsMapping>();
            _monitorIBeaconGuid = new List<WaypointBeaconsMapping>();

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
                _haveIPSKind[IPSType.iBeacon] = true;
                _monitorIBeaconGuid.AddRange(FindTheMappingOfWaypointAndItsBeacon(regionGuid, waypointGuids));
            }
        }
        public void ADDLBeacon(Guid regionGuid, List<Guid> waypointGuids)
        {
            IPSType ipsType = _navigationGraph.GetRegionIPSType(regionGuid);
            if (ipsType == IPSType.LBeacon)
            {
                _haveIPSKind[IPSType.LBeacon] = true;
                _monitorLBeaconGuid.AddRange(FindTheMappingOfWaypointAndItsBeacon(regionGuid, waypointGuids));
            }
        }
        public void ADDGPS(Guid regionGuid, List<Guid> waypointGuids)
        {
            IPSType ipsType = _navigationGraph.GetRegionIPSType(regionGuid);
            if (ipsType == IPSType.GPS)
            {
                _haveIPSKind[IPSType.GPS] = true;
            }
        }
        public List<WaypointBeaconsMapping> FindTheMappingOfWaypointAndItsBeacon(Guid regionGuid, List<Guid> waypoints)
        {
            List<WaypointBeaconsMapping> waypointBeaconsMappings = new List<WaypointBeaconsMapping>();
            foreach (Guid waypointID in waypoints)
            {
                RegionWaypointPoint regionWaypointPoint = new RegionWaypointPoint();
                regionWaypointPoint._regionID = regionGuid;
                regionWaypointPoint._waypointID = waypointID;
                List<Guid> beaconIDs = new List<Guid>();
                beaconIDs = _navigationGraph.GetAllBeaconIDInOneWaypointOfRegion(regionGuid, waypointID);
                Dictionary<Guid, int> beaconThresholdMapping = new Dictionary<Guid, int>();
                for (int i = 0; i < beaconIDs.Count(); i++)
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
            if (_haveIPSKind[IPSType.LBeacon])
            {
                _multipleClient[IPSType.LBeacon] = new WaypointClient();
                _multipleClient[IPSType.LBeacon]._event._eventHandler += new EventHandler(PassMatchedWaypointAndRegionToSession);
                _multipleClient[IPSType.LBeacon].SetWaypointList(_monitorLBeaconGuid);
            }
            if (_haveIPSKind[IPSType.iBeacon])
            {
                _multipleClient[IPSType.iBeacon] = new IBeaconClient();
                _multipleClient[IPSType.iBeacon]._event._eventHandler += new EventHandler(PassMatchedWaypointAndRegionToSession);
                _multipleClient[IPSType.iBeacon].SetWaypointList(_monitorIBeaconGuid);
            }
            if (_haveIPSKind[IPSType.GPS])
            {

            }
            //TODO: If add new IPSClient, here needs to add
        }
        // private static void Detect()
        public IPSType GetIPSType(Guid regionGuid)
        {
            IPSType regionIPSType = new IPSType();
            regionIPSType = _navigationGraph.GetRegionIPSType(regionGuid);
            return regionIPSType;
        }

        #region There is some type, it will call those functions.
        //TODO : If add new client, here needs to add new function just likes below.
        public void IsLBeaconType()
        {
            _multipleClient[IPSType.LBeacon] = new WaypointClient();
            _multipleClient[IPSType.LBeacon]._event._eventHandler += new EventHandler(PassMatchedWaypointAndRegionToSession);
            _haveIPSKind[IPSType.LBeacon] = true;
        }
        public void IsIBeaconType()
        {
            _multipleClient[IPSType.iBeacon] = new IBeaconClient();
            _multipleClient[IPSType.iBeacon]._event._eventHandler += new EventHandler(PassMatchedWaypointAndRegionToSession);
            _haveIPSKind[IPSType.iBeacon] = true;
        }

        public void IsGPSType()
        {

        }
        #endregion
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
            if (!nextIPSType.Equals(currentIPSType) || firstStep == _firstStep)
            {
                HaveBeaconAllFalse();
                CloseStartAllExistClient();                
                OpenCurrentIPSClient(currentIPSType);
                OpenCurrentIPSClient(nextIPSType);
            }
            Console.WriteLine($">>IPSModules CompareToCurrentAndNextIPSType, currentType={currentIPSType}, nextType={nextIPSType}");
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
                    //TODO: if add new client, here needs to add
            }
        }

        public void HaveBeaconAllFalse()
        {
            foreach (IPSType type in Enum.GetValues(typeof(IPSType)))
                _haveIPSKind[type] = false;
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
            if (_haveIPSKind[IPSType.LBeacon])
            {
                _multipleClient[IPSType.LBeacon].SetWaypointList(_monitorLBeaconGuid);
            }
            if (_haveIPSKind[IPSType.iBeacon])
            {
                _multipleClient[IPSType.iBeacon].SetWaypointList(_monitorIBeaconGuid);
            }
            if (_haveIPSKind[IPSType.GPS])
            {

            }
            //TODO:if add new client, here need to add.
        }

        //At first, wee need to open all the client because the user does not know where they are. When we know where they are,
        // we can then close all the client
        public void CloseStartAllExistClient()
        {
            Console.WriteLine(">>IPSmodule :: CloseStartAllExistClient");
            if (_haveIPSKind[IPSType.LBeacon])
            {
                Console.WriteLine(">>IPSmodule :: Close_LBeacon");
                _multipleClient[IPSType.LBeacon].Stop();
                _monitorLBeaconGuid = new List<WaypointBeaconsMapping>();
                _multipleClient[IPSType.LBeacon]._event._eventHandler -= new EventHandler(PassMatchedWaypointAndRegionToSession);

            }
            if (_haveIPSKind[IPSType.iBeacon])
            {
                Console.WriteLine(">>IPSmodule :: Close_iBeacon");
                _multipleClient[IPSType.iBeacon].Stop();
                _monitorIBeaconGuid = new List<WaypointBeaconsMapping>();
                _multipleClient[IPSType.iBeacon]._event._eventHandler -= new EventHandler(PassMatchedWaypointAndRegionToSession);
            }
            if (_haveIPSKind[IPSType.GPS])
            {

            }
        }
        //Start beacon Scanning, TODO: If add new IPSClient, here needs to add new Detection
        public void OpenBeconScanning()
        {
            Console.WriteLine(">>IPSmodule :: OpenBeaconScanning");
            _multipleClient[IPSType.LBeacon].DetectWaypoints();
            _multipleClient[IPSType.iBeacon].DetectWaypoints();
        }

        //Close All the event, TODO : if add new IPSCLient, remember here needs to modify
        public void Close()
        {
            Console.WriteLine(">>IPSmodule :: Close");
            _lbeacon -= new _addInterestedBeacon(ADDIBeacon);
            _ibeacon -= new _addInterestedBeacon(ADDLBeacon);
            _gps -= new _addInterestedBeacon(ADDGPS);
            _monitorIBeaconGuid = new List<WaypointBeaconsMapping>();
            _monitorLBeaconGuid = new List<WaypointBeaconsMapping>();

            HaveBeaconAllFalse();
        }
        public void Dispose()
        {
            Console.WriteLine(">>IPSmodule :: Dispose");
            throw new NotImplementedException();
        }
    }
}
