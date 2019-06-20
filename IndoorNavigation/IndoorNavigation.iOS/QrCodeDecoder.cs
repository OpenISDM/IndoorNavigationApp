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
        private ResourceManager _resourceManager = new ResourceManager(_resourceId, typeof(TranslateExtension).GetTypeInfo().Assembly);
        
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
           
            scanner.CancelButtonText = _resourceManager.GetString("Cancel", ci);
            scanner.FlashButtonText = _resourceManager.GetString("Flash", ci);
            ZXing.Result scanResults = await scanner.Scan(scanOptions);

            if (scanResults != null)
                return scanResults.Text;
            else
                return string.Empty;
        }
    }
}
