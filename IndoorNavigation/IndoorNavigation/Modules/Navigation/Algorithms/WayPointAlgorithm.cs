using IndoorNavigation.Models;
using IndoorNavigation.Modules.SignalProcessingAlgorithms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace IndoorNavigation.Modules.Navigation
{
    public class WayPointAlgorithm : INavigationAlgorithm, IDisposable
    {
        private Beacon currentBeacon;
        private WaypointModel previousWaypoint;
        private WaypointModel endWaypoint;
        private NextStepModel nextInstruction;
        private Queue<NextStepModel> pathQueue;
        private AutoResetEvent nextBeaconWaitEvent =
            new AutoResetEvent(false);
        private AutoResetEvent navigationTaskWaitEvent =
            new AutoResetEvent(false);
        private object resourceLock = new object();
        private readonly EventHandler HSignalProcess;
        private ISignalProcessingAlgorithm signalProcessingAlgorithm;

        public WayPointAlgorithm()
        {
            IsReachingDestination = false;
            HSignalProcess = new EventHandler(HandleSignalProcess);
            Utility.SignalProcess.Event.SignalProcessEventHandler +=
                HSignalProcess;
            signalProcessingAlgorithm = 
                Utility.Service.Get<ISignalProcessingAlgorithm>
                ("Way point signal processing algorithm");
        }

        public void Work()
        {
            // Wait for the navigation task
            navigationTaskWaitEvent.WaitOne();
            while (!IsReachingDestination)
            {
                // Find the current position from the current Beacon
                WaypointModel currentWaypoint;
                lock (resourceLock)
                    currentWaypoint = Utility.Waypoints
                        .First(c => c.Beacons.Contains(currentBeacon));


                lock (resourceLock)
                {
                    // Check if he arrives the destination
                    if (currentWaypoint == endWaypoint)
                    {
                        Utility.MaN.Event.OnEventCall(new WayPointEventArgs
                        {
                            Status = NavigationStatus.Arrival
                        });
                        break;
                    }

                    // if NextInstruction is null, it represents the 
                    // navigation starts at the first waypoint.
                    // The current version, user has to walk in random 
                    // way, once he reaches the second location then 
                    // calibration.
                    if (nextInstruction == null)
                    {
                        nextInstruction = pathQueue.Dequeue();

                        Utility.MaN.Event.OnEventCall(new WayPointEventArgs
                        {
                            Status = NavigationStatus.AdjustDirection
                        });
                    }
                    else
                    {
                        // Check if the user reachs the next location
                        if (currentWaypoint == nextInstruction.NextWaypoint)
                        {
                            nextInstruction = pathQueue.Dequeue();
                            double distance = nextInstruction.NextWaypoint
                                .Coordinates
                                .GetDistanceTo(
                                currentBeacon.GetCoordinates());

                            Utility.MaN.Event.OnEventCall(new WayPointEventArgs
                            {
                                Status = NavigationStatus.Run,
                                Angle = nextInstruction.Angle,
                                Distance = distance
                            });
                        }
                        else
                        {
                            // Alter the wrong path, 
                            // and tell the next step
                            Utility.MaN.Event.OnEventCall(new WayPointEventArgs
                            {
                                Status = NavigationStatus.AdjustRoute
                            });

                            Utility.MaN.Event.OnEventCall(
                                NavigationRouteCorrection(currentWaypoint,
                                endWaypoint));
                        }
                    }
                }

                // Wait for the event of next Beacon
                nextBeaconWaitEvent.WaitOne();

                // Change the current location to the last location
                previousWaypoint = currentWaypoint;
            }
        }

        /// <summary>
        /// Modify the navigation path
        /// </summary>
        /// <param name="CurrentWaypoint"></param>
        /// <returns></returns>
        private WayPointEventArgs NavigationRouteCorrection
            (WaypointModel CurrentWaypoint, WaypointModel EndWaypoint)
        {
            // If the current location is in the path
            if (pathQueue.Where(c => c.NextWaypoint == CurrentWaypoint).Count() > 0)
            {
                // Remove the location in the queue of path until the dequeued
                // location is the same as current location
                var CurrentInstruction = pathQueue
                        .First(c => c.NextWaypoint == CurrentWaypoint);
                while (pathQueue.Dequeue() != CurrentInstruction) ;

                // Keep navigating
                nextInstruction = pathQueue.Dequeue();
                double distance = nextInstruction.NextWaypoint
                    .Coordinates
                    .GetDistanceTo(currentBeacon.GetCoordinates());

                return new WayPointEventArgs
                {
                    Status = NavigationStatus.Run,
                    Distance = distance,
                    Angle = nextInstruction.Angle
                };
            }
            else
            {
                // Check the current waypoint whether is connected to the 
                // previous waypoint in the navigation graph.
                // If connected, it dosn't need to calibrate the direction.
                if (Utility.LocationConnects
                    .Any(c => c.BeaconA == CurrentWaypoint &&
                    c.BeaconB == previousWaypoint) ||
                    Utility.LocationConnects
                    .Any(c => c.BeaconA == previousWaypoint &&
                    c.BeaconB == CurrentWaypoint))
                {
                    // Replan the path, and keep navigating
                    pathQueue = Utility.WaypointRoute.RegainPath(previousWaypoint,
                        currentBeacon, EndWaypoint);
                    nextInstruction = pathQueue.Dequeue();
                    double distance = nextInstruction.NextWaypoint
                        .Coordinates
                        .GetDistanceTo(
                        currentBeacon.GetCoordinates());

                    return new WayPointEventArgs
                    {
                        Status = NavigationStatus.Run,
                        Angle = nextInstruction.Angle,
                        Distance = distance
                    };

                }
                else
                {
                    // Replan the path and calibrate the direction
                    pathQueue = Utility.WaypointRoute.GetPath(currentBeacon, EndWaypoint);
                    nextInstruction = pathQueue.Dequeue();

                    return new WayPointEventArgs
                    {
                        Status = NavigationStatus.AdjustDirection
                    };
                }
            }
        }

        public ISignalProcessingAlgorithm CreateSignalProcessingAlgorithm()
        {
            return signalProcessingAlgorithm;
        }

        public void StopNavigation()
        {
            Dispose();
        }

        public bool IsReachingDestination { get; private set; }

        /// <summary>
        /// Set the destination
        /// </summary>
        /// <param name="EndWaypoint"></param>
        public void SetDestination(WaypointModel EndWaypoint)
        {
            Task.Run(() =>
            {
                // Plan the navigation path
                if (currentBeacon == null)
                {
                    nextBeaconWaitEvent.WaitOne();
                }

                lock (resourceLock)
                {
                    pathQueue = Utility.WaypointRoute.GetPath(currentBeacon, EndWaypoint);
                    endWaypoint = EndWaypoint;
                }

                navigationTaskWaitEvent.Set();
            });
        }

        /// <summary>
        /// Receive the next Beacon returned by signal process model
        /// </summary>
        /// <param name="CurrentBeacon"></param>
        private void HandleSignalProcess(object sender, EventArgs e)
        {
            Beacon _currentBeacon =
                (e as WayPointSignalProcessEventArgs).CurrentBeacon;
            Debug.WriteLine("Way point algorithm: Receive UUID: {0}", (e as WayPointSignalProcessEventArgs).CurrentBeacon.UUID);

            // Check this event of signal processing from the current Beacon 
            // if it is the same as currrent Beacon
            if (this.currentBeacon != _currentBeacon)
            {
                lock (resourceLock)
                    this.currentBeacon = _currentBeacon;
                nextBeaconWaitEvent.Set();
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // 偵測多餘的呼叫

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                IsReachingDestination = true;
                lock (resourceLock)
                    currentBeacon = null;
                nextBeaconWaitEvent.Set();
                Utility.SignalProcess.Event.SignalProcessEventHandler -=
                        HSignalProcess;
                navigationTaskWaitEvent.Set();
                if (disposing)
                {
                    navigationTaskWaitEvent.Dispose();
                    nextBeaconWaitEvent.Dispose();
                    navigationTaskWaitEvent = null;
                    nextBeaconWaitEvent = null;
                }

                currentBeacon = null;
                previousWaypoint = null;
                endWaypoint = null;
                nextInstruction = null;
                pathQueue = null;
                resourceLock = null;
                signalProcessingAlgorithm = null;

                disposedValue = true;
            }
        }

        //TODO: 僅當上方的 Dispose(bool disposing) 具有會釋放非受控資源的程式碼時，才覆寫完成項。
        ~WayPointAlgorithm()
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

    public enum NavigationStatus
    {
        Run = 0,
        Arrival,
        AdjustRoute,
        AdjustDirection
    }

    public class WayPointEventArgs : EventArgs
    {
        /// <summary>
        /// Status of navigation
        /// </summary>
        public NavigationStatus Status { get; set; }
        /// <summary>
        /// The angle of turn
        /// </summary>
        public int Angle { get; set; }
        /// <summary>
        /// The distance to next location
        /// </summary>
        public double Distance { get; set; }
    }
}