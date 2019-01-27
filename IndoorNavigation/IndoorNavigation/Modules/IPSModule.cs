using System;
using System.Diagnostics;
using System.Threading;
using IndoorNavigation.Models;
using IndoorNavigation.Modules.Navigation;

namespace IndoorNavigation.Modules
{
    public class IPSModule : IDisposable
    {
        //private Thread IPSThread;
        private ManualResetEvent threadClosedWait =
            new ManualResetEvent(false);
        public INavigationAlgorithm navigationAlgorithm { get; private set; }

        public IPSModule()
        {
            // Temporary
            navigationAlgorithm = Utility.Service
                .Get<INavigationAlgorithm>("Way point algorithm");
            Utility.MaN.SetAlgorithm(navigationAlgorithm);
            Utility.SignalProcess.SetAlogorithm(
                navigationAlgorithm.CreateSignalProcessingAlgorithm());

            //IPSThread = new Thread(Work);
            //IPSThread.Start();
        }

        public void SetSetDestination(WaypointModel waypoint)
        {
            (navigationAlgorithm as WayPointAlgorithm).SetDestination(waypoint);
        }

        private void Work()
        {
            // IPS algorithms



            Debug.WriteLine("IPS module close");
            threadClosedWait.Set();
            threadClosedWait.Reset();
        }

        #region IDisposable Support
        private bool disposedValue = false; // 偵測多餘的呼叫

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                threadClosedWait.WaitOne();

                if (disposing)
                {
                    threadClosedWait.Dispose();
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
