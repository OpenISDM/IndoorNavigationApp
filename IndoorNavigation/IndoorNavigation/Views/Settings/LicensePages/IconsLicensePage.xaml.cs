using System;
using System.Collections.Generic;

using Xamarin.Forms;
using System.Windows.Input;

namespace IndoorNavigation.Views.Settings.LicensePages
{
    public partial class IconsLicensePage : ContentPage
    {
        public IconsLicensePage()
        {
            InitializeComponent();
            BindingContext = this;
        }

        public ICommand HyperlinkClickCommand => new Command<string>((url) =>
        {
            Device.OpenUri(new Uri(url));
        });
    }

}
