using System;
using System.Collections.Generic;
using System.Text;

namespace IndoorNavigation.Models
{
    public class BeaconSignal
    {
        public Guid UUID { get; set; }
        /// <summary>
        /// IBeacon Major field
        /// </summary>
        public int Major { get; set; }
        /// <summary>
        /// IBeacon Minor field
        /// </summary>
        public int Minor { get; set; }
        public int RSSI { get; set; }
    }

    public class BeaconSignalModel : BeaconSignal
    {
        public int TxPower { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
