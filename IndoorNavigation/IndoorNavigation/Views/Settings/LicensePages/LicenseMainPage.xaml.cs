﻿/*
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
 *      The file contains the code behind for the App class. The code is 
 *      responsible for instantiating the first page that will be displayed by
 *      the application on each platform, and for handling application 
 *      lifecycle events. Both App.xaml and App.xaml.cs contribute to a class 
 *      named App that derives from Application. This is only one entry point 
 *      when the app launch at first time.
 *      
 * Version:
 *
 *      1.0.0, 20190605
 * 
 * File Name:
 *
 *      LicenseMainPage.xaml.cs
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
 *      Kenneth Tang, kenneth@gm.nssh.ntpc.edu.tw
 *      Paul Chang, paulchang@iis.sinica.edu.tw
 *
 */
using Xamarin.Forms;
using Plugin.Multilingual;
using System.Resources;
using IndoorNavigation.Resources.Helpers;
using System.Reflection;

namespace IndoorNavigation.Views.Settings.LicensePages
{
    public partial class LicenseMainPage : ContentPage
    {
        const string _resourceId = "IndoorNavigation.Resources.AppResources";
        ResourceManager _resourceManager =
            new ResourceManager(_resourceId, typeof(TranslateExtension).GetTypeInfo().Assembly);

        public LicenseMainPage()
        {
            InitializeComponent();
            var currentLanguage = CrossMultilingual.Current.CurrentCultureInfo;
            NavigationPage.SetBackButtonTitle(this,
                _resourceManager.GetString("LICENSE_STRING", currentLanguage));
        }

        async void IconsLicenseBtn_Tapped(object sender, System.EventArgs e)
        {
            await Navigation.PushAsync(new IconsLicensePage());
        }
    }
}
