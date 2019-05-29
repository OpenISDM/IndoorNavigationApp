using Xamarin.Forms;
using System;
using System.Windows.Input;
using System.Collections.Generic;
using MvvmHelpers;
using IndoorNavigation.Models.NavigaionLayer;
using NavigationEventArgs = IndoorNavigation.Modules.Navigation.NavigationEventArgs;
using IndoorNavigation.Modules;

namespace IndoorNavigation.ViewModels.Navigation
{
    public class NavigatorPageViewModel : BaseViewModel, IDisposable
    {
        private string Destination;
        private NavigationModule navigationModule;

        public NavigatorPageViewModel(string navigraphName, string destination)
        {
            Destination = destination;
            DestinationWaypointName = destination;

            CurrentWaypointName = "NULL";
            EnterNextWaypointCommand = new Command(() => CurrentWaypointName = NextWaypointName);

            navigationModule = new NavigationModule(navigraphName, destination);
            navigationModule.NavigationEvent.ResultEventHandler += GetNavigationResultEvent;
        }

        /// <summary>
        /// According to each navigation status displays the text and image instructions in UI.
        /// </summary>
        /// <param name="args">Arguments.</param>
        private void DisplayInstructions(EventArgs args)
        {
            NavigationInstruction instruction = (args as NavigationEventArgs).NextInstruction;
            CurrentWaypointName = instruction.NextWaypoint.Name;

            NavigationProgress = (args as NavigationEventArgs).Progress;

            string currentStepImage;
            string currentStepLabel;

            switch ((args as NavigationEventArgs).Result)
            {
                case NavigationResult.Run:
                    SetInstruction(instruction, out currentStepLabel, out currentStepImage);
                    CurrentStepLabel = currentStepLabel;
                    CurrentStepImage = currentStepImage;
                    break;

                case NavigationResult.AdjustRoute:
                    CurrentStepLabel = "走錯路囉, 正在重新規劃路線";
                    CurrentStepImage = "Waiting";
                    break;

                case NavigationResult.Arrival:
                    CurrentStepLabel = "Arrived";
                    CurrentStepImage = "恭喜你！已到達終點囉";
                    //Dispose();  // release resources
                    break;
            }
        }

        private void SetInstruction(NavigationInstruction instruction, out string stepLabel, out string stepImage)
        {
            // TODO: Add go up/down stairs
            switch (instruction.Direction)
            {
                case TurnDirection.FirstDirection:
                    stepLabel = string.Format("請向&#10;{0}&#10;直走", instruction.NextWaypoint.Name);
                    stepImage = "Arrow_front";
                    break;

                case TurnDirection.Forward:
                    stepLabel = string.Format("請向前方的&#10;{0}&#10;直走", instruction.NextWaypoint.Name);
                    stepImage = "Arrow_front";
                    break;

                case TurnDirection.Forward_Right:
                    stepLabel = string.Format("請向右前方的{0}直走", instruction.NextWaypoint.Name);
                    stepImage = "Arrow_frontright";
                    break;

                case TurnDirection.Right:
                    stepLabel = string.Format("請向右轉 並朝{0}直走", instruction.NextWaypoint.Name);
                    stepImage = "Arrow_right";
                    break;

                case TurnDirection.Backward_Right:
                    stepLabel = string.Format("請向右後方的{0}直走", instruction.NextWaypoint.Name);
                    stepImage = "Arrow_rearright";
                    break;

                case TurnDirection.Backward:
                    stepLabel = string.Format("請向後轉 並朝{0}直走", instruction.NextWaypoint.Name);
                    stepImage = "Arrow_rear";
                    break;

                case TurnDirection.Backward_Left:
                    stepLabel = string.Format("請向左後方的{0}直走", instruction.NextWaypoint.Name);
                    stepImage = "Arrow_rearleft";
                    break;


                case TurnDirection.Left:
                    stepLabel = string.Format("請向左轉 並朝{0}直走", instruction.NextWaypoint.Name);
                    stepImage = "Arrow_left";
                    break;

                case TurnDirection.Forward_Left:
                    stepLabel = string.Format("請向左前方的{0}直走", instruction.NextWaypoint.Name);
                    stepImage = "Arrow_frontleft";
                    break;

                default:
                    stepLabel = "You're get ERROR status"; 
                    stepImage = "Warning";
                    break;
            }
        }

        /// <summary>
        /// Gets the navigation status event.
        /// </summary>
        private void GetNavigationResultEvent(object sender, EventArgs args)
        {
            DisplayInstructions(args);
        }

        #region NavigatorPage Binding Args
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

        private string currentWaypointName;
        public string CurrentWaypointName
        {
            get
            {
                return currentWaypointName;
            }

            set
            {
                SetProperty(ref currentWaypointName, value);
            }
        }

        private string destinationWaypointName;
        public string DestinationWaypointName
        {
            get
            {
                return destinationWaypointName;
            }

            set
            {
                SetProperty(ref destinationWaypointName, value);
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

        #region Test entry&button
        private string nextWaypointName;
        public string NextWaypointName
        {
            get
            {
                return nextWaypointName;
            }

            set
            {
                SetProperty(ref nextWaypointName, value);
            }
        }

        public ICommand EnterNextWaypointCommand { private set; get; }
        #endregion

        #endregion

        // TODO: The following enum and class will move to the Session.cs
        #region Needed enum and class include EventArgs from Session

        public enum NavigationResult
        {
            Run = 0,
            AdjustRoute,
            Arrival
        }

        public class NavigationEventArgs : EventArgs
        {
            /// <summary>
            /// Status of navigation
            /// </summary>
            public NavigationResult Result { get; set; }

            /// <summary>
            /// Gets or sets the next instruction. It will send to the ViewModel to
            /// update the UI instruction.
            /// </summary>
            public NavigationInstruction NextInstruction { get; set; }

            /// <summary>
            /// Progress of navigation.
            /// </summary>
            public double Progress { get; set; }
        }

        /// <summary>
        /// Instruction of next location to be delivered at the next waypoint
        /// </summary>
        public class NavigationInstruction
        {
            /// <summary>
            /// The next waypoint within navigation path
            /// </summary>
            public Waypoint NextWaypoint { get; set; }

            /// <summary>
            /// The List of "wrong way" waypoint of the next location
            /// </summary>
            public List<Waypoint> WrongwayWaypointList { get; set; }

            /// <summary>
            /// The direction to turn to the next waypoint using the enum type
            /// </summary>
            public TurnDirection Direction { get; set; }
        }

        #endregion

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    //navigationModule.Event.EventHandler -= GetNavigationStatusEvent
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~NavigatorPageViewModel()
        // {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion

    }
}
