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
 *      Paul Chang, paulchang@iis.sinica.edu.tw
 *      Chun Yu Lai, chunyu1202@gmail.com
 *
 */
using System;
using IndoorNavigation.Models.NavigaionLayer;
using System.Collections.Generic;
using Xamarin.Forms;
using System.Linq;
using IndoorNavigation.Modules.Utilities;
using IndoorNavigation.Models;

namespace IndoorNavigation.Modules
{
    public class NavigationModule : IDisposable
    {
        private Session _session;

        private string _navigraphName;

        private Guid _destinationID;

        private EventHandler _navigationResultEventHandler;

        public NavigationEvent _event { get; private set; }

        public NavigationModule(string navigraphName, Guid destinationID)
        {
            _event = new NavigationEvent();

            _navigraphName = navigraphName;
            _destinationID = destinationID;

            ConstructSession();
        }

        /// <summary>
        /// If it is the first time to get waypoint then get the value of 
        /// route options and start the corresponding session.
        /// </summary>
        private void ConstructSession()
        {
            const int falseInt = -100;
            List<int> avoidList = new List<int>();

            Console.WriteLine("-- begin StartSession --- ");

            Console.WriteLine("-- setup preference --- ");
            if (Application.Current.Properties.ContainsKey("AvoidStair"))
            {
                avoidList.Add(
                        (bool)Application.Current.Properties["AvoidStair"] ?
                        (int)ConnectionType.Stair : falseInt);
                avoidList.Add(
                        (bool)Application.Current.Properties["AvoidElevator"] ?
                        (int)ConnectionType.Elevator : falseInt);
                avoidList.Add(
                        (bool)Application.Current.Properties["AvoidEscalator"] ?
                        (int)ConnectionType.Escalator : falseInt);

                avoidList = avoidList.Distinct().ToList();
                avoidList.Remove(falseInt);
            }
            Console.WriteLine("-- end of setup preference --- ");

            // Start the session
            _session = new Session(
                    NavigraphStorage.LoadNavigraphXML(_navigraphName),
                    _destinationID,
                    avoidList.ToArray());

            _navigationResultEventHandler = new EventHandler(HandleNavigationResult);
            _session._event._eventHandler += _navigationResultEventHandler;

        }

        /// <summary>
        /// Get the navigation result from the session and 
        /// raise event to notify the NavigatorPageViewModel.
        /// </summary>
        private void HandleNavigationResult(object sender, EventArgs args)
        {
            Console.WriteLine("received event raised from Session class");
            _event.OnEventCall(args);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // Dispose managed state (managed objects).
                }
                // Free unmanaged resources (unmanaged objects) and override a finalizer below.
                // Set large fields to null.
                _session._event._eventHandler -= _navigationResultEventHandler;
                _session.CloseSession();
               
                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }

}
