/*
 * Copyright (c) 2018 Academia Sinica, Institude of Information Science
 *
 * License:
 *      GPL 3.0 : The content of this file is subject to the terms and 
 *      conditions defined in file 'COPYING.txt', which is part of this source
 *      code package.
 *
 * Project Name:
 * 
 *      IndoorNavigation
 * 
 * File Description:
 * File Name:
 * 
 *      MaNModule.cs
 * 
 * Abstract:
 *      
 *      Monitor and Notification module負責監視使用者移動的路線，在使用者走錯路之後傳送event給UI通知使用者，並為使用者重新規劃路線
 *
 * Authors:
 * 
 *      Kenneth Tang, kenneth@gm.nssh.ntpc.edu.tw
 * 
 */

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
        private bool IsReachingDestination = false;
        private Beacon currentBeacon;
        private BeaconGroupModel previousPoint;
        private NextInstructionModel nextInstruction;
        private Queue<NextInstructionModel> pathQueue;
        private ManualResetEvent bestBeacon = new ManualResetEvent(false);
        private ManualResetEvent navigationTaskWaitEvent = 
            new ManualResetEvent(false);
        private ManualResetEvent threadClosedWait =
            new ManualResetEvent(false);
        private object resourceLock = new object();
        private EventHandler HSignalProcess;

        public MaNEEvent Event;

        /// <summary>
        /// 初始化一個新的通知模組
        /// </summary>
        public MaNModule()
        {
            Event = new MaNEEvent();
            MaNThread = new Thread(MaNWork) { IsBackground = true};
            MaNThread.Start();
            HSignalProcess = new EventHandler(HandleSignalProcess);
            Utility.SignalProcess.Event.SignalProcessEventHandler += HSignalProcess;
        }

        private void MaNWork()
        {
            while (threadSwitch)
            {
                // 等待導航任務
                navigationTaskWaitEvent.WaitOne();
                while (!IsReachingDestination)
                {
                    // 從目前的Beacon找出現在所在的位置
                    BeaconGroupModel currentPoint;
                    lock(resourceLock)
                        currentPoint = Utility.BeaconGroups
                            .Where(c => c.Beacons.Contains(currentBeacon))
                            .First();

                    // 檢查是否抵達目的地
                    var EndPoint =
                        pathQueue.ToArray()[pathQueue.Count() - 1].NextPoint;

                    if (currentPoint == EndPoint)
                    {
                        Event.OnEventCall(new MaNEventArgs
                        {
                            Status = NavigationStatus.Arrival
                        });
                        break;
                    }


                    lock (resourceLock)
                        // NextInstruction=null代表現在導航開始的第一個位置
                        // 目前版本第一個位置必須讓使用者先隨便行走，到達第二個位置再校正方向
                        if (nextInstruction == null)
                        {
                            nextInstruction = pathQueue.Dequeue();

                            Event.OnEventCall(new MaNEventArgs
                            {
                                Status = NavigationStatus.DirectionCorrection
                            });
                        }
                        else
                        {
                            // 檢查是否抵達下一個位置
                            if (currentPoint == nextInstruction.NextPoint)
                            {
                                nextInstruction = pathQueue.Dequeue();
                                double distance = nextInstruction.NextPoint
                                    .Coordinate
                                    .GetDistanceTo(
                                    currentBeacon.GetCoordinate());

                                Event.OnEventCall(new MaNEventArgs
                                {
                                    Status = NavigationStatus.Run,
                                    Angle = nextInstruction.Angle,
                                    Distance = distance
                                });
                            }
                            else
                            {
                                // 先通知走錯路，再通知下一步怎麼走
                                Event.OnEventCall(new MaNEventArgs
                                {
                                    Status = NavigationStatus.RouteCorrection
                                });

                                Event.OnEventCall(
                                    NavigationRouteCorrection(currentPoint,
                                    EndPoint));
                            }
                        }


                    // 等待抵達下一個最佳Beacon附近事件
                    bestBeacon.WaitOne();

                    // 將現在的位置變成上一個位置
                    previousPoint = currentPoint;
                }
            }

            Debug.WriteLine("MaN module close");
            threadClosedWait.Set();
            threadClosedWait.Reset();
        }

        /// <summary>
        /// 修正導航路線
        /// </summary>
        /// <param name="CurrentPoint"></param>
        /// <returns></returns>
        private MaNEventArgs NavigationRouteCorrection
            (BeaconGroupModel CurrentPoint,
            BeaconGroupModel EndPoint)
        {
            // 如果現在位置為導航路線的其中一個點
            if (pathQueue.Where(c => c.NextPoint == CurrentPoint).Count() > 0)
            {
                // 將路線佇列中的位置移除，直到佇列dequeue出來的位置=現在位置
                var CurrentInstruction = pathQueue
                    .Where(c => c.NextPoint == CurrentPoint).First();
                while (pathQueue.Dequeue() != CurrentInstruction) ;

                // 繼續導航
                nextInstruction = pathQueue.Dequeue();
                double distance = nextInstruction.NextPoint
                    .Coordinate
                    .GetDistanceTo(currentBeacon.GetCoordinate());

                return new MaNEventArgs
                {
                    Status = NavigationStatus.Run,
                    Distance = distance,
                    Angle = nextInstruction.Angle
                };
            }
            else
            {
                // 檢查現在所在位置是否與上一個位置連接
                // 有連接可以不用校正方向
                if (Utility.LocationConnects
                    .Where(c => c.BeaconA == CurrentPoint && 
                    c.BeaconB == previousPoint).Count() > 0 || 
                    Utility.LocationConnects
                    .Where(c => c.BeaconA == previousPoint && 
                    c.BeaconB == CurrentPoint).Count() > 0)
                {
                    // 重新規劃路徑，並繼續導航
                    pathQueue = Utility.Route.RegainPath(previousPoint, 
                        currentBeacon, EndPoint);
                    nextInstruction = pathQueue.Dequeue();
                    double distance = nextInstruction.NextPoint
                        .Coordinate
                        .GetDistanceTo(
                        currentBeacon.GetCoordinate());

                    return new MaNEventArgs
                    {
                        Status = NavigationStatus.Run,
                        Angle = nextInstruction.Angle,
                        Distance = distance
                    };

                }
                else
                {
                    // 重新規劃路徑，並且校正方向再繼續導航
                    pathQueue = Utility.Route.GetPath(currentBeacon,EndPoint);
                    nextInstruction = pathQueue.Dequeue();

                    return new MaNEventArgs
                    {
                        Status = NavigationStatus.DirectionCorrection
                    };
                }
            }
        }

        /// <summary>
        /// 停止導航
        /// </summary>
        public void StopNavigation()
        {
            // 暫停MaN Thread 等待設定新的導航目的地
            IsReachingDestination = true;
            lock(resourceLock)
                currentBeacon = null;
            bestBeacon.Set();
            bestBeacon.Reset();
        }

        /// <summary>
        /// 設定目的地
        /// </summary>
        /// <param name="EndPoint"></param>
        public void SetDestination(BeaconGroupModel EndPoint)
        {
            // 規劃導航路徑
            if (currentBeacon == null)
                bestBeacon.WaitOne();
            lock(resourceLock)
                pathQueue = Utility.Route.GetPath(currentBeacon,EndPoint);

            navigationTaskWaitEvent.Set();
            navigationTaskWaitEvent.Reset();
        }

        /// <summary>
        /// 接收來自signal process model傳送的最佳Beacon
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
                bestBeacon.Set();
                bestBeacon.Reset();
            }
        }

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Utility.SignalProcess.Event.SignalProcessEventHandler -= HSignalProcess;
                    threadSwitch = false;
                    navigationTaskWaitEvent.Set();
                    bestBeacon.Set();
                    threadClosedWait.WaitOne();
                    navigationTaskWaitEvent.Dispose();
                    bestBeacon.Dispose();
                    threadClosedWait.Dispose();
                }

                MaNThread = null;
                resourceLock = null;

                disposedValue = true;
            }
        }

        ~MaNModule()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            // GC.SuppressFinalize(this);
        }
        #endregion

    }

    #region MaN module Event Handler

    public enum NavigationStatus
    {
        Run = 0,
        Arrival,
        RouteCorrection,
        DirectionCorrection
    }

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
        /// 導航狀態
        /// </summary>
        public NavigationStatus Status { get; set; }
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
