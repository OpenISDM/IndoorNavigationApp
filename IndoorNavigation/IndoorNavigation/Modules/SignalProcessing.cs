using IndoorNavigation.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Linq;

namespace IndoorNavigation.Modules
{
    public class SignalProcess:IDisposable
    {
        private Thread SignalProcessThread;
        private  List<BeaconSignalModel> BeaconSignalBuffer = 
            new List<BeaconSignalModel>();
        private bool Switch = true;
        private object BufferLock = new object();

        public SignalProcess()
        {
            SignalProcessThread = 
                new Thread(SignalProcessWork){ IsBackground = true};
            SignalProcessThread.Start();
        }

        public void AddSignal(List<BeaconSignalModel> Signals)
        {
            lock (BufferLock)
                BeaconSignalBuffer.AddRange(Signals);
        }

        public void SignalProcessWork()
        {
            while (Switch)
            {
                List<BeaconSignalModel> SignalAverageList = new List<BeaconSignalModel>();
                // SignalProcess
                // remove buffer old data
                lock (BufferLock)
                {
                    foreach (var BeaconSignal in BeaconSignalBuffer.Where(c => c.Timestamp < DateTime.Now.AddMilliseconds(-500)))
                        BeaconSignalBuffer.Remove(BeaconSignal);

                    //foreach (var BeaconUUID in BeaconSignalBuffer.Select(c => c.UUID).Distinct())
                }
                    


                // wait 500s or wait module close
                SpinWait.SpinUntil(() => Switch, 500);
            }

            Debug.WriteLine("Signal process close");
        }

        public void Dispose()
        {
            Switch = false;
        }
    }
}
