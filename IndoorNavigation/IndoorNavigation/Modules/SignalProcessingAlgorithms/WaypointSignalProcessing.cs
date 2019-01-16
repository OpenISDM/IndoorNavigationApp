using System;
using System.Collections.Generic;
using System.Linq;
using IndoorNavigation.Models;
using System.Threading;

namespace IndoorNavigation.Modules.SignalProcessingAlgorithms
{
    public class WaypointSignalProcessing : ISignalProcessingAlgorithm
    {
        public List<BeaconSignalModel> beaconSignalBuffer =
            new List<BeaconSignalModel>();
        private readonly EventHandler HBeaconScan;
        private object bufferLock = new object();

        public WaypointSignalProcessing()
        {
            HBeaconScan = new EventHandler(HandleBeaconScan);
            Utility.BeaconScanAPI.Event.BeaconScanEventHandler += HBeaconScan;
        }

        public void SignalProcessing()
        {
            // this List used to save the mean value of signals
            List<BeaconSignal> signalAverageList =
                    new List<BeaconSignal>();
            // remove the obsolete data from buffer
            List<BeaconSignalModel> removeSignalBuffer =
                new List<BeaconSignalModel>();

            lock (bufferLock)
            {
                removeSignalBuffer.AddRange(
                beaconSignalBuffer.Where(c =>
                    c.Timestamp < DateTime.Now.AddMilliseconds(-1000)));

                foreach (var obsoleteBeaconSignal in removeSignalBuffer)
                    beaconSignalBuffer.Remove(obsoleteBeaconSignal);

                // Average the intensity of all Beacon signals
                var beacons = beaconSignalBuffer
                    .Select(c => (c.UUID, c.Major, c.Minor)).Distinct();

                foreach (var (UUID, Major, Minor) in beacons)
                {
                    signalAverageList.Add(
                        new BeaconSignal
                        {
                            UUID = (UUID, Major, Minor).UUID,
                            Major = (UUID, Major, Minor).Major,
                            Minor = (UUID, Major, Minor).Minor,
                            RSSI = System.Convert.ToInt32(
                                beaconSignalBuffer.Where(c =>
                                c.UUID == (UUID, Major, Minor).UUID &&
                                c.Major == (UUID, Major, Minor).Major &&
                                c.Minor == (UUID, Major, Minor).Minor)
                                .Select(c => c.RSSI).Average())
                        });
                }
            }
        
            // Find the beacon which closest to me
            if (signalAverageList.Any())
            {
                // Scan all the signal that satisfies the threshold
                var nearbySignal = (from signal in signalAverageList
                                    from beacon in Utility.BeaconsDict
                                        where (
                                        signal.UUID == beacon.Value.UUID &&
                                        signal.RSSI >= beacon.Value.Threshold)
                                        select signal);

                // Find the beacon which closest to me, then send an event
                // to MaN
                if (nearbySignal.Any())
                {
                    var bestNearbySignal = nearbySignal.First();
                    Beacon bestNearbyBeacon =
                        Utility.BeaconsDict[bestNearbySignal.UUID];

                    // Send event to MaN module
                    Utility.SignalProcess.Event.OnEventCall(
                        new WayPointSignalProcessEventArgs
                        {CurrentBeacon = bestNearbyBeacon});
                }
            }

        }

        private void HandleBeaconScan(object sender, EventArgs e)
        {
            // Beacon signal filter, keeps the Beacon's signal recorded in
            // the graph
            IEnumerable<BeaconSignalModel> signals = 
                (e as BeaconScanEventArgs).Signals
                .Where(signal => Utility.BeaconsDict.Values
                .Select(beacon => (beacon.UUID, beacon.Major, beacon.Minor))
                .Contains((signal.UUID, signal.Major, signal.Minor)));

            lock (bufferLock)
                beaconSignalBuffer.AddRange(signals);

        }

        ~WaypointSignalProcessing()
        {
            Utility.BeaconScanAPI.Event.BeaconScanEventHandler -= HBeaconScan;
            beaconSignalBuffer = null;
            bufferLock = null;
        }
    }

    public class WayPointSignalProcessEventArgs : EventArgs
    {
        public Beacon CurrentBeacon { get; set; }
    }
}
