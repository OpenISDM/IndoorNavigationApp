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
using IndoorNavigation.Modules.SignalProcessingAlgorithms;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
using System.IO;

namespace IndoorNavigationTest
{
    [TestClass]
    public class NavigationlogicTest
    {
        private AutoResetEvent SignalProcessWaitEvent = new AutoResetEvent(false);
        private AutoResetEvent MaNWaitEvent = new AutoResetEvent(false);
        private Beacon bestBeacon;
        private EventArgs MaNEventArgs;

        [TestInitialize]
        public void TestInit()
        {
            GenerateMapData.Generate();

            Utility.Service = new Container();
            Utility.Service.Add<WaypointSignalProcessing>
                ("Default signal process algorithm");
            Utility.Service.Add<WaypointSignalProcessing>
                ("Way point signal processing algorithm");
            Utility.Service.Add<WayPointAlgorithm>("Way point algorithm");


            Utility.BeaconScanAPI = new BeaconScan();
            Utility.SignalProcess = new SignalProcessModule();
            Utility.SignalProcess.Event.SignalProcessEventHandler += HandleSignalProcess;
            Utility.WaypointRoute = new WaypointRoutePlan(Utility.Waypoints, Utility.LocationConnects);
            Utility.MaN = new MaNModule();
            Utility.MaN.Event.MaNEventHandler += new EventHandler(HandleMaNModule);
            Utility.IPS = new IPSModule();
            Debug.WriteLine("單元測試初始化完成");
        }

        public void TestClose()
        {
            Utility.SignalProcess.Dispose();
            Utility.MaN.Dispose();
            Utility.IPS.Dispose();
        }

        [TestMethod]
        public void StorageTest()
        {
            Debug.WriteLine("StorageTest start.");
            NavigraphStorage.DeleteAllMap();
            NavigraphStorage.SaveMapInformation("test1", "");
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
            TestClose();
            Debug.WriteLine("StorageTest done.");
        }

        [TestMethod]
        public void MapStorageAccessAndMapDataConvertTest()
        {
            Debug.WriteLine("MapStorageAccessAndMapDataConvertTest start.");

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

            var position1 = Utility.Waypoints.First(c => c.Name == "A1F");
            var position2 = Utility.Waypoints.First(c => c.Name == "B1F");

            Assert.AreEqual(2, position1.Beacons.Count());
            Assert.AreEqual(1, position1.Beacons.Count(c => c == A1));
            Assert.AreEqual(1, position1.Beacons.Count(c => c == A2));
            Assert.AreEqual(2, position2.Beacons.Count());
            Assert.AreEqual(1, position2.Beacons.Count(c => c == B1));
            Assert.AreEqual(1, position2.Beacons.Count(c => c == B1));
            Assert.AreEqual(1, Utility.LocationConnects.Count(c => c.BeaconA == position1 && c.BeaconB == position2));

            TestClose();
            Debug.WriteLine("MapStorageAccessAndMapDataConvertTest done.");
        }

        [TestMethod]
        public void UUIDConvertTest()
        {
            Debug.WriteLine("UUIDConvertTest start.");
            var A1 = Utility.BeaconsDict[Guid.Parse("0000803f-0000-7b3d-c941-0000c15ef342")];
            Assert.AreEqual(25.15502, A1.GetCoordinates().Latitude, 0.00001);
            Assert.AreEqual(121.68507, A1.GetCoordinates().Longitude, 0.00001);
            Assert.AreEqual(1, A1.Floor);

            var B2 = Utility.BeaconsDict[Guid.Parse("0000803f-0000-563d-c941-0000d55ef342")];
            Assert.AreEqual(25.15495, B2.GetCoordinates().Latitude, 0.00001);
            Assert.AreEqual(121.68522, B2.GetCoordinates().Longitude, 0.00001);
            Assert.AreEqual(1, A1.Floor);

            TestClose();
            Debug.WriteLine("UUIDConvertTest done.");
        }

