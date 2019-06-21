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
 *      This file contains the class for the settingpage that includes download/
 *      delete navigation graph, language selection and feedback feature.
 *      
 * Version:
 *
 *      1.0.0, 20190605
 * 
 * File Name:
 *
 *      SettingTableViewPage.xaml.cs
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
using System;
using System.Collections;
using System.Collections.ObjectModel;
using IndoorNavigation.Views.Settings.LicensePages;
using Xamarin.Forms;
using IndoorNavigation.Models;
using Xamarin.Essentials;
using Rg.Plugins.Popup.Services;
using IndoorNavigation.Views.PopUpPage;
using IndoorNavigation.Modules;
using System.Threading.Tasks;
using System.Windows.Input;
using Prism.Commands;
using Plugin.Multilingual;
using IndoorNavigation.Resources;
using System.Globalization;
using Plugin.Permissions.Abstractions;
using Plugin.Permissions;
using System.Diagnostics;
using IndoorNavigation.Modules.Utilities;
using System.Resources;
using IndoorNavigation.Resources.Helpers;
using System.Reflection;

namespace IndoorNavigation.Views.Settings
{
    public partial class SettingTableViewPage : ContentPage
    {
        private DownloadPopUpPage downloadPage = new DownloadPopUpPage();
        private string downloadURL;

        public IList SelectNaviGraphItems { get; } = new ObservableCollection<string>();
        public IList CleanNaviGraphItems { get; } = new ObservableCollection<string>();
        public IList LanguageItems { get; } = new ObservableCollection<string>();
        //public ICommand SelectedMapCommand => new DelegateCommand(HandleSelectedMap);
        public ICommand CleanMapCommand => new DelegateCommand(async () => { await HandleCLeanMapAsync(); });
        public ICommand ChangeLanguageCommand => new DelegateCommand(HandleChangeLanguage);

        const string ResourceId = "IndoorNavigation.Resources.AppResources";
        ResourceManager resmgr = new ResourceManager(ResourceId, typeof(TranslateExtension).GetTypeInfo().Assembly);

        public SettingTableViewPage()
        {
            InitializeComponent();

            downloadPage.Event.DownloadPopUpPageEventHandler += async delegate (object sender, EventArgs e) { await HandleDownloadPageAsync(sender, e); };

            BindingContext = this;

            ((NavigationPage)Application.Current.MainPage).BarBackgroundColor = Color.FromHex("#3F51B5");
            ((NavigationPage)Application.Current.MainPage).BarTextColor = Color.White;

            ReloadNaviGraphItems();

            var ci = CrossMultilingual.Current.CurrentCultureInfo;
            LanguageItems.Add(resmgr.GetString("Chinese", ci));
            LanguageItems.Add(resmgr.GetString("English", ci));

            if (Application.Current.Properties.ContainsKey("LanguagePicker"))
            {
                LanguagePicker.SelectedItem = Application.Current.Properties["LanguagePicker"].ToString();
            }
        }

        async void LicenseBtn_Tapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new LicenseMainPage());
        }

        async void DownloadGraphBtn_Tapped(object sender, EventArgs e)
        {

#if DEBUG
            string qrCodeValue = string.Empty;
            if (DeviceInfo.DeviceType == DeviceType.Virtual)
                qrCodeValue = "https://drive.google.com/uc?authuser=0&id=1TakJcYBgZ07s4WrF1-n6p5mgqttjX5UL&export=download@OpenISDM";
            else
            {
                IQrCodeDecoder qrCodeDecoder = DependencyService.Get<IQrCodeDecoder>();
                qrCodeValue = await qrCodeDecoder.ScanAsync();
            }
#else
            // 開啟鏡頭掃描Barcode
            IQrCodeDecoder qrCodeDecoder = DependencyService.Get<IQrCodeDecoder>();
            string qrCodeValue = await qrCodeDecoder.ScanAsync();

            // In iOS, if the User has denied the permission, you might not be able to request for permissions again.
            var status = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Camera);
            if (status != PermissionStatus.Granted)
            {
                //await DisplayAlert("Oops...", "Sorry for that you denied the permission of camera so you can't scan the QR code.", "OK");
            }
