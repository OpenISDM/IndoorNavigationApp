using System;
using Plugin.Multilingual;
using Xamarin.Forms;

namespace IndoorNavigation.Models.NavigaionLayer
{
    public class PhoneInformation
    {
        public PhoneInformation()
        {
            Console.WriteLine("Give the information of Language or phone type");
        }

        public string GiveCurrentLanguage()
        {

            if (CrossMultilingual.Current.CurrentCultureInfo.ToString() == "en" || CrossMultilingual.Current.CurrentCultureInfo.ToString() == "en-US")
            {
                return "en-US";
            }
            else if (CrossMultilingual.Current.CurrentCultureInfo.ToString() == "zh" || CrossMultilingual.Current.CurrentCultureInfo.ToString() == "zh-TW")
            {
                return "zh.xml";
            }
            else
            {
                return null;
            }
        }
    }
}