        [TestMethod, Timeout(10000)]
        public void SignalProcessTest()
        {
            Debug.WriteLine("SignalProcessTest start.");

            Utility.BeaconScanAPI.Event.OnEventCall(new BeaconScanEventArgs
            {
                Signals = new List<BeaconSignalModel> {
                    new BeaconSignalModel
                    {
                        UUID = Guid.Parse("0000803f-0000-563d-c941-0000d55ef342"),
                        RSSI = -20
                    }}
            });

            Utility.BeaconScanAPI.Event.OnEventCall(new BeaconScanEventArgs
            {
                Signals = new List<BeaconSignalModel> {
                    new BeaconSignalModel
                    {
                        UUID = Guid.Parse("0000803f-0000-863d-c941-0000ea5ef342"),
                        RSSI = -30
                    }}
            });

            var ANSBeacon = Utility.BeaconsDict[Guid.Parse("0000803f-0000-563d-c941-0000d55ef342")];
            SignalProcessWaitEvent.WaitOne();
            Assert.AreEqual(ANSBeacon, bestBeacon);

            Utility.BeaconScanAPI.Event.OnEventCall(new BeaconScanEventArgs
            {
                Signals = new List<BeaconSignalModel> {
                    new BeaconSignalModel
                    {
                        UUID = Guid.Parse("0000803f-0000-563d-c941-0000d55ef342"),
                        RSSI = -63
                    }}
            });

            Utility.BeaconScanAPI.Event.OnEventCall(new BeaconScanEventArgs
            {
                Signals = new List<BeaconSignalModel> {
                    new BeaconSignalModel
                    {
                        UUID = Guid.Parse("0000803f-0000-863d-c941-0000ea5ef342"),
                        RSSI = -57
                    }}
            });

            ANSBeacon = Utility.BeaconsDict[Guid.Parse("0000803f-0000-563d-c941-0000d55ef342")];
            SignalProcessWaitEvent.WaitOne();
            Assert.AreEqual(ANSBeacon, bestBeacon);
            TestClose();
            Debug.WriteLine("SignalProcessTest done.");
        }

        [TestMethod, Timeout(10000)]
        public void NavigationTest()
        {
            Debug.WriteLine("NavigationTest start.");
            var routePath = Utility.WaypointRoute.GetPath(Utility.BeaconsDict[Guid.Parse("0000803f-0000-7b3d-c941-0000c15ef342")], Utility.Waypoints.First(c => c.Name == "K1F"));
            Utility.IPS.SetSetDestination(Utility.Waypoints.First(c => c.Name == "K1F"));
            // A
            Utility.SignalProcess.Event.OnEventCall(new WayPointSignalProcessEventArgs { CurrentBeacon = Utility.BeaconsDict[Guid.Parse("0000803f-0000-7b3d-c941-0000c15ef342")] });
            MaNWaitEvent.WaitOne();
            routePath.Dequeue();
            Assert.AreEqual(NavigationStatus.AdjustDirection, (MaNEventArgs as WayPointEventArgs).Status);
            // B
            Utility.SignalProcess.Event.OnEventCall(new WayPointSignalProcessEventArgs { CurrentBeacon = Utility.BeaconsDict[Guid.Parse("0000803f-0000-803d-c941-0000d45ef342")] });
            MaNWaitEvent.WaitOne();
            Assert.AreEqual(routePath.Dequeue().Angle, (MaNEventArgs as WayPointEventArgs).Angle);
            // C
            Utility.SignalProcess.Event.OnEventCall(new WayPointSignalProcessEventArgs { CurrentBeacon = Utility.BeaconsDict[Guid.Parse("0000803f-0000-863d-c941-0000ea5ef342")] });
            MaNWaitEvent.WaitOne();
            Assert.AreEqual(routePath.Dequeue().Angle, (MaNEventArgs as WayPointEventArgs).Angle);
            // D
            Utility.SignalProcess.Event.OnEventCall(new WayPointSignalProcessEventArgs { CurrentBeacon = Utility.BeaconsDict[Guid.Parse("0000803f-0000-183d-c941-0000eb5ef342")] });
            MaNWaitEvent.WaitOne();
            Assert.AreEqual(routePath.Dequeue().Angle, (MaNEventArgs as WayPointEventArgs).Angle);
            // E
            Utility.SignalProcess.Event.OnEventCall(new WayPointSignalProcessEventArgs { CurrentBeacon = Utility.BeaconsDict[Guid.Parse("0000803f-0000-ee3c-c941-0000005ff342")] });
            MaNWaitEvent.WaitOne();
            Assert.AreEqual(routePath.Dequeue().Angle, (MaNEventArgs as WayPointEventArgs).Angle);
            // F
            Utility.SignalProcess.Event.OnEventCall(new WayPointSignalProcessEventArgs { CurrentBeacon = Utility.BeaconsDict[Guid.Parse("0000803f-0000-be3c-c941-0000185ff342")] });
            MaNWaitEvent.WaitOne();
            Assert.AreEqual(routePath.Dequeue().Angle, (MaNEventArgs as WayPointEventArgs).Angle);
            // L
            Utility.SignalProcess.Event.OnEventCall(new WayPointSignalProcessEventArgs { CurrentBeacon = Utility.BeaconsDict[Guid.Parse("0000803f-0000-be3c-c941-0000395ff342")] });
            MaNWaitEvent.WaitOne();
            Assert.AreEqual(routePath.Dequeue().Angle, (MaNEventArgs as WayPointEventArgs).Angle);
            // K
            Utility.SignalProcess.Event.OnEventCall(new WayPointSignalProcessEventArgs { CurrentBeacon = Utility.BeaconsDict[Guid.Parse("0000803f-0000-b93c-c941-00005a5ff342")] });
            MaNWaitEvent.WaitOne();
            Assert.AreEqual(NavigationStatus.Arrival, (MaNEventArgs as WayPointEventArgs).Status);
            TestClose();

            Debug.WriteLine("NavigationTest done.");
        }

