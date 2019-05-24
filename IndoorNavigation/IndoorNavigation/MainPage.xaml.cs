using System;
using System.Linq;
using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using IndoorNavigation.Views.Settings;
using IndoorNavigation.Views.Navigation.NTUHYunlin;
using MvvmHelpers;
using System.ComponentModel;
using IndoorNavigation.ViewModels;

namespace IndoorNavigation
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPage : ContentPage
    {
        private List<Location> locations;
        public MainPage()
        {
            InitializeComponent();

            NavigationPage.SetBackButtonTitle(this, "首頁");

            switch (Device.RuntimePlatform)
            {
                case Device.Android:
                    NaviSearchBar.BackgroundColor = Color.White;
                    break;

                default:
                    break;
            }

            locations = new List<Location>
            {
                new Location{ Name="台北市政府", City="台北"},
                new Location{ Name="員林榮民之家", City="彰化"},
                new Location{ Name="雲林市政府", City="雲林"},
                new Location{ Name="雲林台大醫院", City="雲林"},
                new Location{ Name="台南市政府", City="台南"},
                new Location{ Name="高雄市政府", City="高雄"},
                new Location{ Name="高雄高鐵站", City="高雄"},
            };

            LocationListView.ItemsSource = GetLocationList();
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

        async void SettingBtn_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new SettingTableViewPage());
        }

        async void Handle_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            if (e.Item is Location location && location.Name == "雲林台大醫院")
            {
                var answser = await DisplayAlert("Turn to next page?", location.Name, "OK", "Cancel");

                if (answser)
                    await Navigation.PushAsync(new NavigationHomePage());
            }

            //await Navigation.PushAsync(new NavigationHomePage());
        }

        void LocationListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            // disable it
            LocationListView.SelectedItem = null;
        }

        void LocationListView_Refreshing(object sender, EventArgs e)
        {
            LocationListView.EndRefresh();
        }

        void Item_Delete(object sender, System.EventArgs e)
        {
            Location item = (Location)((MenuItem)sender).CommandParameter;

            if (item != null)
            {
                locations.Remove(item);
                LocationListView.ItemsSource = GetLocationList();
            }
        }

        private IEnumerable<Grouping<string, Location>> GetLocationList(string name = null)
        {
            var source = string.IsNullOrEmpty(name) ? locations : locations
                         .Where(c => c.Name.Contains(name));

            return from location in source
                   group location by location.City into locationGroup
                   select new Grouping<string, Location>(locationGroup.Key, locationGroup);
        }
    }

    class Location
    {
        public string Name { get; set; }
        public string City { get; set; }
    }
}
