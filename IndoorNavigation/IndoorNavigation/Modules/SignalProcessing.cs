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
 *      from IOS or Android, and find the Beacon within the threshold and have
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
        private  List<BeaconSignalModel> beaconSignalBuffer =
            new List<BeaconSignalModel>();
        private bool isThreadRunning = true;
        private object bufferLock = new object();

        public SignalProcessEvent Event = new SignalProcessEvent();

        /// <summary>
        /// Launch the thread to find the Beacon within the threshold and have 
        /// the highest value in background when this class is startup.
        /// </summary>
        public SignalProcessModule()
        {
            signalProcessThread =
                new Thread(SignalProcessWork){ IsBackground = true};
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
                .Where(signal => Utility.Beacons.Values
                .Select(beacon => (beacon.UUID,beacon.Major,beacon.Minor))
                .Contains((signal.UUID, signal.Major, signal.Minor)));

            lock (bufferLock)
                beaconSignalBuffer.AddRange(signals);
        }

        private void SignalProcessWork()
        {
            while (isThreadRunning)
            {
                List<BeaconSignal> signalAverageList =
                    new List<BeaconSignal>();

                // SignalProcess
                lock (bufferLock)
                {
                    // remove buffer old data
                    List<BeaconSignalModel> roveSignalBuffer =
                        new List<BeaconSignalModel>();
                    
                    roveSignalBuffer.AddRange(beaconSignalBuffer.Where(c =>
                        c.Timestamp < DateTime.Now.AddMilliseconds(-1000) ));

                    foreach (var beaconSignal in roveSignalBuffer)
                        beaconSignalBuffer.Remove(beaconSignal);

                    // Average the intensity of all Beacon signals
                    var beacons = beaconSignalBuffer
                        .Select(c => (c.UUID, c.Major, c.Minor)).Distinct();
                    foreach (var (UUID, Major, Minor) in beacons)
                    {
                        signalAverageList.Add(
                            new BeaconSignal {
                                UUID = (UUID, Major, Minor).UUID,
                                Major = (UUID, Major, Minor).Major,
                                Minor = (UUID, Major, Minor).Minor,
                                RSSI = System.Convert.ToInt32(
                                    beaconSignalBuffer
                                    .Where(c =>
                                    c.UUID == (UUID, Major, Minor).UUID &&
                                    c.Major == (UUID, Major, Minor).Major &&
                                    c.Minor == (UUID, Major, Minor).Minor)
                                    .Select(c => c.RSSI).Average())
                            });
                    }
                }

                // Find the beacon closest to me
                if (signalAverageList.Count() > 0)
                {
                    // Scan all the signal that satisfies the threshold
                    var nearbySignal = (from single in signalAverageList
                                        from beacon in Utility.Beacons
                                        where (
                                        single.UUID == beacon.Value.UUID &&
                                        single.RSSI >= beacon.Value.Threshold)
                                        select single);

                    // Find the beacon closest to me, then send an event
                    // to MaN
                    if (nearbySignal.Count() > 0)
                    {
                        var bestNearbySignal = nearbySignal.First();
                        Beacon bestNearbyBeacon =
                            Utility.Beacons[bestNearbySignal.UUID];

                        // Send event to MaN module
                        Event.OnEventCall(new SignalProcessEventArgs {
                            CurrentBeacon = bestNearbyBeacon });
                    }
                }

                // wait 1s or wait module close
                SpinWait.SpinUntil(() => isThreadRunning, 1000);
            }

            Debug.WriteLine("Signal process close");
            threadClosedWait.Set();
            threadClosedWait.Reset();
        }

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    isThreadRunning = false;
                    threadClosedWait.WaitOne();

                    threadClosedWait.Dispose();
                }


                signalProcessThread = null;
                threadClosedWait = null;
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
