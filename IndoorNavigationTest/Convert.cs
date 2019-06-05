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

        public static string NavigraphJson(List<LBeaconModel> Beacons, List<WaypointModel> BeaconGroups, List<LocationConnectModel> LocationConnects)
        {
            NaviGraph naviGraph = new NaviGraph();
            naviGraph.Name = "中研院";

            List<Region_DEPRECATED> regions = new List<Region_DEPRECATED>();
            foreach (float floor in BeaconGroups.Select(c => c.Floor).Distinct())
            {
                
                Region_DEPRECATED region = new Region_DEPRECATED()
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
