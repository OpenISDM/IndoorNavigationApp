using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using IndoorNavigation.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace IndoorNavigation.Modules
{
    public static class MapStorage
    {
        private static readonly string mapFolder =
            Path.Combine(Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData), "Maps");

        private static object fileLock = new object();

        /// <summary>
        /// 傳回地圖存放區的所有場所名稱
        /// </summary>
        /// <returns></returns>
        public static string[] GetAllPlace()
        {
            // 檢查地圖資料夾是否存在
            if (!Directory.Exists(mapFolder))
                Directory.CreateDirectory(mapFolder);

            return Directory.GetFiles(mapFolder)
                .Select(path => Path.GetFileName(path))
                .OrderBy(file => file).ToArray();
        }

        /// <summary>
        /// 載入地圖
        /// </summary>
        /// <param name="Place"></param>
        /// <returns></returns>
        public static bool LoadMap(string Place)
        {
            try
            {
                // 轉換Beacon及地圖相關資料
                JObject data = JsonConvert.DeserializeObject<JObject>(LoadFile(Place));

                string beaconJson = data["Beacon"].ToString();
                Utility.Beacons = beaconJson.ToBeacons();
                Utility.BeaconGroups = (JsonConvert.DeserializeObject<List<BeaconGroupModelForMapFile>>(data["BeaconGroup"].ToString()) as List<BeaconGroupModelForMapFile>).ToBeaconGroup(Utility.Beacons);
                Utility.LocationConnects = (JsonConvert.DeserializeObject<List<LocationConnectModelForMapFile>>(data["LocationConnect"].ToString()) as List<LocationConnectModelForMapFile>).ToLocationConnect(Utility.BeaconGroups);

                // 初始化路徑規劃物件及設定地圖資料
                Utility.Route = new Navigation.RoutePlan(
                    Utility.BeaconGroups, 
                    Utility.LocationConnects);

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 傳回指定場所的地圖資料
        /// </summary>
        /// <param name="FileName"></param>
        /// <returns></returns>
        private static string LoadFile(string FileName)
        {
            string filePath = Path.Combine(mapFolder, FileName);

            // 檢查地圖資料夾是否存在
            if (!Directory.Exists(mapFolder))
            {
                Directory.CreateDirectory(mapFolder);
                return string.Empty;
            }

            // 檢查地圖檔案是否存在
            if (!File.Exists(filePath))
                return string.Empty;

            lock(fileLock)
                return File.ReadAllText(filePath);
        }

        /// <summary>
        /// 儲存場所的地圖資料
        /// </summary>
        /// <param name="Place"></param>
        /// <param name="MapDatas"></param>
        /// <returns></returns>
        public static bool SaveMapInformation(string Place,string MapDatas)
        {
            string filePath = Path.Combine(mapFolder, Place);
            try
            {
                // 檢查地圖資料夾是否存在
                if (!Directory.Exists(mapFolder))
                    Directory.CreateDirectory(mapFolder);

                // 寫入地圖資料
                lock (fileLock)
                    File.WriteAllText(filePath, MapDatas);

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 刪除指定場所的地圖資料
        /// </summary>
        /// <param name="Place"></param>
        public static void DeleteMap(string Place)
        {
            string filePath = Path.Combine(mapFolder, Place);

            // 檢查地圖資料夾是否存在
            if (!Directory.Exists(mapFolder))
                Directory.CreateDirectory(mapFolder);

            lock (fileLock)
                File.Delete(filePath);
        }

        /// <summary>
        /// 刪除所有地圖資料
        /// </summary>
        public static void DeleteAllMap()
        {
            foreach (string place in GetAllPlace())
                DeleteMap(place);
        }
    }
}
