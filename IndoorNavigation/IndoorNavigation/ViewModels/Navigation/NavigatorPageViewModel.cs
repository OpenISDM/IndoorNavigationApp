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
 *      Chun-Yu Lai, chunyu1202@gmail.com
 *
 */

using System;
using MvvmHelpers;
using IndoorNavigation.Models.NavigaionLayer;
using IndoorNavigation.Modules;
using static IndoorNavigation.Modules.Session;
using Plugin.Multilingual;
using System.Resources;
using IndoorNavigation.Resources.Helpers;
using System.Reflection;
using IndoorNavigation.Views.Navigation;
using Xamarin.Forms;
using IndoorNavigation.Modules.Utilities;
using System.IO;

namespace IndoorNavigation.ViewModels.Navigation
{
	public class NavigatorPageViewModel : BaseViewModel, IDisposable
	{
		private Guid _destinationID;
		private NavigationModule _navigationModule;
        private const string _pictureType = "picture";
        private const int _originalInstructionLocation = 3;
        private const int _firstDirectionInstructionLocation = 4;
        private const int _firstDirectionInstructionScale = 2;
        private const int _originalInstructionScale = 1;
        private const int _millisecondsTimeout = 2000;
        private const int _initialFaceDirection = 0;
        private const int _initialBackDirection = 1;
        private string _currentStepLabelName=" ";
		private string _currentStepImageName;
        private string _firstDirectionPicture;
        private int _firstDirectionRotationValue;
        private int _firsrDirectionInstructionScaleVale;
        private int _instructionLocation;
		private string _currentWaypointName;
        private string _firstDirectiionPicture;
		private string _destinationWaypointName;
		private double _navigationProgress;
		private bool _disposedValue = false; // To detect redundant calls
		public ResourceManager _resourceManager;
        public NavigatorPage _navigatorPage;
        private FirstDirectionInstruction _firstDirectionInstruction;
        private NavigationGraph _navigationGraph;
        private XMLInformation _xmlInformation;

        public NavigatorPageViewModel(string navigationGraphName,
                                      Guid destinationRegionID,
                                      Guid destinationWaypointID,
                                      string destinationWaypointName,
                                      XMLInformation informationXML)
                                    
        {
            _firsrDirectionInstructionScaleVale = 1;
            _destinationID = destinationWaypointID;
            _destinationWaypointName = destinationWaypointName;
            CurrentStepImage = "waittingscan.gif";

            _instructionLocation = _originalInstructionLocation;
            _navigationModule = new NavigationModule(navigationGraphName,
                                                     destinationRegionID,
                                                     destinationWaypointID);
            _navigationModule._event._eventHandler += GetNavigationResultEvent;
            const string resourceId = "IndoorNavigation.Resources.AppResources";
            _resourceManager = new ResourceManager(resourceId, typeof(TranslateExtension).GetTypeInfo().Assembly);
            CurrentWaypointName = _resourceManager.GetString("NULL_STRING", CrossMultilingual.Current.CurrentCultureInfo);
            CurrentStepLabel = _resourceManager.GetString("NO_SIGNAL_STRING", CrossMultilingual.Current.CurrentCultureInfo);
            var currentLanguage = CrossMultilingual.Current.CurrentCultureInfo;

            if (CrossMultilingual.Current.CurrentCultureInfo.ToString() == "en" || CrossMultilingual.Current.CurrentCultureInfo.ToString() == "en-US")
            {
                _firstDirectionInstruction = NavigraphStorage.LoadFirstDirectionXML(navigationGraphName + "_en-US.xml");
            }
            else if (CrossMultilingual.Current.CurrentCultureInfo.ToString() == "zh" || CrossMultilingual.Current.CurrentCultureInfo.ToString() == "zh-TW")
            {
                _firstDirectionInstruction = NavigraphStorage.LoadFirstDirectionXML(navigationGraphName + "_zh.xml");
            }
            _navigationGraph = NavigraphStorage.LoadNavigationGraphXML(navigationGraphName);
            _xmlInformation = informationXML;
        }     

        public void Stop() {

            _navigationModule.Stop();
        }

