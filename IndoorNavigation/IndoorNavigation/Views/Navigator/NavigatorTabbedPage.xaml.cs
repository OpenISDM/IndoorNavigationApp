using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace IndoorNavigation.Views.Navigator
{
    public partial class NavigatorTabbedPage : ContentPage
    {
        public NavigatorTabbedPage()
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
    }
}
