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
 * 
 *     
 *      
 * Version:
 *
 *      1.0.0, 201911125
 * 
 * File Name:
 *
 *      PhoneInformation.cs
 *
 * Abstract:
 *
 *     This file used to get the information of the phone
 *     Includes, current language, current existing map
 *      
 * Authors:
 *
 *      Eric Lee, ericlee@iis.sinica.edu.tw
 *     
 *
 */

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Resources;
using IndoorNavigation.Resources.Helpers;
using Plugin.Multilingual;
using Xamarin.Forms;

namespace IndoorNavigation.Models.NavigaionLayer
{
    public class PhoneInformation
    {
        private string _en = "en";
        private string _returnEnglish = "en-US";
        private string _returnChinese = "zh";
        private string _zhTW = "zh-TW"; const string _resourceId = "IndoorNavigation.Resources.AppResources";
        ResourceManager _resourceManager =
        new ResourceManager(_resourceId, typeof(TranslateExtension).GetTypeInfo().Assembly);
        public PhoneInformation()
        {
           
        }

        public string GiveCurrentLanguage()
        {
            //If add one more language, here needs to add
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
            giveAllLanguage.Add(_returnEnglish);
            giveAllLanguage.Add(_returnChinese);
            return giveAllLanguage;
        }

        public string GiveCurrentMapName(string userNaming)
        {
            var ci = CrossMultilingual.Current.CurrentCultureInfo;
            string NTUH_YunLin = _resourceManager.GetString("HOSPITAL_NAME_STRING", ci).ToString();
            string Taipei_City_Hall = _resourceManager.GetString("TAIPEI_CITY_HALL_STRING", ci).ToString();
            string Lab = _resourceManager.GetString("LAB_STRING", ci).ToString();
            string Yuanlin_Christian_Hospital = _resourceManager.GetString("YUANLIN_CHRISTIAN_HOSPITAL_STRING", ci).ToString();
            string loadFileName = "";

            if (userNaming == NTUH_YunLin)
            {
                loadFileName = "NTUH Yunlin Branch";
            }
            else if (userNaming == Taipei_City_Hall)
            {
                loadFileName = "Taipei City Hall";
            }
            else if (userNaming == Lab)
            {
                loadFileName = "Lab";
            }
            else if (userNaming == Yuanlin_Christian_Hospital)
            {
                loadFileName = "Yuanlin Christian Hospital";
            }
            return loadFileName;
        }
        public List<string>GiveGenerateMapName(string userNaming)
        {
            var ci = CrossMultilingual.Current.CurrentCultureInfo;
            string NTUH_YunLin = _resourceManager.GetString("HOSPITAL_NAME_STRING", ci).ToString();
            string Taipei_City_Hall = _resourceManager.GetString("TAIPEI_CITY_HALL_STRING", ci).ToString();
            string Lab = _resourceManager.GetString("LAB_STRING", ci).ToString();
            string Yuanlin_Christian_Hospital = _resourceManager.GetString("YUANLIN_CHRISTIAN_HOSPITAL_STRING", ci).ToString();
            List<string> loadFileName = new List<string>();

            if (userNaming == NTUH_YunLin)
            {
                loadFileName.Add("NTUH Yunlin Branch");
                loadFileName.Add("NTUH_YunLin");
            }
            else if (userNaming == Taipei_City_Hall)
            {
                loadFileName.Add("Taipei City Hall");
                loadFileName.Add("Taipei_City_Hall");
            }
            else if (userNaming == Lab)
            {
                loadFileName.Add("Lab");
                loadFileName.Add("Lab");
            }
            else if (userNaming == Yuanlin_Christian_Hospital)
            {
                loadFileName.Add("Yuanlin Christian Hospital");
                loadFileName.Add("Yuanlin_Christian_Hospital");
            }
            return loadFileName;
        }
    }
}