		/// <summary>
		/// According to each navigation status displays the text and image instructions in UI.
		/// </summary>
		/// <param name="args">Arguments.</param>
		private void DisplayInstructions(EventArgs args)
		{

			Console.WriteLine(">> DisplayInstructions");
			NavigationInstruction instruction = (args as Session.NavigationEventArgs)._nextInstruction;
			var currentLanguage = CrossMultilingual.Current.CurrentCultureInfo;
			string currentStepImage;
			string currentStepLabel;
            //string currentWaypointName;
            string firstDirectionPicture = null;
            int rotationValue = 0;
            int locationValue = _originalInstructionLocation;
            int instructionScale = _originalInstructionScale;
			switch ((args as Session.NavigationEventArgs)._result)
			{
				case NavigationResult.Run:
					SetInstruction(instruction, out currentStepLabel, out currentStepImage, out firstDirectionPicture, out rotationValue, out locationValue, out instructionScale);
					CurrentStepLabel = currentStepLabel;
					CurrentStepImage = currentStepImage;
                    FirstDirectionPicture = firstDirectionPicture;
                    InstructionLocationValue = locationValue;
                    RotationValue = rotationValue;
                    InstructionScaleValue = instructionScale;
                    CurrentWaypointName = _xmlInformation.GiveWaypointName(instruction._currentWaypointGuid);
					NavigationProgress = instruction._progress;
                    
                    Utility._textToSpeech.Speak(
                        CurrentStepLabel,
                        _resourceManager.GetString("CULTURE_VERSION_STRING", currentLanguage));

					break;

				case NavigationResult.AdjustRoute:
					CurrentStepLabel =
                        _resourceManager.GetString("DIRECTION_WRONG_WAY_STRING", currentLanguage);
					CurrentStepImage = "Waiting";

                    Utility._textToSpeech.Speak(
                        CurrentStepLabel,
                        _resourceManager.GetString("CULTURE_VERSION_STRING", currentLanguage));
                    System.Threading.Thread.Sleep(_millisecondsTimeout);
					break;

				case NavigationResult.Arrival:
					CurrentWaypointName = _xmlInformation.GiveWaypointName(_destinationID);
					CurrentStepLabel =
                        _resourceManager.GetString("DIRECTION_ARRIVED_STRING", currentLanguage);
					CurrentStepImage = "Arrived";
					NavigationProgress = 100;

                    Utility._textToSpeech.Speak(
                        CurrentStepLabel,
                        _resourceManager.GetString("CULTURE_VERSION_STRING", currentLanguage));
                    Stop();
					break;

                case NavigationResult.NoRoute:
                    Console.WriteLine("No Route");                   
                    GoAdjustAvoidType();
                    Stop();
                    break;
                case NavigationResult.ArriveVirtualPoint:
                    SetInstruction(instruction, out currentStepLabel, out currentStepImage, out firstDirectionPicture, out rotationValue, out locationValue, out instructionScale);
                    CurrentStepLabel = currentStepLabel;
                    CurrentStepImage = "Arrived";
                    NavigationProgress = 100;
                    CurrentWaypointName = _xmlInformation.GiveWaypointName(instruction._currentWaypointGuid);
                    FirstDirectionPicture = firstDirectionPicture;
                    InstructionLocationValue = locationValue;
                    RotationValue = rotationValue;
                    Utility._textToSpeech.Speak(
                        CurrentStepLabel,
                        _resourceManager.GetString("CULTURE_VERSION_STRING", currentLanguage));

                    Stop();
                    break;

            }
		}

