using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using IndoorNavigation.Modules;
using IndoorNavigation.Models;
using IndoorNavigation.Modules.SignalProcessingAlgorithms;
using IndoorNavigation.Modules.Navigation;

using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using IndoorNavigation.Resources;
using Plugin.Multilingual;
using IndoorNavigation.Views.Settings;

[assembly: XamlCompilation (XamlCompilationOptions.Compile)]
namespace IndoorNavigation
{
    public partial class App : Application
    {

        public App()
        {
            InitializeComponent();

            // Get the current device language setting
            AppResources.Culture = CrossMultilingual.Current.DeviceCultureInfo;
            if (AppResources.Culture.ToString().Contains("zh"))
            {
                Current.Properties["LanguagePicker"] = "Chinese";
            }
            else if (AppResources.Culture.ToString().Contains("en"))
            {
                Current.Properties["LanguagePicker"] = "English";
            }

            MainPage = new NavigationPage(new MainPage());
        }

        protected override void OnStart()
        {
            AppCenter.Start("ios=3efcee27-6067-4f41-a94f-87c97d6b8118;" +
             "android=8cc03d85-0d94-4cec-b4b6-808719a60857",
             typeof(Analytics), typeof(Crashes));

            // Handle when your app starts
            Utility.Service = new Container();
            Utility.Service.Add<WaypointSignalProcessing>
             ("Default signal process algorithm");
            Utility.Service.Add<WaypointSignalProcessing>
             ("Waypoint signal processing algorithm");
            Utility.Service.Add<WaypointAlgorithm>("Waypoint algorithm");

            // Beacon scan api must adjust later, it should regist after
            // navigraph is be loaded.
            // 由於android尚未實作beacon scanner的功能，如要在android上偵錯，請註解以下代碼
            Utility.BeaconScan = DependencyService.Get<IBeaconScan>();
            Utility.TextToSpeech = DependencyService.Get<ITextToSpeech>();
            //Utility.SignalProcess = new SignalProcessModule();
            //Utility.MaN = new MaNModule();
            //Utility.IPS = new IPSModule();
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
