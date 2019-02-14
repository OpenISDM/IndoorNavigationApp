using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using IndoorNavigation.Modules;
using IndoorNavigation.Models;
using IndoorNavigation.Modules.SignalProcessingAlgorithms;
using IndoorNavigation.Modules.Navigation;

[assembly: XamlCompilation (XamlCompilationOptions.Compile)]
namespace IndoorNavigation
{
	public partial class App : Application
	{
		public App ()
		{
			InitializeComponent();

			MainPage = new NavigationPage(new MainPage());
		}

		protected override void OnStart ()
		{
            // Handle when your app starts
            Utility.Service = new Container();
            Utility.Service.Add<WaypointSignalProcessing>
                ("Default signal process algorithm");
            Utility.Service.Add<WaypointSignalProcessing>
                ("Way point signal processing algorithm");
            Utility.Service.Add<WayPointAlgorithm>("Way point algorithm");

            // Beacon scan api must adjust later, it should regist after
			// navigraph is be loaded.
            Utility.BeaconScanAPI = DependencyService.Get<IBeaconScan>();
            Utility.SignalProcess = new SignalProcessModule();
            Utility.MaN = new MaNModule();
            Utility.IPS = new IPSModule();
        }

		protected override void OnSleep ()
		{
			// Handle when your app sleeps
		}

		protected override void OnResume ()
		{
			// Handle when your app resumes
		}
	}
}
