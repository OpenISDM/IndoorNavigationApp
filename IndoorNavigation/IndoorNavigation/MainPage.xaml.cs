using System;
using System.Linq;
using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using IndoorNavigation.Views.Settings;
using IndoorNavigation.Views.Navigation.NTUHYunlin;
using MvvmHelpers;
using System.ComponentModel;
using IndoorNavigation.ViewModels;
using IndoorNavigation.Resources;
using Plugin.Multilingual;
using System.Diagnostics;
using System.Resources;
using IndoorNavigation.Resources.Helpers;
using System.Reflection;
using IndoorNavigation.Modules;

namespace IndoorNavigation
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPage : ContentPage
    {
        MainPageViewModel viewModel;

        const string ResourceId = "IndoorNavigation.Resources.AppResources";
        ResourceManager resmgr = new ResourceManager(ResourceId, typeof(TranslateExtension).GetTypeInfo().Assembly);

        public MainPage()
        {
            InitializeComponent();

            var ci = CrossMultilingual.Current.CurrentCultureInfo;
            NavigationPage.SetBackButtonTitle(this, resmgr.GetString("Home", ci));
            NavigationPage.SetHasBackButton(this, false);

            switch (Device.RuntimePlatform)
            {
                case Device.Android:
                    NaviSearchBar.BackgroundColor = Color.White;
                    break;

                default:
                    break;
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            ((NavigationPage)Application.Current.MainPage).BarBackgroundColor = Color.FromHex("#3F51B5");
            ((NavigationPage)Application.Current.MainPage).BarTextColor = Color.White;

            viewModel = new MainPageViewModel();
            BindingContext = viewModel;

            // This will remove all the pages in the navigation stack excluding the Main Page and another one page
            for (int PageIndex = Navigation.NavigationStack.Count - 2; PageIndex > 0; PageIndex--)
            {
                Navigation.RemovePage(Navigation.NavigationStack[PageIndex]);
            }

            switch (Device.RuntimePlatform)
            {
                case Device.Android:
                    //NavigatorButton.Padding = new Thickness(30, 1, 1, 1);
                    //AbsoluteLayout.SetLayoutBounds(NavigatorButton, new Rectangle(0.5, 0.52, 0.7, 0.1));
                    break;

                case Device.iOS:
                    // customize CurrentInstruction UI for iPhone 5s/SE
                    if (Height < 600)
                    {
                        //WelcomeLabel.FontSize = 36;
                        //BeDISLabel.FontSize = 39;
                        //SloganLabel.Text = "";
                        //AbsoluteLayout.SetLayoutBounds(NavigatorButton, new Rectangle(0.5, 0.47, 0.7, 0.12));
                    }
                    break;

                default:
                    break;
            }
        }

        async void SettingBtn_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new SettingTableViewPage());
        }

        async void Handle_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            if (e.Item is Location location)
            {
                switch (location.Name)
                {
                    case "雲林台大醫院":
                        var answser = await DisplayAlert("Turn to navigation homepage", location.Name, "OK", "Cancel");

                        if (answser)
                            await Navigation.PushAsync(new NavigationHomePage(location.Name));
                        break;

                    default:
                        break;
                }

            }
        }

        void LocationListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            // disable it
            LocationListView.SelectedItem = null;
        }

        void LocationListView_Refreshing(object sender, EventArgs e)
        {
            LocationListView.EndRefresh();
        }

        void Item_Delete(object sender, EventArgs e)
        {
            var item = (Location)((MenuItem)sender).CommandParameter;

            if (item != null)
            {
                NavigraphStorage.DeleteNavigraph(item.Name);
                viewModel.LoadNavigationGraph();
            }
        }
    }
}
