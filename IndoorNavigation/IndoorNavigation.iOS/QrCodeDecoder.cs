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
 *      This file contains all the interfaces required by the application,
 *      such as the interface of IPSClient and the interface for 
 *      both iOS project and the Android project to allow the Xamarin.Forms 
 *      app to access the APIs on each platform.
 *      
 * Version:
 *
 *      1.0.0, 20190605
 * 
 * File Name:
 *
 *      QrCodeDecoder.cs
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
using System.Collections.Generic;
using System.Reflection;
using System.Resources;
using System.Threading.Tasks;
using IndoorNavigation.iOS;
using IndoorNavigation.Models;
using IndoorNavigation.Resources.Helpers;
using Plugin.Multilingual;
using ZXing.Mobile;

[assembly: Xamarin.Forms.Dependency(typeof(QrCodeDecoder))]
namespace IndoorNavigation.iOS
{
    public class QrCodeDecoder : IQrCodeDecoder
    {
        private const string _resourceId = "IndoorNavigation.Resources.AppResources";
        private ResourceManager _resourceManager =
            new ResourceManager(_resourceId, typeof(TranslateExtension).GetTypeInfo().Assembly);
        
        public async Task<string> ScanAsync()
        {
            MobileBarcodeScanner scanner = new MobileBarcodeScanner();

            MobileBarcodeScanningOptions scanOptions = 
                new MobileBarcodeScanningOptions
                {
                    PossibleFormats = new List<ZXing.BarcodeFormat>()
                    {
                        ZXing.BarcodeFormat.QR_CODE, 
                        ZXing.BarcodeFormat.CODE_39
                    }
                };

            var ci = CrossMultilingual.Current.CurrentCultureInfo;
           
            scanner.CancelButtonText = _resourceManager.GetString("CANCEL_STRING", ci);
            scanner.FlashButtonText = _resourceManager.GetString("FLASH_STRING", ci);
            ZXing.Result scanResults = await scanner.Scan(scanOptions);

            if (scanResults != null)
                return scanResults.Text;
            else
                return string.Empty;
        }
    }
}
