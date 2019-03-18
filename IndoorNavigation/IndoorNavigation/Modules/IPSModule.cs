using System;
using System.Diagnostics;
using System.Threading;
using IndoorNavigation.Models;
using IndoorNavigation.Modules.Navigation;

namespace IndoorNavigation.Modules
{
    /// <summary>
    /// 控制導航演算法，在適當時機切換導航演算法
    /// </summary>
    public class IPSModule : IDisposable
    {
        private Thread IPSThread;
        private AutoResetEvent threadWait =
            new AutoResetEvent(false);
        private AutoResetEvent setDestinationWait =
            new AutoResetEvent(false);

        private bool isThreadRunning = true;
        private INavigationAlgorithm navigationAlgorithm;
        private WaypointModel destination;

        /// <summary>
        /// 初始化IPS module
        /// </summary>
        public IPSModule()
        {
            IPSThread = new Thread(Work);
            IPSThread.Start();
            threadWait.WaitOne();

            Debug.WriteLine("IPSModule initialization completed.");
        }

        /// <summary>
        /// Use IPS to set the destination
        /// </summary>
        public void SetDestination(WaypointModel waypoint)
        {
            destination = waypoint;
            setDestinationWait.Set();
            setDestinationWait.WaitOne();
        }

        /// <summary>
        /// Stops the navigation.
        /// </summary>
        public void StopNavigation()
        {
            if (navigationAlgorithm != null)
                navigationAlgorithm.StopNavigation();
        }

        private void Work()
        {
            // IPS algorithms
            // Temporary
            threadWait.Set();

            while(isThreadRunning)
            {
                setDestinationWait.WaitOne();

                if (isThreadRunning)
                {
                    navigationAlgorithm = Utility.Service
                        .Get<INavigationAlgorithm>("Waypoint algorithm");
                    Utility.MaN.SetAlgorithm(navigationAlgorithm);
                    Utility.SignalProcess.SetAlogorithm(
                        navigationAlgorithm.CreateSignalProcessingAlgorithm());


                    (navigationAlgorithm as WaypointAlgorithm)
                        .SetDestination(destination);

                    setDestinationWait.Set();
                }

            }

            Debug.WriteLine("IPS module close");
            threadWait.Set();
        }

        #region IDisposable Support
        private bool disposedValue = false; // 偵測多餘的呼叫

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                isThreadRunning = false;
                setDestinationWait.Set();
                StopNavigation();
                threadWait.WaitOne();

                if (disposing)
                {
                    threadWait.Dispose();
                    setDestinationWait.Dispose();
                    // TODO: 處置受控狀態 (受控物件)。
                }

                // TODO: 釋放非受控資源 (非受控物件) 並覆寫下方的完成項。
                // TODO: 將大型欄位設為 null。

                disposedValue = true;
            }
        }

        // TODO: 僅當上方的 Dispose(bool disposing) 具有會釋放非受控資源的程式碼時，才覆寫完成項。
        ~IPSModule()
        {
            // 請勿變更這個程式碼。請將清除程式碼放入上方的 Dispose(bool disposing) 中。
            Dispose(false);
        }

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
}
