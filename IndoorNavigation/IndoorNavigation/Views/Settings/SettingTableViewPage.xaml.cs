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
 *      Eric Lee, ericlee@iis.sinica.edu.tw
 *
 */
using IndoorNavigation.Models;
using IndoorNavigation.Modules;
using IndoorNavigation.Modules.Utilities;
using IndoorNavigation.Resources;
using IndoorNavigation.Resources.Helpers;
using IndoorNavigation.Views.PopUpPage;
using IndoorNavigation.Views.Settings.LicensePages;
using Plugin.Multilingual;
using Prism.Commands;
using Rg.Plugins.Popup.Services;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace IndoorNavigation.Views.Settings
{
    public partial class SettingTableViewPage : ContentPage
    {
        private DownloadPopUpPage _downloadPage = new DownloadPopUpPage();
        private string _downloadURL;
        public IList _selectNaviGraphItems { get; } = new ObservableCollection<string>();
        public IList _cleanNaviGraphItems { get; } = new ObservableCollection<string>();
        public IList _languageItems { get; } = new ObservableCollection<string>();
        public IList _chooseMap { get; } = new ObservableCollection<string>();


        public ICommand _chooseMapCommand => new DelegateCommand(HandleChooseMap);
        //public ICommand SelectedMapCommand => new DelegateCommand(HandleSelectedMap);
        public ICommand _cleanMapCommand => new DelegateCommand(async () =>
            { await HandleCLeanMapAsync(); });

        public ICommand _changeLanguageCommand => new DelegateCommand(HandleChangeLanguage);

        const string _resourceId = "IndoorNavigation.Resources.AppResources";
        ResourceManager _resourceManager =
            new ResourceManager(_resourceId, typeof(TranslateExtension).GetTypeInfo().Assembly);

        public SettingTableViewPage()
        {
            InitializeComponent();
            AddMapItems();
            _downloadPage._event.DownloadPopUpPageEventHandler +=
                async delegate (object sender, EventArgs e) { await HandleDownloadPageAsync(sender, e); };

            BindingContext = this;

            ((NavigationPage)Application.Current.MainPage).BarBackgroundColor = Color.FromHex("#3F51B5");
            ((NavigationPage)Application.Current.MainPage).BarTextColor = Color.White;

            ReloadNaviGraphItems();

            var currentLanguage = CrossMultilingual.Current.CurrentCultureInfo;
            _languageItems.Add(_resourceManager.GetString("CHINESE_STRING", currentLanguage));
            _languageItems.Add(_resourceManager.GetString("ENGLISH_STRING", currentLanguage));

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
            {
                // qrCodeValue = "https://drive.google.com/uc?authuser=0&id=1C-JgyOHEikxuqgVi9S7Ww9g05u2Jb3-q&export=download@OpenISDM";
                qrCodeValue = "https://drive.google.com/uc?authuser=0&id=1w_cc8pp483Dd5KTbM3-JaCelhMh8wTQs&export=download@OpenISDM";
            }
            else
            {
                IQrCodeDecoder qrCodeDecoder = DependencyService.Get<IQrCodeDecoder>();
                var currentLanguage = CrossMultilingual.Current.CurrentCultureInfo;
                if (Connectivity.NetworkAccess == NetworkAccess.Internet)
                {
                    qrCodeValue = await qrCodeDecoder.ScanAsync();
                }
                else
                {
                    await DisplayAlert(
                            _resourceManager.GetString("WARN_STRING", currentLanguage),
                            _resourceManager.GetString("PLEASE_CHECK_INTERNET_STRING", currentLanguage),
                            _resourceManager.GetString("CANCEL_STRING", currentLanguage));
                }


            }
#else
            // Open the camera to scan Barcode
            IQrCodeDecoder qrCodeDecoder = DependencyService.Get<IQrCodeDecoder>();
            string qrCodeValue = await qrCodeDecoder.ScanAsync();

            // In iOS, if the User has denied the permission, you might not be able to request for
            // permissions again.
            /*
            var status = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Camera);
            if (status != PermissionStatus.Granted)
            {
            }*/
#endif

            if (!string.IsNullOrEmpty(qrCodeValue))
            {
                var currentLanguage = CrossMultilingual.Current.CurrentCultureInfo;
                // Determine it is URL or string
                if ((qrCodeValue.Substring(0, 7) == "http://") ||
                    (qrCodeValue.Substring(0, 8) == "https://"))
                {
                    // Determine it is map data or website
                    string[] buffer = qrCodeValue.Split('@');
                    if (buffer[buffer.Length - 1] == "OpenISDM")
                    {
                        // open the page to input the data
                        _downloadURL = buffer[0];

                        await PopupNavigation.Instance.PushAsync(_downloadPage as DownloadPopUpPage);
                    }
                    else
                    {
                        // Use the browser to open data
                        bool answer = await DisplayAlert(_resourceManager.GetString("NOTIFY_STRING", currentLanguage),
                                                         _resourceManager.GetString("SURE_TO_OPEN_WEBSITE_STRING", currentLanguage),
                                                         _resourceManager.GetString("OK_STRING", currentLanguage),
                                                         _resourceManager.GetString("CANCEL_STRING", currentLanguage));
                        if (answer)
                            await Browser.OpenAsync(qrCodeValue, BrowserLaunchMode.SystemPreferred);
                    }
                }
                else
                {
                    await DisplayAlert(_resourceManager.GetString("QRCODE_CONTENT_STRING", currentLanguage),
                                       qrCodeValue,
                                       _resourceManager.GetString("OK_STRING", currentLanguage));
                }
            }
        }

        void SpeechTestBtn_Tapped(object sender, EventArgs e)
        {
            var ci = CrossMultilingual.Current.CurrentCultureInfo;
            Utility._textToSpeech.Speak(_resourceManager.GetString("VOICE_SPEAK_STRING", ci),
                _resourceManager.GetString("CULTURE_VERSION_STRING", ci));
        }

        private void ReloadNaviGraphItems()
        {
            var ci = CrossMultilingual.Current.CurrentCultureInfo;
            _selectNaviGraphItems.Clear();
            _selectNaviGraphItems.Add(_resourceManager.GetString("CHOOSE_MAP_STRING", ci));

            _cleanNaviGraphItems.Clear();
            _cleanNaviGraphItems.Add(_resourceManager.GetString("ALL_STRING", ci));

            foreach (var naviGraphName in NavigraphStorage.GetAllNavigationGraphs())
            {
                _selectNaviGraphItems.Add(naviGraphName);
                _cleanNaviGraphItems.Add(naviGraphName);
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
            var ci = CrossMultilingual.Current.CurrentCultureInfo;
            string fileName = (e as DownloadPopUpPageEventArgs).FileName;
            if (!string.IsNullOrEmpty(_downloadURL) && !string.IsNullOrEmpty(fileName))
            {

                if (Utility.DownloadNavigraph(_downloadURL, fileName))
                {
                    await DisplayAlert(_resourceManager.GetString("MESSAGE_STRING", ci),
                                       _resourceManager.GetString("SUCCESSFULLY_DOWNLOAD_MAP_STRING", ci),
                                       _resourceManager.GetString("OK_STRING", ci));
                }
                else
                {
                    await DisplayAlert(_resourceManager.GetString("ERROR_STRING", ci),
                                       _resourceManager.GetString("FAILED_DOWNLOAD_MAP_STRING", ci),
                                       _resourceManager.GetString("OK_STRING", ci));
                }
            }
            else
            {
                await DisplayAlert(_resourceManager.GetString("ERROR_STRING", ci),
                                   _resourceManager.GetString("FAILED_DOWNLOAD_MAP_STRIN", ci),
                                   _resourceManager.GetString("OK_STRING", ci));
            }
            string fileLanguageTaiwanChinese = fileName + "_zh.xml";
            //Testing in Lab
            string firstDirectionFile_zh_TW = "https://drive.google.com/uc?authuser=0&id=1C_ncshn2Q2veLMVMgvqW81xLP3DJnQpW&export=download";

            //Testing in Taipei City hall 2F
           // string firstDirectionFile_zh_TW = "https://drive.google.com/uc?authuser=0&id=17AarNw7QqBFlRqSNMTjwU8_WkMK98SPI&export=download";
            Utility.DownloadFirstDirectionFile(firstDirectionFile_zh_TW, fileLanguageTaiwanChinese);

            string stringfileLanguageUSEnglish = fileName + "_en-US.xml";
            //Testing in Lab
            string firstDirectionFile_en_US = "https://drive.google.com/uc?authuser=0&id=1dvmo3WjW_2dljvJ0qY1sVK5qX6PNWg_g&export=download";

            //Testing in Taipei City Hall 2F
            //string firstDirectionFile_en_US = "https://drive.google.com/uc?authuser=0&id=1f8zTIMWJFOsNybVwm-kkSo4enNM7lIKY&export=download";

            Utility.DownloadFirstDirectionFile(firstDirectionFile_en_US, stringfileLanguageUSEnglish);

            string infoTaiwanChinese = fileName + "_info_zh.xml";
            string infoEnglish = fileName + "_info_en-US.xml";


            string infoFile_zh_TW = "https://drive.google.com/uc?authuser=0&id=1Fajcicwcrg_GHhabuygEZyhyUJxxDY3f&export=download";
            Utility.DownloadInformationFile(infoFile_zh_TW, infoTaiwanChinese);

            string infoFile_en_US = "https://drive.google.com/uc?authuser=0&id=1KCbZUDPrfGv5H14OTSX2PaTnREG8Xk94&export=download";
            Utility.DownloadInformationFile(infoFile_en_US,infoEnglish);

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
                            languageSelected = _resourceManager.GetString("ENGLISH_STRING", ci);
                            break;
                        case "中文":
                        case "Chinese":
                            languageSelected = _resourceManager.GetString("CHINESE_STRING", ci);
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
            var ci = CrossMultilingual.Current.CurrentCultureInfo;
            try
            {
                if (CleanMapPicker.SelectedItem.ToString() == _resourceManager.GetString("ALL_STRING", ci))
                {
                    if (await DisplayAlert(_resourceManager.GetString("WARN_STRING", ci),
                                           _resourceManager.GetString("ASK_IF_CANCEL_ALL_MAP_STRING", ci),
                                           _resourceManager.GetString("OK_STRING", ci),
                                           _resourceManager.GetString("CANCEL_STRING", ci)))
                    {
                        // Cancel All Map
                        NavigraphStorage.DeleteAllNavigationGraph();
                        NavigraphStorage.DeleteAllFirstDirectionXML();
                        NavigraphStorage.DeleteAllInformationXML();
                        await DisplayAlert(_resourceManager.GetString("MESSAGE_STRING", ci),
                                           _resourceManager.GetString("SUCCESSFULLY_DELETE_STRING", ci),
                                           _resourceManager.GetString("OK_STRING", ci));
                    }
                }
                else
                {
                    if (await DisplayAlert(_resourceManager.GetString("WARN_STRING", ci),
                                           //_resourceManager.GetString("ASK_IF_CANCEL_MAP_STRING", ci),
                                           //_resourceManager.GetString("MAP_STRING", ci),
                                           //string.Format(":{0}？", CleanMapPicker.SelectedItem),
                                           string.Format(_resourceManager.GetString("ASK_IF_CANCEL_MAP_STRING", ci) + _resourceManager.GetString("MAP_STRING", ci) + ":{0}？", CleanMapPicker.SelectedItem),
                                           _resourceManager.GetString("OK_STRING", ci),
                                           _resourceManager.GetString("CANCEL_STRING", ci)))

                    {
                        // Delete selected map
                        NavigraphStorage.DeleteNavigationGraph(CleanMapPicker.SelectedItem.ToString());
                        NavigraphStorage.DeleteFirstDirectionXML(CleanMapPicker.SelectedItem.ToString());
                        NavigraphStorage.DeleteInformationML(CleanMapPicker.SelectedItem.ToString());
                        await DisplayAlert(_resourceManager.GetString("MESSAGE_STRING", ci),
                                           _resourceManager.GetString("SUCCESSFULLY_DELETE_STRING", ci),
                                           _resourceManager.GetString("OK_STRING", ci));
                    }
                }

            }
            catch
            {
                await DisplayAlert(_resourceManager.GetString("ERROR_STRING", ci),
                                   _resourceManager.GetString("ERROR_TO_DELETE_STRING", ci),
                                   _resourceManager.GetString("OK_STRING", ci));
            }

            CleanMapPicker.SelectedItem = "";
            ReloadNaviGraphItems();
        }

        private void AddMapItems()
        {
            var ci = CrossMultilingual.Current.CurrentCultureInfo;
            _chooseMap.Clear();
            _chooseMap.Add(_resourceManager.GetString("TAIPEI_CITY_HALL_STRING", ci));
            _chooseMap.Add(_resourceManager.GetString("HOSPITAL_NAME_STRING", ci));
            _chooseMap.Add(_resourceManager.GetString("LAB_STRING", ci));
        }

        private async void HandleChooseMap()
        {
            var ci = CrossMultilingual.Current.CurrentCultureInfo;
            string NTUH_YunLin = _resourceManager.GetString("HOSPITAL_NAME_STRING", ci).ToString();
            string Taipei_City_Hall = _resourceManager.GetString("TAIPEI_CITY_HALL_STRING", ci).ToString();
            string Lab = _resourceManager.GetString("LAB_STRING", ci).ToString();
            
            if (OptionPicker.SelectedItem.ToString().Trim() == NTUH_YunLin)
            {
                NavigraphStorage.GenerateFileRoute(NTUH_YunLin, "NTUH_YunLin");
            }
            else if (OptionPicker.SelectedItem.ToString().Trim() == Taipei_City_Hall)
            {
                NavigraphStorage.GenerateFileRoute(Taipei_City_Hall, "Taipei_City_Hall");
            }
            else if (OptionPicker.SelectedItem.ToString().Trim() == Lab)
            {
                NavigraphStorage.GenerateFileRoute(Lab, "Lab");
            }

            ReloadNaviGraphItems();

        }
    }
}