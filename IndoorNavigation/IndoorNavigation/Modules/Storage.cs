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
        private static readonly string MapFolder =
            Path.Combine(Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData), "Maps");

        private static object FileLock = new object();

        /// <summary>
        /// 傳回地圖存放區的所有場所名稱
        /// </summary>
        /// <returns></returns>
        public static string[] GetAllPlace()
        {
            // 檢查地圖資料夾是否存在
            if (!Directory.Exists(MapFolder))
                Directory.CreateDirectory(MapFolder);

            return Directory.GetFiles(MapFolder)
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
                JObject Data = JsonConvert.DeserializeObject<JObject>(LoadFile(Place));

                string BeaconJson = Data["Beacon"].ToString();
                Utility.Beacons = BeaconJson.ToBeacons();
                Utility.BeaconGroups = (JsonConvert.DeserializeObject<List<BeaconGroupModelForMapFile>>(Data["BeaconGroup"].ToString()) as List<BeaconGroupModelForMapFile>).ToBeaconGroup(Utility.Beacons);
                Utility.LocationConnects = (JsonConvert.DeserializeObject<List<LocationConnectModelForMapFile>>(Data["LocationConnect"].ToString()) as List<LocationConnectModelForMapFile>).ToLocationConnect(Utility.BeaconGroups);

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
            string FilePath = Path.Combine(MapFolder, FileName);

            // 檢查地圖資料夾是否存在
            if (!Directory.Exists(MapFolder))
            {
                Directory.CreateDirectory(MapFolder);
                return string.Empty;
            }

            // 檢查地圖檔案是否存在
            if (!File.Exists(FilePath))
                return string.Empty;

            lock(FileLock)
                return File.ReadAllText(FilePath);
        }

        /// <summary>
        /// 儲存場所的地圖資料
        /// </summary>
        /// <param name="Place"></param>
        /// <param name="MapDatas"></param>
        /// <returns></returns>
        public static bool SaveMapInformation(string Place,string MapDatas)
        {
            string FilePath = Path.Combine(MapFolder, Place);
            try
            {
                // 檢查地圖資料夾是否存在
                if (!Directory.Exists(MapFolder))
                    Directory.CreateDirectory(MapFolder);

                // 寫入地圖資料
                lock (FileLock)
                    File.WriteAllText(FilePath, MapDatas);

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
            string FilePath = Path.Combine(MapFolder, Place);

            // 檢查地圖資料夾是否存在
            if (!Directory.Exists(MapFolder))
                Directory.CreateDirectory(MapFolder);

            lock (FileLock)
                File.Delete(FilePath);
        }

        /// <summary>
        /// 刪除所有地圖資料
        /// </summary>
        public static void DeleteAllMap()
        {
            foreach (string Place in GetAllPlace())
                DeleteMap(Place);
        }
    }
}
