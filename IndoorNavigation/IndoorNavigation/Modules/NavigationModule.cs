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
using IndoorNavigation.Models.NavigaionLayer;
using IndoorNavigation.Modules.Navigation;
using System.Collections.Generic;
using Xamarin.Forms;
using System.Linq;

namespace IndoorNavigation.Modules
{
    public class NavigationModule : IDisposable
    {
        private bool _isFirstTimeGetWaypoint;

        private IPSModule _IPSmodule;
        private Session _session;

        private string _navigraphName;
        private Guid _destinationID;

        private EventHandler _currentWaypointHandler;
        private EventHandler _navigationResultHandler;

        public WaypointEvent WaypointEvent { get; private set; }
        public NavigationEvent NavigationEvent { get; private set; }

        public NavigationModule(string navigraphName, Guid destinationID)
        {
            _isFirstTimeGetWaypoint = true;

            WaypointEvent = new WaypointEvent();
            NavigationEvent = new NavigationEvent();

            _IPSmodule = new IPSModule();
            _currentWaypointHandler = new EventHandler(HandleCurrentWaypoint);
            //IPSModule.Event.WaypointHandler += CurrentWaypointHandler;

            _navigraphName = navigraphName;
            _destinationID = destinationID;
        }

        private void FirstTimeGetWaypoint(EventArgs args)
        {
            List<int> avoidList = new List<int>();

            if (Application.Current.Properties.ContainsKey("AvoidStair"))
            {
                avoidList.Add((bool)Application.Current.Properties["AvoidStair"] ? (int)ConnectionType.Stair : -100);
                avoidList.Add((bool)Application.Current.Properties["AvoidElevator"] ? (int)ConnectionType.Elevator : -100);
                avoidList.Add((bool)Application.Current.Properties["AvoidEscalator"] ? (int)ConnectionType.Escalator : -100);

                avoidList = avoidList.Distinct().ToList();
                avoidList.Remove(-1000);
            }

            _session = new Session(
                    NavigraphStorage.LoadNavigraphXML(_navigraphName),
                    (args as WaypointScanEventArgs).WaypointID,
                    _destinationID,
                    avoidList.ToArray());

            _navigationResultHandler = new EventHandler(HandleNavigationResult);
            _session.Event.SessionResultHandler += _navigationResultHandler;

            WaypointEvent.CurrentWaypointEventHandler += _session.DetectRoute;
        }

        // Get current waypoint and raise event to notify the session
        public void HandleCurrentWaypoint(object sender, EventArgs args)
        {
            if (_isFirstTimeGetWaypoint)
            {
                FirstTimeGetWaypoint(args);
                _isFirstTimeGetWaypoint = false;
            }

            WaypointEvent.OnEventCall(args);
        }

        private void HandleNavigationResult(object sender, EventArgs args)
        {
            // get the navigation result from the session and raise event to notify the NavigatorPageViewModel
            NavigationEvent.OnEventCall(args);
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
                    _session.Event.SessionResultHandler -= _navigationResultHandler;
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
