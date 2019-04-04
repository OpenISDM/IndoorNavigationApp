using System;
using System.Collections.Generic;
using IndoorNavigation.Models;
using MvvmHelpers;
using System.Linq;
using System.Collections;
using System.Windows.Input;
using Xamarin.Forms;
using IndoorNavigation.Modules;
using System.Diagnostics;
using IndoorNavigation.Views.Navigation;
using IndoorNavigation.Views.Settings;

namespace IndoorNavigation.ViewModels.Navigation
{
    public class NaviHomePageViewModel : MvvmHelpers.BaseViewModel
    {
        //waypoints from Utility(source)
        private ObservableRangeCollection<WaypointModel> waypoints;
        //waypoints used by search method
        private IEnumerable<WaypointModel> returnedWaypoints;

        public NaviHomePageViewModel()
        {
            Title = "Pick destination";
            waypoints = new ObservableRangeCollection<WaypointModel>();
            returnedWaypoints = new ObservableRangeCollection<WaypointModel>();
            LoadNavigationGraph();
        }

        private async void LoadNavigationGraph()
        {
            if (Utility.Waypoints == null)
            {
                var mainPage = Application.Current.MainPage;
                await mainPage.DisplayAlert("Opps...", "You should pick the navigation graph first", "OK");
                await mainPage.Navigation.PushAsync(new SettingTableViewPage());
            }
            else
            {
                Utility.BeaconScan.StartScan(Utility.BeaconsDict.Keys.ToList());
                waypoints.AddRange(Utility.Waypoints);
                returnedWaypoints = waypoints;
            }
        }

        public IList<Grouping<string, WaypointModel>> GroupWaypoints
        {
            get
            {
                return (from waypoint in returnedWaypoints
                        orderby waypoint.Beacons[0].Floor
                        group waypoint by waypoint.Beacons[0].Floor into waypointGroup
                        orderby waypointGroup.Key
                        select new Grouping<string, WaypointModel>(waypointGroup.Key.ToString(), waypointGroup))
                        .ToList();
            }
        }

        private WaypointModel selectedItem;
        public WaypointModel SelectedItem
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
                    HandleItemSelected(selectedItem);
                    selectedItem = null;
                }
            }
        }

        private async void HandleItemSelected(WaypointModel selectedItem)
        {
            //await Application.Current.MainPage.DisplayAlert("", "", "", "");
            if (selectedItem != null)
            {
                var navigation = Application.Current.MainPage.Navigation;
                await navigation.PushAsync(new NavigationTabbedPage(selectedItem.Name));
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
                                        waypoints : waypoints
                                        .Where(c => c.Name.Contains(value));
                returnedWaypoints = searchedWaypoints;
                OnPropertyChanged("GroupWaypoints");
            }
        }
    }
}
