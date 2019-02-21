using System;
using System.Collections.Generic;
using IndoorNavigation.Views.Setting.LicensePages;
using Xamarin.Forms;

namespace IndoorNavigation.Views.Setting
{
    public partial class SettingTableViewPage : ContentPage
    {
        public SettingTableViewPage()
        {
            InitializeComponent();

            ((NavigationPage)Application.Current.MainPage).BarBackgroundColor = Color.FromHex("#009FCC");
            ((NavigationPage)Application.Current.MainPage).BarTextColor = Color.White;
        }

        async void LicenseBtn_Tapped(object sender, System.EventArgs e)
        {
            await Navigation.PushAsync(new LicenseMainPage());
        }
    }
}
