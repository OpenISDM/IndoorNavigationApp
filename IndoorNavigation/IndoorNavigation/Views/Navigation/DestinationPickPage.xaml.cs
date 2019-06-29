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
 *      The file contains the class for the page that displays the names of 
 *      destinations of all categories of the user is to reflect a destination 
 *      from the displayed list.
 *      
 * Version:
 *
 *      1.0.0, 20190605
 * 
 * File Name:
 *
 *      DestinationPickPage.xaml.cs
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
 *      Chun-Yu Lai, chunyu1202@gmail.com
 *
 */
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xamarin.Forms;
using MvvmHelpers;
using IndoorNavigation.Models.NavigaionLayer;
using System.Linq;
using IndoorNavigation.Modules.Utilities;
using Plugin.Multilingual;
using System.Resources;
using IndoorNavigation.Resources.Helpers;
using System.Reflection;
using IndoorNavigation.Models;


namespace IndoorNavigation.Views.Navigation
{
    public partial class DestinationPickPage : ContentPage
    {
        private string _navigationGraphName;
        private NavigationGraph _navigationGraph;
		public ResourceManager _resourceManager;

		public ObservableCollection<string> _items { get; set; }
        public ObservableCollection<DestinationItem> _destinationItems { get; set; }

        public DestinationPickPage(string navigationGraphName, CategoryType category)
        {
            InitializeComponent();

			const string resourceId = "IndoorNavigation.Resources.AppResources";
			_resourceManager = new ResourceManager(resourceId, typeof(TranslateExtension).GetTypeInfo().Assembly);

			_destinationItems = new ObservableCollection<DestinationItem>();

            _navigationGraphName = navigationGraphName;

            _navigationGraph = NavigraphStorage.LoadNavigationGraphXML(navigationGraphName);

            foreach (KeyValuePair<Guid, IndoorNavigation.Models.Region> pairRegion in _navigationGraph.GetResions())
            {
                string floorName = pairRegion.Value._floor.ToString() + " "
                    + _resourceManager.GetString("FLOOR_STRING",
                                                 CrossMultilingual.Current.CurrentCultureInfo);

                if (pairRegion.Value._waypointsByCategory.ContainsKey(category))
                {
                    foreach (Waypoint waypoint in pairRegion.Value._waypointsByCategory[category])
                    {
                        _destinationItems.Add(new DestinationItem
                        {
                            _regionID = pairRegion.Key,
                            _waypointID = waypoint._id,
                            _waypointName = waypoint._name,
                            _floor = floorName
                        }) ;
                    }
                }
            }

            MyListView.ItemsSource = from waypoint in _destinationItems
                                     group waypoint by waypoint._floor into waypointGroup
                                     orderby waypointGroup.Key
                                     select new Grouping<string, DestinationItem>(waypointGroup.Key,
                                                                               waypointGroup);
        }

        async void Handle_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            if (e.Item is DestinationItem destination)
            {
                Console.WriteLine(">>Handle_ItemTapped in DestinationPickPage");

                Guid sourceRegionID = new Guid("11111111-1111-1111-1111-111111111111");
                await Navigation.PushAsync(new NavigatorPage(_navigationGraphName,
                                                             sourceRegionID,
                                                             destination._regionID,
                                                             destination._waypointID,
                                                             destination._waypointName));
                Console.WriteLine("<<Handle_ItemTapped in DestinationPickPage");
            }

            //Deselect Item
            ((ListView)sender).SelectedItem = null;
        }

    }

    public class DestinationItem
    {
        public Guid _regionID { get; set; }
        public Guid _waypointID { get; set; }
        public string _waypointName { get; set; }
        public string _floor { get; set; }
    }

}
