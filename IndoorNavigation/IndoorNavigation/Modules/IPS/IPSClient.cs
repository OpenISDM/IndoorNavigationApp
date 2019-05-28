using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IndoorNavigation.Models;

namespace IndoorNavigation.Modules.IPS
{
    class IPSClient:IIPSClient
    {
        //this List is used to save the beacons we want
        private List<Beacon> BeaconList;

        public List<BeaconSignalModel> beaconSignalBuffer = new List<BeaconSignalModel>();
        private readonly EventHandler HBeaconScan;
        private object bufferLock = new object();
        public IPSClient()
        {
            HBeaconScan = new EventHandler(HandleBeaconScan);
            BeaconList = null;
        }

        //set the list of beacons we want
        public void setBeaconList(List<Beacon> BeaconList)
        {
            if (BeaconList != null)
                this.BeaconList = BeaconList;
            else
                throw new System.ArgumentException("Parameter cannot be null", "BeaconList");
        }

        public Beacon SignalProcessing()
        {
            // this List used to save the mean value of signals
            List<BeaconSignal> signalAverageList = new List<BeaconSignal>();
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
                    //confirmation the BeaconList has been set
                    if (BeaconList != null)
                    {
                        //Confirm the beacon signal is on the list we want
                        if (BeaconList.Exists(x => x.UUID == (UUID, Major, Minor).UUID))
                        {
                            signalAverageList.Add
                                (
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
                                    }
                                );
                        }
                    }
                    else
                    {
                        signalAverageList.Add
                                (
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
                                    }
                                );
                    }
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

                // Find the beacon which closest to me, then return
                if (nearbySignal.Any())
                {
                    var bestNearbySignal = nearbySignal.First();
                    Beacon bestNearbyBeacon =
                        Utility.BeaconsDict[bestNearbySignal.UUID];

                    return bestNearbyBeacon;
                }
                else
                    return null;
            }
            else
                return null;
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

        public void Stop()
        {
            BeaconList = null;
            Utility.BeaconScan.Event.BeaconScanEventHandler -= HBeaconScan;
            beaconSignalBuffer = null;
            bufferLock = null;
        }

        ~IPSClient()
        {
            BeaconList = null;
            Utility.BeaconScan.Event.BeaconScanEventHandler -= HBeaconScan;
            beaconSignalBuffer = null;
            bufferLock = null;
        }
    }
}
