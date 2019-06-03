using System;
using System.Collections.Generic;
using IndoorNavigation.Views.Settings;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using IndoorNavigation.Models.NavigaionLayer;

namespace IndoorNavigation.Views.Navigation.NTUHYunlin
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class NavigationHomePage : ContentPage
    {
        private string navigraphName;

        public NavigationHomePage(string navigraphName)
        {
            InitializeComponent();

            this.navigraphName = navigraphName;

            NavigationPage.SetBackButtonTitle(this, "返回");

            ((NavigationPage)Application.Current.MainPage).BarBackgroundColor = Color.FromHex("#3F51B5");
            ((NavigationPage)Application.Current.MainPage).BarTextColor = Color.White;

            switch (Device.RuntimePlatform)
            {
                case Device.Android:
                    break;

                case Device.iOS:
                    // customize CurrentInstruction UI for iPhone 5s/SE
                    if (Height < 600)
                    {

                    }
                    break;

                default:
                    break;
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
        }

        async void InfoButton_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new NavigatorSettingPage());
        }

        async void ClinicList_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new DestinationPickPage(navigraphName, 
                    CategoryType.Clinics));
        }

        async void Cashier_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new DestinationPickPage(navigraphName, 
                    CategoryType.Cashier));
        }

        async void ExitList_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new DestinationPickPage(navigraphName, 
                    CategoryType.Exit));
        }

        async void ExaminationRoomList_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new DestinationPickPage(navigraphName, 
                    CategoryType.ExaminationRoom));
        }

        async void Pharmacy_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new DestinationPickPage(navigraphName, 
                    CategoryType.Pharmacy));
        }

        async void ConvenienceStore_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new DestinationPickPage(navigraphName, 
                    CategoryType.ConvenienceStore));
        }

        async void OthersList_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new DestinationPickPage(navigraphName, 
                    CategoryType.Others));
        }

        async void BathroomList_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new DestinationPickPage(navigraphName, 
                    CategoryType.Bathroom));
        }

        async void BloodCollectionCounter_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new DestinationPickPage(navigraphName, 
                    CategoryType.BloodCollectionCounter));
        }
    }
}
