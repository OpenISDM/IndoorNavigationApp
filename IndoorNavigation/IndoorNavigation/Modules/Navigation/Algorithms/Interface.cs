using IndoorNavigation.Modules.SignalProcessingAlgorithms;
using System;
using System.Collections.Generic;
using System.Text;

namespace IndoorNavigation.Modules.Navigation.Algorithms
{
    public interface INavigationAlgorithm
    {
        void Work();
        void StopNavigation();
        ISignalProcessingAlgorithm CreateSignalProcessingAlgorithm();
        bool IsReachingDestination { get; }
    }
}
