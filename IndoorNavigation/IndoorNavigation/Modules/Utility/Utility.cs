using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using GeoCoordinatePortable;
using IndoorNavigation.Models;
using IndoorNavigation.Modules.Navigation;

namespace IndoorNavigation.Modules
{
    public class Utility
    {
        public static List<Beacon> Beacons;
        public static List<BeaconGroupModel> BeaconGroups;
        public static List<LocationConnectModel> LocationConnects;
        public static RoutePlan Route;
        public static SignalProcessModule SignalProcess;

        //跳過SSL檢查
        private static bool ValidateServerCertificate(Object sender,
            X509Certificate certificate,
            X509Chain chain,
            SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        public static string DownloadMap(string URL)
        {
            try
            {
                //跳過SSL檢查
                ServicePointManager.ServerCertificateValidationCallback
                    = new RemoteCertificateValidationCallback
                    (ValidateServerCertificate);

                var request = WebRequest.Create(URL) as HttpWebRequest;
                request.Method = WebRequestMethods.Http.Get;
                request.ContentType = "application/json";

                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        using (var stream = response.GetResponseStream())
                        {
                            using (var reader = new StreamReader(stream))
                            {
                                var retMsg = reader.ReadToEnd();
                                retMsg = retMsg.Trim(new char[] { '"' });
                                retMsg = retMsg.Replace(@"\", "");
                                return retMsg;
                            }
                        }
                    }
                }

                throw new ArgumentException("Download faild");
            }
            catch(Exception ex)
            {
                throw new ArgumentException(ex.Message);
            }
        }
    }

    public class RotateAngle
    {
        /// <summary>
        /// 水平角度(含方向)
        /// </summary>
        /// <param name="Current">現在位置</param>
        /// <param name="Previous">上一個位置</param>
        /// <param name="Next">下一個位置</param>
        /// <returns></returns>
        public static int GetRotateAngle(GeoCoordinate Current, 
            GeoCoordinate Previous, GeoCoordinate Next)
        {
            double cosineANS = CosineAngle(Current, Previous, Next);
            double outerProductANS = OuterProductAngle(Current,Previous,Next);

            if (outerProductANS < 0)
                return System.Convert.ToInt32(180 - cosineANS * 180/Math.PI);
            else
                return -System.Convert.ToInt32(180 - cosineANS * 180/Math.PI);
        }

        /// <summary>
        /// 餘弦定理角度
        /// </summary>
        /// <param name="Current">現在位置</param>
        /// <param name="Previous">上一個位置</param>
        /// <param name="Next">下一個位置</param>
        /// <returns></returns>
        private static double CosineAngle(GeoCoordinate Current, 
            GeoCoordinate Previous, GeoCoordinate Next)
        {
            double centerToTarget = Current.GetDistanceTo(Next);
            double centerToFace = Current.GetDistanceTo(Previous);
            double faceToTarget = Previous.GetDistanceTo(Next);

            return Math.Acos(
                (centerToTarget * centerToTarget + 
                centerToFace * centerToFace - 
                faceToTarget * faceToTarget) /
                (2 * centerToTarget * centerToFace));
        }

        /// <summary>
        /// 外積算角度
        /// </summary>
        /// <param name="Current">現在位置</param>
        /// <param name="Previous">上一個位置</param>
        /// <param name="Next">下一個位置</param>
        /// <returns></returns>
        private static double OuterProductAngle(GeoCoordinate Current, 
            GeoCoordinate Previous, GeoCoordinate Next)
        {
            double Xa, Xb, Ya, Yb;
            double angle;

            Xa = Previous.Longitude - Current.Longitude;
            Ya = Previous.Latitude - Current.Latitude;

            Xb = Next.Longitude - Current.Longitude;
            Yb = Next.Latitude - Current.Latitude;

            double c = Math.Sqrt(Xa * Xa + Ya * Ya) * 
                Math.Sqrt(Xb * Xb + Yb * Yb);

            angle = Math.Asin((Xa * Yb - Xb * Ya) / c);

            return angle;
        }
    }
}
