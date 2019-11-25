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
 *      The file contains the class for the main page that contains the 
 *      listview of locations that are waypoints defined by the navigation 
 *      grash in use.
 *      
 * Version:
 *
 *      1.0.0, 20190605
 * 
 * File Name:
 *
 *      MainPage.xaml.cs
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
 *      Chun-Yu Lai, chunyu1202@gmail.com
 *
 */
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using IndoorNavigation.Views.Settings;
using IndoorNavigation.Views.Navigation;
using IndoorNavigation.ViewModels;
using Plugin.Multilingual;
using System.Resources;
using IndoorNavigation.Resources.Helpers;
using System.Reflection;
using IndoorNavigation.Modules.Utilities;
using IndoorNavigation.Models.NavigaionLayer;
using System.Xml;
using System.IO;
using System.Collections.Generic;

namespace IndoorNavigation
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPage : ContentPage
    {
        MainPageViewModel _viewModel;
        internal static readonly string _versionRoute = "IndoorNavigation.Resources.Map_Version.xml";
        const string _resourceId = "IndoorNavigation.Resources.AppResources";
        internal static readonly string _versionRouteInPhone
             = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Version");
        ResourceManager _resourceManager =
            new ResourceManager(_resourceId, typeof(TranslateExtension).GetTypeInfo().Assembly);
        private bool updateMapOrNot;
        private static PhoneInformation _phoneInformation = new PhoneInformation();
        public MainPage()
        {
            InitializeComponent();

            var currentLanguage = CrossMultilingual.Current.CurrentCultureInfo;
            NavigationPage.SetBackButtonTitle(this, _resourceManager.GetString("HOME_STRING", currentLanguage));
            NavigationPage.SetHasBackButton(this, false);
            updateMapOrNot = false;
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

            _viewModel = new MainPageViewModel();
            BindingContext = _viewModel;

            // This will remove all the pages in the navigation stack excluding the Main Page
            // and another one page
            //Console.WriteLine("NavigationStack : " +Navigation.NavigationStack.Count);
            //for (int PageIndex = Navigation.NavigationStack.Count-2; PageIndex > 0; PageIndex--)
            //{
            //    Console.WriteLine("PageIndex : " +PageIndex);
            //    Navigation.RemovePage(Navigation.NavigationStack[PageIndex]);
            //}

            switch (Device.RuntimePlatform)
            {
                case Device.Android:
                    //NavigatorButton.Padding = new Thickness(30, 1, 1, 1);
                    //AbsoluteLayout.SetLayoutBounds(NavigatorButton,
                    //    new Rectangle(0.5, 0.52, 0.7, 0.1));
                    break;

                case Device.iOS:
                    // customize CurrentInstruction UI for iPhone 5s/SE
                    if (Height < 600)
                    {
                        //WelcomeLabel.FontSize = 36;
                        //BeDISLabel.FontSize = 39;
                        //SloganLabel.Text = "";
                        //AbsoluteLayout.SetLayoutBounds(NavigatorButton,
                        //    new Rectangle(0.5, 0.47, 0.7, 0.12));
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
             var currentLanguage = CrossMultilingual.Current.CurrentCultureInfo;
            if (e.Item is Location location)
            {
                

                // UpdateMap(location.UserNaming, navigationGraph);
                var ci = CrossMultilingual.Current.CurrentCultureInfo;
                //string NTUH_YunLin = _resourceManager.GetString("HOSPITAL_NAME_STRING", ci).ToString();
                //string Lab = _resourceManager.GetString("LAB_STRING", ci).ToString();
               // string loadFileName="";
                string map = _phoneInformation.GiveCurrentMapName(location.UserNaming);
                
                NavigationGraph navigationGraph = NavigraphStorage.LoadNavigationGraphXML(map);

                XmlDocument xmlDocument = new XmlDocument();
                using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(_versionRoute))
                {
                    StreamReader tr = new StreamReader(stream);
                    string fileContents = tr.ReadToEnd();
                    xmlDocument.LoadXml(fileContents);
                }


                ReadVersion readVersion = new ReadVersion(xmlDocument);
                double newVersion = readVersion.ReturnVersion(navigationGraph.GetBuildingName());
                if (navigationGraph.GetVersion() != newVersion)
                {
                    //var currentLanguage = CrossMultilingual.Current.CurrentCultureInfo;
                    var answser = await DisplayAlert(
                                _resourceManager.GetString("UPDATE_MAP_STRING", currentLanguage),
                                location.UserNaming, _resourceManager.GetString("OK_STRING", currentLanguage),
                                _resourceManager.GetString("CANCEL_STRING", currentLanguage));
                    
                    
                    if (answser)
                    {

                        List<string> generateName = _phoneInformation.GiveGenerateMapName(location.UserNaming);

                        NavigraphStorage.GenerateFileRoute(generateName[0], generateName[1]);
                        updateMapOrNot = true;
                    }
                    else
                    {

                        updateMapOrNot = false;
                    }
                }
                else
                {
                    updateMapOrNot = true;
                }

                if(updateMapOrNot == true)
                {
                    switch (navigationGraph.GetIndustryServer())
                    {
                        case "hospital":
                            await Navigation.PushAsync(new NavigationHomePage(location.UserNaming));
   
                            break;

                        case "city_hall":
                            await Navigation.PushAsync(new CityHallHomePage(location.UserNaming));
                            
                            break;

                        default:
                            Console.WriteLine("Unknown _industryService");
                            break;
                    }
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
            var ci = CrossMultilingual.Current.CurrentCultureInfo;
            string NTUH_YunLin = _resourceManager.GetString("HOSPITAL_NAME_STRING", ci).ToString();
            string Taipei_City_Hall = _resourceManager.GetString("TAIPEI_CITY_HALL_STRING", ci).ToString();
            string Lab = _resourceManager.GetString("LAB_STRING", ci).ToString();
            string Yuanlin_Christian_Hospital = _resourceManager.GetString("YUANLIN_CHRISTIAN_HOSPITAL_STRING", ci).ToString();
            string loadFileName = "";

            if (item.UserNaming == NTUH_YunLin)
            {
                loadFileName = "NTUH Yunlin Branch";
            }
            else if (item.UserNaming == Taipei_City_Hall)
            {
                loadFileName = "Taipei City Hall";
            }
            else if (item.UserNaming == Lab)
            {
                loadFileName = "Lab";
            }
            else if (item.UserNaming == Yuanlin_Christian_Hospital)
            {
                loadFileName = "Yuanlin Christian Hospital";
            }
            if (item != null)
            {
                NavigraphStorage.DeleteNavigationGraph(loadFileName);
                _viewModel.LoadNavigationGraph();
            }
        }
        
    }
}
