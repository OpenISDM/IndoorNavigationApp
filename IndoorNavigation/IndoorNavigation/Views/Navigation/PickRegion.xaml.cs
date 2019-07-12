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
 *      This file let the users selects which regions are they. Because we need the region information
 *      to get the possible IPSType and Beacon's UUID.
 *      
 * Version:
 *
 *      1.0.0, 20190712
 * 
 * File Name:
 *
 *     PickRegion.xaml.cs
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
 *      Eric Lee, ericlee@iis.sinica.edu.tw
 *
 */

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using IndoorNavigation.Models.NavigaionLayer;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace IndoorNavigation.Views.Navigation
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PickRegion : ContentPage
    {
        private NavigationGraph _navigationGraph;
        private string _navigationGraphName;
        public ObservableCollection<string> Items { get; set; }
        public ObservableCollection<RegionItem> _regionItems;
        private Guid _destinationWaypointID;
        private Guid _destinationRegionID;
        public string _destinationWaypointName { get; set; }
        public PickRegion(string navigationGraphName,
                           NavigationGraph navigationgraph,
                           Guid destinationRegionID,
                             Guid destinationWaypointID,
                             string destinationWaypointName
                            )
        {
            InitializeComponent();
            _navigationGraphName = navigationGraphName;          
            _regionItems = new ObservableCollection<RegionItem>();
            _navigationGraph = navigationgraph;
            _destinationRegionID = destinationRegionID;
            _destinationWaypointID = destinationWaypointID;
            _destinationWaypointName = destinationWaypointName;
            foreach (KeyValuePair<Guid, IndoorNavigation.Models.Region> pairRegion in navigationgraph.GetRegions())
            {           
                _regionItems.Add(new RegionItem
                {
                    _regionID = pairRegion.Value._id,
                    _floor = pairRegion.Value._floor,
                    _regionName = pairRegion.Value._name
                });                              
            }

            MyListView.ItemsSource = _regionItems;
        }

        async void Handle_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            if (e.Item is RegionItem regionItem)
            {
                Console.WriteLine(">>Handle_ItemTapped in DestinationPickPage");

                await Navigation.PushAsync(new NavigatorPage(_navigationGraphName,                                                         
                                                             regionItem._regionID,
                                                             _destinationRegionID,
                                                             _destinationWaypointID,
                                                             _destinationWaypointName));
            }

            ((ListView)sender).SelectedItem = null;
        }
    }
    public class RegionItem
    {
        public Guid _regionID { get; set; }
        public string _regionName { get; set; }
        public int _floor { get; set; }
    }
}

