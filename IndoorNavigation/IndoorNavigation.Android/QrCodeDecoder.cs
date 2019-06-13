using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using IndoorNavigation.Droid;
using IndoorNavigation.Models;
using ZXing.Mobile;

[assembly: Xamarin.Forms.Dependency(typeof(QrCodeDecoder))]
namespace IndoorNavigation.Droid
{
    public class QrCodeDecoder : IQrCodeDecoder
    {
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

            ZXing.Result scanResults = await scanner.Scan(scanOptions);

            if (scanResults != null)
                return scanResults.Text;
            else
                return string.Empty;
        }
    }
}