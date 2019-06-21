using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using IndoorNavigation.Modules;
using IndoorNavigation.Models;
using System.Collections.Generic;

[assembly: XamlCompilation (XamlCompilationOptions.Compile)]
namespace IndoorNavigation
{
	public partial class App : Application
	{
		public App ()
		{
			InitializeComponent();

			MainPage = new MainPage();
		}

		protected override void OnStart ()
		{
            // Handle when your app starts
            Utility.SignalProcess = new SignalProcessModule();
            var SendSignalFunction = new Action<List<BeaconSignalModel>>
                (Utility.SignalProcess.AddSignal);
            DependencyService.Get<IBeaconScan>().Init(SendSignalFunction);
        }

		protected override void OnSleep ()
		{
			// Handle when your app sleeps
		}

<<<<<<< HEAD
		protected override void OnResume ()
		{
			// Handle when your app resumes
		}
	}
=======
            // Beacon scan api must adjust later, it should regist after
            // navigraph is be loaded.
            // 由於android尚未實作beacon scanner的功能，如要在android上偵錯，請註解以下代碼
            Utility.BeaconScan = DependencyService.Get<IBeaconScan>();
            Utility.TextToSpeech = DependencyService.Get<ITextToSpeech>();
            //Utility.SignalProcess = new SignalProcessModule();
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
>>>>>>> parent of 2749c0a... Merge pull request #7 from OpenISDM/develop
}
