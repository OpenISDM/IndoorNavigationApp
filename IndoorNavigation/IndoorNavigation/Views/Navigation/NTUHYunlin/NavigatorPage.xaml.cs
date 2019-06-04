using System;

using Xamarin.Forms;
using IndoorNavigation.ViewModels.Navigation;

namespace IndoorNavigation.Views.Navigation
{
    public partial class NavigatorPage : ContentPage
    {
        private NavigatorPageViewModel viewModel;

        public NavigatorPage(string navigraphName, string desinationName, Guid destinationID)
        {
            InitializeComponent();

            viewModel = new NavigatorPageViewModel(navigraphName, desinationName, destinationID);
            BindingContext = viewModel;
        }

    }
}
