using System;
using System.Linq;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using IndoorNavigation.Models;
using IndoorNavigation.Modules;
using IndoorNavigation.Modules.SignalProcessingAlgorithms;
using System.Collections.Generic;
using IndoorNavigation.Modules.Navigation;
using MvvmHelpers;

namespace IndoorNavigation.ViewModels.Navigation
{
    public class TabbedNaviViewModel : MvvmHelpers.BaseViewModel, IDisposable
    {
        private string destination;
        private bool firstBeacon = true;
        private bool wrongDirection = false;
        private int currentStepNum = 0;
        private List<NextStepModel> navigationPath;

        public TabbedNaviViewModel(string Destination)
        {
            destination = Destination;
            returnedRoutes = new ObservableRangeCollection<RoutesDataClass>();
            Utility.SignalProcess.Event.SignalProcessEventHandler += GetPathEvent;
            Utility.MaN.Event.MaNEventHandler += GetNavigationStatusEvent;
            Utility.IPS.SetDestination(Utility.Waypoints.First(
                                        c => c.Name == destination));
        }

        private void DisplayInstructions(EventArgs waypointArgs)
        {
            UpdateRoutes(currentStepNum);
            var _currentStep = navigationPath[currentStepNum];
            NavigationProgress = (currentStepNum + 1) / navigationPath.Count;
            string _currentStepImage, _currentStepLabel;
            string _nextStepImage, _nextStepLabelName;

            switch ((waypointArgs as WayPointEventArgs).Status)
            {
                //first step
                case NavigationStatus.AdjustDirection:
                    SetInstruction(_currentStep, out _currentStepImage, out _currentStepLabel);
                    SetInstruction(navigationPath[++currentStepNum], out _nextStepImage, out _nextStepLabelName);
                    CurrentStepImage = _currentStepImage;
                    CurrentStepLabel = _currentStepLabel;
                    NextStepImage = _nextStepImage;
                    NextStepLabel = _nextStepLabelName;
                    break;

                //keep navigation     
                case NavigationStatus.Run:
                    SetInstruction(_currentStep, out _currentStepImage, out _currentStepLabel);
                    SetInstruction(navigationPath[++currentStepNum], out _nextStepImage, out _nextStepLabelName);
                    CurrentStepImage = _currentStepImage;
                    CurrentStepLabel = _currentStepLabel;
                    NextStepImage = _nextStepImage;
                    NextStepLabel = _nextStepLabelName;
                    break;

                //re-navigation
                case NavigationStatus.AdjustRoute:
                    CurrentStepImage = "Warning";
                    CurrentStepLabel = "走錯路囉,正在重新規劃路線";
                    NextStepImage = "Waiting";
                    NextStepLabel = " ";
                    wrongDirection = true;
                    break;

                //finish navigation
                case NavigationStatus.Arrival:
                    CurrentStepImage = "Arrived";
                    CurrentStepLabel = "恭喜你！已到達終點囉";
                    NextStepImage = "";
                    NextStepLabel = " ";
                    Dispose();
                    break;
            }
        }

        private void SetInstruction(NextStepModel step, out string stepImage, out string stepLabel)
        {
            switch(step.Angle)
            {
                //front
                case int n when (n >= -5 && n <= 5):
                    stepImage = "Arrow_front";
                    stepLabel = string.Format("請向前方的{0}直走", step.NextWaypoint.Name);
                    break;

                //front-right
                case int n when (n > 5 && n <= 75):
                    stepImage = "Arrow_frontright";
                    stepLabel = string.Format("請向右前方的{0}直走", step.NextWaypoint.Name);
                    break;

                //right
                case int n when (n > 75 && n < 105):
                    stepImage = "Arrow_right";
                    stepLabel = string.Format("請向右轉 並朝{0}直走", step.NextWaypoint.Name);
                    break;

                //rear-right
                case int n when (n > 105 && n < 175):
                    stepImage = "Arrow_rearright";
                    stepLabel = string.Format("請向右後方的{0}直走", step.NextWaypoint.Name);
                    break;

                //rear
                case int n when (n >= 175 || n <= -175):
                    stepImage = "Arrow_rear";
                    stepLabel = string.Format("請向後轉 並朝{0}直走", step.NextWaypoint.Name);
                    break;

                //rear-left
                case int n when (n > -175 && n <= -105):
                    stepImage = "Arrow_rearleft";
                    stepLabel = string.Format("請向左後方的{0}直走", step.NextWaypoint.Name);
                    break;

                //left
                case int n when (n > -105 && n <= -75):
                    stepImage = "Arrow_left";
                    stepLabel = string.Format("請向左轉 並朝{0}直走", step.NextWaypoint.Name);
                    break;

                //front-left
                case int n when (n > -75 && n < -5):
                    stepImage = "Arrow_frontleft";
                    stepLabel = string.Format("請向左前方的{0}直走", step.NextWaypoint.Name);
                    break;

                default:
                    stepImage = "Warning";
                    stepLabel = "You're get ERROR status";
                    break;
            }
        }

