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
        // sample of TextPickerCell(選擇語音)
        public IList VoiceSearchItems { get; } = new ObservableCollection<string>(new List<string> { "中文", "英文" });

        public NavigatorSettingPage()
        {
            InitializeComponent();

            VoiceSearchItems.Add("日文");

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