        [TestMethod, Timeout(10000)]
        public void StartingPointCorrection()
        {
            Debug.WriteLine("StartingPointCorrection start.");

            var routePath = Utility.WaypointRoute.RegainPath(Utility.Waypoints.First(c => c.Name == "D1F"), Utility.BeaconsDict[Guid.Parse("0000803f-0000-563d-c941-0000e85ef342")], Utility.Waypoints.First(c => c.Name == "K1F"));
            Utility.IPS.SetSetDestination(Utility.Waypoints.First(c => c.Name == "K1F"));
            // D
            Utility.SignalProcess.Event.OnEventCall(new WayPointSignalProcessEventArgs { CurrentBeacon = Utility.BeaconsDict[Guid.Parse("0000803f-0000-223d-c941-0000ff5ef342")] });
            MaNWaitEvent.WaitOne();
            Assert.AreEqual(NavigationStatus.AdjustDirection, (MaNEventArgs as WayPointEventArgs).Status);
            // C
            Utility.SignalProcess.Event.OnEventCall(new WayPointSignalProcessEventArgs { CurrentBeacon = Utility.BeaconsDict[Guid.Parse("0000803f-0000-563d-c941-0000e85ef342")] });
            MaNWaitEvent.WaitOne();
            Assert.AreEqual(NavigationStatus.AdjustRoute, (MaNEventArgs as WayPointEventArgs).Status);
            MaNWaitEvent.WaitOne();
            Assert.AreEqual(routePath.Dequeue().Angle, (MaNEventArgs as WayPointEventArgs).Angle);
            // D
            Utility.SignalProcess.Event.OnEventCall(new WayPointSignalProcessEventArgs { CurrentBeacon = Utility.BeaconsDict[Guid.Parse("0000803f-0000-223d-c941-0000ff5ef342")] });
            MaNWaitEvent.WaitOne();
            Assert.AreEqual(routePath.Dequeue().Angle, (MaNEventArgs as WayPointEventArgs).Angle);
            // E
            Utility.SignalProcess.Event.OnEventCall(new WayPointSignalProcessEventArgs { CurrentBeacon = Utility.BeaconsDict[Guid.Parse("0000803f-0000-ee3c-c941-0000005ff342")] });
            MaNWaitEvent.WaitOne();
            Assert.AreEqual(routePath.Dequeue().Angle, (MaNEventArgs as WayPointEventArgs).Angle);
            // F
            Utility.SignalProcess.Event.OnEventCall(new WayPointSignalProcessEventArgs { CurrentBeacon = Utility.BeaconsDict[Guid.Parse("0000803f-0000-be3c-c941-0000185ff342")] });
            MaNWaitEvent.WaitOne();
            Assert.AreEqual(routePath.Dequeue().Angle, (MaNEventArgs as WayPointEventArgs).Angle);
            // L
            Utility.SignalProcess.Event.OnEventCall(new WayPointSignalProcessEventArgs { CurrentBeacon = Utility.BeaconsDict[Guid.Parse("0000803f-0000-be3c-c941-0000395ff342")] });
            MaNWaitEvent.WaitOne();
            Assert.AreEqual(routePath.Dequeue().Angle, (MaNEventArgs as WayPointEventArgs).Angle);
            // K
            Utility.SignalProcess.Event.OnEventCall(new WayPointSignalProcessEventArgs { CurrentBeacon = Utility.BeaconsDict[Guid.Parse("0000803f-0000-b93c-c941-00005a5ff342")] });
            MaNWaitEvent.WaitOne();
            Assert.AreEqual(NavigationStatus.Arrival, (MaNEventArgs as WayPointEventArgs).Status);
            TestClose();

            Debug.WriteLine("StartingPointCorrection done.");
        }

