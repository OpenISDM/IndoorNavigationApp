using IndoorNavigation.Modules;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Linq;
using IndoorNavigation;
using System.Collections.Generic;
using IndoorNavigation.Models;
using System.Threading;

namespace IndoorNavigationTest
{
    [TestClass]
    public class NavigationlogicTest
    {
        private ManualResetEvent bestBeaconWait = new ManualResetEvent(false);
        private Beacon bestBeacon = null;

        [TestInitialize]
        public void TestInit()
        {
            GenerateMapData.Generate();
            Utility.SignalProcess = new SignalProcessModule();
            Utility.SignalProcess.Event.SignalProcessEventHandler += new EventHandler(HandleSignalProcess);
        }

        [TestMethod]
        public void StorageTest()
        {
            NavigraphStorage.DeleteAllMap();
            NavigraphStorage.SaveMapInformation("test1","");
            NavigraphStorage.SaveMapInformation("test1", "");
            string[] Maps = NavigraphStorage.GetAllPlace();
            Assert.AreEqual(1, Maps.Length);

            NavigraphStorage.SaveMapInformation("test2", "");
            NavigraphStorage.SaveMapInformation("test3", "");
            Maps = NavigraphStorage.GetAllPlace();
            Assert.AreEqual(3, Maps.Length);

            NavigraphStorage.DeleteNavigraph("test4");
            Maps = NavigraphStorage.GetAllPlace();
            Assert.AreEqual(3, Maps.Length);

            NavigraphStorage.DeleteNavigraph("test3");
            Maps = NavigraphStorage.GetAllPlace();
            Assert.AreEqual(2, Maps.Length);

            NavigraphStorage.DeleteAllMap();
            Maps = NavigraphStorage.GetAllPlace();
            Assert.AreEqual(0, Maps.Length);
        }

        [TestMethod]
        public void MapStorageAccessAndMapDataConvertTest()
        {
            var MapJson = JsonConvert.SerializeObject(
                new
                {
                    Beacon = Utility.Beacons.Values.ToList().ToJsonString(),
                    BeaconGroup = Utility.BeaconGroups.ToJsonString(),
                    LocationConnect = Utility.LocationConnects.ToJsonString()
                });

            NavigraphStorage.SaveMapInformation("Map1", MapJson);
            string[] Maps = NavigraphStorage.GetAllPlace();
            Assert.AreEqual(1, Maps.Length);
            Utility.Beacons = null;
            Utility.BeaconGroups = null;
            Utility.LocationConnects = null;
            NavigraphStorage.LoadNavigraph("Map1");

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

        [TestMethod]
        public void UUIDConvertTest()
        {
            var A1 = Utility.Beacons[Guid.Parse("0000803f-0000-7b3d-c941-0000c15ef342")];
            Assert.AreEqual(25.15502, A1.GetCoordinates().Latitude,0.00001);
            Assert.AreEqual(121.68507, A1.GetCoordinates().Longitude, 0.00001);
            Assert.AreEqual(1, A1.Floor);

            var B2 = Utility.Beacons[Guid.Parse("0000803f-0000-563d-c941-0000d55ef342")];
            Assert.AreEqual(25.15495, B2.GetCoordinates().Latitude, 0.00001);
            Assert.AreEqual(121.68522, B2.GetCoordinates().Longitude, 0.00001);
            Assert.AreEqual(1, A1.Floor);
        }

        [TestMethod, Timeout(30000)]
        public void SignalProcessTest()
        {
            Utility.SignalProcess.AddSignal(
                new List<BeaconSignalModel> {
                    new BeaconSignalModel
                    {
                        UUID = Guid.Parse("0000803f-0000-563d-c941-0000d55ef342"),
                        RSSI = -20
                    }});

            Utility.SignalProcess.AddSignal(
                new List<BeaconSignalModel> {
                    new BeaconSignalModel
                    {
                        UUID = Guid.Parse("0000803f-0000-863d-c941-0000ea5ef342"),
                        RSSI = -30
                    }});

            var ANSBeacon = Utility.Beacons[Guid.Parse("0000803f-0000-563d-c941-0000d55ef342")];
            bestBeaconWait.WaitOne();
            Assert.AreEqual(ANSBeacon, bestBeacon);

            Utility.SignalProcess.AddSignal(
                new List<BeaconSignalModel> {
                    new BeaconSignalModel
                    {
                        UUID = Guid.Parse("0000803f-0000-563d-c941-0000d55ef342"),
                        RSSI = -63
                    }});

            Utility.SignalProcess.AddSignal(
                new List<BeaconSignalModel> {
                    new BeaconSignalModel
                    {
                        UUID = Guid.Parse("0000803f-0000-863d-c941-0000ea5ef342"),
                        RSSI = -57
                    }});

            ANSBeacon = Utility.Beacons[Guid.Parse("0000803f-0000-563d-c941-0000d55ef342")];
            bestBeaconWait.WaitOne();
            Assert.AreEqual(ANSBeacon, bestBeacon);
        }

        /// <summary>
        /// 接收來?signal process model傳送?最佳Beacon
        /// </summary>
        /// <param name="CurrentBeacon"></param>
        private void HandleSignalProcess(object sender, EventArgs e)
        {
            Beacon currentBeacon =
                (e as SignalProcessEventArgs).CurrentBeacon;

            bestBeacon = currentBeacon;

            bestBeaconWait.Set();
            bestBeaconWait.Reset();
        }
    }
}
