using IndoorNavigation.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace IndoorNavigation.Modules
{
    class MaNModule : IDisposable
    {
        private Thread MaNThread;
        private bool threadSwitch = true;
        private bool IsReachingTheDestination = false;
        private Beacon currentBeacon = null;
        private Queue<(BeaconGroupModel Next, int Angle)> pathQueue;
        private ManualResetEvent waitEvent = new ManualResetEvent(false);
        private ManualResetEvent navigationTaskWaitEvent = 
            new ManualResetEvent(false);
        private object currentBeaconLock = new object();

        public MaNModule()
        {
            MaNThread = new Thread(MaNWork) { IsBackground = true};
            MaNThread.Start();
        }

        private void MaNWork()
        {
            while (threadSwitch)
            {
                navigationTaskWaitEvent.WaitOne();
                while (!IsReachingTheDestination)
                {

                    waitEvent.WaitOne();
                }
            }

            Debug.WriteLine("MaN module close");
        }

        /// <summary>
        /// 停止導航
        /// </summary>
        public void StopNavigation()
        {
            // 暫停MaN Thread 等待設定新的導航目的地
            IsReachingTheDestination = true;
            lock(currentBeaconLock)
                currentBeacon = null;
            waitEvent.Set();
            waitEvent.Reset();
        }

        /// <summary>
        /// 設定目的地
        /// </summary>
        /// <param name="EndPoint"></param>
        public void SetDestination(BeaconGroupModel EndPoint)
        {
            // 規劃導航路徑
            if (currentBeacon == null)
                waitEvent.WaitOne();
            lock(currentBeaconLock)
                pathQueue = Utility.Route.GetPath(currentBeacon,EndPoint);

            navigationTaskWaitEvent.Set();
            navigationTaskWaitEvent.Reset();
        }

        /// <summary>
        /// 設定目前位置的Beacon
        /// </summary>
        /// <param name="CurrentBeacon"></param>
        private void HandleSignalProcess(object sender, EventArgs e)
        {
            Beacon currentBeacon = 
                (e as SignalProcessEventArgs).CurrentBeacon;
            lock (currentBeaconLock)
                this.currentBeacon = currentBeacon;
            waitEvent.Set();
            waitEvent.Reset();
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

                threadSwitch = false;
                // TODO: 釋放非受控資源 (非受控物件) 並覆寫下方的完成項。
                // TODO: 將大型欄位設為 null。

                disposedValue = true;
            }
        }

        // TODO: 僅當上方的 Dispose(bool disposing) 具有會釋放非受控資源的程式碼時，才覆寫完成項。
        // ~MaNModule() {
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

    #region MaN module Event Handler
    public class MaNEEvent
    {
        public event EventHandler MaNEventHandler;

        public void OnEventCall(MaNEventArgs e)
        {
            MaNEventHandler(this, e);
        }
    }

    public class MaNEventArgs : EventArgs
    {
    }
    #endregion
}
