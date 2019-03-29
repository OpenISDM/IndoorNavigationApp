using IndoorNavigation.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IndoorNavigationTest
{
    public static class INConvert
    {
        public static JObject ToJsonObject(this List<Beacon> Beacons)
        {
            List<IBeaconModel> IBeacons = Beacons.Where(Beacon => Beacon.GetType() == typeof(IBeaconModel)).Select(Beacon => (Beacon as IBeaconModel)).ToList();
            List<LBeaconModel> LBeacons = Beacons.Where(Beacon => Beacon.GetType() == typeof(LBeaconModel)).Select(Beacon => (Beacon as LBeaconModel)).ToList();
            return JObject.FromObject(
                new
                {
                    iBeacons = IBeacons,
                    lBeacons = LBeacons
                });
        }

        public static JArray ToJsonArray(this List<WaypointModel> BeaconGroups)
        {
            List<WaypointModelForNavigraphFile> BeaconGroupModels = BeaconGroups.Select(BeaconGroup => new WaypointModelForNavigraphFile { Id = BeaconGroup.Id, Name = BeaconGroup.Name, Beacons = BeaconGroup.Beacons.Select(Beacon => Beacon.UUID).ToList() }).ToList();
            return JArray.FromObject(BeaconGroupModels);
        }

        public static JArray ToJsonArray(this List<LocationConnectModel> LocationConnects)
        {
            List<LocationConnectModelForNavigraphFile> LocationConnectModelForMapFiles = LocationConnects.Select(LocationConnect => new LocationConnectModelForNavigraphFile { BeaconA = LocationConnect.SourceWaypoint.Id, BeaconB = LocationConnect.TargetWaypoint.Id, Target = LocationConnect.Target }).ToList();
            return JArray.FromObject(LocationConnectModelForMapFiles);
        }

        public static string NavigraphJson(List<LBeaconModel> Beacons, List<WaypointModel> BeaconGroups, List<LocationConnectModel> LocationConnects)
        {
            NaviGraph naviGraph = new NaviGraph();
            naviGraph.Name = "中研院";

            List<Region> regions = new List<Region>();
            foreach (float floor in BeaconGroups.Select(c => c.Floor).Distinct())
            {
                
                Region region = new Region()
                {
                    Name = string.Format("{0}F", Convert.ToInt32(floor)),
                    LBeacons = Beacons.Where(c => c.Floor == floor).ToList(),
                    Waypoints = BeaconGroups.Where(c => c.Floor ==  floor).Select(c => new WaypointModelForNavigraphFile {
                        Id = c.Id,
                        Beacons = c.Beacons.Select(d => d.UUID).ToList(),
                        Name = c.Name,
                        Neighbors = LocationConnects.Where(d => d.SourceWaypoint == c).Select(d => new Neighbor { TargetWaypointId = d.TargetWaypoint.Id, Target = d.Target}).ToList()
                    }).ToList()
                };

                regions.Add(region);
            }

            naviGraph.Regions = regions;

            JArray array = JArray.FromObject(new List<NaviGraph> { naviGraph });
            return (JsonConvert.SerializeObject(array, Formatting.Indented));
        }
    }
}
