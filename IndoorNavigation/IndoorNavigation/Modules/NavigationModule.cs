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
            session = new Session(NavigraphStorage.LoadNavigraphXML(navigraphName));
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
