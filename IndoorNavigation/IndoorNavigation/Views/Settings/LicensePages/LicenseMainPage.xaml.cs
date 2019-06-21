using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace IndoorNavigation.Views.Settings.LicensePages
{
    public partial class LicenseMainPage : ContentPage
    {
        public LicenseMainPage()
        {
            InitializeComponent();
            NavigationPage.SetBackButtonTitle(this, "授權");
        }

        async void IconsLicenseBtn_Tapped(object sender, System.EventArgs e)
        {
            await Navigation.PushAsync(new IconsLicensePage());
        }
    }
}