		private void SetInstruction(NavigationInstruction instruction,
									out string stepLabel,
									out string stepImage,
                                    out string firstDirectionImage,
                                    out int rotation,
                                    out int location,
                                    out int instructionValue)
		{
            var currentLanguage = CrossMultilingual.Current.CurrentCultureInfo;
            string connectionTypeString = "";
            string nextWaypointName = instruction._nextWaypointName;
            nextWaypointName = _xmlInformation.GiveWaypointName(instruction._nextWaypointGuid);
            string nextRegionName = instruction._information._regionName;
            firstDirectionImage = null;
            rotation = 0;
            stepImage = "";
            instructionValue = _originalInstructionScale;
            location = _originalInstructionLocation;
            nextRegionName = _xmlInformation.GiveRegionName(instruction._currentRegionGuid);
            switch (instruction._information._turnDirection)
			{
				case TurnDirection.FirstDirection:
                    string firstDirection_Landmark = _firstDirectionInstruction.returnLandmark(instruction._currentWaypointGuid);
                    CardinalDirection firstDirection_Direction = _firstDirectionInstruction.returnDirection(instruction._currentWaypointGuid);
                    int faceDirection = (int)firstDirection_Direction;
                    int turnDirection = (int)instruction._information._relatedDirectionOfFirstDirection;
                    string initialDirectionString = "";
                    int directionFaceorBack = _firstDirectionInstruction.returnFaceOrBack(instruction._currentWaypointGuid);
                    Console.WriteLine("Face : " + faceDirection);
                    Console.WriteLine("First Turn : " + turnDirection);
                    if (faceDirection>turnDirection)
                    {
                        turnDirection = (turnDirection + 8) - faceDirection;
                    }
                    else
                    {
                        turnDirection = turnDirection - faceDirection;
                    }

                    if (directionFaceorBack == _initialFaceDirection)
                    {
                        initialDirectionString = _resourceManager.GetString(
                        "DIRECTION_INITIAIL_FACE_STRING",
                        currentLanguage);
                        
                    }
                    else if (directionFaceorBack == _initialBackDirection)
                    {
                        
                        initialDirectionString = _resourceManager.GetString(
                        "DIRECTION_INITIAIL_BACK_STRING",
                        currentLanguage);
                        if (turnDirection < 4)
                        {
                            turnDirection = turnDirection + 4;
                        }
                        else if (turnDirection >= 4)
                        {
                            turnDirection = turnDirection - 4;
                        }
                    }
                    string instructionDirection = "";
                    string stepImageString = "";
                    
                    CardinalDirection cardinalDirection = (CardinalDirection)turnDirection;
                    switch(cardinalDirection)
                    {
                        case CardinalDirection.North:
                            instructionDirection = _resourceManager.GetString(
                            "GO_STRAIGHT_STRING",
                            currentLanguage);
                            stepImageString = "Arrow_up";
                            break;
                        case CardinalDirection.Northeast:
                            instructionDirection = _resourceManager.GetString(
                            "GO_RIGHT_FRONT_STRING",
                            currentLanguage);
                            stepImageString = "Arrow_frontright";
                            break;
                        case CardinalDirection.East:
                            instructionDirection = _resourceManager.GetString(
                            "TURN_RIGHT_STRING",
                            currentLanguage);
                            stepImageString = "Arrow_right";
                            break;
                        case CardinalDirection.Southeast:
                            instructionDirection = _resourceManager.GetString(
                            "TURN_RIGHT_REAR_STRING",
                            currentLanguage);
                            stepImageString = "Arrow_rearright";
                            break;
                        case CardinalDirection.South:
                            instructionDirection = _resourceManager.GetString(
                            "TURN_BACK_STRING",
                            currentLanguage);
                            stepImageString = "Arrow_down";
                            break;
                        case CardinalDirection.Southwest:
                            instructionDirection = _resourceManager.GetString(
                            "TURN_RIGHT_REAR_STRING",
                            currentLanguage);
                            stepImageString = "Arrow_rearleft";
                            break;
                        case CardinalDirection.West:
                            instructionDirection = _resourceManager.GetString(
                            "TURN_LEFT_STRING",
                            currentLanguage);
                            stepImageString = "Arrow_left";
                            break;
                        case CardinalDirection.Northwest:
                            instructionDirection = _resourceManager.GetString(
                            "TURN_LEFT_FRONT_STRING",
                            currentLanguage);
                            stepImageString = "Arrow_frontleft";
                            break;
                    }
                    if (instruction._previousRegionGuid != Guid.Empty && instruction._previousRegionGuid != instruction._currentRegionGuid)
                    {
                        stepLabel = string.Format(
                            _resourceManager.GetString(
                            "DIRECTION_INITIAIL_CROSS_REGION_STRING",
                            currentLanguage),
                            instructionDirection,
                            Environment.NewLine,
                            nextWaypointName,
                            Environment.NewLine,
                            instruction._information._distance);
                        stepImage = stepImageString;
                        break;
                    }
                    else if (firstDirection_Landmark == _pictureType)
                    {
                        string pictureName;

                        string regionString = instruction._currentRegionGuid.ToString();
                        string waypointString = instruction._currentWaypointGuid.ToString();

                        pictureName = _navigationGraph.GetBuildingName() + regionString.Substring(33, 3) + waypointString.Substring(31, 5);
                        string picturePath = Path.Combine(_navigationGraph.GetBuildingName(),pictureName);
                        Console.WriteLine("PictureName : " + picturePath);
  
                        stepLabel = string.Format(
                            initialDirectionString,
                            _resourceManager.GetString(
                            "PICTURE_DIRECTION_STRING",
                            currentLanguage),
                            Environment.NewLine,
                            instructionDirection,
                            Environment.NewLine,
                            nextWaypointName,
                            " ",
                            instruction._information._distance);
                        firstDirectionImage = picturePath;
                        stepImage = stepImageString;
                        rotation = 75;
                        location = _firstDirectionInstructionLocation;
                        instructionValue = _firstDirectionInstructionScale;
                        break;
                    }
                    else
                    {
                        
                        stepLabel = string.Format(
                            initialDirectionString,
                            firstDirection_Landmark,
                            Environment.NewLine,
                            instructionDirection,
                            Environment.NewLine,
                            nextWaypointName,
                            Environment.NewLine,
                            instruction._information._distance);
                        stepImage = stepImageString;
                        break;
                    }

				case TurnDirection.Forward:
					stepLabel = string.Format(
                        _resourceManager.GetString(
                            "DIRECTION_STRAIGHT_STRING",
                            currentLanguage),
                            Environment.NewLine,
                            instruction._information._distance,
                            Environment.NewLine,
                            nextWaypointName);
					stepImage = "Arrow_up";

                   	break;

				case TurnDirection.Forward_Right:
					stepLabel = string.Format(
                        _resourceManager.GetString(
                            "DIRECTION_RIGHT_FRONT_STRING",
                            currentLanguage),
                            Environment.NewLine,
                            instruction._information._distance,
                            Environment.NewLine,
                            nextWaypointName);
					stepImage = "Arrow_frontright";
	
					break;

				case TurnDirection.Right:
					stepLabel = string.Format(
                        _resourceManager.GetString(
                            "DIRECTION_RIGHT_STRING",
                            currentLanguage),
                            Environment.NewLine,
                            instruction._information._distance,
                            Environment.NewLine,
                            nextWaypointName);
					stepImage = "Arrow_right";
	
					break;

				case TurnDirection.Backward_Right:
					stepLabel = string.Format(
                        _resourceManager.GetString(
                            "DIRECTION_RIGHT_REAR_STRING",
                            currentLanguage),
                            Environment.NewLine,
                            instruction._information._distance,
                            Environment.NewLine,
                            nextWaypointName);
					stepImage = "Arrow_rearright";

					break;

				case TurnDirection.Backward:
					stepLabel = string.Format(
                        _resourceManager.GetString(
                            "DIRECTION_REAR_STRING",
                            currentLanguage),
                            Environment.NewLine,
                            instruction._information._distance,
                            Environment.NewLine,
                            nextWaypointName);
					stepImage = "Arrow_down";

					break;

				case TurnDirection.Backward_Left:
					stepLabel = string.Format(
                        _resourceManager.GetString(
                            "DIRECTION_LEFT_REAR_STRING",
                            currentLanguage),
                            Environment.NewLine,
                            instruction._information._distance,
                            Environment.NewLine,
                            nextWaypointName);
					stepImage = "Arrow_rearleft";

					break;

				case TurnDirection.Left:
					stepLabel = string.Format(
                        _resourceManager.GetString(
                            "DIRECTION_LEFT_STRING",
                            currentLanguage),
                            Environment.NewLine,
                            instruction._information._distance,
                            Environment.NewLine,
                            nextWaypointName);
					stepImage = "Arrow_left";

					break;

				case TurnDirection.Forward_Left:
					stepLabel = string.Format(
                        _resourceManager.GetString(
                            "DIRECTION_LEFT_FRONT_STRING",
                            currentLanguage),
                            Environment.NewLine,
                            instruction._information._distance,
                            Environment.NewLine,
                            nextWaypointName);
					stepImage = "Arrow_frontleft";

					break;

				case TurnDirection.Up:
                    switch (instruction._information._connectionType)
                    {
                        case ConnectionType.Elevator:
                            connectionTypeString = _resourceManager.GetString("ELEVATOR_STRING", currentLanguage);
                            stepImage = "Elevator_up";
                            break;
                        case ConnectionType.Escalator:
                            connectionTypeString = _resourceManager.GetString("ESCALATOR_STRING", currentLanguage);
                             stepImage = "Stairs_up";
                            break;
                        case ConnectionType.Stair:
                            connectionTypeString = _resourceManager.GetString("STAIR_STRING", currentLanguage);
                            stepImage = "Stairs_up";
                            break;
                        case ConnectionType.NormalHallway:
                            connectionTypeString = _resourceManager.GetString("NORMALHALLWAY_STRING", currentLanguage);
                            stepImage = "Stairs_up";
                            break;
                    }
                    stepLabel = string.Format(
                        _resourceManager.GetString(
                            "DIRECTION_UP_STRING",
							currentLanguage),
                            connectionTypeString,
                            Environment.NewLine,
                            nextRegionName);
					break;

				case TurnDirection.Down:
                    switch(instruction._information._connectionType)
                    {
                        case ConnectionType.Elevator:
                            connectionTypeString = _resourceManager.GetString("ELEVATOR_STRING", currentLanguage);
                            stepImage = "Elevtor_down";
                            break;
                        case ConnectionType.Escalator:
                            connectionTypeString = _resourceManager.GetString("ESCALATOR_STRING", currentLanguage);
                            stepImage = "Stairs_down";
                            break;
                        case ConnectionType.Stair:
                            connectionTypeString = _resourceManager.GetString("STAIR_STRING", currentLanguage);
                            stepImage = "Stairs_down";
                            break;
                        case ConnectionType.NormalHallway:
                            connectionTypeString = _resourceManager.GetString("NORMALHALLWAY_STRING", currentLanguage);
                            stepImage = "Stairs_down";
                            break;
                    }

					stepLabel = string.Format(
                        _resourceManager.GetString(
                            "DIRECTION_DOWN_STRING",
                            currentLanguage),
                            connectionTypeString,
                            Environment.NewLine,
                            nextRegionName);
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
			Console.WriteLine("recevied event raised from NavigationModule");
			DisplayInstructions(args);
		}

        #region NavigatorPage Binding Args

        public void GoAdjustAvoidType()
        {  
            var currentLanguage = CrossMultilingual.Current.CurrentCultureInfo;
            Page tempMainPage = Application.Current.MainPage;

            Device.BeginInvokeOnMainThread(async () =>
            {
                for (int PageIndex = tempMainPage.Navigation.NavigationStack.Count-1; PageIndex > 1; PageIndex--)
                {
                    tempMainPage.Navigation.RemovePage(tempMainPage.Navigation.NavigationStack[PageIndex]);
                }
                await tempMainPage.Navigation.PushAsync(new NavigatorSettingPage(), true);
                await tempMainPage.DisplayAlert(
                    _resourceManager.GetString("WARN_STRING", currentLanguage),
                    _resourceManager.GetString("PLEASE_ADJUST_AVOID_ROUTE_STRING", currentLanguage),
                    _resourceManager.GetString("OK_STRING", currentLanguage));
            });
        }

        public string CurrentStepLabel
		{
			get
			{
				return _currentStepLabelName;
			}

			set
			{
				SetProperty(ref _currentStepLabelName, value);
			}
		}

		public string CurrentStepImage
		{
			get
			{
				return string.Format("{0}.png", _currentStepImageName);
			}

			set
			{
				if (_currentStepImageName != value)
				{
					_currentStepImageName = value;
					OnPropertyChanged("CurrentStepImage");
				}
			}
		}

        public int RotationValue
        {
            get
            {
                return _firstDirectionRotationValue;
            }
            set
            {
                SetProperty(ref _firstDirectionRotationValue, value);
            }
        }

        public int InstructionScaleValue
        {
            get
            {
                return _firsrDirectionInstructionScaleVale;
            }
            set
            {
                SetProperty(ref _firsrDirectionInstructionScaleVale, value);
            }
        }

        public int InstructionLocationValue
        {
            get
            {
                return _instructionLocation;
            }
            set
            {
                SetProperty(ref _instructionLocation, value);
            }
        }

        public string FirstDirectionPicture
        {
            get
            {
                return string.Format("{0}.png", _firstDirectionPicture);
            }

            set
            {
                if (_firstDirectionPicture != value)
                {
                    _firstDirectionPicture = value;
                    OnPropertyChanged("FirstDirectionPicture");
                }
            }
        }

        public string CurrentWaypointName
		{
			get
			{
				return _currentWaypointName;
			}

			set
			{
				SetProperty(ref _currentWaypointName, value);
			}
		}

		public string DestinationWaypointName
		{
			get
			{
				return _destinationWaypointName;
			}

			set
			{
				SetProperty(ref _destinationWaypointName, value);
			}
		}

		public double NavigationProgress
		{
			get
			{
				return _navigationProgress;
			}

			set
			{
				SetProperty(ref _navigationProgress, value);
			}
		}
		#endregion

		#region IDisposable Support
		protected virtual void Dispose(bool disposing)
		{
			if (!_disposedValue)
			{
				if (disposing)
				{
					// TODO: dispose managed state (managed objects).
					_navigationModule._event._eventHandler -= GetNavigationResultEvent;
                    _navigationModule.Dispose();
				}

				// TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
				// TODO: set large fields to null.

				_disposedValue = true;
			}
		}

		// TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged
		// resources.
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
