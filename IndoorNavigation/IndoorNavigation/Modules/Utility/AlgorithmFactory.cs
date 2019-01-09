using System;
using System.Reflection;
using IndoorNavigation.Modules.SignalProcessingAlgorithms;
using IndoorNavigation.Models;
using System.ComponentModel;

namespace IndoorNavigation.Modules
{
    public static class AlgorithmFactory
    {
        //public static ISignalProcessingAlgorithm Create<T>() where T : ISignalProcessingAlgorithm, new()
        //{
        //    return new T();
        //}

        public static ISignalProcessingAlgorithm CreateSignalProcessing(BeaconType beaconType)
        {
            switch(beaconType)
            {
                case BeaconType.Waypoint:
                    return new WaypointSignalProcessing();

                case BeaconType.iBeacon:
                    
                case BeaconType.GeoBeacon:

                default:
                    throw new InvalidEnumArgumentException();
            }
        }
    }
}
