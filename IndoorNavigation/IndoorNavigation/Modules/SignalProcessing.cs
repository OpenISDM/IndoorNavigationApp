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
        private Thread signalProcessThread;
        private  List<BeaconSignalModel> beaconSignalBuffer = 
            new List<BeaconSignalModel>();
        private bool threadSwitch = true;
        private object bufferLock = new object();

        public SignalProcessEvent Event = new SignalProcessEvent();

        public SignalProcessModule()
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
                    c.Timestamp < DateTime.Now.AddMilliseconds(-1000)))
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

                        // Send event to MaN module
                        Event.OnEventCall(new SignalProcessEventArgs {
                            CurrentBeacon = bestNearbyBeacon });
                    }
                }

                // wait 1s or wait module close
                SpinWait.SpinUntil(() => threadSwitch, 1000);
            }

            Debug.WriteLine("Signal process close");
        }

        #region IDisposable Support
        private bool disposedValue = false; // 偵測多餘的呼叫

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 處置受控狀態 (受控物件)。
                }

                // TODO: 釋放非受控資源 (非受控物件) 並覆寫下方的完成項。
                // TODO: 將大型欄位設為 null。

                disposedValue = true;
            }
        }

        // TODO: 僅當上方的 Dispose(bool disposing) 具有會釋放非受控資源的程式碼時，才覆寫完成項。
        // ~SignalProcessModule() {
        //   // 請勿變更這個程式碼。請將清除程式碼放入上方的 Dispose(bool disposing) 中。
        //   Dispose(false);
        // }

        // 加入這個程式碼的目的在正確實作可處置的模式。
        public void Dispose()
        {
            // 請勿變更這個程式碼。請將清除程式碼放入上方的 Dispose(bool disposing) 中。
            Dispose(true);
            // TODO: 如果上方的完成項已被覆寫，即取消下行的註解狀態。
            // GC.SuppressFinalize(this);
        }
        #endregion
    }

    #region Signal Process Event Handler
    public class SignalProcessEvent
    {
        public event EventHandler SignalProcessEventHandler;

        public void OnEventCall(SignalProcessEventArgs e)
        {
            SignalProcessEventHandler(this, e);
        }
    }

    public class SignalProcessEventArgs : EventArgs
    {
        public Beacon CurrentBeacon { get; set; }
    }
    #endregion
}
