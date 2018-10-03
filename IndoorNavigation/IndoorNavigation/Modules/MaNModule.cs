using IndoorNavigation.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Linq;

namespace IndoorNavigation.Modules
{
    /// <summary>
    /// 通知模組
    /// </summary>
    class MaNModule : IDisposable
    {
        private Thread MaNThread;
        private bool threadSwitch = true;
        private bool IsReachingTheDestination = false;
        private Beacon currentBeacon = null;
        private NextInstructionModel NextInstruction;
        private Queue<NextInstructionModel> pathQueue;
        private ManualResetEvent waitEvent = new ManualResetEvent(false);
        private ManualResetEvent navigationTaskWaitEvent = 
            new ManualResetEvent(false);
        private object resourceLock = new object();

        public MaNEEvent Event;

        /// <summary>
        /// 初始化一個新的通知模組
        /// </summary>
        public MaNModule()
        {
            Event = new MaNEEvent();
            MaNThread = new Thread(MaNWork) { IsBackground = true};
            MaNThread.Start();
        }

        private void MaNWork()
        {
            while (threadSwitch)
            {
                // 等待導航任務
                navigationTaskWaitEvent.WaitOne();
                while (!IsReachingTheDestination)
                {
                    lock(resourceLock)
                        // NextInstruction=null代表現在導航開始的第一個位置
                        if (NextInstruction == null)
                        {
                            NextInstruction = pathQueue.Dequeue();
                            double distance = NextInstruction.NextPoint
                                .Coordinate
                                .GetDistanceTo(currentBeacon.GetCoordinate());

                            Event.OnEventCall(new MaNEventArgs
                            {
                                Angle = NextInstruction.Angle,
                                Distance = distance
                            });
                        }
                        else
                        {
                            BeaconGroupModel currentPoint = 
                                Utility.BeaconGroups
                            .Where(c => c.Beacons.Contains(currentBeacon))
                            .First();

                            // 檢查現在位置是否跟規劃的下一個位置一樣
                            if (currentPoint == NextInstruction.NextPoint)
                            {
                                NextInstruction = pathQueue.Dequeue();
                                double distance = NextInstruction.NextPoint
                                    .Coordinate
                                    .GetDistanceTo(
                                    currentBeacon.GetCoordinate());

                                Event.OnEventCall(new MaNEventArgs
                                {
                                    Angle = NextInstruction.Angle,
                                    Distance = distance
                                });
                            }
                            else
                            {
                                Event.OnEventCall(
                                    NavigationRouteCorrection(currentPoint));
                            }
                        }

                    NextInstruction = pathQueue.Dequeue();


                    // 等待最佳Beacon事件
                    waitEvent.WaitOne();
                }
            }

            Debug.WriteLine("MaN module close");
        }

        /// <summary>
        /// 修正導航路線
        /// </summary>
        /// <param name="CurrentPoint"></param>
        /// <returns></returns>
        private MaNEventArgs NavigationRouteCorrection
            (BeaconGroupModel CurrentPoint)
        {
            // 如果現在位置為導航路線的其中一個點
            if (pathQueue.Where(c => c.NextPoint == CurrentPoint).Count() > 0)
            {
                // 將路線佇列中的移除，直到佇列dequeue出來的位置=現在位置
                var CurrentInstruction = pathQueue
                    .Where(c => c.NextPoint == CurrentPoint).First();
                while (pathQueue.Dequeue() != CurrentInstruction) ;

                // 繼續導航
                NextInstruction = pathQueue.Dequeue();
                double distance = NextInstruction.NextPoint
                    .Coordinate
                    .GetDistanceTo(currentBeacon.GetCoordinate());

                return new MaNEventArgs
                {
                    Distance = distance,
                    Angle = NextInstruction.Angle
                };
            }
            else
            {
                // 重新規劃路徑，並繼續導航
                var EndPoint = 
                    pathQueue.ToArray()[pathQueue.Count() - 1].NextPoint;
                pathQueue = Utility.Route.GetPath(currentBeacon, EndPoint);

                NextInstruction = pathQueue.Dequeue();
                double distance = NextInstruction.NextPoint
                    .Coordinate
                    .GetDistanceTo(currentBeacon.GetCoordinate());

                return new MaNEventArgs
                {
                    Distance = distance,
                    Angle = NextInstruction.Angle
                };
            }
        }

        /// <summary>
        /// 停止導航
        /// </summary>
        public void StopNavigation()
        {
            // 暫停MaN Thread 等待設定新的導航目的地
            IsReachingTheDestination = true;
            lock(resourceLock)
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
            lock(resourceLock)
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

            // 檢查本次Signal Process事件的Current Beacon
            // 是否和Current Beacon相同
            if (this.currentBeacon != currentBeacon)
            {
                lock (resourceLock)
                    this.currentBeacon = currentBeacon;
                waitEvent.Set();
                waitEvent.Reset();
            }
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
        /// <summary>
        /// 旋轉角度
        /// </summary>
        public int Angle { get; set; }
        /// <summary>
        /// 到下個點的距離
        /// </summary>
        public double Distance { get; set; }
    }
    #endregion
}
