using System;
using System.Collections.Generic;
using Xamarin.Forms;
using System.Linq;
using IndoorNavigation.ViewModels;

namespace IndoorNavigation.Views.Navigator
{
    public partial class NavigatorHomePage : ContentPage
    {
        public NavigatorHomePage()
        {
            InitializeComponent();

            ((NavigationPage)Application.Current.MainPage).BarBackgroundColor = Color.FromHex("#009FCC");
            ((NavigationPage)Application.Current.MainPage).BarTextColor = Color.White;

            LocationListView.ItemsSource = GetLocationList();
        }

        void NaviSearchBar_TextChanged(object sender, TextChangedEventArgs e)
        {
            LocationListView.ItemsSource = GetLocationList(e.NewTextValue);
        }

        void LocationListView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            if (e.Item is Location location)
                DisplayAlert("Turn to next page?", location.Name, "OK", "Cancel");
        }

        void LocationListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            // disable it
            LocationListView.SelectedItem = null;
        }

        void LocationListView_Refreshing(object sender, EventArgs e)
        {
            LocationListView.ItemsSource = GetLocationList();
            LocationListView.EndRefresh();
        }

        // test fake data group and put into listview
        private static IEnumerable<Grouping<string, Location>> GetLocationList(string name = null)
        {
            List<Location> locations = new List<Location>
            {
                new Location{ Name="402", Floor="1樓", Distance=12.3 },
                new Location{ Name="資訊室", Floor="3樓"},
                new Location{ Name="廁所", Floor="1樓", Distance=6},
                new Location{ Name="415", Floor="2樓", Distance=17.6},
                new Location{ Name="420", Floor="1樓", Distance=5},
                new Location{ Name="櫃檯", Floor="3樓"},
                new Location{ Name="廁所", Floor="2樓", Distance=16},
                new Location{ Name="405", Floor="2樓", Distance=8.6},
                new Location{ Name="460", Floor="3樓", Distance=65},
                new Location{ Name="40", Floor="6樓", Distance=5},
                new Location{ Name="人事", Floor="5樓"},
                new Location{ Name="廁所", Floor="4樓", Distance=25},
                new Location{ Name="廁所", Floor="5樓", Distance=15},
                new Location{ Name="廁所", Floor="6樓", Distance=8.6},
                new Location{ Name="茶水間", Floor="6樓", Distance=6.5},
                new Location{ Name="茶水間", Floor="4樓", Distance=5}
            };

            var source = string.IsNullOrEmpty(name) ? locations : locations
                         .Where(c => c.Name.Contains(name));

            return from location in source
                   orderby location.Distance
                   group location by location.Floor into locationGroup
                   orderby int.Parse(locationGroup.Key.Split('樓')[0])
                   select new Grouping<string, Location>(locationGroup.Key, locationGroup);
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
            List<Grouping<string, Location>> locations = ((IEnumerable<Grouping<string, Location>>)LocationListView.ItemsSource).ToList();
            //scroll to the first location of second group
            LocationListView.ScrollTo(locations[1][0], ScrollToPosition.Start, true);
        }
    }

    // fake data for listview testing
    class Location
    {
        public string Name { get; set; }
        public string Floor { get; set; }
        public double Distance { get; set; }
    }
}
