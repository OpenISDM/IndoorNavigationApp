using System.Collections.Generic;
using Dijkstra.NET.Model;
using Dijkstra.NET.Extensions;
using IndoorNavigation.Models;
using System.Linq;
using IndoorNavigation.Modules.Utility;

namespace IndoorNavigation.Modules.Navigation
{
    /// <summary>
    /// 路徑規劃
    /// </summary>
    public class RoutePlan
    {
        private Graph<BeaconGroupModel, string> Map = 
            new Graph<BeaconGroupModel, string>();

        /// <summary>
        /// 路徑規劃
        /// </summary>
        /// <param name="BeaconGroups">地點資料</param>
        /// <param name="LocationConnects">地點關聯</param>
        public RoutePlan(List<BeaconGroupModel> BeaconGroups, 
            List<LocationConnectModel> LocationConnects)
        {
            // 加入地點
            foreach (var BeaconGroup in BeaconGroups)
                Map.AddNode(BeaconGroup);

            // 設定道路
            foreach (var LocationConnect in LocationConnects)
            {
                // 取得兩個地點間的距離
                // 距離以公分為單位
                int Distance = System.Convert.ToInt32(
                    LocationConnect.BeaconA.Coordinate
                    .GetDistanceTo(LocationConnect.BeaconB.Coordinate) * 100);

                // 將兩個地點連接起來
                if (LocationConnect.IsTwoWay)
                {
                    Map.Connect(
                        Map.Where(c => c.Item == LocationConnect.BeaconA)
                        .Select(c => c.Key).First(),
                        Map.Where(c => c.Item == LocationConnect.BeaconB)
                        .Select(c => c.Key).First(),
                        Distance,
                        string.Empty
                    );
                    Map.Connect(
                        Map.Where(c => c.Item == LocationConnect.BeaconB)
                        .Select(c => c.Key).First(),
                        Map.Where(c => c.Item == LocationConnect.BeaconA)
                        .Select(c => c.Key).First(),
                        Distance,
                        string.Empty
                    );
                }
                else
                    Map.Connect(
                        Map.Where(c => c.Item == LocationConnect.BeaconA)
                        .Select(c => c.Key).First(),
                        Map.Where(c => c.Item == LocationConnect.BeaconB)
                        .Select(c => c.Key).First(),
                        Distance,
                        string.Empty
                    );

            }

        }

        /// <summary>
        /// 取得最佳路線
        /// </summary>
        /// <param name="StartPoint"></param>
        /// <param name="EndPoint"></param>
        /// <returns></returns>
        public Queue<(BeaconGroupModel Next, int Angle)> GetPath(
            BeaconModel StartPoint, BeaconGroupModel EndPoint)
        {
            var StartPoingKey = Map
                .Where(beaconGroup => beaconGroup.Item.Beacons
                .Contains(StartPoint)).Select(c => c.Key).First();
            var EndPointKey = Map
                .Where(c => c.Item == EndPoint).Select(c => c.Key).First();
            // 取得最佳路線
            var Path = Map.Dijkstra(StartPoingKey,EndPointKey).GetPath();

            Queue<(BeaconGroupModel Next, int Angle)> PathQueue =
                new Queue<(BeaconGroupModel Next, int Angle)>();
            // 計算走向下一個地點要旋轉的角度
            for (int i = 0; i < Path.Count() - 1; i++)
            {
                if (i == 0)
                    PathQueue.Enqueue((Map[Path.ToList()[i + 1]].Item, 
                        RotateAngle.GetRotateAngle(
                        Convert.ToCoordinate(StartPoint.UUID),
                        StartPoint.MarkCoordinate,
                        Map[Path.ToList()[i + 1]].Item.Coordinate
                        )));
                else
                    PathQueue.Enqueue((Map[Path.ToList()[i + 1]].Item, 
                        RotateAngle.GetRotateAngle(
                        Map[Path.ToList()[i]].Item.Coordinate,
                        Map[Path.ToList()[i - 1]].Item.Coordinate,
                        Map[Path.ToList()[i + 1]].Item.Coordinate
                        )));
            }

            return PathQueue;
        }
    }
}
