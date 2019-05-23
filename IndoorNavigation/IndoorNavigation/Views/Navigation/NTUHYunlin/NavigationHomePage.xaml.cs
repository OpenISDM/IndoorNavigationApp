﻿using System;
using System.Collections.Generic;
using IndoorNavigation.Views.Settings;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace IndoorNavigation.Views.Navigation.NTUHYunlin
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
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

        async void InfoButton_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new SettingTableViewPage());
        }

        void ClinicList_Clicked(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        async void Cashier_Clicked(object sender, EventArgs e)
        {
            // TODO: This destination name can be any location which you want to test.
            // After the navigation graph is done, it will be replaced by the completed function.
            await Navigation.PushAsync(new NavigatorPage("某個名字很長的終點"));
        }

        void ExitList_Clicked(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        void ExaminationRoomList_Clicked(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        void Pharmacy_Clicked(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        void ConvenienceStore_Clicked(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        void OthersList_Clicked(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        void BathroomList_Clicked(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        void BloodCollectionCounter_Clicked(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
