using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using IndoorNavigation.Views.Navigation;
using IndoorNavigation.Views.Settings;
using Xamarin.Forms.Xaml;
using System.Diagnostics;

namespace IndoorNavigation
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();

            // removes Navigation Bar
            NavigationPage.SetHasNavigationBar(this, false);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            switch (Device.RuntimePlatform)
            {
                case Device.Android:
                    NavigatorButton.Padding = new Thickness(30, 1, 1, 1);
                    AbsoluteLayout.SetLayoutBounds(NavigatorButton, new Rectangle(0.5, 0.52, 0.7, 0.1));
                    TrackingButton.Padding = new Thickness(30, 1, 1, 1);
                    AbsoluteLayout.SetLayoutBounds(TrackingButton, new Rectangle(0.5, 0.78, 0.7, 0.1));
                    break;

                case Device.iOS:
                    // customize CurrentInstruction UI for iPhone 5s/SE
                    if (Height < 600)
                    {
                        WelcomeLabel.FontSize = 36;
                        BeDISLabel.FontSize = 39;
                        SloganLabel.Text = "";
                        AbsoluteLayout.SetLayoutBounds(NavigatorButton, new Rectangle(0.5, 0.47, 0.7, 0.12));
                        AbsoluteLayout.SetLayoutBounds(TrackingButton, new Rectangle(0.5, 0.75, 0.7, 0.12));
                    }
                    break;

                default:
                    break;
            }
        }

        async void SettingImageButton_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new SettingTableViewPage());
        }

        async void NavigatorButton_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new NavigationHomePage());
        }
    }
}
