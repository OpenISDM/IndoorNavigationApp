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
 *
 *      The global variables and functions
 *
 * File Name:
 *
 *      Utility.cs
 *
 * Abstract:
 *
 *      
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
    public static class Utility
    {
        public static Dictionary<Guid, Beacon> BeaconsDict;
        public static List<WaypointModel> Waypoints;
        public static List<LocationConnectModel> LocationConnects;
        public static WaypointRoutePlan WaypointRoute;
        public static SignalProcessModule SignalProcess;
        public static MaNModule MaN;

        // Skip SSL checking
        private static bool ValidateServerCertificate(Object sender,
            X509Certificate certificate,
            X509Chain chain,
            SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        /// <summary>
        /// Download navigation graph from Server
        /// </summary>
        /// <param name="URL"></param>
        /// <returns></returns>
        public static string DownloadNavigraph(string URL)
        {
            try
            {
                // Skip SSL checking
                // If the server doesn't use validated license, WebRequest
                // element would return error.
                ServicePointManager.ServerCertificateValidationCallback
                    = new RemoteCertificateValidationCallback
                    (ValidateServerCertificate);

                // download data
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
            catch (Exception ex)
            {
                throw new ArgumentException(ex.Message);
            }
        }
    }

    public class RotateAngle
    {
        /// <summary>
        /// Compute the angle to turn, including direction at the next waypoint
        /// </summary>
        /// <param name="Current">current location</param>
        /// <param name="Previous">last location</param>
        /// <param name="Next">next location</param>
        /// <returns></returns>
        public static int GetRotateAngle(GeoCoordinates Current,
            GeoCoordinates Previous, GeoCoordinates Next)
        {
            double cosineAngle =
                CalculatingCosineAngle(Current, Previous, Next);
            double outerProductAngle =
                CalculatingOuterProductAngle(Current, Previous, Next);

            if (outerProductAngle < 0)
                return System.Convert.ToInt32(180 - cosineAngle * 180 / Math.PI);
            else
                return -System.Convert.ToInt32(180 - cosineAngle * 180 / Math.PI);
        }

        /// <summary>
        /// The angle compute by law of cosines
        /// </summary>
        /// <param name="Current">Current location</param>
        /// <param name="Previous">last location </param>
        /// <param name="Next">next location </param>
        /// <returns></returns>
        private static double CalculatingCosineAngle(GeoCoordinates Current,
            GeoCoordinates Previous, GeoCoordinates Next)
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
        private static double CalculatingOuterProductAngle(
            GeoCoordinates Current, GeoCoordinates Previous, GeoCoordinates Next)
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
