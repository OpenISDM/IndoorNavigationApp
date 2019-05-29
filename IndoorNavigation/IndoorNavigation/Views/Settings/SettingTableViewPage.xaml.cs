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

namespace IndoorNavigation.Views.Settings
{
    public partial class SettingTableViewPage : ContentPage
    {
        private DownloadPopUpPage downloadPage = new DownloadPopUpPage();
        private string downloadURL;

        // sample of TextPickerCell(選擇圖資, ref: https://github.com/muak/AiForms.SettingsView#textpickercell)
        public IList SelectNaviGraphItems { get; } = new ObservableCollection<string>();
        public IList CleanNaviGraphItems { get; } = new ObservableCollection<string>();
        public IList LanguageItems { get; } = new ObservableCollection<string>();
        //public ICommand SelectedMapCommand => new DelegateCommand(HandleSelectedMap);
        public ICommand CleanMapCommand => new DelegateCommand(async () => { await HandleCLeanMapAsync(); });
        public ICommand ChangeLanguageCommand => new DelegateCommand(HandleChangeLanguage);

        public SettingTableViewPage()
        {
            InitializeComponent();

            downloadPage.Event.DownloadPopUpPageEventHandler += async delegate (object sender, EventArgs e) { await HandleDownloadPageAsync(sender, e); };

            BindingContext = this;

            ((NavigationPage)Application.Current.MainPage).BarBackgroundColor = Color.FromHex("#3F51B5");
            ((NavigationPage)Application.Current.MainPage).BarTextColor = Color.White;

            ReloadNaviGraphItems();

            LanguageItems.Add("Chinese");
            LanguageItems.Add("English");

            if (Application.Current.Properties.ContainsKey("LanguagePicker"))
            {
                LanguagePicker.SelectedItem = Application.Current.Properties["LanguagePicker"];
            }
        }

        async void LicenseBtn_Tapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new LicenseMainPage());
        }

        async void DownloadMapBtn_Tapped(object sender, EventArgs e)
        {

#if DEBUG
            string qrCodeValue = string.Empty;
            if (DeviceInfo.DeviceType == DeviceType.Virtual)
                qrCodeValue = "https://drive.google.com/a/gm.nssh.ntpc.edu.tw/uc?authuser=0&id=1t0aTpm5rC-Z6h66DXsv0UXrDBmHWK4Ja&export=download@OpenISDM";
            else
            {
                IQrCodeDecoder qrCodeDecoder = DependencyService.Get<IQrCodeDecoder>();
                qrCodeValue = await qrCodeDecoder.ScanAsync();
            }
#else
            // 開啟鏡頭掃描Barcode
            IQrCodeDecoder qrCodeDecoder = DependencyService.Get<IQrCodeDecoder>();
            string qrCodeValue = await qrCodeDecoder.ScanAsync();
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
            Utility.TextToSpeech.Speak("Welcome to the hospital!", "zh-TW");
        }

        private void ReloadNaviGraphItems()
        {
            SelectNaviGraphItems.Clear();
            SelectNaviGraphItems.Add("--請選擇圖資--");

            CleanNaviGraphItems.Clear();
            CleanNaviGraphItems.Add("--全部--");

            if (Utility.Waypoints == null)
                //MapPicker.SelectedItem = "--請選擇圖資--";

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

        //private void HandleSelectedMap()
        //{
        //    if (MapPicker.SelectedItem.ToString() == "--請選擇圖資--")
        //    {
        //        // 移除已經載入的地圖資訊
        //        Utility.Waypoints = null;
        //        Utility.WaypointRoute = null;
        //        Utility.BeaconsDict = null;
        //        Utility.LocationConnects = null;
        //    }
        //    else
        //    {
        //        if (!NavigraphStorage.LoadNavigraph(MapPicker.SelectedItem.ToString()))
        //            MapPicker.SelectedItem = "--請選擇圖資--";
        //    }
        //}

        private async void HandleChangeLanguage()
        {
            switch (LanguagePicker.SelectedItem.ToString())
            {
                case "English":
                    CrossMultilingual.Current.CurrentCultureInfo = new CultureInfo("en");
                    break;
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
                    Application.Current.Properties["LanguagePicker"] = LanguagePicker.SelectedItem;

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