using IndoorNavigation.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace IndoorNavigationTest
{
    public class BeaconScan : IBeaconScan
    {
        public NavigationEvent Event { get; private set; }

        public BeaconScan()
        {
            Event = new NavigationEvent();
        }

        public void Close()
        {
            throw new NotImplementedException();
        }

        public void StartScan(List<Guid> BeaconsUUID)
        {
            throw new NotImplementedException();
        }

        public void StopScan()
        {
            throw new NotImplementedException();
        }
    }
}
