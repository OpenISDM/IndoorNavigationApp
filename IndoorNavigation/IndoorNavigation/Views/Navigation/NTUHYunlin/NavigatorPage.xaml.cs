using System;
using System.Collections.Generic;

using Xamarin.Forms;
using IndoorNavigation.ViewModels.Navigation;

namespace IndoorNavigation.Views.Navigation.NTUHYunlin
{
    public partial class NavigatorPage : ContentPage
    {
        private NavigatorPageViewModel viewModel;

        public NavigatorPage(string desination)
        {
            InitializeComponent();

            viewModel = new NavigatorPageViewModel(desination);
            BindingContext = viewModel;
        }

    }
}
