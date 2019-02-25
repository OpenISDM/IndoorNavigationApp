using Rg.Plugins.Popup.Extensions;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace IndoorNavigation.Views.PopUpPage
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class DownloadPopUpPage : PopupPage
    {
		public DownloadPopUpPage ()
		{
			InitializeComponent ();
		}

        protected override void OnAppearingAnimationBegin()
        {
            base.OnAppearingAnimationBegin();

            FrameContainer.HeightRequest = -1;

            if (!IsAnimationEnabled)
            {
                CloseImage.Rotation = 0;
                CloseImage.Scale = 1;
                CloseImage.Opacity = 1;

                SaveButton.Scale = 1;
                SaveButton.Opacity = 1;

                mapNameLabel.TranslationX = FileNameEntry.TranslationX = 0;
                mapNameLabel.Opacity = FileNameEntry.Opacity = 1;

                return;
            }

            CloseImage.Rotation = 30;
            CloseImage.Scale = 0.3;
            CloseImage.Opacity = 0;

            SaveButton.Scale = 0.3;
            SaveButton.Opacity = 0;

            mapNameLabel.TranslationX = FileNameEntry.TranslationX = -10;
            mapNameLabel.Opacity = FileNameEntry.Opacity = 0;
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
                        FileNameEntry.TranslateTo(0, 0, easing: Easing.SpringOut, length: translateLength),
                        FileNameEntry.FadeTo(1));

                }))());

            await Task.WhenAll(
                CloseImage.FadeTo(1),
                CloseImage.ScaleTo(1, easing: Easing.SpringOut),
                CloseImage.RotateTo(0),
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
            //var loadingPage = new LoadingPopupPage();
            //await Navigation.PushPopupAsync(loadingPage);
            //await Task.Delay(2000);
            //await Navigation.RemovePopupPageAsync(loadingPage);
            //await Navigation.PushPopupAsync(new LoginSuccessPopupPage());
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
}