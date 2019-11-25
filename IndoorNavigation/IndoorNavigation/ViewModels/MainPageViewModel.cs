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
using Plugin.Multilingual;
using System.Resources;
using IndoorNavigation.Resources.Helpers;
using System.Reflection;
using System;

namespace IndoorNavigation.ViewModels
{
    public class MainPageViewModel : BaseViewModel
    {
        private ObservableRangeCollection<Location> _locations;
        // IEnumerable of Locations which used by search method
        private IEnumerable<Location> _returnedLocations;
        private Location _selectedItem;
        private string _searchedText;
        const string _resourceId = "IndoorNavigation.Resources.AppResources";
        ResourceManager _resourceManager =
            new ResourceManager(_resourceId, typeof(TranslateExtension).GetTypeInfo().Assembly);

        public MainPageViewModel()
        {
            _returnedLocations = new ObservableRangeCollection<Location>();
            LoadNavigationGraph();
        }

        public async void LoadNavigationGraph()
        {
            _locations = new ObservableRangeCollection<Location>();

            var ci = CrossMultilingual.Current.CurrentCultureInfo;

            if (!Application.Current.Properties.ContainsKey("FirstUse"))
            {
                NavigraphStorage.GenerateFileRoute("NTUH Yunlin Branch", "NTUH_YunLin");
                NavigraphStorage.GenerateFileRoute("Taipei City Hall", "Taipei_City_Hall");
                NavigraphStorage.GenerateFileRoute("Yuanlin Christian Hospital", "Yuanlin_Christian_Hospital");
                Application.Current.Properties["FirstUse"] = false;
            }
         

            foreach (string naviGraphName in NavigraphStorage.GetAllNavigationGraphs())
            {
                _locations.Add(new Location { UserNaming = naviGraphName });
            }

            if (_locations.Any())
            {
                NavigationGraphFiles = _locations;
            }
            else
            {
                var currentLanguage = CrossMultilingual.Current.CurrentCultureInfo;
                Page mainPage = Application.Current.MainPage;
                await mainPage.DisplayAlert(
                    _resourceManager.GetString("GO_SETTING_PAGE_STRING", currentLanguage),
                    _resourceManager.GetString("DOWNLOAD_NAVIGATION_GRAPH_STRING", currentLanguage),
                    _resourceManager.GetString("OK_STRING", currentLanguage));
                await mainPage.Navigation.PushAsync(new SettingTableViewPage());
            }
        }

        public IList<Location> NavigationGraphFiles
        {
            get
            {
                return (from location in _returnedLocations
                        orderby location.UserNaming[0]
                        select location).ToList();
            }
            set
            {
                SetProperty(ref _returnedLocations, value);
            }
        }

        public Location SelectedItem
        {
            get
            {
                return _selectedItem;
            }
            set
            {
                if (_selectedItem != value)
                {
                    _selectedItem = value;
                    OnPropertyChanged("SelectedItem");
                    _selectedItem = null;
                }
            }
        }

        public string SearchedText
        {
            get
            {
                return _searchedText;
            }

            set
            {
                _searchedText = value;
                OnPropertyChanged("SearchedText");

                // Search waypoints
                var searchedWaypoints = string.IsNullOrEmpty(value) ?
                                        _locations : _locations
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
