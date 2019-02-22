using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using IndoorNavigation.Views.Settings.LicensePages;
using Xamarin.Forms;

namespace IndoorNavigation.Views.Settings
{
    public partial class SettingTableViewPage : ContentPage
    {
        // sample of TextPickerCell(選擇圖資, ref: https://github.com/muak/AiForms.SettingsView#textpickercell)
        public IList NaviGraphItems { get; } = new ObservableCollection<string>(new List<string> { "OpenHouse", "台大醫院" });

        public SettingTableViewPage()
        {
            InitializeComponent();

            BindingContext = this;

            ((NavigationPage)Application.Current.MainPage).BarBackgroundColor = Color.FromHex("#009FCC");
            ((NavigationPage)Application.Current.MainPage).BarTextColor = Color.White;

            NaviGraphItems.Add("雲林醫院");
            NaviGraphItems.Add("中研院");
        }

        async void LicenseBtn_Tapped(object sender, System.EventArgs e)
        {
            await Navigation.PushAsync(new LicenseMainPage());
        }
    }
}
