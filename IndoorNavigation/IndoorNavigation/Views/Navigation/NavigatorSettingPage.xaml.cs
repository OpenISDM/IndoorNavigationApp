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
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xamarin.Forms;
using System.Diagnostics;
using IndoorNavigation.Modules;

namespace IndoorNavigation.Views.Navigation
{
    public partial class NavigatorSettingPage : ContentPage
    {
        public IList VoiceSearchItems { get; } = new ObservableCollection<string>(new List<string> { "中文", "英文" });

        public NavigatorSettingPage()
        {
            InitializeComponent();

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
        }

        protected override void OnDisappearing()
        {
            // Before page close, store the status of each route options
            Application.Current.Properties["AvoidStair"] = AvoidStair.On;
            Application.Current.Properties["AvoidElevator"] = AvoidElevator.On;
            Application.Current.Properties["AvoidEscalator"] = AvoidEscalator.On;

            base.OnDisappearing();
        }

        async void Handle_OptionPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == AiForms.Renderers.SwitchCell.OnProperty.PropertyName)
            {
                if (AvoidStair.On && AvoidElevator.On && AvoidEscalator.On)
                {
                    (sender as AiForms.Renderers.SwitchCell).On = false;
                    await DisplayAlert("Oops...", "The route options only could choose the two at the same time", "OK");
                }
            }
        }

    }
}
