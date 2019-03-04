using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xamarin.Forms;

namespace IndoorNavigation.Views.Navigation
{
    public partial class NavigationTabbedPage : ContentPage
    {
        public NavigationTabbedPage()
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
        }

        void NavigationTab_Tapped(object sender, EventArgs e)
        {
            var page = new TabbedPageNavigation();
            TabbedContentView.Content = page.Content;
            BindingContext = page;
            Title = "Navigation";

            NavigationTabImage.Source = "tabitem1_navigator_tabbed.png";
            NavigationTabLabel.TextColor = Color.FromHex("#009FCC");

            //recover other item pic and color
            RoutesTabImage.Source = "tabitem2_routes.png";
            RoutesTabLabel.TextColor = Color.FromHex("#808080");
            SettingTabImage.Source = "tabitem3_setting.png";
            SettingTabLabel.TextColor = Color.FromHex("#808080");
        }

        void RoutesTab_Tapped(object sender, EventArgs e)
        {
            var page = new TabbedPageRoutes();
            TabbedContentView.Content = page.Content;
            BindingContext = page;
            Title = "Routes";

            RoutesTabImage.Source = "tabitem2_routes_tabbed.png";
            RoutesTabLabel.TextColor = Color.FromHex("#009FCC");

            //recover other item pic and color
            NavigationTabImage.Source = "tabitem1_navigator.png";
            NavigationTabLabel.TextColor = Color.FromHex("#808080");
            SettingTabImage.Source = "tabitem3_setting.png";
            SettingTabLabel.TextColor = Color.FromHex("#808080");
        }

        void SettingTab_Tapped(object sender, EventArgs e)
        {
            var page = new TabbedPageSetting();
            TabbedContentView.Content = page.Content;
            BindingContext = page;
            Title = "Setting";

            SettingTabImage.Source = "tabitem3_setting_tabbed.png";
            SettingTabLabel.TextColor = Color.FromHex("#009FCC");

            //recover other item pic and color
            NavigationTabImage.Source = "tabitem1_navigator.png";
            NavigationTabLabel.TextColor = Color.FromHex("#808080");
            RoutesTabImage.Source = "tabitem2_routes.png";
            RoutesTabLabel.TextColor = Color.FromHex("#808080");
        }
    }
}
