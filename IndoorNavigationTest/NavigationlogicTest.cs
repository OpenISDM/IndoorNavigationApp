using IndoorNavigation.Modules;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Linq;
using IndoorNavigation;
using System.Collections.Generic;
using IndoorNavigation.Models;
using System.Threading;
using IndoorNavigation.Modules.Navigation;

namespace IndoorNavigationTest
{
    [TestClass]
    public class NavigationlogicTest
    {
        private ManualResetEvent WaitEvent = new ManualResetEvent(false);
        private Beacon bestBeacon = null;
        private MaNEventArgs maNEventArgs = null;

        [TestInitialize]
        public void TestInit()
        {
            GenerateMapData.Generate();
            Utility.SignalProcess = new SignalProcessModule();
            Utility.SignalProcess.Event.SignalProcessEventHandler += new EventHandler(HandleSignalProcess);
            Utility.Route = new RoutePlan(Utility.Waypoints,Utility.LocationConnects);
            Utility.MaN = new MaNModule();
            Utility.MaN.Event.MaNEventHandler += new EventHandler(HandleMaNModule);
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
                    Beacon = Utility.BeaconsDict.Values.ToList().ToJsonString(),
                    BeaconGroup = Utility.Waypoints.ToJsonString(),
                    LocationConnect = Utility.LocationConnects.ToJsonString()
                });

            NavigraphStorage.SaveMapInformation("Map1", MapJson);
            string[] Maps = NavigraphStorage.GetAllPlace();
            Assert.AreEqual(1, Maps.Length);
            Utility.BeaconsDict = null;
            Utility.Waypoints = null;
            Utility.LocationConnects = null;
            NavigraphStorage.LoadNavigraph("Map1");

            var A1 = Utility.BeaconsDict[Guid.Parse("0000803f-0000-7b3d-c941-0000c15ef342")];
            var A2 = Utility.BeaconsDict[Guid.Parse("0000803f-0000-4c3d-c941-0000c35ef342")];
            var B1 = Utility.BeaconsDict[Guid.Parse("0000803f-0000-803d-c941-0000d45ef342")];
            var B2 = Utility.BeaconsDict[Guid.Parse("0000803f-0000-563d-c941-0000d55ef342")];
            Assert.AreEqual(-60, A1.Threshold);
            Assert.AreEqual(-70, A2.Threshold);
            Assert.AreEqual(-75, B1.Threshold);
            Assert.AreEqual(-65, B2.Threshold);

            var position1 = Utility.Waypoints.Where(c => c.Name == "A1F").First();
            var position2 = Utility.Waypoints.Where(c => c.Name == "B1F").First();

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
            var A1 = Utility.BeaconsDict[Guid.Parse("0000803f-0000-7b3d-c941-0000c15ef342")];
            Assert.AreEqual(25.15502, A1.GetCoordinates().Latitude,0.00001);
            Assert.AreEqual(121.68507, A1.GetCoordinates().Longitude, 0.00001);
            Assert.AreEqual(1, A1.Floor);

