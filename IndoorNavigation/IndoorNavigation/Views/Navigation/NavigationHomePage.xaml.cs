/*
 * Copyright (c) 2019 Academia Sinica, Institude of Information Science
 *
 * License:
 *      GPL 3.0 : The content of this file is subject to the terms and
 *      conditions defined in file 'COPYING.txt', which is part of this source
 *      code package.
 *
 * Project Name:
 *
 *      IndoorNavigation
 *
 * File Description:
 *
 *      The file contains the class for the homepage. The page that displays 
 *      the items of all category of the waypoints and navigates to the 
 *      DestinationPickPage which contains the listview of waypoints with the 
 *      specified category when user click.
 *      
 * Version:
 *
 *      1.0.0, 20190605
 * 
 * File Name:
 *
 *      NavigationHomePage.xaml.cs
 *
 * Abstract:
 *
 *      Waypoint-based navigator is a mobile Bluetooth navigation application
 *      that runs on smart phones. It is structed to support anywhere 
 *      navigation indoors in areas covered by different indoor positioning 
 *      system (IPS) and outdoors covered by GPS.In particilar, it can rely on
 *      BeDIS (Building/environment Data and Information System) for indoor 
 *      positioning. This IPS provides a location beacon at every waypoint. The 
 *      beacon brocasts its own coordinates; Consequesntly, the navigator does 
 *      not need to continuously monitor its own position.
 *      This version makes use of Xamarin.Forms, which is a cross-platform UI 
 *      tookit that runs on both iOS and Android.
 *
 * Authors:
 *
 *      Paul Chang, paulchang@iis.sinica.edu.tw
 *
 */
using System;
using Xamarin.Forms;
using MvvmHelpers;
using Xamarin.Forms.Xaml;
using IndoorNavigation.Models.NavigaionLayer;
using Plugin.Multilingual;
using System.Resources;
using IndoorNavigation.Resources.Helpers;
using System.Reflection;
using System.Collections.ObjectModel;
using System.Collections;

namespace IndoorNavigation.Views.Navigation
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class NavigationHomePage : ContentPage
    {
        const string _resourceId = "IndoorNavigation.Resources.AppResources";
        ResourceManager _resourceManager =
            new ResourceManager(_resourceId, typeof(TranslateExtension).GetTypeInfo().Assembly);
        private string _navigationGraphName;
        public string myProperty { get; } = "";
        public NavigationHomePage(string navigationGraphName)
        {
            InitializeComponent();

            var currentLanguage = CrossMultilingual.Current.CurrentCultureInfo;
            myProperty = navigationGraphName;
            BindingContext = this;
            this._navigationGraphName = navigationGraphName;
            NavigationPage.SetBackButtonTitle(this, _resourceManager.GetString("BACK_STRING", currentLanguage));
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
            await Navigation.PushAsync(new DestinationPickPage(_navigationGraphName,
                                                               CategoryType.Clinics));
        }

        async void Cashier_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new DestinationPickPage(_navigationGraphName,
                                                               CategoryType.Cashier));
        }

        async void ExitList_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new DestinationPickPage(_navigationGraphName,
                                                               CategoryType.Exit));
        }

        async void ExaminationRoomList_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new DestinationPickPage(_navigationGraphName,
                                                               CategoryType.ExaminationRoom));
        }

        async void Pharmacy_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new DestinationPickPage(_navigationGraphName,
                                                               CategoryType.Pharmacy));
        }

        async void ConvenienceStore_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new DestinationPickPage(_navigationGraphName,
                                                               CategoryType.ConvenienceStore));
        }

        async void OthersList_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new DestinationPickPage(_navigationGraphName,
                                                               CategoryType.Others));
        }

        async void BathroomList_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new DestinationPickPage(_navigationGraphName,
                                                               CategoryType.Bathroom));
        }

        async void BloodCollectionCounter_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new DestinationPickPage(_navigationGraphName,
                                                               CategoryType.BloodCollectionCounter));
        }
    }
}