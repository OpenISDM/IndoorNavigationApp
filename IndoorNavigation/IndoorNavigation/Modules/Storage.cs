using System;
using System.IO;
using System.Linq;

namespace IndoorNavigation.Modules
{
    public static class MapStorage
    {
        private static readonly string MapFolder =
            Path.Combine(Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData), "Maps");

        private static object FileLock = new object();

        public static string[] GetAllPlace()
        {
            if (!Directory.Exists(MapFolder))
                Directory.CreateDirectory(MapFolder);

            return Directory.GetFiles(MapFolder)
                .Select(path => Path.GetFileName(path))
                .OrderBy(file => file).ToArray();
        }

        private static string LoadFile(string FileName)
        {
            string FilePath = Path.Combine(MapFolder, FileName);
            if (!Directory.Exists(MapFolder))
            {
                Directory.CreateDirectory(MapFolder);
                return string.Empty;
            }

            if (!File.Exists(FilePath))
                return string.Empty;

            lock(FileLock)
                return File.ReadAllText(FilePath);
        }

        public static bool SaveMapInformation(string Place,string MapDatas)
        {
            string FilePath = Path.Combine(MapFolder, Place);
            try
            {
                if (!Directory.Exists(MapFolder))
                    Directory.CreateDirectory(MapFolder);

                lock (FileLock)
                    File.WriteAllText(FilePath, MapDatas);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static void DeleteMap(string Place)
        {
            string FilePath = Path.Combine(MapFolder, Place);
            if (!Directory.Exists(MapFolder))
                Directory.CreateDirectory(MapFolder);

            lock (FileLock)
                File.Delete(FilePath);
        }

        public static void DeleteAllMap()
        {
            foreach (string Place in GetAllPlace())
                DeleteMap(Place);
        }
    }
}
