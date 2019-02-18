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

namespace IndoorNavigation.Modules
{
    /// <summary>
    /// Signal processing of LBeacon
    /// </summary>
    public class SignalProcessModule : IDisposable
    {
        private Thread signalProcessThread;
        private ManualResetEvent threadWait =
            new ManualResetEvent(false);
        private ISignalProcessingAlgorithm signalProcessingAlgorithm;
        private bool isThreadRunning = true;
        private object algorithmLock = new object();

        public SignalProcessEvent Event { get; private set; }

        /// <summary>
        /// Launch the thread to find the Beacon within the threshold and have 
        /// the highest value in background when this class is startup.
        /// </summary>
        public SignalProcessModule()
        {
            Event = new SignalProcessEvent();

            signalProcessingAlgorithm = 
                Utility.Service.Get<ISignalProcessingAlgorithm>
                ("Default signal process algorithm");

            signalProcessThread =
                new Thread(SignalProcessWork) { IsBackground = true };
            signalProcessThread.Start();
            threadWait.WaitOne();
            threadWait.Reset();

            Debug.WriteLine("SignalProcessModule initialization completed.");
        }

        private void SignalProcessWork()
        {
            threadWait.Set();

            while (isThreadRunning)
            {
                lock(algorithmLock)
                    signalProcessingAlgorithm.SignalProcessing();

                // wait 1 sec or wait module close
                SpinWait.SpinUntil(() => isThreadRunning, 1000);
            }

            Debug.WriteLine("Signal process close");
            threadWait.Set();
        }

        public void SetAlogorithm(
            ISignalProcessingAlgorithm SignalProcessingAlgorithm)
        {
            lock(algorithmLock)
                signalProcessingAlgorithm = SignalProcessingAlgorithm;
        }

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                isThreadRunning = false;
                threadWait.WaitOne();
                threadWait.Reset();
                if (disposing)
                {
                    threadWait.Dispose();
                }


                signalProcessThread = null;
                threadWait = null;

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

        public void OnEventCall(EventArgs e)
        {
            SignalProcessEventHandler?.Invoke(this, e);
        }
    }
    #endregion
}
