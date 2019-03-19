using System;
using System.Diagnostics;
using System.Threading;
using IndoorNavigation.Models;
using IndoorNavigation.Modules.Navigation;

namespace IndoorNavigation.Modules
{
    /// <summary>
    /// TODO: Add dynamic algorithm changing
    /// Manage the navigation algorithm and switch it when needed
    /// </summary>
    public class IPSModule : IDisposable
    {
        private Thread IPSThread;
        private AutoResetEvent threadWait =
            new AutoResetEvent(false);
        private AutoResetEvent setDestinationWait =
            new AutoResetEvent(false);

        private bool isThreadRunning = true;
        private INavigationAlgorithm navigationAlgorithm;
        private WaypointModel destination;

        /// <summary>
        /// Initializes and run the thread of the IPS module
        /// </summary>
        public IPSModule()
        {
            IPSThread = new Thread(Work);
            IPSThread.Start();
            threadWait.WaitOne();

            Debug.WriteLine("IPSModule initialization completed.");
        }

        /// <summary>
        /// Use IPS to set the destination and raise the event to Work thread
        /// </summary>
        public void SetDestination(WaypointModel waypoint)
        {
            destination = waypoint;
            setDestinationWait.Set();
            setDestinationWait.WaitOne();
        }

        /// <summary>
        /// Stops the navigation.
        /// </summary>
        public void StopNavigation()
        {
            if (navigationAlgorithm != null)
                navigationAlgorithm.StopNavigation();
        }

        private void Work()
        {
            // IPS algorithms
            threadWait.Set();

            while(isThreadRunning)
            {
                setDestinationWait.WaitOne();

                if (isThreadRunning)
                {
                    navigationAlgorithm = Utility.Service
                        .Get<INavigationAlgorithm>("Waypoint algorithm");
                    Utility.MaN.SetAlgorithm(navigationAlgorithm);
                    Utility.SignalProcess.SetAlogorithm(
                        navigationAlgorithm.CreateSignalProcessingAlgorithm());


                    (navigationAlgorithm as WaypointAlgorithm)
                        .SetDestination(destination);

                    setDestinationWait.Set();
                }

            }

            Debug.WriteLine("IPS module close");
            threadWait.Set();
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                isThreadRunning = false;
                setDestinationWait.Set();
                StopNavigation();
                threadWait.WaitOne();

                // dispose managed state (managed objects)
                if (disposing)
                {
                    threadWait.Dispose();
                    setDestinationWait.Dispose();
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
}
