using IndoorNavigation.Modules;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using IndoorNavigation.Models;
using System.Threading;
using System.Diagnostics;
using IndoorNavigation.Modules.Utilities;
using IndoorNavigation.Modules.IPSClients;

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


            Utility.BeaconScan = new BeaconScan();
            Debug.WriteLine("單元測試初始化完成");
        }

        public void TestClose()
        {

        }

        [TestMethod]
        public void StorageTest()
        {
            Debug.WriteLine("StorageTest start.");
            NavigraphStorage.DeleteAllNavigraph();
            NavigraphStorage.SaveNavigraphInformation("test1", "");
            NavigraphStorage.SaveNavigraphInformation("test1", "");
            string[] Maps = NavigraphStorage.GetAllNavigraphs();
            Assert.AreEqual(1, Maps.Length);

            NavigraphStorage.SaveNavigraphInformation("test2", "");
            NavigraphStorage.SaveNavigraphInformation("test3", "");
            Maps = NavigraphStorage.GetAllNavigraphs();
            Assert.AreEqual(3, Maps.Length);

            NavigraphStorage.DeleteNavigraph("test4");
            Maps = NavigraphStorage.GetAllNavigraphs();
            Assert.AreEqual(3, Maps.Length);

            NavigraphStorage.DeleteNavigraph("test3");
            Maps = NavigraphStorage.GetAllNavigraphs();
            Assert.AreEqual(2, Maps.Length);

            NavigraphStorage.DeleteAllNavigraph();
            Maps = NavigraphStorage.GetAllNavigraphs();
            Assert.AreEqual(0, Maps.Length);
            TestClose();
            Debug.WriteLine("StorageTest done.");
        }

        [TestMethod, Timeout(10000)]
        public void SignalProcessTest()
        {
            Debug.WriteLine("SignalProcessTest start.");

            Utility.BeaconScan.Event.OnEventCall(new BeaconScanEventArgs
            {
                Signals = new List<BeaconSignalModel> {
                    new BeaconSignalModel
                    {
                        UUID = Guid.Parse("0000803f-0000-563d-c941-0000d55ef342"),
                        RSSI = -20
                    }}
            });

            Utility.BeaconScan.Event.OnEventCall(new BeaconScanEventArgs
            {
                Signals = new List<BeaconSignalModel> {
                    new BeaconSignalModel
                    {
                        UUID = Guid.Parse("0000803f-0000-863d-c941-0000ea5ef342"),
                        RSSI = -30
                    }}
            });

            SignalProcessWaitEvent.WaitOne();

            Utility.BeaconScan.Event.OnEventCall(new BeaconScanEventArgs
            {
                Signals = new List<BeaconSignalModel> {
                    new BeaconSignalModel
                    {
                        UUID = Guid.Parse("0000803f-0000-563d-c941-0000d55ef342"),
                        RSSI = -63
                    }}
            });

            Utility.BeaconScan.Event.OnEventCall(new BeaconScanEventArgs
            {
                Signals = new List<BeaconSignalModel> {
                    new BeaconSignalModel
                    {
                        UUID = Guid.Parse("0000803f-0000-863d-c941-0000ea5ef342"),
                        RSSI = -57
                    }}
            });

            SignalProcessWaitEvent.WaitOne();
            TestClose();
            Debug.WriteLine("SignalProcessTest done.");
        }

        private void HandleSignalProcess(object sender, EventArgs e)
        {
            Beacon currentBeacon =
                (e as WayPointSignalEventArgs).CurrentBeacon;

            bestBeacon = currentBeacon;

            SignalProcessWaitEvent.Set();
        }

        private void HandleMaNModule(object sender, EventArgs e)
        {
            MaNEventArgs = e;
            MaNWaitEvent.Set();
        }
    }
}