        [TestMethod, Timeout(10000)]
        public void SkipSomeLocationsTest()
        {
            Debug.WriteLine("SkipSomeLocationsTest start.");

            var routePath = Utility.WaypointRoute.GetPath(Utility.BeaconsDict[Guid.Parse("0000803f-0000-7b3d-c941-0000c15ef342")], Utility.Waypoints.First(c => c.Name == "K1F"));
            Utility.IPS.SetSetDestination(Utility.Waypoints.First(c => c.Name == "K1F"));
            // A
            Utility.SignalProcess.Event.OnEventCall(new WayPointSignalProcessEventArgs { CurrentBeacon = Utility.BeaconsDict[Guid.Parse("0000803f-0000-7b3d-c941-0000c15ef342")] });
            MaNWaitEvent.WaitOne();
            routePath.Dequeue();
            Assert.AreEqual(NavigationStatus.AdjustDirection, (MaNEventArgs as WayPointEventArgs).Status);
            // B
            Utility.SignalProcess.Event.OnEventCall(new WayPointSignalProcessEventArgs { CurrentBeacon = Utility.BeaconsDict[Guid.Parse("0000803f-0000-803d-c941-0000d45ef342")] });
            MaNWaitEvent.WaitOne();
            Assert.AreEqual(routePath.Dequeue().Angle, (MaNEventArgs as WayPointEventArgs).Angle);
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
            Utility.SignalProcess.Event.OnEventCall(new WayPointSignalProcessEventArgs { CurrentBeacon = Utility.BeaconsDict[Guid.Parse("0000803f-0000-be3c-c941-0000185ff342")] });
            MaNWaitEvent.WaitOne();
            Assert.AreEqual(NavigationStatus.AdjustRoute, (MaNEventArgs as WayPointEventArgs).Status);
            MaNWaitEvent.WaitOne();
            Assert.AreEqual(routePath.Dequeue().Angle, (MaNEventArgs as WayPointEventArgs).Angle);
            // L
            Utility.SignalProcess.Event.OnEventCall(new WayPointSignalProcessEventArgs { CurrentBeacon = Utility.BeaconsDict[Guid.Parse("0000803f-0000-be3c-c941-0000395ff342")] });
            MaNWaitEvent.WaitOne();
            Assert.AreEqual(routePath.Dequeue().Angle, (MaNEventArgs as WayPointEventArgs).Angle);
            // K
            Utility.SignalProcess.Event.OnEventCall(new WayPointSignalProcessEventArgs { CurrentBeacon = Utility.BeaconsDict[Guid.Parse("0000803f-0000-b93c-c941-00005a5ff342")] });
            MaNWaitEvent.WaitOne();
            Assert.AreEqual(NavigationStatus.Arrival, (MaNEventArgs as WayPointEventArgs).Status);
            TestClose();

            Debug.WriteLine("SkipSomeLocationsTest done.");
        }

