﻿using IndoorNavigation.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IndoorNavigationTest
{
    public static class INConvert
    {
        public static string ToJsonString(this List<Beacon> Beacons)
        {
            List<IBeaconModel> IBeacons = Beacons.Where(Beacon => Beacon.GetType() == typeof(IBeaconModel)).Select(Beacon => (Beacon as IBeaconModel)).ToList();
            List<LBeaconModel> LBeacons = Beacons.Where(Beacon => Beacon.GetType() == typeof(LBeaconModel)).Select(Beacon => (Beacon as LBeaconModel)).ToList();
            return JsonConvert.SerializeObject(
                new
                {
                    iBeacons = IBeacons,
                    lBeacons = LBeacons
                });
        }

        public static string ToJsonString(this List<BeaconGroupModel> BeaconGroups)
        {
            List<BeaconGroupModelForMapFile> BeaconGroupModels = BeaconGroups.Select(BeaconGroup => new BeaconGroupModelForMapFile { Id = BeaconGroup.Id, Name = BeaconGroup.Name, Beacons = BeaconGroup.Beacons.Select(Beacon => Beacon.UUID).ToList() }).ToList();
            return JsonConvert.SerializeObject(BeaconGroupModels);
        }

        public static string ToJsonString(this List<LocationConnectModel> LocationConnects)
        {
            List<LocationConnectModelForMapFile> LocationConnectModelForMapFiles = LocationConnects.Select(LocationConnect => new LocationConnectModelForMapFile { BeaconA = LocationConnect.BeaconA.Id, BeaconB = LocationConnect.BeaconB.Id, IsTwoWay = LocationConnect.IsTwoWay }).ToList();
            return JsonConvert.SerializeObject(LocationConnectModelForMapFiles);
        }
    }
}
