using IndoorNavigation.Modules;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Linq;

namespace IndoorNavigationTest
{
    [TestClass]
    public class NavigationlogicTest
    {
        [TestInitialize]
        public void TestInit()
        {
            GenerateMapData.Generate();
        }

        [TestMethod]
        public void StorageTest()
        {
            MapStorage.DeleteAllMap();
            MapStorage.SaveMapInformation("test1","");
            MapStorage.SaveMapInformation("test1", "");
            string[] Maps = MapStorage.GetAllPlace();
            Assert.AreEqual(1, Maps.Length);

            MapStorage.SaveMapInformation("test2", "");
            MapStorage.SaveMapInformation("test3", "");
            Maps = MapStorage.GetAllPlace();
            Assert.AreEqual(3, Maps.Length);

            MapStorage.DeleteMap("test4");
            Maps = MapStorage.GetAllPlace();
            Assert.AreEqual(3, Maps.Length);

            MapStorage.DeleteMap("test3");
            Maps = MapStorage.GetAllPlace();
            Assert.AreEqual(2, Maps.Length);

            MapStorage.DeleteAllMap();
            Maps = MapStorage.GetAllPlace();
            Assert.AreEqual(0, Maps.Length);
        }

        [TestMethod]
        public void MapStorageAccessAndMapDataConvert()
        {
            var MapJson = JsonConvert.SerializeObject(
                new
                {
                    Beacon = Utility.Beacons.Values.ToList().ToJsonString(),
                    BeaconGroup = Utility.BeaconGroups.ToJsonString(),
                    LocationConnect = Utility.LocationConnects.ToJsonString()
                });

            MapStorage.SaveMapInformation("Map1", MapJson);
            string[] Maps = MapStorage.GetAllPlace();
            Assert.AreEqual(1, Maps.Length);
            Utility.Beacons = null;
            Utility.BeaconGroups = null;
            Utility.LocationConnects = null;
            MapStorage.LoadMap("Map1");

            var A1 = Utility.Beacons[Guid.Parse("0000803f-0000-7b3d-c941-0000c15ef342")];
            var A2 = Utility.Beacons[Guid.Parse("0000803f-0000-4c3d-c941-0000c35ef342")];
            var B1 = Utility.Beacons[Guid.Parse("0000803f-0000-803d-c941-0000d45ef342")];
            var B2 = Utility.Beacons[Guid.Parse("0000803f-0000-563d-c941-0000d55ef342")];
            Assert.AreEqual(60, A1.Threshold);
            Assert.AreEqual(70, A2.Threshold);
            Assert.AreEqual(75, B1.Threshold);
            Assert.AreEqual(65, B2.Threshold);

            var position1 = Utility.BeaconGroups.Where(c => c.Name == "A1F").First();
            var position2 = Utility.BeaconGroups.Where(c => c.Name == "B1F").First();

            Assert.AreEqual(2, position1.Beacons.Count());
            Assert.AreEqual(1, position1.Beacons.Where(c => c == A1).Count());
            Assert.AreEqual(1, position1.Beacons.Where(c => c == A2).Count());
            Assert.AreEqual(2, position2.Beacons.Count());
            Assert.AreEqual(1, position2.Beacons.Where(c => c == B1).Count());
            Assert.AreEqual(1, position2.Beacons.Where(c => c == B1).Count());
            Assert.AreEqual(1, Utility.LocationConnects.Where(c => c.BeaconA == position1 && c.BeaconB == position2).Count());
        }
    }
}