        private void GetPathEvent(object sender, EventArgs args)
        {
            Beacon currentBeacon =
                (args as WayPointSignalProcessEventArgs).CurrentBeacon;

            if ((firstBeacon || wrongDirection) && (currentBeacon != null))
            {
                currentStepNum = 0;
                navigationPath = Utility.WaypointRoute.GetPath(currentBeacon,
                                 Utility.Waypoints.First(
                                 c => c.Name == destination)).ToList();
                returnedRoutes.Clear();
                UpdateRoutes(currentStepNum);

                firstBeacon = false;
                wrongDirection = false;
            }
        }

        private void GetNavigationStatusEvent(object sender, EventArgs args)
        {
            if (!firstBeacon && !wrongDirection)
            {
                DisplayInstructions(args);
            }
        }

        #region TabbedPageNavigation binding args
        private string currentStepLabelName;
        public string CurrentStepLabel
        {
            get
            {
                return currentStepLabelName;
            }

            set
            {
                SetProperty(ref currentStepLabelName, value);
            }
        }

        private string currentStepImageName;
        public string CurrentStepImage
        {
            get
            {
                return string.Format("{0}.png", currentStepImageName);
            }

            set
            {
                if (currentStepImageName != value)
                {
                    currentStepImageName = value;
                    OnPropertyChanged("CurrentStepImage");
                }
            }
        }

        private string nextStepLabelName;
        public string NextStepLabel
        {
            get
            {
                if (nextStepLabelName.Contains("的"))
                {
                    return nextStepLabelName.Replace("的", "&#10;");
                }
                else
                {
                    return nextStepLabelName.Split(' ')[0];
                }
            }

            set
            {
                SetProperty(ref nextStepLabelName, value);
            }
        }

        private string nextStepImageName;
        public string NextStepImage
        {
            get
            {
                return string.Format("{0}.png", nextStepImageName);
            }

            set
            {
                if (nextStepImageName != value)
                {
                    nextStepImageName = value;
                    OnPropertyChanged("NextStepImage");
                }
            }
        }

        private double navigationProgress;
        public double NavigationProgress
        {
            get
            {
                return navigationProgress;
            }

            set
            {
                SetProperty(ref navigationProgress, value);
            }
        }
        #endregion

        #region TabbedPageRoutes binding args
        ObservableRangeCollection<RoutesDataClass> returnedRoutes;

        public IList<Grouping<string, RoutesDataClass>> GroupRoutes
        {
            get
            {
                if (returnedRoutes.Count > 0)
                {
                    return (from route in returnedRoutes
                           orderby route.Order
                           group route by route.Floor into routeGroup
                           select new Grouping<string, RoutesDataClass>(routeGroup.Key, routeGroup)).ToList();
                }

                return null;
            }
        }

        private void UpdateRoutes(int currentRouteNum)
        {
            if (currentRouteNum == 0)
            {
                for (int i = 0; i < navigationPath.Count; i++)
                {
                    SetInstruction(navigationPath[i], out string _instructionImage, out string _instructionLabel);
                    returnedRoutes.Add(new RoutesDataClass
                    {
                        Order = i + 1,
                        OpacityValue = (currentRouteNum == i) ? 1 : 0.3,
                        Image = _instructionImage,
                        Instruction = _instructionLabel,
                        Floor = navigationPath[i].NextWaypoint.Beacons[0].Floor.ToString()
                    });
                }
            }
            else
            {
                returnedRoutes[currentRouteNum - 1].OpacityValue = 0.3;
                returnedRoutes[currentRouteNum].OpacityValue = 1;
            }

            OnPropertyChanged("GroupRoutes");
        }

        public class RoutesDataClass
        {
            public int Order { get; set; }
            public double OpacityValue { get; set; }
            public string Image { get; set; }
            public string Instruction { get; set; }
            public string Floor { get; set; }
        }
        #endregion

        #region IDisposable Support
        private bool disposedValue = false; // 偵測多餘的呼叫

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                Utility.BeaconScan.StopScan();
                Utility.SignalProcess.Event.SignalProcessEventHandler -= GetPathEvent;
                Utility.MaN.Event.MaNEventHandler -= GetNavigationStatusEvent;

                if (disposing)
                {
                    // TODO: 處置 Managed 狀態 (Managed 物件)。
                }

                // TODO: 釋放 Unmanaged 資源 (Unmanaged 物件) 並覆寫下方的完成項。
                // TODO: 將大型欄位設為 null。

                disposedValue = true;
            }
        }

        // TODO: 僅當上方的 Dispose(bool disposing) 具有會釋放 Unmanaged 資源的程式碼時，才覆寫完成項。
        // ~TabbedNaviViewModel() {
        //   // 請勿變更這個程式碼。請將清除程式碼放入上方的 Dispose(bool disposing) 中。
        //   Dispose(false);
        // }

        // 加入這個程式碼的目的在正確實作可處置的模式。
        public void Dispose()
        {
            // 請勿變更這個程式碼。請將清除程式碼放入上方的 Dispose(bool disposing) 中。
            Dispose(true);
            // TODO: 如果上方的完成項已被覆寫，即取消下行的註解狀態。
            // GC.SuppressFinalize(this);
        }
        #endregion

    }
}
