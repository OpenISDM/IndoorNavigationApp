/*
 * Copyright (c) 2019 Academia Sinica, Institude of Information Science
 *
 * License:
 *      GPL 3.0 : The content of this file is subject to the terms and
 *      conditions defined in file 'COPYING.txt', which is part of this source
 *      code package.
 *
 * Project Name:
 *
 *      IndoorNavigation
 *
 * File Description:
 * 
 *      This view model implements properties and commands for MainPage
 *      can bind the data.
 *      It will display the list of locations according to the Navigation 
 *      graph in phone's storage which user has downloaded.
 *      
 * Version:
 *
 *      1.0.0, 20190605
 * 
 * File Name:
 *
 *      MainPageViewModel.cs
 *
 * Abstract:
 *
 *      Waypoint-based navigator is a mobile Bluetooth navigation application
 *      that runs on smart phones. It is structed to support anywhere 
 *      navigation indoors in areas covered by different indoor positioning 
 *      system (IPS) and outdoors covered by GPS.In particilar, it can rely on
 *      BeDIS (Building/environment Data and Information System) for indoor 
 *      positioning. This IPS provides a location beacon at every waypoint. The 
 *      beacon brocasts its own coordinates; Consequesntly, the navigator does 
 *      not need to continuously monitor its own position.
 *      This version makes use of Xamarin.Forms, which is a cross-platform UI 
 *      tookit that runs on both iOS and Android.
 *
 * Authors:
 *
 *      Paul Chang, paulchang@iis.sinica.edu.tw
 *      
 */
using System.Collections.Generic;
using System.Linq;
using IndoorNavigation.Modules.Utilities;
using IndoorNavigation.Views.Settings;
using MvvmHelpers;
using Xamarin.Forms;

namespace IndoorNavigation.ViewModels
{
    public class MainPageViewModel : BaseViewModel
    {
        private ObservableRangeCollection<Location> locations;
        // IEnumerable of Locations which used by search method
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
                locations.Add(new Location { UserNaming = naviGraphName });
            }

            if (locations.Any())
            {
                NavigationGraphFiles = locations;
            }
            else
            {
                Page mainPage = Application.Current.MainPage;
                await mainPage.DisplayAlert("Go to the Setting page",
                    "You should download the navigation graph first", "OK");
                await mainPage.Navigation.PushAsync(new SettingTableViewPage());
            }
        }

        public IList<Location> NavigationGraphFiles
        {
            get
            {
                return (from location in returnedLocations
                        orderby location.UserNaming[0]
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

                // Search waypoints
                var searchedWaypoints = string.IsNullOrEmpty(value) ?
                                        locations : locations
                                        .Where(c => c.UserNaming.Contains(value));
                NavigationGraphFiles = searchedWaypoints.ToList();
            }
        }
    }

    public class Location
    {
        public string UserNaming { get; set; }
    }
}
