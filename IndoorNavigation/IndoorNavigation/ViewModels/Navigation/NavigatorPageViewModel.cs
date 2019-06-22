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
 *      Chun Yu Lai, chunyu1202@gmail.com
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
using Plugin.Multilingual;
using IndoorNavigation.Resources;
using System.Resources;
using IndoorNavigation.Resources.Helpers;
using System.Reflection;

namespace IndoorNavigation.ViewModels.Navigation
{
	public class NavigatorPageViewModel : BaseViewModel, IDisposable
	{
		private Guid _destinationID;
		private NavigationModule _navigationModule;

		private string _currentStepLabelName;
		private string _currentStepImageName;
		private string _currentWaypointName;
		private string _destinationWaypointName;
		private double _navigationProgress;
		private bool _disposedValue = false; // To detect redundant calls
		public ResourceManager _resourceManager;

		public NavigatorPageViewModel(string navigraphName, string destinationName, Guid destinationID)
		{
			_destinationID = destinationID;
			DestinationWaypointName = destinationName;

			_navigationModule = new NavigationModule(navigraphName, destinationID);
			_navigationModule._event._eventHandler += GetNavigationResultEvent;

			const string resourceId = "IndoorNavigation.Resources.AppResources";
			_resourceManager = new ResourceManager(resourceId, typeof(TranslateExtension).GetTypeInfo().Assembly);
			CurrentWaypointName = _resourceManager.GetString("NULL_STRING", CrossMultilingual.Current.CurrentCultureInfo);
		}

		/// <summary>
		/// TODO: Add voice instructions and vibration
		/// According to each navigation status displays the text and image instructions in UI.
		/// </summary>
		/// <param name="args">Arguments.</param>
		private void DisplayInstructions(EventArgs args)
		{
			Console.WriteLine(">> DisplayInstructions");
			NavigationInstruction instruction = (args as NavigationEventArgs).NextInstruction;
			var currentLanguage = CrossMultilingual.Current.CurrentCultureInfo;
			string currentStepImage;
			string currentStepLabel;

			switch ((args as NavigationEventArgs).Result)
			{
				case NavigationResult.Run:
					SetInstruction(instruction, out currentStepLabel, out currentStepImage);
					CurrentStepLabel = currentStepLabel;
					CurrentStepImage = currentStepImage;
					CurrentWaypointName = instruction.CurrentWaypoint.Name;
					NavigationProgress = instruction.Progress;
					break;

				case NavigationResult.AdjustRoute:
					CurrentStepLabel = _resourceManager.GetString("DIRECTION_WRONG_WAY_STRING", currentLanguage);
					CurrentStepImage = "Waiting";
					break;

				case NavigationResult.Arrival:
					CurrentWaypointName = DestinationWaypointName;
					CurrentStepLabel = _resourceManager.GetString("DIRECTION_ARRIVED_STRING", currentLanguage);
					CurrentStepImage = "Arrived";
					NavigationProgress = 100;
					//Dispose();  // release resources
					break;
			}
		}

		private void SetInstruction(NavigationInstruction instruction,
									out string stepLabel,
									out string stepImage)
		{
			var currentLanguage = CrossMultilingual.Current.CurrentCultureInfo;
			switch (instruction.Direction)
			{
				case TurnDirection.FirstDirection:
					stepLabel = string.Format(_resourceManager.GetString("DIRECTION_STRAIGHT_STRING", currentLanguage) + "\n{0}", instruction.NextWaypoint.Name);
					stepImage = "Arrow_up";
					break;

				case TurnDirection.Forward:
					stepLabel = string.Format(_resourceManager.GetString("DIRECTION_STRAIGHT_STRING", currentLanguage) + "\n{0}", instruction.NextWaypoint.Name);
					stepImage = "Arrow_up";
					break;

				case TurnDirection.Forward_Right:
					stepLabel = string.Format(_resourceManager.GetString("DIRECTION_RIGHT_FRONT_STRING", currentLanguage) + "\n{0}", instruction.NextWaypoint.Name);
					stepImage = "Arrow_frontright";
					break;

				case TurnDirection.Right:
					stepLabel = string.Format(_resourceManager.GetString("DIRECTION_RIGHT_STRING", currentLanguage) + "\n{0}", instruction.NextWaypoint.Name);
					stepImage = "Arrow_right";
					break;

				case TurnDirection.Backward_Right:
					stepLabel = string.Format(_resourceManager.GetString("DIRECTION_RIGHT_REAR_STRING", currentLanguage) + "\n{0}", instruction.NextWaypoint.Name);
					stepImage = "Arrow_rearright";
					break;

				case TurnDirection.Backward:
					stepLabel = string.Format(_resourceManager.GetString("DIRECTION_REAR_STRING", currentLanguage) + "\n{0}", instruction.NextWaypoint.Name);
					stepImage = "Arrow_rear";
					break;

				case TurnDirection.Backward_Left:
					stepLabel = string.Format(_resourceManager.GetString("DIRECTION_LEFT_REAR_STRING", currentLanguage) + "\n{0}", instruction.NextWaypoint.Name);
					stepImage = "Arrow_rearleft";
					break;

				case TurnDirection.Left:
					stepLabel = string.Format(_resourceManager.GetString("DIRECTION_LEFT_STRING", currentLanguage) + "\n{0}", instruction.NextWaypoint.Name);
					stepImage = "Arrow_left";
					break;

				case TurnDirection.Forward_Left:
					stepLabel = string.Format(_resourceManager.GetString("DIRECTION_LEFT_FRONT_STRING", currentLanguage) + "\n{0}", instruction.NextWaypoint.Name);
					stepImage = "Arrow_frontleft";
					break;

				case TurnDirection.Up:
					stepLabel = string.Format(_resourceManager.GetString("DIRECTION_UP_STRING", currentLanguage) + "\n{0}", instruction.NextWaypoint.Name);
					stepImage = "Stairs_up";
					break;

				case TurnDirection.Down:
					stepLabel = string.Format(_resourceManager.GetString("DIRECTION_DOWN_STRING", currentLanguage) + "\n{0}", instruction.NextWaypoint.Name);
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
			Console.WriteLine("recevied event raised from NavigationModule");
			DisplayInstructions(args);
		}

		#region NavigatorPage Binding Args
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
