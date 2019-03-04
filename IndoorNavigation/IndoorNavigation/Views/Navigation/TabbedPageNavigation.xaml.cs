using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace IndoorNavigation.Views.Navigation
{
    public partial class TabbedPageNavigation : ContentPage
    {
        public TabbedPageNavigation()
        {
            InitializeComponent();

#if __ANDROID__
            if (Application.Current.MainPage.Height < 600)
            {
                CurrentInstructionLabel.Margin = new Thickness(1, 50, 1, -50);
                CurrentInstructionImage.Scale = 0.5;
            }
#endif


#if __IOS__
            // customize CurrentInstruction UI for iPhone X/XR/XS/XS Max
            if (Application.Current.MainPage.Height > 800)
            {
                CurrentInstructionLabel.Margin = new Thickness(1, 50, 1, -2);
                CurrentInstructionImage.Scale = 0.7;
            }
#endif

        }

        //async void Entry_TextChanged(object sender, Xamarin.Forms.TextChangedEventArgs e)
        //{
        //    if (double.TryParse(e.NewTextValue, out double number))
        //        await NavigationProgressBar.ProgressTo(number, 500, Easing.Linear);  // example of progressbar animation
        //}

    }
}
