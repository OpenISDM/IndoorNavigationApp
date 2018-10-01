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
    public class SignalProcessModule : IDisposable
    {
        private Thread SignalProcessThread;
        private  List<BeaconSignalModel> BeaconSignalBuffer = 
            new List<BeaconSignalModel>();
        private bool Switch = true;
        private object BufferLock = new object();

        public SignalProcessModule()
        {
            SignalProcessThread = 
                new Thread(SignalProcessWork){ IsBackground = true};
            SignalProcessThread.Start();
        }

        /// <summary>
        /// 插入搜尋到的Beacon訊號至Buffer
        /// </summary>
        /// <param name="Signals"></param>
        public void AddSignal(List<BeaconSignalModel> Signals)
        {
            // Beacon訊號過濾，保留存在地圖記錄的Beacon訊號
            IEnumerable<BeaconSignalModel> _Signals = Signals
                .Where(Signal => Utility.Beacons
                .Select(beacon => (beacon.UUID,beacon.Major,beacon.Minor))
                .Contains((Signal.UUID, Signal.Major, Signal.Minor)));

            lock (BufferLock)
                BeaconSignalBuffer.AddRange(_Signals);
        }

        private void SignalProcessWork()
        {
            while (Switch)
            {
                List<BeaconSignal> SignalAverageList = 
                    new List<BeaconSignal>();

                // SignalProcess
                lock (BufferLock)
                {
                    // remove buffer old data
                    foreach (var BeaconSignal in BeaconSignalBuffer.Where(c =>
                    c.Timestamp < DateTime.Now.AddMilliseconds(-1000)))
                        BeaconSignalBuffer.Remove(BeaconSignal);

                    // Average the intensity of all Beacon signals
                    var Beacons = BeaconSignalBuffer
                        .Select(c => (c.UUID, c.Major, c.Minor)).Distinct();
                    foreach (var (UUID, Major, Minor) in Beacons)
                    {
                        SignalAverageList.Add(
                            new BeaconSignal {
                                UUID = (UUID, Major, Minor).UUID,
                                Major = (UUID, Major, Minor).Major,
                                Minor = (UUID, Major, Minor).Minor,
                                RSSI = System.Convert.ToInt32(
                                    BeaconSignalBuffer
                                    .Where(c => 
                                    c.UUID == (UUID, Major, Minor).UUID && 
                                    c.Major == (UUID, Major, Minor).Major && 
                                    c.Minor == (UUID, Major, Minor).Minor)
                                    .Select(c => c.RSSI).Average())
                            });
                    }
                }

                // Find the beacon closest to me
                if (SignalAverageList.Count() > 0)
                {
                    // 尋找所有滿足門檻值條件的訊號
                    var nearbySignal = (from single in SignalAverageList
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

                // wait 1s or wait module close
                SpinWait.SpinUntil(() => Switch, 1000);
            }

            Debug.WriteLine("Signal process close");
        }

        public void Dispose()
        {
            Switch = false;
        }
    }
}
