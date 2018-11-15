/*
 * Copyright (c) 2018 Academia Sinica, Institude of Information Science
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
 * File Name:
 *
 *      Utility.cs
 *
 * Abstract:
 *
 *      The public variables and functions
 *
 * Authors:
 *
 *      Kenneth Tang, kenneth@gm.nssh.ntpc.edu.tw
 *
 */

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
        public static Dictionary<Guid,Beacon> Beacons;
        public static List<BeaconGroupModel> BeaconGroups;
        public static List<LocationConnectModel> LocationConnects;
        public static RoutePlan Route;
        public static SignalProcessModule SignalProcess;

        // Skip SSL checking
        private static bool ValidateServerCertificate(Object sender,
            X509Certificate certificate,
            X509Chain chain,
            SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        /// <summary>
        /// Download map information from Server
        /// </summary>
        /// <param name="URL"></param>
        /// <returns></returns>
        public static string DownloadMap(string URL)
        {
            try
            {
                // Skip SSL checking
                // If the server doesn't use validated license, WebRequest
                // element would return error.
                ServicePointManager.ServerCertificateValidationCallback
                    = new RemoteCertificateValidationCallback
                    (ValidateServerCertificate);

                // 下載資料
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
        /// Compute the angle to turn, including direction.
        /// </summary>
        /// <param name="Current">current location</param>
        /// <param name="Previous">last location</param>
        /// <param name="Next">next location</param>
        /// <returns></returns>
        public static int GetRotateAngle(GeoCoordinate Current,
            GeoCoordinate Previous, GeoCoordinate Next)
        {
            double cosineAngle = CalculatingCosineAngle(Current, Previous, Next);
            double outerProductAngle = CalculatingOuterProductAngle(Current,Previous,Next);

            if (outerProductAngle < 0)
                return System.Convert.ToInt32(180 - cosineAngle * 180/Math.PI);
            else
                return -System.Convert.ToInt32(180 - cosineAngle * 180/Math.PI);
        }

        /// <summary>
        /// The angle computed by law of cosines
        /// </summary>
        /// <param name="Current">Current location</param>
        /// <param name="Previous">last location </param>
        /// <param name="Next">next location </param>
        /// <returns></returns>
        private static double CalculatingCosineAngle(GeoCoordinate Current,
            GeoCoordinate Previous, GeoCoordinate Next)
        {
            double centerToNext = Current.GetDistanceTo(Next);
            double centerToPrevious = Current.GetDistanceTo(Previous);
            double PreviousToNext = Previous.GetDistanceTo(Next);

            return Math.Acos(
                (centerToNext * centerToNext +
                centerToPrevious * centerToPrevious -
                PreviousToNext * PreviousToNext) /
                (2 * centerToNext * centerToPrevious));
        }

        /// <summary>
        /// Compute angle by croos product 
        /// </summary>
        /// <param name="Current">Current location </param>
        /// <param name="Previous">last location </param>
        /// <param name="Next">next location</param>
        /// <returns></returns>
        private static double CalculatingOuterProductAngle(GeoCoordinate Current,
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
