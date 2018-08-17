using System;
using System.Collections.Generic;
using System.Text;
using GeoCoordinatePortable;

namespace IndoorNavigation
{
    /// <summary>
    /// Data conversion tool for indoor navigation
    /// </summary>
    public partial class Convert
    {
        /// <summary>
        /// Convert UUID to a Coordinate object.
        /// </summary>
        /// <param name="UUID"></param>
        /// <returns></returns>
        public static GeoCoordinate ToCoordinate(Guid UUID)
        {
            // Combine coordinate Hex data from UUID
            string[] IdShards = UUID.ToString().Split('-');
            string LonHexStr = IdShards[2] + IdShards[3];
            string LatHexStr = IdShards[4].Substring(4,8);

            // Convert coordinate hex data to coordinate
            float Longitude = HexToFloat(LonHexStr);
            float Latitude = HexToFloat(LatHexStr);

            return new GeoCoordinate(Latitude,Longitude);
        }

        /// <summary>
        /// Convert hex string content to a float.
        /// 0xff20f342 -> 121.564445F
        /// </summary>
        /// <param name="Hex"></param>
        /// <returns></returns>
        private static float HexToFloat(string Hex)
        {
            // Hex string content to a byte array.
            byte[] Bytes = new byte[4];
            Bytes[0] = System.Convert.ToByte(Hex.Substring(0, 2), 16);
            Bytes[1] = System.Convert.ToByte(Hex.Substring(2, 2), 16);
            Bytes[2] = System.Convert.ToByte(Hex.Substring(4, 2), 16);
            Bytes[3] = System.Convert.ToByte(Hex.Substring(6, 2), 16);

            // byte array to a float.
            return BitConverter.ToSingle(Bytes,0);
        }
    }
}
