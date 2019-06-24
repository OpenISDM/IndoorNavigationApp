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
using IndoorNavigation.Resources;
using System.Resources;
using IndoorNavigation.Resources.Helpers;
using System.Reflection;

namespace IndoorNavigation.Views.Navigation
{
    public partial class DestinationPickPage : ContentPage
    {
        private string _navigraphName;
		public ResourceManager _resourceManager;
		public ObservableCollection<string> _items { get; set; }
        public ObservableCollection<DestinationItem> _destinationItems { get; set; }

        public DestinationPickPage(string navigraphName, CategoryType category)
        {
            InitializeComponent();

			const string resourceId = "IndoorNavigation.Resources.AppResources";
			_resourceManager = new ResourceManager(resourceId, typeof(TranslateExtension).GetTypeInfo().Assembly);

			_destinationItems = new ObservableCollection<DestinationItem>();

            _navigraphName = navigraphName;

            IEnumerable<Waypoint> waypoints = from region in NavigraphStorage.
                                              LoadNavigraphXML(navigraphName).Regions
                                              from waypoint in region.Waypoints
                                              where waypoint.Category.Equals(category)
                                              select waypoint;

            foreach (Waypoint waypoint in waypoints)
            {
                string FloorName = waypoint.Floor.ToString()+" "+ _resourceManager.GetString("FLOOR_STRING", CrossMultilingual.Current.CurrentCultureInfo);
				_destinationItems.Add(new DestinationItem
				{
					ID = waypoint.ID,
					WaypointName = waypoint.Name,
					Floor = FloorName
				});
            }
            
            MyListView.ItemsSource = from waypoint in _destinationItems
                                     group waypoint by waypoint.Floor into waypointGroup
                                     orderby waypointGroup.Key
                                     select new Grouping<string, DestinationItem>(waypointGroup.Key,
                                                                               waypointGroup);
        }

        async void Handle_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            if (e.Item is DestinationItem destination)
            {
                Console.WriteLine("start of Handle_ItemTapped in DestinationPickPage");
                await Navigation.PushAsync(new NavigatorPage(_navigraphName,
                                                             destination.WaypointName,
                                                             destination.ID));
                Console.WriteLine("end of Handle_ItemTapped in DestinationPickPage");
            }

            //Deselect Item
            ((ListView)sender).SelectedItem = null;
        }

    }

    public class DestinationItem
    {
        public Guid ID { get; set; }
        public string WaypointName { get; set; }
        //public int Floor { get; set; }
        public string Floor { get; set; }
    }

}
