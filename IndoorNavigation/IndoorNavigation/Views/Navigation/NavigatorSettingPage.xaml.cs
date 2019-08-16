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
 *      This file contains the class for the setting page that includes route 
 *      and audio instruction options/preferences.
 *      
 * Version:
 *
 *      1.0.0, 20190605
 * 
 * File Name:
 *
 *      NavigatorSettingPage.xaml.cs
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
using IndoorNavigation.Resources.Helpers;
using Plugin.Multilingual;
using Prism.Commands;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Resources;
using System.Windows.Input;
using Xamarin.Forms;

namespace IndoorNavigation.Views.Navigation
{
    public partial class NavigatorSettingPage : ContentPage
    {
        
        public IList _chooseRssi { get; } = new ObservableCollection<string>();
        public ICommand _changeRssiCommand => new DelegateCommand(HandleChangeRssi);
        public IList _voiceSearchItems { get; } =
            new ObservableCollection<string>(new List<string> { "中文", "英文" });
		const string _resourceId = "IndoorNavigation.Resources.AppResources";
		ResourceManager _resourceManager =
			new ResourceManager(_resourceId, typeof(TranslateExtension).GetTypeInfo().Assembly);
        //Application.Current.Properties["StrongRssi"] = new object ();
        //Application.Current.Properties["MediumRssi"] = new object ();
        //Application.Current.Properties["WeakRssi"] = new object ();
		public NavigatorSettingPage()
        {
            InitializeComponent();
            AddItems();

            BindingContext = this;

            switch (Device.RuntimePlatform)
            {
                case Device.Android:
                    NavigationSettingsView.HeaderHeight = 50;
                    NavigationSettingsView.HeaderPadding = new Thickness(14, 0, 0, 16);
                    break;

                case Device.iOS:
                    break;

                default:
                    break;
            }
            
            // Restore the status of route options
            if (Application.Current.Properties.ContainsKey("AvoidStair"))
            {
                AvoidStair.On = (bool)Application.Current.Properties["AvoidStair"];
                AvoidElevator.On = (bool)Application.Current.Properties["AvoidElevator"];
                AvoidEscalator.On = (bool)Application.Current.Properties["AvoidEscalator"];
            }

            if(Application.Current.Properties.ContainsKey("StrongRssi"))
            {
                if ((bool)Application.Current.Properties["StrongRssi"] == true)
                {
                    OptionPicker.SelectedItem = _resourceManager.GetString("STRONG_STRING", CrossMultilingual.Current.CurrentCultureInfo);
                }
                else if ((bool)Application.Current.Properties["MediumRssi"] == true)
                {
                    OptionPicker.SelectedItem = _resourceManager.GetString("MEDIUM_STRING", CrossMultilingual.Current.CurrentCultureInfo);
                }
                else if ((bool)Application.Current.Properties["WeakRssi"] == true)
                {
                    OptionPicker.SelectedItem = _resourceManager.GetString("WEAK_STRING", CrossMultilingual.Current.CurrentCultureInfo);
                }
            }
            
        }

        private async void HandleChangeRssi()
        {
            switch (OptionPicker.SelectedItem.ToString().Trim())
            {
                case "Strong":
                case "強":
                    Application.Current.Properties["StrongRssi"] = true;
					Application.Current.Properties["MediumRssi"] = false;
					Application.Current.Properties["WeakRssi"] = false;
                    break;
                case "Weak":
                case "弱":
					Application.Current.Properties["StrongRssi"] = false;
					Application.Current.Properties["MediumRssi"] = false;
					Application.Current.Properties["WeakRssi"] = true;
                    break;
                case "Medium":
                case "中":
					Application.Current.Properties["StrongRssi"] = false;
					Application.Current.Properties["MediumRssi"] = true;
                    Application.Current.Properties["WeakRssi"] = false;
                    break;
            }
        }

        protected override void OnDisappearing()
        {
            // Before page close, store the status of each route options
            Application.Current.Properties["AvoidStair"] = AvoidStair.On;
            Application.Current.Properties["AvoidElevator"] = AvoidElevator.On;
            Application.Current.Properties["AvoidEscalator"] = AvoidEscalator.On;
			if (OptionPicker.SelectedItem != null)
			{
				Device.BeginInvokeOnMainThread(async () =>
				{	
					switch (OptionPicker.SelectedItem.ToString().Trim())
					{
						case "Strong":
						case "強":
							Application.Current.Properties["StrongRssi"] = true;
							Application.Current.Properties["MediumRssi"] = false;
							Application.Current.Properties["WeakRssi"] = false;
							break;
						case "Medium":
						case "中":
                            Application.Current.Properties["StrongRssi"] = false;
                            Application.Current.Properties["MediumRssi"] = true;
                            Application.Current.Properties["WeakRssi"] = false;
                            break;
						case "Weak":
						case "弱":
                            Application.Current.Properties["StrongRssi"] = false;
                            Application.Current.Properties["MediumRssi"] = false;
                            Application.Current.Properties["WeakRssi"] = true;
                            break;
					}
					await Application.Current.SavePropertiesAsync();
				});
			}
			base.OnDisappearing();
        }

        private void AddItems()
        {
            var ci = CrossMultilingual.Current.CurrentCultureInfo;
            _chooseRssi.Clear();
            _chooseRssi.Add(_resourceManager.GetString("STRONG_STRING", ci));
            _chooseRssi.Add(_resourceManager.GetString("MEDIUM_STRING", ci));
            _chooseRssi.Add(_resourceManager.GetString("WEAK_STRING", ci));
        }

        async void Handle_OptionPropertyChanged(object sender,
                                                System.ComponentModel.PropertyChangedEventArgs e)
        {
            var currentLanguage = CrossMultilingual.Current.CurrentCultureInfo;
            if (e.PropertyName == AiForms.Renderers.SwitchCell.OnProperty.PropertyName)
            {
                if (AvoidStair.On && AvoidElevator.On && AvoidEscalator.On)
                {
                    (sender as AiForms.Renderers.SwitchCell).On = false;

                    await DisplayAlert(_resourceManager.GetString("ERROR_STRING", currentLanguage),
                        _resourceManager.GetString("AVOID_ALL_CONNECTION_TYPE_STRING",
                                                   currentLanguage),
                        _resourceManager.GetString("OK_STRING", currentLanguage));
                }
            }

        }

    }
}
