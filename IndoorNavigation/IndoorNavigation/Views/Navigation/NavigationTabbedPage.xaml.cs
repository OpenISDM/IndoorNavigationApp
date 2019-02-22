using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace IndoorNavigation.Views.Navigation
{
    public partial class NavigationTabbedPage : ContentPage
    {
        public NavigationTabbedPage()
        {
            InitializeComponent();

            switch (Device.RuntimePlatform)
            {
                case Device.Android:
                    break;

                case Device.iOS:

                    break;

                default:
                    break;
            }
        }

        void NavigatorTab_Tapped(object sender, System.EventArgs e)
        {
            DisplayAlert("Turn to next page?", "Navigator", "Cancel");
        }

        void RoutesTab_Tapped(object sender, System.EventArgs e)
        {
            DisplayAlert("Turn to next page?", "Routes", "Cancel");
        }

        void SettingTab_Tapped(object sender, System.EventArgs e)
        {
            DisplayAlert("Turn to next page?", "Setting", "Cancel");
        }
    }
}
