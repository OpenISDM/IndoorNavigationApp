using IndoorNavigation.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Linq;

namespace IndoorNavigation.Modules
{
    /// <summary>
    /// beacon訊號後處理
    /// </summary>
    public class SignalProcess:IDisposable
    {
        private Thread signalProcessThread;
        private  List<BeaconSignalModel> beaconSignalBuffer = 
            new List<BeaconSignalModel>();
        private bool threadSwitch = true;
        private object bufferLock = new object();

        public SignalProcess()
        {
            signalProcessThread = 
                new Thread(SignalProcessWork){ IsBackground = true};
            signalProcessThread.Start();
        }

        /// <summary>
        /// 插入搜尋到的Beacon訊號至Buffer
        /// </summary>
        /// <param name="Signals"></param>
        public void AddSignal(List<BeaconSignalModel> Signals)
        {
            // Beacon訊號過濾，保留存在地圖記錄的Beacon訊號
            IEnumerable<BeaconSignalModel> signals = Signals
                .Where(signal => Utility.Beacons
                .Select(beacon => (beacon.UUID,beacon.Major,beacon.Minor))
                .Contains((signal.UUID, signal.Major, signal.Minor)));

            lock (bufferLock)
                beaconSignalBuffer.AddRange(signals);
        }

        private void SignalProcessWork()
        {
            while (threadSwitch)
            {
                List<BeaconSignal> signalAverageList = 
                    new List<BeaconSignal>();

                // SignalProcess
                lock (bufferLock)
                {
                    // remove buffer old data
                    foreach (var beaconSignal in beaconSignalBuffer.Where(c =>
                    c.Timestamp < DateTime.Now.AddMilliseconds(-500)))
                        beaconSignalBuffer.Remove(beaconSignal);

                    // Average the intensity of all Beacon signals
                    var beacons = beaconSignalBuffer
                        .Select(c => (c.UUID, c.Major, c.Minor)).Distinct();
                    foreach (var (UUID, Major, Minor) in beacons)
                    {
                        signalAverageList.Add(
                            new BeaconSignal {
                                UUID = (UUID, Major, Minor).UUID,
                                Major = (UUID, Major, Minor).Major,
                                Minor = (UUID, Major, Minor).Minor,
                                RSSI = System.Convert.ToInt32(
                                    beaconSignalBuffer
                                    .Where(c => 
                                    c.UUID == (UUID, Major, Minor).UUID && 
                                    c.Major == (UUID, Major, Minor).Major && 
                                    c.Minor == (UUID, Major, Minor).Minor)
                                    .Select(c => c.RSSI).Average())
                            });
                    }
                }

                // Find the beacon closest to me
                if (signalAverageList.Count() > 0)
                {
                    // 尋找所有滿足門檻值條件的訊號
                    var nearbySignal = (from single in signalAverageList
                                        from beacon in Utility.Beacons
                                        where (single.UUID == beacon.UUID && 
                                        single.Major == beacon.Major && 
                                        single.Minor == beacon.Minor && 
                                        single.RSSI >= beacon.Threshold)
                                        select single);

                    // Find the beacon closest to me, then send an event 
                    // to MaN
                    if (nearbySignal.Count() > 0)
                    {
                        var bestNearbySignal = nearbySignal.First();
                        Beacon bestNearbyBeacon = Utility.Beacons
                            .Where(beacon => 
                            beacon.UUID == bestNearbySignal.UUID && 
                            beacon.Major == bestNearbySignal.Major && 
                            beacon.Minor == bestNearbySignal.Minor)
                            .First();

                        // Send event to MaN Module

                    }
                }

                // wait 500s or wait module close
                SpinWait.SpinUntil(() => threadSwitch, 500);
            }

            Debug.WriteLine("Signal process close");
        }

        public void Dispose()
        {
            threadSwitch = false;
        }
    }
}
