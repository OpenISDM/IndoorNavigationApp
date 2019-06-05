using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xamarin.Forms;
using MvvmHelpers;
using IndoorNavigation.Modules;
using IndoorNavigation.Models.NavigaionLayer;
using System.Linq;
using System.Diagnostics;
using IndoorNavigation.Modules.Utilities;

namespace IndoorNavigation.Views.Navigation
{
    public partial class DestinationPickPage : ContentPage
    {
        private string _navigraphName;

        public ObservableCollection<string> Items { get; set; }
        public ObservableCollection<DestinationItem> DestinationItems { get; set; }

        public DestinationPickPage(string navigraphName, CategoryType category)
        {
            InitializeComponent();

            DestinationItems = new ObservableCollection<DestinationItem>();

            _navigraphName = navigraphName;

            IEnumerable<Waypoint> waypoints = from region in NavigraphStorage.LoadNavigraphXML(navigraphName).Regions
                                              from waypoint in region.Waypoints
                                              where waypoint.Category.Equals(category)
                                              select waypoint;

            foreach (Waypoint waypoint in waypoints)
            {
                DestinationItems.Add(new DestinationItem
                {
                    ID = waypoint.ID,
                    WaypointName = waypoint.Name,
                    Floor = waypoint.Floor
                });
            }

            MyListView.ItemsSource = from waypoint in DestinationItems
                                     group waypoint by waypoint.Floor into waypointGroup
                                     orderby waypointGroup.Key
                                     select new Grouping<int, DestinationItem>(waypointGroup.Key, waypointGroup);
        }

        async void Handle_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            if (e.Item is DestinationItem destination)
            {
                await Navigation.PushAsync(new NavigatorPage(_navigraphName, destination.WaypointName, destination.ID));
            }

            //Deselect Item
            ((ListView)sender).SelectedItem = null;
        }

    }

    public class DestinationItem
    {
        public Guid ID { get; set; }
        public string WaypointName { get; set; }
        public int Floor { get; set; }
    }

}
