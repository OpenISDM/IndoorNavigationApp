using System;
using System.Collections.Generic;
using System.Text;

namespace IndoorNavigation
{
    public partial class Convert
    {
        public static (float Longitude, float Latitude) ToCoordinate(Guid UUID)
        {
            string[] IdShards = UUID.ToString().Split('-');
            string LonHexStr = IdShards[2] + IdShards[3];
            string LatHexStr = IdShards[4].Substring(4,8);

            float Longitude = BitConverter.ToSingle(HexToBytes(LonHexStr), 0);
            float Latitude = BitConverter.ToSingle(HexToBytes(LatHexStr), 0);

            return (Longitude, Latitude);
        }

        private static byte[] HexToBytes(string Hex)
        {
            byte[] Bytes = new byte[4];
            Bytes[0] = System.Convert.ToByte(Hex.Substring(0, 2), 16);
            Bytes[1] = System.Convert.ToByte(Hex.Substring(2, 2), 16);
            Bytes[2] = System.Convert.ToByte(Hex.Substring(4, 2), 16);
            Bytes[3] = System.Convert.ToByte(Hex.Substring(6, 2), 16);
            return Bytes;
        }
    }
}
