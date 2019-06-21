/*
 * Copyright (c) 2019 Academia Sinica, Institude of Information Science
 *
 * License:
 *      GPL 3.0 : The content of this file is subject to the terms and
 *      conditions defined in file 'COPYING.txt', which is part of this source
 *      code package.
 *
 * Project Name:
 *
 *      IndoorNavigation
 *
 * File Description:
 *
 *      This view model implements properties and commands for NavigatorPage
 *      to build the data.
 *      It will create NavigationModule and subscribe to the needed event.
 *      
 * Version:
 *
 *      1.0.0, 20190605
 * 
 * File Name:
 *
 *      NavigatorPageViewModel.cs
 *
 * Abstract:
 *
 *      Waypoint-based navigator is a mobile Bluetooth navigation application
 *      that runs on smart phones. It is structed to support anywhere 
 *      navigation indoors in areas covered by different indoor positioning 
 *      system (IPS) and outdoors covered by GPS.In particilar, it can rely on
 *      BeDIS (Building/environment Data and Information System) for indoor 
 *      positioning. This IPS provides a location beacon at every waypoint. The 
 *      beacon brocasts its own coordinates; Consequesntly, the navigator does 
 *      not need to continuously monitor its own position.
 *      This version makes use of Xamarin.Forms, which is a cross-platform UI 
 *      tookit that runs on both iOS and Android.
 *
 * Authors:
 *
 *      Paul Chang, paulchang@iis.sinica.edu.tw
 *
 */
using Xamarin.Forms;
using System;
using System.Windows.Input;
using System.Collections.Generic;
using MvvmHelpers;
using IndoorNavigation.Models.NavigaionLayer;
using IndoorNavigation.Modules;
using static IndoorNavigation.Modules.Session;
using NavigationEventArgs = IndoorNavigation.Modules.Session.NavigationEventArgs;

namespace IndoorNavigation.ViewModels.Navigation
{
    public class NavigatorPageViewModel : BaseViewModel, IDisposable
    {
        private Guid _destinationID;
        private NavigationModule _navigationModule;

        public TestEvent testEvent;

        public NavigatorPageViewModel(string navigraphName, string destinationName, Guid destinationID)
        {
            testEvent = new TestEvent();

            _destinationID = destinationID;
            DestinationWaypointName = destinationName;

            CurrentWaypointName = "NULL";

            _navigationModule = new NavigationModule(navigraphName, destinationID);
            _navigationModule.NavigationEvent.ResultEventHandler += GetNavigationResultEvent;

            //Test function button
            testEvent.TestEventHandler += _navigationModule.HandleCurrentWaypoint;
            EnterNextWaypointCommand = new Command(TestEnterNextWaypointCommand);
        }

        public void TestEnterNextWaypointCommand()
        {
            Guid guidOutput;
            guidOutput = Guid.TryParse(NextWaypointName, out guidOutput) ? guidOutput : Guid.Empty;

            // TODO: Should also check this UUID whether it is in the navigation graph
            if (guidOutput != Guid.Empty)
            {
                testEvent.OnEventCall(new WaypointScanEventArgs
                {
                    WaypointID = guidOutput
                });
            }
        }

        /// <summary>
        /// TODO: Add voice instructions and vibration
        /// According to each navigation status displays the text and image instructions in UI.
        /// </summary>
        /// <param name="args">Arguments.</param>
        private void DisplayInstructions(EventArgs args)
        {
            NavigationInstruction instruction = (args as NavigationEventArgs).NextInstruction;

            string currentStepImage;
            string currentStepLabel;

            switch ((args as NavigationEventArgs).Result)
            {
                case NavigationResult.Run:
                    SetInstruction(instruction, out currentStepLabel, out currentStepImage);
                    CurrentStepLabel = currentStepLabel;
                    CurrentStepImage = currentStepImage;
                    CurrentWaypointName = instruction.NextWaypoint.Name;
                    NavigationProgress = instruction.Progress;
                    break;

                case NavigationResult.AdjustRoute:
                    CurrentStepLabel = "走錯路囉, 正在重新規劃路線";
                    CurrentStepImage = "Waiting";
                    break;

                case NavigationResult.Arrival:
                    CurrentWaypointName = DestinationWaypointName;
                    CurrentStepLabel = "恭喜你！已到達終點囉";
                    CurrentStepImage = "Arrived";
                    NavigationProgress = 100;
                    //Dispose();  // release resources
                    break;
            }
        }

        private void SetInstruction(NavigationInstruction instruction, out string stepLabel, out string stepImage)
        {
            switch (instruction.Direction)
            {
                case TurnDirection.FirstDirection:
                    stepLabel = string.Format("請向\n{0}\n直走", instruction.NextWaypoint.Name);
                    stepImage = "Arrow_up";
                    break;

                case TurnDirection.Forward:
                    stepLabel = string.Format("請向前方的\n{0}\n直走", instruction.NextWaypoint.Name);
                    stepImage = "Arrow_up";
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

                case TurnDirection.Up:
                    stepLabel = string.Format("請上樓\n並朝 {0}直走", instruction.NextWaypoint.Name);
                    stepImage = "Stairs_up";
                    break;

                case TurnDirection.Down:
                    stepLabel = string.Format("請下樓\n並朝 {0}直走", instruction.NextWaypoint.Name);
                    stepImage = "Stairs_down";
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

    public class TestEvent
    {
        public event EventHandler TestEventHandler;

        public void OnEventCall(EventArgs args)
        {
            TestEventHandler?.Invoke(this, args);
        }
    }
}
