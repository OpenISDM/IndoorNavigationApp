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
 *      This class is used to create and initialize all resources, including 
 *      dedicated worker threads and processing events.
 *      
 * Version:
 *
 *      1.0.0-beta.1, 20190521
 * 
 * File Name:
 *
 *      NavigationModule.cs
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
 */
using System;
using IndoorNavigation.Modules.Navigation;

namespace IndoorNavigation.Modules
{
    public class NavigationModule : IDisposable
    {
        private IPSModule IPSmodule;
        private Session session;
        private string destination;

        private EventHandler CurrentWaypointHandler;
        private EventHandler NavigationResultHandler;

        public WaypointEvent WaypointEvent { get; private set; }
        public NavigationEvent NavigationEvent { get; private set; }

        public NavigationModule(string navigraphName, string destination)
        {
            NavigationEvent = new NavigationEvent();

            IPSmodule = new IPSModule();
            CurrentWaypointHandler = new EventHandler(HandleCurrentWaypoint);
            //IPSModule.Event.WaypointHandler += CurrentWaypointHandler;

            this.destination = destination;
            //session = new Session(NavigraphStorage.LoadNavigraphXML(navigraphName));
            NavigationResultHandler = new EventHandler(HandleNavigationResult);
            //session.Event.SessionResultHandler += NavigationResultHandler;
        }

        private void HandleCurrentWaypoint(object sender, EventArgs args)
        {
            // get current waypoint and raise event to notify the session
        }

        private void HandleNavigationResult(object sender, EventArgs args)
        {
            // get the navigation result from the session and raise event to notify the NavigatorPageViewModel
        }

        public void CloseNavigationModule()
        {
            Dispose();
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    //IPSModule.Event.WaypointHandler -= CurrentWaypointHandler;
                    //session.Event.SessionResultHandler -= NavigationResultHandler;
                }
                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~NavigationModule()
        // {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

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

    public class WaypointEvent
    {
        public event EventHandler CurrentWaypointEventHandler;

        public void OnEventCall(EventArgs args)
        {
            CurrentWaypointEventHandler?.Invoke(this, args);
        }
    }

    public class NavigationEvent
    {
        public event EventHandler ResultEventHandler;

        public void OnEventCall(EventArgs args)
        {
            ResultEventHandler?.Invoke(this, args);
        }
    }
}