#endif

            if (!string.IsNullOrEmpty(qrCodeValue))
            {
                // 判斷是URL還是一般字串
                if ((qrCodeValue.Substring(0, 7) == "http://") || (qrCodeValue.Substring(0, 8) == "https://"))
                {
                    // 判斷是圖資或者網頁
                    string[] buffer = qrCodeValue.Split('@');
                    if (buffer[buffer.Length - 1] == "OpenISDM")
                    {
                        // 開啟輸入圖資名稱對話頁
                        downloadURL = buffer[0];
                        await PopupNavigation.Instance.PushAsync(downloadPage as DownloadPopUpPage);
                    }
                    else
                    {
                        // 使用瀏覽器開啟網頁
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

        void SpeechTestBtn_Tapped(object sender, EventArgs e)
        {
            Utility.TextToSpeech.Speak("歡迎使用畢迪科技室內導航", "zh-TW");
        }

        private void ReloadNaviGraphItems()
        {
            SelectNaviGraphItems.Clear();
            SelectNaviGraphItems.Add("--請選擇圖資--");

            CleanNaviGraphItems.Clear();
            CleanNaviGraphItems.Add("--全部--");

            foreach (var naviGraphName in NavigraphStorage.GetAllNavigraphs())
            {
                SelectNaviGraphItems.Add(naviGraphName);
                CleanNaviGraphItems.Add(naviGraphName);
            }
        }

        /// <summary>
        /// Handles the download page event.
        /// </summary>
        /// <returns>The download page async.</returns>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        private async Task HandleDownloadPageAsync(object sender, EventArgs e)
        {
            string fileName = (e as DownloadPopUpPageEventArgs).FileName;
            if (!string.IsNullOrEmpty(downloadURL) && !string.IsNullOrEmpty(fileName))
            {

                if (Utility.DownloadNavigraph(downloadURL, fileName))
                {
                    await DisplayAlert("訊息", "地圖下載完成", "OK");
                }
                else
                {
                    await DisplayAlert("錯誤", "地圖下載失敗", "OK");
                }
            }
            else
            {
                await DisplayAlert("錯誤", "地圖下載失敗", "OK");
            }

            ReloadNaviGraphItems();
        }

        private async void HandleChangeLanguage()
        {
            switch (LanguagePicker.SelectedItem.ToString())
            {
                case "英文":
                case "English":
                    CrossMultilingual.Current.CurrentCultureInfo = new CultureInfo("en");
                    break;
                case "中文":
                case "Chinese":
                    CrossMultilingual.Current.CurrentCultureInfo = new CultureInfo("zh");
                    break;

                default:
                    break;
            }

            AppResources.Culture = CrossMultilingual.Current.CurrentCultureInfo;
            await Navigation.PushAsync(new MainPage());
        }

        protected override void OnDisappearing()
        {
            if (LanguagePicker.SelectedItem != null)
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    var ci = CrossMultilingual.Current.CurrentCultureInfo;
                    var languageSelected = "";

                    switch (LanguagePicker.SelectedItem.ToString())
                    {
                        case "英文":
                        case "English":
                            languageSelected = resmgr.GetString("English", ci);
                            break;
                        case "中文":
                        case "Chinese":
                            languageSelected = resmgr.GetString("Chinese", ci);
                            break;
                    }

                    Application.Current.Properties["LanguagePicker"] = languageSelected;

                    await Application.Current.SavePropertiesAsync();
                });
            }

            base.OnDisappearing();
        }

        private async Task HandleCLeanMapAsync()
        {
            try
            {
                if (CleanMapPicker.SelectedItem.ToString() == "--全部--")
                {
                    if (await DisplayAlert("警告", "確定要刪除所有地圖嗎？", "Yes", "No"))
                    {
                        // 刪除所有地圖資料
                        NavigraphStorage.DeleteAllNavigraph();
                        await DisplayAlert("訊息", "刪除成功", "OK");
                    }
                }
                else
                {
                    if (await DisplayAlert("警告", string.Format("確定要刪除 地圖:{0} 嗎？",CleanMapPicker.SelectedItem), "Yes", "No"))
                    {
                        // 刪除選擇的地圖資料
                        NavigraphStorage.DeleteNavigraph(CleanMapPicker.SelectedItem.ToString());
                        await DisplayAlert("訊息", "刪除成功", "OK");
                    }
                }

            }
            catch
            {
                await DisplayAlert("錯誤", "刪除地圖時發生不明錯誤", "確定");
            }

            CleanMapPicker.SelectedItem = "";
            ReloadNaviGraphItems();
        }
    }
}