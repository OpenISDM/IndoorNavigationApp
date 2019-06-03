using System;

using Xamarin.Forms;
using IndoorNavigation.ViewModels.Navigation;

namespace IndoorNavigation.Views.Navigation
{
    public partial class NavigatorPage : ContentPage
    {
        private NavigatorPageViewModel viewModel;

        public NavigatorPage(string navigraphName, string desination)
        {
            InitializeComponent();

            // TODO: Temporarily test
            viewModel = new NavigatorPageViewModel(navigraphName, desination, new Guid("00000000-0000-0000-0000-000000000001"));
            BindingContext = viewModel;
        }

    }
}
