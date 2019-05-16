using System;
using Xamarin.Forms;
using IndoorNavigation.Views.Navigation;
using IndoorNavigation.Views.Settings;
using Xamarin.Forms.Xaml;
using Plugin.Multilingual;
using System.Globalization;
using IndoorNavigation.Resources;

namespace IndoorNavigation
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPage : ContentPage
    {

        public MainPage()
        {
            InitializeComponent();

            // removes Navigation Bar
            //NavigationPage.SetHasNavigationBar(this, false);

            NavigationPage.SetBackButtonTitle(this, "首頁");
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            ((NavigationPage)Application.Current.MainPage).BarBackgroundColor = Color.FromHex("#3F51B5");
            ((NavigationPage)Application.Current.MainPage).BarTextColor = Color.White;

            switch (Device.RuntimePlatform)
            {
                case Device.Android:
                    //NavigatorButton.Padding = new Thickness(30, 1, 1, 1);
                    //AbsoluteLayout.SetLayoutBounds(NavigatorButton, new Rectangle(0.5, 0.52, 0.7, 0.1));
                    break;

                case Device.iOS:
                    // customize CurrentInstruction UI for iPhone 5s/SE
                    if (Height < 600)
                    {
                        //WelcomeLabel.FontSize = 36;
                        //BeDISLabel.FontSize = 39;
                        //SloganLabel.Text = "";
                        //AbsoluteLayout.SetLayoutBounds(NavigatorButton, new Rectangle(0.5, 0.47, 0.7, 0.12));
                    }
                    break;

                default:
                    break;
            }
        }

        async void SettingBtn_Clicked(object sender, System.EventArgs e)
        {
            await Navigation.PushAsync(new SettingTableViewPage());
        }
    }
}