            var B2 = Utility.BeaconsDict[Guid.Parse("0000803f-0000-563d-c941-0000d55ef342")];
            Assert.AreEqual(25.15495, B2.GetCoordinates().Latitude, 0.00001);
            Assert.AreEqual(121.68522, B2.GetCoordinates().Longitude, 0.00001);
            Assert.AreEqual(1, A1.Floor);
        }

        [TestMethod, Timeout(10000)]
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

            var ANSBeacon = Utility.BeaconsDict[Guid.Parse("0000803f-0000-563d-c941-0000d55ef342")];
            WaitEvent.WaitOne();
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

            ANSBeacon = Utility.BeaconsDict[Guid.Parse("0000803f-0000-563d-c941-0000d55ef342")];
            WaitEvent.WaitOne();
            Assert.AreEqual(ANSBeacon, bestBeacon);
        }

        [TestMethod, Timeout(10000)]
        public void NavigationTest()
        {
            var routePath = Utility.Route.GetPath(Utility.BeaconsDict[Guid.Parse("0000803f-0000-7b3d-c941-0000c15ef342")], Utility.Waypoints.Where(c => c.Name == "K1F").First());
            Utility.MaN.SetDestination(Utility.Waypoints.Where(c => c.Name == "K1F").First());
            // A
            Utility.SignalProcess.Event.OnEventCall(new SignalProcessEventArgs { CurrentBeacon = Utility.BeaconsDict[Guid.Parse("0000803f-0000-7b3d-c941-0000c15ef342")]});
            WaitEvent.WaitOne();
            routePath.Dequeue();
            Assert.AreEqual(NavigationStatus.DirectionCorrect,maNEventArgs.Status);
            // B
            Utility.SignalProcess.Event.OnEventCall(new SignalProcessEventArgs { CurrentBeacon = Utility.BeaconsDict[Guid.Parse("0000803f-0000-803d-c941-0000d45ef342")] });
            WaitEvent.WaitOne();
            Assert.AreEqual(routePath.Dequeue().Angle, maNEventArgs.Angle);
            // C
            Utility.SignalProcess.Event.OnEventCall(new SignalProcessEventArgs { CurrentBeacon = Utility.BeaconsDict[Guid.Parse("0000803f-0000-863d-c941-0000ea5ef342")] });
            WaitEvent.WaitOne();
            Assert.AreEqual(routePath.Dequeue().Angle, maNEventArgs.Angle);
            // D
            Utility.SignalProcess.Event.OnEventCall(new SignalProcessEventArgs { CurrentBeacon = Utility.BeaconsDict[Guid.Parse("0000803f-0000-183d-c941-0000eb5ef342")] });
            WaitEvent.WaitOne();
            Assert.AreEqual(routePath.Dequeue().Angle, maNEventArgs.Angle);
            // E
            Utility.SignalProcess.Event.OnEventCall(new SignalProcessEventArgs { CurrentBeacon = Utility.BeaconsDict[Guid.Parse("0000803f-0000-ee3c-c941-0000005ff342")] });
            WaitEvent.WaitOne();
            Assert.AreEqual(routePath.Dequeue().Angle, maNEventArgs.Angle);
            // F
            Utility.SignalProcess.Event.OnEventCall(new SignalProcessEventArgs { CurrentBeacon = Utility.BeaconsDict[Guid.Parse("0000803f-0000-be3c-c941-0000185ff342")] });
            WaitEvent.WaitOne();
            Assert.AreEqual(routePath.Dequeue().Angle, maNEventArgs.Angle);
            // L
            Utility.SignalProcess.Event.OnEventCall(new SignalProcessEventArgs { CurrentBeacon = Utility.BeaconsDict[Guid.Parse("0000803f-0000-be3c-c941-0000395ff342")] });
            WaitEvent.WaitOne();
            Assert.AreEqual(routePath.Dequeue().Angle, maNEventArgs.Angle);
            // K
            Utility.SignalProcess.Event.OnEventCall(new SignalProcessEventArgs { CurrentBeacon = Utility.BeaconsDict[Guid.Parse("0000803f-0000-b93c-c941-00005a5ff342")] });
            WaitEvent.WaitOne();
            Assert.AreEqual(NavigationStatus.Arrival, maNEventArgs.Status);
            Utility.MaN.StopNavigation();
        }

        [TestMethod, Timeout(10000)]
        public void StartingPointCorrection()
        {
            var routePath = Utility.Route.RegainPath(Utility.Waypoints.Where(c => c.Name == "D1F").First(),Utility.BeaconsDict[Guid.Parse("0000803f-0000-563d-c941-0000e85ef342")], Utility.Waypoints.Where(c => c.Name == "K1F").First());
            Utility.MaN.SetDestination(Utility.Waypoints.Where(c => c.Name == "K1F").First());
            // D
            Utility.SignalProcess.Event.OnEventCall(new SignalProcessEventArgs { CurrentBeacon = Utility.BeaconsDict[Guid.Parse("0000803f-0000-223d-c941-0000ff5ef342")] });
            WaitEvent.WaitOne();
            Assert.AreEqual(NavigationStatus.DirectionCorrect, maNEventArgs.Status);
            // C
            Utility.SignalProcess.Event.OnEventCall(new SignalProcessEventArgs { CurrentBeacon = Utility.BeaconsDict[Guid.Parse("0000803f-0000-563d-c941-0000e85ef342")] });
            WaitEvent.WaitOne();
            Assert.AreEqual(NavigationStatus.RouteCorrect, maNEventArgs.Status);
            WaitEvent.WaitOne();
            Assert.AreEqual(routePath.Dequeue().Angle, maNEventArgs.Angle);
            // D
            Utility.SignalProcess.Event.OnEventCall(new SignalProcessEventArgs { CurrentBeacon = Utility.BeaconsDict[Guid.Parse("0000803f-0000-223d-c941-0000ff5ef342")] });
            WaitEvent.WaitOne();
            Assert.AreEqual(routePath.Dequeue().Angle, maNEventArgs.Angle);
            // E
            Utility.SignalProcess.Event.OnEventCall(new SignalProcessEventArgs { CurrentBeacon = Utility.BeaconsDict[Guid.Parse("0000803f-0000-ee3c-c941-0000005ff342")] });
            WaitEvent.WaitOne();
            Assert.AreEqual(routePath.Dequeue().Angle, maNEventArgs.Angle);
            // F
            Utility.SignalProcess.Event.OnEventCall(new SignalProcessEventArgs { CurrentBeacon = Utility.BeaconsDict[Guid.Parse("0000803f-0000-be3c-c941-0000185ff342")] });
            WaitEvent.WaitOne();
            Assert.AreEqual(routePath.Dequeue().Angle, maNEventArgs.Angle);
            // L
            Utility.SignalProcess.Event.OnEventCall(new SignalProcessEventArgs { CurrentBeacon = Utility.BeaconsDict[Guid.Parse("0000803f-0000-be3c-c941-0000395ff342")] });
            WaitEvent.WaitOne();
            Assert.AreEqual(routePath.Dequeue().Angle, maNEventArgs.Angle);
            // K
            Utility.SignalProcess.Event.OnEventCall(new SignalProcessEventArgs { CurrentBeacon = Utility.BeaconsDict[Guid.Parse("0000803f-0000-b93c-c941-00005a5ff342")] });
            WaitEvent.WaitOne();
            Assert.AreEqual(NavigationStatus.Arrival, maNEventArgs.Status);
            Utility.MaN.StopNavigation();
        }

        [TestMethod, Timeout(10000)]
        public void SkipSomeLocationsTest()
        {
            var routePath = Utility.Route.GetPath(Utility.BeaconsDict[Guid.Parse("0000803f-0000-7b3d-c941-0000c15ef342")], Utility.Waypoints.Where(c => c.Name == "K1F").First());
            Utility.MaN.SetDestination(Utility.Waypoints.Where(c => c.Name == "K1F").First());
            // A
            Utility.SignalProcess.Event.OnEventCall(new SignalProcessEventArgs { CurrentBeacon = Utility.BeaconsDict[Guid.Parse("0000803f-0000-7b3d-c941-0000c15ef342")] });
            WaitEvent.WaitOne();
            routePath.Dequeue();
            Assert.AreEqual(NavigationStatus.DirectionCorrect, maNEventArgs.Status);
            // B
            Utility.SignalProcess.Event.OnEventCall(new SignalProcessEventArgs { CurrentBeacon = Utility.BeaconsDict[Guid.Parse("0000803f-0000-803d-c941-0000d45ef342")] });
            WaitEvent.WaitOne();
            Assert.AreEqual(routePath.Dequeue().Angle, maNEventArgs.Angle);
            //// C
            //Utility.SignalProcess.Event.OnEventCall(new SignalProcessEventArgs { CurrentBeacon = Utility.Beacons[Guid.Parse("0000803f-0000-863d-c941-0000ea5ef342")] });
            //WaitEvent.WaitOne();
            //Assert.AreEqual(routePath.Dequeue().Angle, maNEventArgs.Angle);
            //// D
            //Utility.SignalProcess.Event.OnEventCall(new SignalProcessEventArgs { CurrentBeacon = Utility.Beacons[Guid.Parse("0000803f-0000-183d-c941-0000eb5ef342")] });
            //WaitEvent.WaitOne();
            //Assert.AreEqual(routePath.Dequeue().Angle, maNEventArgs.Angle);
            //// E
            //Utility.SignalProcess.Event.OnEventCall(new SignalProcessEventArgs { CurrentBeacon = Utility.Beacons[Guid.Parse("0000803f-0000-ee3c-c941-0000005ff342")] });
            //WaitEvent.WaitOne();
            //Assert.AreEqual(routePath.Dequeue().Angle, maNEventArgs.Angle);
            routePath.Dequeue();
            routePath.Dequeue();
            routePath.Dequeue();
            // F
            Utility.SignalProcess.Event.OnEventCall(new SignalProcessEventArgs { CurrentBeacon = Utility.BeaconsDict[Guid.Parse("0000803f-0000-be3c-c941-0000185ff342")] });
            WaitEvent.WaitOne();
            Assert.AreEqual(NavigationStatus.RouteCorrect, maNEventArgs.Status);
            WaitEvent.WaitOne();
            Assert.AreEqual(routePath.Dequeue().Angle, maNEventArgs.Angle);
            // L
            Utility.SignalProcess.Event.OnEventCall(new SignalProcessEventArgs { CurrentBeacon = Utility.BeaconsDict[Guid.Parse("0000803f-0000-be3c-c941-0000395ff342")] });
            WaitEvent.WaitOne();
            Assert.AreEqual(routePath.Dequeue().Angle, maNEventArgs.Angle);
            // K
            Utility.SignalProcess.Event.OnEventCall(new SignalProcessEventArgs { CurrentBeacon = Utility.BeaconsDict[Guid.Parse("0000803f-0000-b93c-c941-00005a5ff342")] });
            WaitEvent.WaitOne();
            Assert.AreEqual(NavigationStatus.Arrival, maNEventArgs.Status);
            Utility.MaN.StopNavigation();
        }

        [TestMethod, Timeout(10000)]
        public void ReNavigationTest()
        {
            var routePath = Utility.Route.GetPath(Utility.BeaconsDict[Guid.Parse("0000803f-0000-7b3d-c941-0000c15ef342")], Utility.Waypoints.Where(c => c.Name == "K1F").First());
            Utility.MaN.SetDestination(Utility.Waypoints.Where(c => c.Name == "K1F").First());
            // A
            Utility.SignalProcess.Event.OnEventCall(new SignalProcessEventArgs { CurrentBeacon = Utility.BeaconsDict[Guid.Parse("0000803f-0000-7b3d-c941-0000c15ef342")] });
            WaitEvent.WaitOne();
            routePath.Dequeue();
            Assert.AreEqual(NavigationStatus.DirectionCorrect, maNEventArgs.Status);
            // B
            Utility.SignalProcess.Event.OnEventCall(new SignalProcessEventArgs { CurrentBeacon = Utility.BeaconsDict[Guid.Parse("0000803f-0000-803d-c941-0000d45ef342")] });
            WaitEvent.WaitOne();
            Assert.AreEqual(routePath.Dequeue().Angle, maNEventArgs.Angle);
            // C
            Utility.SignalProcess.Event.OnEventCall(new SignalProcessEventArgs { CurrentBeacon = Utility.BeaconsDict[Guid.Parse("0000803f-0000-863d-c941-0000ea5ef342")] });
            WaitEvent.WaitOne();
            Assert.AreEqual(routePath.Dequeue().Angle, maNEventArgs.Angle);
            // D
            Utility.SignalProcess.Event.OnEventCall(new SignalProcessEventArgs { CurrentBeacon = Utility.BeaconsDict[Guid.Parse("0000803f-0000-183d-c941-0000eb5ef342")] });
            WaitEvent.WaitOne();
            Assert.AreEqual(routePath.Dequeue().Angle, maNEventArgs.Angle);
            // ?·s­pºâ¸Ñµª
            routePath = Utility.Route.RegainPath(Utility.Waypoints.Where(c => c.Name == "D1F").First(), Utility.BeaconsDict[Guid.Parse("0000803f-0000-713d-c941-0000395ff342")], Utility.Waypoints.Where(c => c.Name == "K1F").First());
            // G
            Utility.SignalProcess.Event.OnEventCall(new SignalProcessEventArgs { CurrentBeacon = Utility.BeaconsDict[Guid.Parse("0000803f-0000-713d-c941-0000395ff342")] });
            WaitEvent.WaitOne();
            Assert.AreEqual(NavigationStatus.RouteCorrect, maNEventArgs.Status);
            WaitEvent.WaitOne();
            Assert.AreEqual(routePath.Dequeue().Angle, maNEventArgs.Angle);
            // H
            Utility.SignalProcess.Event.OnEventCall(new SignalProcessEventArgs { CurrentBeacon = Utility.BeaconsDict[Guid.Parse("0000803f-0000-b03d-c941-0000575ff342")] });
            WaitEvent.WaitOne();
            Assert.AreEqual(routePath.Dequeue().Angle, maNEventArgs.Angle);
            // I
            Utility.SignalProcess.Event.OnEventCall(new SignalProcessEventArgs { CurrentBeacon = Utility.BeaconsDict[Guid.Parse("0000803f-0000-323d-c941-0000585ff342")] });
            WaitEvent.WaitOne();
            Assert.AreEqual(routePath.Dequeue().Angle, maNEventArgs.Angle);
            // J
            Utility.SignalProcess.Event.OnEventCall(new SignalProcessEventArgs { CurrentBeacon = Utility.BeaconsDict[Guid.Parse("0000803f-0000-f83c-c941-00005a5ff342")] });
            WaitEvent.WaitOne();
            Assert.AreEqual(routePath.Dequeue().Angle, maNEventArgs.Angle);
            // K
            Utility.SignalProcess.Event.OnEventCall(new SignalProcessEventArgs { CurrentBeacon = Utility.BeaconsDict[Guid.Parse("0000803f-0000-b93c-c941-00005a5ff342")] });
            WaitEvent.WaitOne();
            Assert.AreEqual(NavigationStatus.Arrival, maNEventArgs.Status);
            Utility.MaN.StopNavigation();
        }

        private void HandleSignalProcess(object sender, EventArgs e)
        {
            Beacon currentBeacon =
                (e as SignalProcessEventArgs).CurrentBeacon;

            bestBeacon = currentBeacon;

            WaitEvent.Set();
            WaitEvent.Reset();
        }

        private void HandleMaNModule(object sender, EventArgs e)
        {
            maNEventArgs = e as MaNEventArgs;

            WaitEvent.Set();
            WaitEvent.Reset();
        }
    }
}
