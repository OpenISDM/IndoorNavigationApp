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
 *      The file contains the Navigator page that all contents and methods are 
 *      binds to the NavigatorPageViewModel fully.
 *      
 * Version:
 *
 *      1.0.0, 20190605
 * 
 * File Name:
 *
 *      NavigatorPage.xaml.cs
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
