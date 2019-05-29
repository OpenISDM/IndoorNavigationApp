using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xamarin.Forms;
using System.Diagnostics;
using IndoorNavigation.ViewModels.Navigation;

namespace IndoorNavigation.Views.Navigation
{
    public partial class NavigationTabbedPage : ContentPage
    {
        TabbedNaviViewModel tabbedNaviViewModel;

        public NavigationTabbedPage(string Destination)
        {
            InitializeComponent();

            switch (Device.RuntimePlatform)
            {
                case Device.Android:
                    break;

                case Device.iOS:

                    break;

                default:
                    break;
            }

            tabbedNaviViewModel = new TabbedNaviViewModel(Destination);
            tabbedPageNavigation = new TabbedPageNavigation();
            tabbedPageRoutes = new TabbedPageRoutes();

            NavigationTab_Tapped(this, new EventArgs());
        }

        TabbedPageNavigation tabbedPageNavigation;
        void NavigationTab_Tapped(object sender, EventArgs e)
        {
            TabbedContentView.Content = tabbedPageNavigation.Content;
            BindingContext = tabbedNaviViewModel;
            Title = "Navigation";

            NavigationTabImage.Source = "tabitem1_navigator_tabbed.png";
            NavigationTabLabel.TextColor = Color.FromHex("#009FCC");

            //recover other item's image and color
            RoutesTabImage.Source = "tabitem2_routes.png";
            RoutesTabLabel.TextColor = Color.FromHex("#808080");
            SettingTabImage.Source = "tabitem3_setting.png";
            SettingTabLabel.TextColor = Color.FromHex("#808080");
        }

        TabbedPageRoutes tabbedPageRoutes;
        void RoutesTab_Tapped(object sender, EventArgs e)
        {
            TabbedContentView.Content = tabbedPageRoutes.Content;
            Title = "Routes";

            RoutesTabImage.Source = "tabitem2_routes_tabbed.png";
            RoutesTabLabel.TextColor = Color.FromHex("#009FCC");

            //recover other item's image and color
            NavigationTabImage.Source = "tabitem1_navigator.png";
            NavigationTabLabel.TextColor = Color.FromHex("#808080");
            SettingTabImage.Source = "tabitem3_setting.png";
            SettingTabLabel.TextColor = Color.FromHex("#808080");
        }

        void SettingTab_Tapped(object sender, EventArgs e)
        {
            var page = new NavigatorSettingPage();
            TabbedContentView.Content = page.Content;
            //BindingContext = page;
            Title = "Setting";

            SettingTabImage.Source = "tabitem3_setting_tabbed.png";
            SettingTabLabel.TextColor = Color.FromHex("#009FCC");

            //recover other item's image and color
            NavigationTabImage.Source = "tabitem1_navigator.png";
            NavigationTabLabel.TextColor = Color.FromHex("#808080");
            RoutesTabImage.Source = "tabitem2_routes.png";
            RoutesTabLabel.TextColor = Color.FromHex("#808080");
        }
    }
}
