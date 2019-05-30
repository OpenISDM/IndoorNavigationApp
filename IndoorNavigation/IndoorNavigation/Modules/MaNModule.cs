/*
  Copyright (c) 2018 Academia Sinica, Institude of Information Science
 
    License:
        GPL 3.0 : The content of this file is subject to the terms and
        conditions defined in file 'COPYING.txt', which is part of this source
        code package.

    Project Name:
 
        IndoorNavigation
 
    File Description:
  
        Monitor and Notification module is respondsible for monitoring user's
        path. After the user gets to the wrong way, this module sends an event
        to notice the user and redirects the path.
          
    Version:

        1.0.0-beta.1, 20190319

    File Name:

        MaNModule.cs

    Abstract:

        

    Authors:
 
        Kenneth Tang, kenneth@gm.nssh.ntpc.edu.tw
 
 */

///TODO: DUPLICATE

using IndoorNavigation.Models;
using IndoorNavigation.Modules.Navigation;
using System;
using System.Diagnostics;
using System.Threading;

namespace IndoorNavigation.Modules
{
    /// <summary>
    /// Notification module
    /// </summary>
    public class MaNModule : IDisposable
    {
        private Thread MaNThread;
        private bool isThreadRunning = true;
        private AutoResetEvent navigationAlgorithmWait =
            new AutoResetEvent(false);
        private AutoResetEvent threadWait =
            new AutoResetEvent(false);
        public MaNEEvent Event { get; private set; }
        private INavigationAlgorithm navigationAlgorithm;

        /// <summary>
        /// Initializes a Monitor and Notification Model
        /// </summary>
        public MaNModule()
        {
            Event = new MaNEEvent();
            MaNThread = new Thread(MaNWork) { IsBackground = true };
            MaNThread.Start();
            threadWait.WaitOne();

            Debug.WriteLine("MaNModule initialization completed.");
        }

        private void MaNWork()
        {
            threadWait.Set();

            while (isThreadRunning)
            {
                // waiting for the algorithm to use
                navigationAlgorithmWait.WaitOne();
                if (isThreadRunning)
                    navigationAlgorithm.Work();
            }

            Debug.WriteLine("MaN module close");
            threadWait.Set();
        }

        /// <summary>
        /// Set the navigation alogrithm via dependency injection
        /// e.g. waypoint algorithm or triangulation algorithm
        /// </summary>
        /// <param name="NavigationAlgorithm"></param>
        public void SetAlgorithm(INavigationAlgorithm NavigationAlgorithm)
        {
            navigationAlgorithm = NavigationAlgorithm;
            navigationAlgorithmWait.Set();
        }

        /// <summary>
        /// Stops the navigation algorithm
        /// </summary>
        public void StopNavigation()
        {
            navigationAlgorithm.StopNavigation();
            Utility.BeaconScan.StopScan();
        }

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                isThreadRunning = false;
                navigationAlgorithmWait.Set();
                if (navigationAlgorithm != null)
                    navigationAlgorithm.StopNavigation();
                threadWait.WaitOne();

                if (disposing)
                {
                    navigationAlgorithmWait.Dispose();
                    threadWait.Dispose();
                    navigationAlgorithmWait = null;
                    threadWait = null;
                }

                MaNThread = null;
                Event = null;
                navigationAlgorithm = null;

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
            GC.SuppressFinalize(this);
        }
        #endregion

    }

    /// <summary>
    /// This event will notify the navigation status when finish each step
    /// </summary>
    public class MaNEEvent
    {
        public event EventHandler MaNEventHandler;

        public void OnEventCall(EventArgs e)
        {
            MaNEventHandler?.Invoke(this, e);
#if DEBUG
            if (MaNEventHandler != null)
                Debug.WriteLineIf(e.GetType() == typeof(WaypointEventArgs),
                    string.Format("MaN module: Send event, " +
                                  "content is status: {0} and angle: {1}",
                                  (e as WaypointEventArgs).Status,
                                  (e as WaypointEventArgs).Angle));
#endif
        }
    }

}
