using System.Collections.Generic;
using System.Threading.Tasks;
using IndoorNavigation.iOS;
using IndoorNavigation.Models;
using ZXing.Mobile;

[assembly: Xamarin.Forms.Dependency(typeof(QrCodeDecoder))]
namespace IndoorNavigation.iOS
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
