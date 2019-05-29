using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xamarin.Forms;
using System.Diagnostics;

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

        }

        void Handle_OptionPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == AiForms.Renderers.SwitchCell.OnProperty.PropertyName)
            {
                if (AvoidStair.On && AvoidElevator.On && AvoidEscalator.On)
                {
                    (sender as AiForms.Renderers.SwitchCell).On = false;
                    DisplayAlert("Oops...", "The route options only could choose the two at the same time", "OK");
                }
                else
                {
                    //TODO: Send the prefence to the NavigationModule
                    //switch ((sender as AiForms.Renderers.SwitchCell).Title) { }
                }
            }
        }

    }
}
