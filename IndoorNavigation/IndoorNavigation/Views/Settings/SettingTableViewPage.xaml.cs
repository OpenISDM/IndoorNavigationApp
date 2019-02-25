using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using IndoorNavigation.Views.Settings.LicensePages;
using Xamarin.Forms;
using IndoorNavigation.Models;
using Xamarin.Essentials;
using Rg.Plugins.Popup.Services;
using IndoorNavigation.Views.PopUpPage;
using IndoorNavigation.Modules;

namespace IndoorNavigation.Views.Settings
{
    public partial class SettingTableViewPage : ContentPage
    {
        private DownloadPopUpPage downloadPage = new DownloadPopUpPage();

        // sample of TextPickerCell(選擇圖資, ref: https://github.com/muak/AiForms.SettingsView#textpickercell)
        public IList NaviGraphItems { get; } = new ObservableCollection<string>(NavigraphStorage.GetAllPlace());

        public SettingTableViewPage()
        {
            InitializeComponent();

            BindingContext = this;

            ((NavigationPage)Application.Current.MainPage).BarBackgroundColor = Color.FromHex("#009FCC");
            ((NavigationPage)Application.Current.MainPage).BarTextColor = Color.White;
            
            //NaviGraphItems.Add("雲林醫院");
            //NaviGraphItems.Add("中研院");

        }

        async void LicenseBtn_Tapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new LicenseMainPage());
        }

        async void DownloadMapBtn_Tapped(object sender, EventArgs e)
        {
            IQrCodeDecoder qrCodeDecoder = DependencyService.Get<IQrCodeDecoder>();
            string qrCodeValue = await qrCodeDecoder.ScanAsync();
            if ((qrCodeValue.Substring(0, 7) == "http://") || (qrCodeValue.Substring(0, 8) == "https://"))
            {
                string[] buffer = qrCodeValue.Split('@');
                if (buffer[buffer.Length-1] == "OpenISDM")
                {
                    downloadPage.DownloadURL = buffer[0];
                    await PopupNavigation.Instance.PushAsync(downloadPage);
                }
                else
                {
                    bool answer = await DisplayAlert("通知", "是否開啟網頁", "Yes", "No");
                    if (answer)
                        await Browser.OpenAsync(qrCodeValue, BrowserLaunchMode.SystemPreferred);
                }
            }
            else
            {
                await DisplayAlert("QrCode內容", qrCodeValue, "OK");
            }
        }
    }
}
