/*
  Copyright (c) 2018 Academia Sinica, Institude of Information Science
 
    License:
        GPL 3.0 : The content of this file is subject to the terms and
        conditions defined in file 'COPYING.txt', which is part of this source
        code package.
 
    Project Name:
 
        IndoorNavigation
 
    File Description:
 
        The algorithm for waypoint navigation used

    File Name:

        WayPointAlgorithm.cs

    Abstract:

        The mobile application of indoor navigation, it was built using 
        Xamarin.Forms.

    Authors:
 
        Kenneth Tang, kenneth@gm.nssh.ntpc.edu.tw
 
 */

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
    public class WaypointAlgorithm : INavigationAlgorithm, IDisposable
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

        /// <summary>
        /// Initializes the waypoint navigation algorithm
        /// </summary>
        public WaypointAlgorithm()
        {
            IsReachingDestination = false;
            HSignalProcess = new EventHandler(HandleSignalProcess);
            Utility.SignalProcess.Event.SignalProcessEventHandler +=
                HSignalProcess;
            signalProcessingAlgorithm =
                Utility.Service.Get<ISignalProcessingAlgorithm>
                ("Waypoint signal processing algorithm");
        }

        public void Work()
        {
            // Waiting for the destination set
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
                    // Check whether arrived the destination
                    if (currentWaypoint == endWaypoint)
                    {
                        Utility.MaN.Event.OnEventCall(new WaypointEventArgs
                        {
                            Status = NavigationStatus.Arrival
                        });
                        break;
                    }

                    // if NextInstruction is null, it represents the 
                    // navigation starts at the first waypoint.
                    if (nextInstruction == null)
                    {
                        nextInstruction = pathQueue.Dequeue();

                        Utility.MaN.Event.OnEventCall(new WaypointEventArgs
                        {
                            Status = NavigationStatus.AdjustDirection
                        });
                    }
                    else
                    {
                        // Check if the user reaches the next waypoint
                        if (currentWaypoint == nextInstruction.NextWaypoint)
                        {
                            nextInstruction = pathQueue.Dequeue();
                            double distance = nextInstruction.NextWaypoint
                                .Coordinates
                                .GetDistanceTo(
                                currentBeacon.GetCoordinates());

                            Utility.MaN.Event.OnEventCall(
                                new WaypointEventArgs
                                {
                                    Status = NavigationStatus.Run,
                                    Angle = nextInstruction.Angle,
                                    Distance = distance
                                });
                        }
                        else
                        {
                            // Alter the wrong path, and tell the next step
                            Utility.MaN.Event.OnEventCall(
                                new WaypointEventArgs
                                {
                                    Status = NavigationStatus.AdjustRoute
                                });

                            Utility.MaN.Event.OnEventCall(
                                NavigationRouteCorrection(currentWaypoint,
                                endWaypoint));
                        }
                    }
                }

                // Waiting for the event of get next Beacon
                nextBeaconWaitEvent.WaitOne();

                // Set the current waypoint to the previous waypoint
                previousWaypoint = currentWaypoint;
            }
        }

        /// <summary>
        /// Calibrate the wrong path to get the correct path
        /// </summary>
        /// <param name="CurrentWaypoint"></param>
        /// <returns></returns>
        private WaypointEventArgs NavigationRouteCorrection
            (WaypointModel CurrentWaypoint, WaypointModel EndWaypoint)
        {
            // If the current location is in the path
            if (pathQueue.Any(c => c.NextWaypoint == CurrentWaypoint))
            {
                // Remove the location in the queue of path until the dequeued
                // location is the same as current location
                var CurrentInstruction = pathQueue
                        .First(c => c.NextWaypoint == CurrentWaypoint);
                while (pathQueue.Dequeue() != CurrentInstruction)
                {
                    //get the current path instruction
                }

                // Keep navigating
                nextInstruction = pathQueue.Dequeue();
                double distance = nextInstruction.NextWaypoint
                    .Coordinates
                    .GetDistanceTo(currentBeacon.GetCoordinates());

                return new WaypointEventArgs
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
                // If connected, it doesn't need to calibrate the direction.
                if (Utility.LocationConnects
                    .Any(c => c.SourceWaypoint == CurrentWaypoint &&
                    c.TargetWaypoint == previousWaypoint) ||
                    Utility.LocationConnects
                    .Any(c => c.SourceWaypoint == previousWaypoint &&
                    c.TargetWaypoint == CurrentWaypoint))
                {
                    // Re-plan the path, and keep navigating
                    pathQueue = Utility.WaypointRoute.RegainPath(
                                previousWaypoint, currentBeacon, EndWaypoint);
                    nextInstruction = pathQueue.Dequeue();
                    double distance = nextInstruction.NextWaypoint
                        .Coordinates
                        .GetDistanceTo(
                        currentBeacon.GetCoordinates());

                    return new WaypointEventArgs
                    {
                        Status = NavigationStatus.Run,
                        Angle = nextInstruction.Angle,
                        Distance = distance
                    };

                }
                else
                {
                    // Re-plan the path and calibrate the direction
                    pathQueue = Utility.WaypointRoute.GetPath(currentBeacon,
                                                                EndWaypoint);
                    nextInstruction = pathQueue.Dequeue();

                    return new WaypointEventArgs
                    {
                        Status = NavigationStatus.AdjustDirection
                    };
                }
            }
        }

        /// <summary>
        /// Return the signal processing algorithm of corresponding 
        /// navigation algorithm.
        /// </summary>
        /// <returns></returns>
        public ISignalProcessingAlgorithm CreateSignalProcessingAlgorithm()
        {
            return signalProcessingAlgorithm;
        }

        /// <summary>
        /// Stop navigation and release the resources
        /// </summary>
        public void StopNavigation()
        {
            Dispose();
        }

        /// <summary>
        /// The boolean of whether reach the destination
        /// </summary>
        public bool IsReachingDestination { get; private set; }

        /// <summary>
        /// Set the destination and raise the event to run the algorithm
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
                    pathQueue = Utility.WaypointRoute.GetPath(currentBeacon, 
                                                                EndWaypoint);
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
            Debug.WriteLine("Waypoint algorithm: Receive UUID: {0}", 
                      (e as WayPointSignalProcessEventArgs).CurrentBeacon.UUID);

            // Check this event of signal processing from the current Beacon 
            // if it is the same as current Beacon
            if (this.currentBeacon != _currentBeacon)
            {
                lock (resourceLock)
                    this.currentBeacon = _currentBeacon;
                nextBeaconWaitEvent.Set();
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

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

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        ~WaypointAlgorithm()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(false);
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
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

    public class WaypointEventArgs : EventArgs
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