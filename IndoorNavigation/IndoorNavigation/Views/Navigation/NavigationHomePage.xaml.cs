using System;
using System.Collections.Generic;
using Xamarin.Forms;
using System.Linq;
using IndoorNavigation.ViewModels;
using System.Diagnostics;
using IndoorNavigation.Modules;
using IndoorNavigation.Models;
using IndoorNavigation.ViewModels.Navigation;

namespace IndoorNavigation.Views.Navigation
{
    public partial class NavigationHomePage : ContentPage
    {
        NaviHomePageViewModel viewModel;

        public NavigationHomePage()
        {
            InitializeComponent();

            NavigationPage.SetBackButtonTitle(this, "Back");

            switch (Device.RuntimePlatform)
            {
                case Device.Android:
                    NaviSearchBar.BackgroundColor = Color.White;
                    break;

                default:
                    break;
            }

            ((NavigationPage)Application.Current.MainPage).BarBackgroundColor = Color.FromHex("#009FCC");
            ((NavigationPage)Application.Current.MainPage).BarTextColor = Color.White;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            viewModel = new NaviHomePageViewModel();
            BindingContext = viewModel;
        }

        void LocationListView_Refreshing(object sender, EventArgs e)
        {
            LocationListView.EndRefresh();
        }

        void FindLocationFAB_Pressed(object sender, EventArgs e)
        {
            ButtonFrame.HasShadow = false;
            ButtonFrame.BackgroundColor = Color.White;
            FindLocationFAB.Image = "position_FAB.png";
        }

        void FindLocationFAB_Released(object sender, EventArgs e)
        {
            ButtonFrame.HasShadow = true;
            ButtonFrame.BackgroundColor = Color.FromHex("#009FCC");
            FindLocationFAB.Image = "position_FAB_white.png";
        }

        void FindLocationFAB_Clicked(object sender, EventArgs e)
        {
            //List<Grouping<string, Location>> locations = ((IEnumerable<Grouping<string, Location>>)LocationListView.ItemsSource).ToList();
            ////scroll to the first location of second group
            //LocationListView.ScrollTo(locations[1][0], ScrollToPosition.Start, true);

            //int n = 1;
            //foreach (WaypointModel waypoint in Utility.Waypoints)
            //{
            //    DisplayAlert("(" + n.ToString() + ") waypoint", "name is " + waypoint.Name + ", floor is " + waypoint.Beacons[0].Floor, "Cancel");
            //    n++;
            //}
        }
    }
}
