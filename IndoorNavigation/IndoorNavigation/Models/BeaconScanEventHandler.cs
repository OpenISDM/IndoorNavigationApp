using System;
using System.Collections.Generic;
using System.Text;

namespace IndoorNavigation.Models
{
    public class BeaconScanEvent
    {
        public event EventHandler BeaconScanEventHandler;

        public void OnEventCall(EventArgs e)
        {
            BeaconScanEventHandler?.Invoke(this, e);
        }
    }

    public class BeaconScanEventArgs : EventArgs
    {
        public List<BeaconSignalModel> Signals { get; set; }
    }
}
