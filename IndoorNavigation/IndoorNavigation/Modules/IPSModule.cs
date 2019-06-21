/*
 * Copyright (c) 2019 Academia Sinica, Institude of Information Science
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
 *      IPS(Indoor Positioning System) manage different IPS clients such as a 
 *      BeDIPS, a triangulation system or a fingerprint-based system. During 
 *      travel, The IPS will periodly query the current waypoint to determine 
 *      if it has reached the next waypoint. If the next region requires
 *      aother IPS client, it will switch to the desired IPS client and 
 *      continue to query current waypoint.
 *      
 * Version:
 *
 *      1.0.0-beta.1, 20190521
 * 
 * File Name:
 *
 *      IPSModule.cs
 *
 * Abstract:
 *
 *      Waypoint-based navigator is a mobile Bluetooth navigation application
 *      that runs on smart phones. It is structed to support anywhere 
 *      navigation. Indoors in areas covered by different indoor positioning 
 *      system (IPS) and outdoors covered by GPS. In particilar, it can rely on 
 *      BeDIS (Building/environment Data and Information System) for indoor 
 *      positioning. Using this IPS, the navigator does not need to 
 *      continuously monitor its own position, since the IPS broadcast to the 
 *      navigator the location of each waypoint. 
 *      This version makes use of Xamarin.Forms, which is a complete 
 *      cross-platform UI tookit that runs on both iOS and Android.
 *
 * Authors:
 *
 *      Kenneth Tang, kenneth@gm.nssh.ntpc.edu.tw
 *      
 */
using System;
using System.Diagnostics;
using System.Threading;
using IndoorNavigation.Models;
using IndoorNavigation.Models.NavigaionLayer;

// TODO: After adding beacon information into NavigationGraph, it will be finished.
namespace IndoorNavigation.Modules
{
    public class IPSModule : IDisposable
    {
        private Thread IPSThread;

        /// <summary>
        /// Initializes and run the thread of the IPS module
        /// </summary>
        public IPSModule()
        {

        }

        /// <summary>
        /// Stops the IPS Module and the running client.
        /// </summary>
        public void Close()
        {

        }

        private void Work()
        {

        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                Close();

                // dispose managed state (managed objects)
                if (disposing)
                {

                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        ~IPSModule()
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

    class WaypointScanEventArgs : EventArgs
    {
        public Guid WaypointID { get; set; }
    }
}
