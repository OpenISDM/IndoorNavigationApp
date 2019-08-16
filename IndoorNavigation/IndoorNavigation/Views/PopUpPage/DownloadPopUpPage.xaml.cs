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
 *      DownloadPopUpPage.xaml.cs
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
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Resources;
using IndoorNavigation.Resources.Helpers;
using System.Reflection;
using Plugin.Multilingual;

namespace IndoorNavigation.Views.PopUpPage
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DownloadPopUpPage : PopupPage
    {
		const string _resourceId = "IndoorNavigation.Resources.AppResources";
		ResourceManager _resourceManager =
			new ResourceManager(_resourceId, typeof(TranslateExtension).GetTypeInfo().Assembly);

		public DownloadPopUpPageEvent _event { get; private set; }

        public DownloadPopUpPage()
        {
            InitializeComponent();
            _event = new DownloadPopUpPageEvent();
        }

        protected override void OnAppearingAnimationBegin()
        {
            base.OnAppearingAnimationBegin();

			var currentLanguage = CrossMultilingual.Current.CurrentCultureInfo;
            FrameContainer.HeightRequest = -1;

            if (!IsAnimationEnabled)
            {
                SaveButton.Scale = 1;
                SaveButton.Opacity = 1;

				mapNameLabel.TranslationX = FileNameEntry.TranslationX = 0;
                mapNameLabel.Opacity = FileNameEntry.Opacity = 1;

                return;
            }
            
            SaveButton.Scale = 0.3;
            SaveButton.Opacity = 0;

			mapNameLabel.Text = _resourceManager.GetString("INPUT_MAP_NAME_STRING", currentLanguage);
			FileNameEntry.Placeholder = _resourceManager.GetString("MAP_NAME_STRING", currentLanguage);

			mapNameLabel.TranslationX = FileNameEntry.TranslationX = -10;
            mapNameLabel.Opacity = FileNameEntry.Opacity = 0;

            this.FileNameEntry.Text = "";
        }

        protected override async Task OnAppearingAnimationEndAsync()
        {
			if (!IsAnimationEnabled)
                return;

            var translateLength = 400u;

            await Task.WhenAll(
                mapNameLabel.TranslateTo(0, 0, easing: Easing.SpringOut, length: translateLength),
                mapNameLabel.FadeTo(1),
                (new Func<Task>(async () =>
                {
                    await Task.Delay(200);
                    await Task.WhenAll(
                        FileNameEntry.TranslateTo(0, 0, easing: Easing.SpringOut,
                                                  length: translateLength),
                        FileNameEntry.FadeTo(1));

                }))());

            await Task.WhenAll(
                SaveButton.ScaleTo(1),
                SaveButton.FadeTo(1));
        }

        protected override async Task OnDisappearingAnimationBeginAsync()
        {
			if (!IsAnimationEnabled)
                return;

            var taskSource = new TaskCompletionSource<bool>();

            var currentHeight = FrameContainer.Height;

            await Task.WhenAll(
                mapNameLabel.FadeTo(0),
                FileNameEntry.FadeTo(0),
                SaveButton.FadeTo(0));

            FrameContainer.Animate("HideAnimation", d =>
            {
                FrameContainer.HeightRequest = d;
            },
            start: currentHeight,
            end: 170,
            finished: async (d, b) =>
            {
                await Task.Delay(300);
                taskSource.TrySetResult(true);
            });

            await taskSource.Task;
        }

        private async void OnSave(object sender, EventArgs e)
        {
			var currentLanguage = CrossMultilingual.Current.CurrentCultureInfo;
			if (!string.IsNullOrEmpty(FileNameEntry.Text))
            {
                _event.OnEventCall(new DownloadPopUpPageEventArgs { FileName = FileNameEntry.Text });
                CloseAllPopup();
            }
            else
            {
				await DisplayAlert(_resourceManager.GetString("MESSAGE_STRING", currentLanguage),
									_resourceManager.GetString("INPUT_MAP_NAME_STRING", currentLanguage),
									_resourceManager.GetString("OK_STRING", currentLanguage));
            }
        }

        private void OnCloseButtonTapped(object sender, EventArgs e)
        {
            CloseAllPopup();
        }

        protected override bool OnBackgroundClicked()
        {
            CloseAllPopup();

            return false;
        }

        private async void CloseAllPopup()
        {
            await PopupNavigation.Instance.PopAllAsync();
        }

    }

    #region Download PopUp Page Event
    public class DownloadPopUpPageEvent
    {
        public event EventHandler DownloadPopUpPageEventHandler;

        public void OnEventCall(EventArgs e)
        {
            DownloadPopUpPageEventHandler?.Invoke(this, e);
        }
    }

    public class DownloadPopUpPageEventArgs : EventArgs
    { 
        public string FileName { get; set; }
    }
    #endregion
}