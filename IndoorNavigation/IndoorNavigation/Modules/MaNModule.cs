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
 * 
 *      Monitor and Notification module is respondsible for monitoring user's
 *      path. After the user gets to the wrong way, this module sends an event
 *      to notice the user and redirects the path.
 * 
 * File Name:
 *
 *      MaNModule.cs
 *
 * Abstract:
 *
 *       
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
using System.Threading.Tasks;

namespace IndoorNavigation.Modules
{
    /// <summary>
    /// Notification module
    /// </summary>
    public class MaNModule : IDisposable
    {
        private Thread MaNThread;
        private bool isThreadRunning = true;
        private bool isReachingDestination = false;
        private Beacon currentBeacon;
        private WaypointModel previousWaypoint;
        private WaypointModel endWaypoint;
        private NextInstructionModel nextInstruction;
        private Queue<NextInstructionModel> pathQueue;
        private ManualResetEvent nextBeaconWaitEvent = 
            new ManualResetEvent(false);
        private ManualResetEvent navigationTaskWaitEvent =
            new ManualResetEvent(false);
        private ManualResetEvent threadClosedWait =
            new ManualResetEvent(false);
        private object resourceLock = new object();
        private EventHandler HSignalProcess;
        public MaNEEvent Event;

        /// <summary>
        /// Initialize a Monitor and Notification Model
        /// </summary>
        public MaNModule()
        {
            Event = new MaNEEvent();
            MaNThread = new Thread(MaNWork) { IsBackground = true };
            MaNThread.Start();
            HSignalProcess = new EventHandler(HandleSignalProcess);
            Utility.SignalProcess.Event.SignalProcessEventHandler +=
                HSignalProcess;
        }

        private void MaNWork()
        {
            while (isThreadRunning)
            {
                // Wait for the navigation task
                navigationTaskWaitEvent.WaitOne();
                while (!isReachingDestination)
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
                            Event.OnEventCall(new MaNEventArgs
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

                            Event.OnEventCall(new MaNEventArgs
                            {
                                Status = NavigationStatus.DirectionCorrect
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

                                Event.OnEventCall(new MaNEventArgs
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
                                Event.OnEventCall(new MaNEventArgs
                                {
                                    Status = NavigationStatus.RouteCorrect
                                });

                                Event.OnEventCall(
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

            Debug.WriteLine("MaN module close");
            threadClosedWait.Set();
            threadClosedWait.Reset();
        }

        /// <summary>
        /// Modify the navigation path
        /// </summary>
        /// <param name="CurrentWaypoint"></param>
        /// <returns></returns>
        private MaNEventArgs NavigationRouteCorrection
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

                return new MaNEventArgs
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
                    pathQueue = Utility.Route.RegainPath(previousWaypoint,
                        currentBeacon, EndWaypoint);
                    nextInstruction = pathQueue.Dequeue();
                    double distance = nextInstruction.NextWaypoint
                        .Coordinates
                        .GetDistanceTo(
                        currentBeacon.GetCoordinates());

                    return new MaNEventArgs
                    {
                        Status = NavigationStatus.Run,
                        Angle = nextInstruction.Angle,
                        Distance = distance
                    };

                }
                else
                {
                    // Replan the path and calibrate the direction
                    pathQueue = Utility.Route.GetPath(currentBeacon, EndWaypoint);
                    nextInstruction = pathQueue.Dequeue();

                    return new MaNEventArgs
                    {
                        Status = NavigationStatus.DirectionCorrect
                    };
                }
            }
        }

        /// <summary>
        /// Stop navigation
        /// </summary>
        public void StopNavigation()
        {
            // Stop MaN Thread, and wait for setting to a new destination
            isReachingDestination = true;
            lock (resourceLock)
                currentBeacon = null;
            nextBeaconWaitEvent.Set();
            nextBeaconWaitEvent.Reset();
        }

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
                    nextBeaconWaitEvent.WaitOne();

                lock (resourceLock)
                {
                    pathQueue = Utility.Route.GetPath(currentBeacon, EndWaypoint);
                    endWaypoint = EndWaypoint;
                }

                navigationTaskWaitEvent.Set();
                navigationTaskWaitEvent.Reset();
            });
        }

        /// <summary>
        /// Receive the next Beacon returned by signal process model
        /// </summary>
        /// <param name="CurrentBeacon"></param>
        private void HandleSignalProcess(object sender, EventArgs e)
        {
            Beacon _currentBeacon =
                (e as SignalProcessEventArgs).CurrentBeacon;

            // Check this event of signal processing from the current Beacon 
            // if it is the same as currrent Beacon
            if (this.currentBeacon != _currentBeacon)
            {
                lock (resourceLock)
                    this.currentBeacon = _currentBeacon;
                nextBeaconWaitEvent.Set();
                nextBeaconWaitEvent.Reset();
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
                    Utility.SignalProcess.Event.SignalProcessEventHandler -=
                        HSignalProcess;
                    isThreadRunning = false;
                    navigationTaskWaitEvent.Set();
                    nextBeaconWaitEvent.Set();
                    threadClosedWait.WaitOne();
                    navigationTaskWaitEvent.Dispose();
                    nextBeaconWaitEvent.Dispose();
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
        RouteCorrect,
        DirectionCorrect
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

    #endregion
}
