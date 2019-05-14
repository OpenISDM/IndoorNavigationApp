using System;
using System.Collections.Generic;
using IndoorNavigation.Views.Settings;
using Xamarin.Forms;

namespace IndoorNavigation.Views.Navigation.NTUH.Yunlin
{
    public partial class NavigationHomePage : ContentPage
    {
        public NavigationHomePage()
        {
            InitializeComponent();

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

        async void InfoButton_Clicked(object sender, System.EventArgs e)
        {
            await Navigation.PushAsync(new SettingTableViewPage());
        }

        void ClinicList_Clicked(object sender, System.EventArgs e)
        {
            throw new NotImplementedException();
        }

        void Cashier_Clicked(object sender, System.EventArgs e)
        {
            throw new NotImplementedException();
        }

        void ExitList_Clicked(object sender, System.EventArgs e)
        {
            throw new NotImplementedException();
        }

        void ExaminationRoomList_Clicked(object sender, System.EventArgs e)
        {
            throw new NotImplementedException();
        }

        void Pharmacy_Clicked(object sender, System.EventArgs e)
        {
            throw new NotImplementedException();
        }

        void ConvenienceStore_Clicked(object sender, System.EventArgs e)
        {
            throw new NotImplementedException();
        }

        void OthersList_Clicked(object sender, System.EventArgs e)
        {
            throw new NotImplementedException();
        }

        void BathroomList_Clicked(object sender, System.EventArgs e)
        {
            throw new NotImplementedException();
        }

        void BloodCollectionCounter_Clicked(object sender, System.EventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
