using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xamarin.Forms;

namespace IndoorNavigation.Views.Navigation
{
    public partial class TabbedPageSetting : ContentPage
    {
        // sample of TextPickerCell(選擇語音)
        public IList VoiceSearchItems { get; } = new ObservableCollection<string>(new List<string> { "中文", "英文" });

        public TabbedPageSetting()
        {
            InitializeComponent();

            VoiceSearchItems.Add("日文");

#if __ANDROID__
            NavigationSettingsView.HeaderHeight = 50;
            NavigationSettingsView.HeaderPadding = new Thickness(14, 0, 0, 16);
#endif

        }
    }
}
