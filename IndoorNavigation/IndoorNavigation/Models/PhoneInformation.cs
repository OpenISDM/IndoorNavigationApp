using System;
using System.Collections.Generic;
using Plugin.Multilingual;
using Xamarin.Forms;

namespace IndoorNavigation.Models.NavigaionLayer
{
    public class PhoneInformation
    {
        private string _en = "en";
        private string _returnEnglish = "en-US";
        private string _returnChinese = "zh";
        private string _zhTW = "zh-TW";
        public PhoneInformation()
        {
            Console.WriteLine("Give the information of Language or phone type");
        }

        public string GiveCurrentLanguage()
        {

            if (CrossMultilingual.Current.CurrentCultureInfo.ToString() == _en || CrossMultilingual.Current.CurrentCultureInfo.ToString() == _returnEnglish)
            {
                return _returnEnglish;
            }
            else if (CrossMultilingual.Current.CurrentCultureInfo.ToString() == _returnChinese || CrossMultilingual.Current.CurrentCultureInfo.ToString() == _zhTW)
            {
                return _returnChinese;
            }
            else
            {
                return null;
            }
        }
        public List<string> GiveAllLanguage()
        {
            List<string> giveAllLanguage = new List<string>();
            giveAllLanguage.Add(_returnChinese);
            giveAllLanguage.Add(_returnChinese);
            return giveAllLanguage;
        }
    }
}

