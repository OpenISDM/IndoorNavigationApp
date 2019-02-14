using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using IndoorNavigation.Views.Navigator;

namespace IndoorNavigation
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();

            // removes Navigation Bar
            NavigationPage.SetHasNavigationBar(this, false);

            switch (Device.RuntimePlatform)
            {
                case Device.Android:
                    NavigatorButton.Padding = new Thickness(20, 1, 1, 1);
                    TrackingButton.Padding = new Thickness(20, 1, 1, 1);
                    break;

                default:
                    break;
            }
        }

        async void NavigatorButton_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new NavigatorHomePage());
        }
    }
}
