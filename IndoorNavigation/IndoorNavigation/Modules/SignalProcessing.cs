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
 *      This file contains class to receive status of nearby Beacons by the API
 *      from iOS or Android, and find the Beacon within the threshold and have
 *      the highest value.
 * 
 * File Name:
 *
 *      SignalProcessing.cs
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
using IndoorNavigation.Modules.SignalProcessingAlgorithms;

namespace IndoorNavigation.Modules
{
    /// <summary>
    /// Signal processing of LBeacon
    /// </summary>
    public class SignalProcessModule : IDisposable
    {
        private Thread signalProcessThread;
        private ManualResetEvent threadClosedWait =
            new ManualResetEvent(false);
        private ManualResetEvent nextBeaconWaitEvent =
            new ManualResetEvent(false);

        public volatile List<BeaconSignalModel> beaconSignalBuffer =
            new List<BeaconSignalModel>();

        private BeaconType currentBeaconType;

        private bool isThreadRunning = true;
        private object bufferLock = new object();
        // private Semaphore bufferSemaphore = new Semaphore(0, 1, "BufferLock");
        private Mutex bufferMutex;

        public SignalProcessEvent Event = new SignalProcessEvent();

        /// <summary>
        /// Launch the thread to find the Beacon within the threshold and have 
        /// the highest value in background when this class is startup.
        /// </summary>
        public SignalProcessModule()
        {
            Event.SignalProcessEventHandler +=
                new EventHandler(GetCurrentBeaconType);

            // bufferSemaphore = new Semaphore(0, 1, "BufferLock");
            bufferMutex = new Mutex(false, "BufferLock");
            currentBeaconType = BeaconType.Waypoint;

            signalProcessThread =
                new Thread(SignalProcessWork) { IsBackground = true };
            signalProcessThread.Start();
        }

        /// <summary>
        /// Insert the signal of discovered Beacon into the buffer(List)
        /// </summary>
        /// <param name="Signals"></param>
        public void AddSignal(List<BeaconSignalModel> Signals)
        {
            // Beacon signal filter, keeps the Beacon's signal recorded in
            // the graph
            IEnumerable<BeaconSignalModel> signals = Signals
                .Where(signal => Utility.BeaconsDict.Values
                .Select(beacon => (beacon.UUID, beacon.Major, beacon.Minor))
                .Contains((signal.UUID, signal.Major, signal.Minor)));

            //lock (bufferLock)
            //bufferSemaphore.WaitOne();
            bufferMutex.WaitOne();
            beaconSignalBuffer.AddRange(signals);
            bufferMutex.ReleaseMutex();
            //bufferSemaphore.Release();
        }

        private void SignalProcessWork()
        {
            ISignalProcessingAlgorithm signalProcessingAlgorithm;

            while (isThreadRunning)
            {
                nextBeaconWaitEvent.WaitOne(1000);
                nextBeaconWaitEvent.Reset();

                signalProcessingAlgorithm = AlgorithmFactory.CreateSignalProcessing(currentBeaconType);
                signalProcessingAlgorithm.SignalProcessing();

                // wait 1 sec or wait module close
                SpinWait.SpinUntil(() => isThreadRunning, 1000);
            }

            Debug.WriteLine("Signal process close");
            threadClosedWait.Set();
            threadClosedWait.Reset();
        }

        private void GetCurrentBeaconType(object sender, EventArgs e)
        {
            Beacon _currentBeacon = 
                (e as SignalProcessEventArgs).CurrentBeacon;

            currentBeaconType = _currentBeacon.Type;
            nextBeaconWaitEvent.Set();
        }

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Event.SignalProcessEventHandler -=
                        new EventHandler(GetCurrentBeaconType);
                    isThreadRunning = false;
                    threadClosedWait.WaitOne();

                    threadClosedWait.Dispose();
                    nextBeaconWaitEvent.Set();
                    nextBeaconWaitEvent.Dispose();
                }


                signalProcessThread = null;
                threadClosedWait = null;
                nextBeaconWaitEvent = null;
                beaconSignalBuffer = null;
                bufferLock = null;

                disposedValue = true;
            }
        }

        ~SignalProcessModule()
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
