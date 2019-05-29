using System;
using System.Collections.Generic;

using Xamarin.Forms;
using IndoorNavigation.ViewModels.Navigation;

namespace IndoorNavigation.Views.Navigation.NTUHYunlin
{
    public partial class NavigatorPage : ContentPage
    {
        private NavigatorPageViewModel viewModel;

        public NavigatorPage(string navigraphName, string desination)
        {
            InitializeComponent();

            viewModel = new NavigatorPageViewModel(navigraphName, desination);
            BindingContext = viewModel;
        }

    }
}