        [TestMethod, Timeout(10000)]
        public void ReNavigationTest()
        {
            Debug.WriteLine("ReNavigationTest start.");
            var routePath = Utility.WaypointRoute.GetPath(Utility.BeaconsDict[Guid.Parse("0000803f-0000-7b3d-c941-0000c15ef342")], Utility.Waypoints.First(c => c.Name == "K1F"));
            Utility.IPS.SetSetDestination(Utility.Waypoints.First(c => c.Name == "K1F"));
            // A
            Utility.SignalProcess.Event.OnEventCall(new WayPointSignalProcessEventArgs { CurrentBeacon = Utility.BeaconsDict[Guid.Parse("0000803f-0000-7b3d-c941-0000c15ef342")] });
            MaNWaitEvent.WaitOne();
            routePath.Dequeue();
            Assert.AreEqual(NavigationStatus.AdjustDirection, (MaNEventArgs as WayPointEventArgs).Status);
            // B
            Utility.SignalProcess.Event.OnEventCall(new WayPointSignalProcessEventArgs { CurrentBeacon = Utility.BeaconsDict[Guid.Parse("0000803f-0000-803d-c941-0000d45ef342")] });
            MaNWaitEvent.WaitOne();
            Assert.AreEqual(routePath.Dequeue().Angle, (MaNEventArgs as WayPointEventArgs).Angle);
            // C
            Utility.SignalProcess.Event.OnEventCall(new WayPointSignalProcessEventArgs { CurrentBeacon = Utility.BeaconsDict[Guid.Parse("0000803f-0000-863d-c941-0000ea5ef342")] });
            MaNWaitEvent.WaitOne();
            Assert.AreEqual(routePath.Dequeue().Angle, (MaNEventArgs as WayPointEventArgs).Angle);
            // D
            Utility.SignalProcess.Event.OnEventCall(new WayPointSignalProcessEventArgs { CurrentBeacon = Utility.BeaconsDict[Guid.Parse("0000803f-0000-183d-c941-0000eb5ef342")] });
            MaNWaitEvent.WaitOne();
            Assert.AreEqual(routePath.Dequeue().Angle, (MaNEventArgs as WayPointEventArgs).Angle);
            // ?�s�p��ѵ�
            routePath = Utility.WaypointRoute.RegainPath(Utility.Waypoints.First(c => c.Name == "D1F"), Utility.BeaconsDict[Guid.Parse("0000803f-0000-713d-c941-0000395ff342")], Utility.Waypoints.First(c => c.Name == "K1F"));
            // G
            Utility.SignalProcess.Event.OnEventCall(new WayPointSignalProcessEventArgs { CurrentBeacon = Utility.BeaconsDict[Guid.Parse("0000803f-0000-713d-c941-0000395ff342")] });
            MaNWaitEvent.WaitOne();
            Assert.AreEqual(NavigationStatus.AdjustRoute, (MaNEventArgs as WayPointEventArgs).Status);
            MaNWaitEvent.WaitOne();
            Assert.AreEqual(routePath.Dequeue().Angle, (MaNEventArgs as WayPointEventArgs).Angle);
            // H
            Utility.SignalProcess.Event.OnEventCall(new WayPointSignalProcessEventArgs { CurrentBeacon = Utility.BeaconsDict[Guid.Parse("0000803f-0000-b03d-c941-0000575ff342")] });
            MaNWaitEvent.WaitOne();
            Assert.AreEqual(routePath.Dequeue().Angle, (MaNEventArgs as WayPointEventArgs).Angle);
            // I
            Utility.SignalProcess.Event.OnEventCall(new WayPointSignalProcessEventArgs { CurrentBeacon = Utility.BeaconsDict[Guid.Parse("0000803f-0000-323d-c941-0000585ff342")] });
            MaNWaitEvent.WaitOne();
            Assert.AreEqual(routePath.Dequeue().Angle, (MaNEventArgs as WayPointEventArgs).Angle);
            // J
            Utility.SignalProcess.Event.OnEventCall(new WayPointSignalProcessEventArgs { CurrentBeacon = Utility.BeaconsDict[Guid.Parse("0000803f-0000-f83c-c941-00005a5ff342")] });
            MaNWaitEvent.WaitOne();
            Assert.AreEqual(routePath.Dequeue().Angle, (MaNEventArgs as WayPointEventArgs).Angle);
            // K
            Utility.SignalProcess.Event.OnEventCall(new WayPointSignalProcessEventArgs { CurrentBeacon = Utility.BeaconsDict[Guid.Parse("0000803f-0000-b93c-c941-00005a5ff342")] });
            MaNWaitEvent.WaitOne();
            Assert.AreEqual(NavigationStatus.Arrival, (MaNEventArgs as WayPointEventArgs).Status);
            TestClose();

            Debug.WriteLine("ReNavigationTest done.");
        }

        private void HandleSignalProcess(object sender, EventArgs e)
        {
            Beacon currentBeacon =
                (e as WayPointSignalProcessEventArgs).CurrentBeacon;

            bestBeacon = currentBeacon;

            SignalProcessWaitEvent.Set();
        }

        private void HandleMaNModule(object sender, EventArgs e)
        {
            MaNEventArgs = e;
            Debug.WriteLineIf(e.GetType() == typeof(WayPointEventArgs), string.Format("單元測試: 收到MaN事件, content is status: {0} and angle: {1}", (e as WayPointEventArgs).Status, (e as WayPointEventArgs).Angle));
            MaNWaitEvent.Set();
        }
    }
}
