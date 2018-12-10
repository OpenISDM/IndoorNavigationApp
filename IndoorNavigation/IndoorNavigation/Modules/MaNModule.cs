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
 *      Monitor and Notification module is respondsible for monitoring user's
 *      path. After the user gets to the wrong way, this module sends an event
 *      to notice the user and redirects the path. 
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
        private bool threadSwitch = true;
        private bool IsReachingDestination = false;
        private Beacon currentBeacon;
        private BeaconGroupModel previousPoint;
        private BeaconGroupModel endPoint;
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
        /// Initialize a new Notification model
        /// </summary>
        public MaNModule()
        {
            Event = new MaNEEvent();
            MaNThread = new Thread(MaNWork) { IsBackground = true};
            MaNThread.Start();
            HSignalProcess = new EventHandler(HandleSignalProcess);
            Utility.SignalProcess.Event.SignalProcessEventHandler += 
                HSignalProcess;
        }

        private void MaNWork()
        {
            while (threadSwitch)
            {
                // Wait for the navigatino task
                navigationTaskWaitEvent.WaitOne();
                while (!IsReachingDestination)
                {
                    // Find the current position from the current Beacon
                    BeaconGroupModel currentPoint;
                    lock(resourceLock)
                        currentPoint = Utility.BeaconGroups
                            .Where(c => c.Beacons.Contains(currentBeacon))
                            .First();


                    lock (resourceLock)
                    {
                        // Check if he arrives the destination
                        if (currentPoint == endPoint)
                        {
                            Event.OnEventCall(new MaNEventArgs
                            {
                                Status = NavigationStatus.Arrival
                            });
                            break;
                        }

                        // if NextInstruction=null, it represents the 
                        // navigation starts at the first location.
                        // The current version, user has to walk in random 
                        // way, once he reaches the second location then 
                        // calibration.
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
                            // Check if the user reachs the next location
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
                                // Alter the wrong path, 
                                // and tell the next step
                                Event.OnEventCall(new MaNEventArgs
                                {
                                    Status = NavigationStatus.RouteCorrection
                                });

                                Event.OnEventCall(
                                    NavigationRouteCorrection(currentPoint,
                                    endPoint));
                            }
                        }
                    }

                    // Wait for the event of next best Beacon
                    bestBeacon.WaitOne();

                    // Change the current location to the last location
                    previousPoint = currentPoint;
                }
            }

            Debug.WriteLine("MaN module close");
            threadClosedWait.Set();
            threadClosedWait.Reset();
        }

        /// <summary>
        /// Modify the navigation path
        /// </summary>
        /// <param name="CurrentPoint"></param>
        /// <returns></returns>
        private MaNEventArgs NavigationRouteCorrection
            (BeaconGroupModel CurrentPoint,
            BeaconGroupModel EndPoint)
        {
            // If the current location is in the path
            if (pathQueue.Where(c => c.NextPoint == CurrentPoint).Count() > 0)
            {
                // Remove the location in the queue of path until the dequeued
                // location is the same as current location
                var CurrentInstruction = pathQueue
                    .Where(c => c.NextPoint == CurrentPoint).First();
                while (pathQueue.Dequeue() != CurrentInstruction) ;

                // Keep navigation
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
                // Check the current location if is connected to the last
                // location. If the Wifi is connected, it dosn't need to
                // calibrate the direction
                if (Utility.LocationConnects
                    .Where(c => c.BeaconA == CurrentPoint &&
                    c.BeaconB == previousPoint).Count() > 0 ||
                    Utility.LocationConnects
                    .Where(c => c.BeaconA == previousPoint &&
                    c.BeaconB == CurrentPoint).Count() > 0)
                {
                    // Replan the path, and keep navigating
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
                    // Replan the path and calibrate the direction
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
        /// Stop navigation
        /// </summary>
        public void StopNavigation()
        {
            // Stop MaN Thread, and wait for setting to a new destination
            IsReachingDestination = true;
            lock(resourceLock)
                currentBeacon = null;
            bestBeacon.Set();
            bestBeacon.Reset();
        }

        /// <summary>
        /// Set the destination
        /// </summary>
        /// <param name="EndPoint"></param>
        public void SetDestination(BeaconGroupModel EndPoint)
        {
            Task.Run(() => {
                // Plan the navigation path
                if (currentBeacon == null)
                    bestBeacon.WaitOne();
                lock (resourceLock)
                {
                    pathQueue = Utility.Route.GetPath(currentBeacon, EndPoint);
                    endPoint = EndPoint;
                }

                navigationTaskWaitEvent.Set();
                navigationTaskWaitEvent.Reset();
            });
        }

        /// <summary>
        /// Receive the best Beacon returned by signal process model
        /// </summary>
        /// <param name="CurrentBeacon"></param>
        private void HandleSignalProcess(object sender, EventArgs e)
        {
            Beacon currentBeacon =
                (e as SignalProcessEventArgs).CurrentBeacon;

            // Check this event of signal processing from the current Beacon 
            // if is the same as currrent Beacon
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
                    Utility.SignalProcess.Event.SignalProcessEventHandler -= 
                        HSignalProcess;
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
