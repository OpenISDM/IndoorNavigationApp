using System;
using System.Collections.Generic;
using System.Text;

namespace IndoorNavigation.Models
{
    public class BeaconScanEventArgs : EventArgs
    {
        public List<BeaconSignalModel> Signals { get; set; }
    }
}
