using System;
using System.Collections.Generic;
using System.Linq;
using IndoorNavigation.Modules;
using IndoorNavigation.Views.Settings;
using MvvmHelpers;
using Xamarin.Forms;

namespace IndoorNavigation.ViewModels
{
    public class MainPageViewModel : BaseViewModel
    {
        private ObservableRangeCollection<Location> locations;
        //Locations used by search method
        private IEnumerable<Location> returnedLocations;

        public MainPageViewModel()
        {
            returnedLocations = new ObservableRangeCollection<Location>();
            LoadNavigationGraph();
        }

        public async void LoadNavigationGraph()
        {
            locations = new ObservableRangeCollection<Location>();

            foreach (string naviGraphName in NavigraphStorage.GetAllNavigraphs())
            {
                locations.Add(new Location { Name = naviGraphName });
            }

            if (locations.Any())
            {
                NavigationGraphFiles = locations;
            }
            else
            {
                Page mainPage = Application.Current.MainPage;
                await mainPage.DisplayAlert("Let's go to download the graph", "You should download the navigation graph first", "OK");
                await mainPage.Navigation.PushAsync(new SettingTableViewPage());
            }
        }

        public IList<Location> NavigationGraphFiles
        {
            get
            {
                return (from location in returnedLocations
                        orderby location.Name[0]
                        select location).ToList();
            }

            set
            {
                SetProperty(ref returnedLocations, value);
            }
        }

        private Location selectedItem;
        public Location SelectedItem
        {
            get
            {
                return selectedItem;
            }
            set
            {
                if (selectedItem != value)
                {
                    selectedItem = value;
                    OnPropertyChanged("SelectedItem");
                    selectedItem = null;
                }
            }
        }

        private string searchedText;
        public string SearchedText
        {
            get
            {
                return searchedText;
            }

            set
            {
                searchedText = value;
                OnPropertyChanged("SearchedText");

                //search waypoints
                var searchedWaypoints = string.IsNullOrEmpty(value) ?
                                        locations : locations
                                        .Where(c => c.Name.Contains(value));
                NavigationGraphFiles = searchedWaypoints.ToList();
            }
        }
    }

    public class Location
    {
        public string Name { get; set; }
    }
}
