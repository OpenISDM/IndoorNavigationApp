using System.Collections.Generic;
using Dijkstra.NET.Model;
using Dijkstra.NET.Extensions;
using IndoorNavigation.Models;
using System.Linq;
using IndoorNavigation.Modules.Utility;
using System;

namespace IndoorNavigation.Modules.Navigation
{
    /// <summary>
    /// 路徑規劃
    /// </summary>
    public class RoutePlan
    {
        private Graph<BeaconGroupModel, string> Map =
            new Graph<BeaconGroupModel, string>();
        private List<LocationConnectModel> LocationConnects;

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
            this.LocationConnects = LocationConnects;
            foreach (var LocationConnect in LocationConnects)
            {
                // 取得兩個地點間的距離
                // 距離以公分為單位
                int Distance = System.Convert.ToInt32(
                    LocationConnect.BeaconA.Coordinate
                    .GetDistanceTo(LocationConnect.BeaconB.Coordinate) * 100);

                uint BeaconA = Map.Where(BeaconGroup =>
                        BeaconGroup.Item == LocationConnect.BeaconA)
                        .Select(BeaconGroup => BeaconGroup.Key).First();
                uint BeaconB = Map.Where(BeaconGroup =>
                        BeaconGroup.Item == LocationConnect.BeaconB)
                        .Select(BeaconGroup => BeaconGroup.Key).First();

                // 將兩個地點連接起來
                if (LocationConnect.IsTwoWay)
                {
                    Map.Connect(BeaconA,BeaconB,Distance,string.Empty);
                    Map.Connect(BeaconB,BeaconA,Distance,string.Empty);
                }
                else
                    Map.Connect(BeaconA,BeaconB,Distance,string.Empty);
            }
        }

        /// <summary>
        /// 取得最佳路線
        /// </summary>
        /// <param name="StartBeacon"></param>
        /// <param name="EndPoint"></param>
        /// <returns></returns>
        public Queue<(BeaconGroupModel Next, int Angle)> GetPath(
            Beacon StartBeacon, BeaconGroupModel EndPoint)
        {
            var StartPoingKey = Map
                .Where(BeaconGroup => BeaconGroup.Item.Beacons
                .Contains(StartBeacon)).Select(c => c.Key).First();
            var EndPointKey = Map
                .Where(c => c.Item == EndPoint).Select(c => c.Key).First();
            // 取得最佳路線
            var Path = Map.Dijkstra(StartPoingKey, EndPointKey).GetPath();

            Queue<(BeaconGroupModel Next, int Angle)> PathQueue =
                new Queue<(BeaconGroupModel Next, int Angle)>();
            // 計算走向下一個地點要旋轉的角度
            for (int i = 0; i < Path.Count() - 1; i++)
            {
                BeaconGroupModel CurrentPoint = Map[Path.ToList()[i]].Item;
                BeaconGroupModel NextPoint = Map[Path.ToList()[i + 1]].Item;

                if (i == 0)
                    // 檢查起始點是否為LBeacon
                    // LBeacon上方有提示使用者一開始要面向的方向
                    if (StartBeacon.GetType() == typeof(LBeaconModel))
                        PathQueue.Enqueue((NextPoint,
                            RotateAngle.GetRotateAngle(
                            StartBeacon.GetCoordinate(),
                            (StartBeacon as LBeaconModel).MarkCoordinate,
                            NextPoint.Coordinate
                            )));
                    else
                        PathQueue.Enqueue(
                            (NextPoint, int.MinValue));
                else
                {
                    BeaconGroupModel PreviousPoint = 
                        Map[Path.ToList()[i - 1]].Item;

                    PathQueue.Enqueue((NextPoint,
                        RotateAngle.GetRotateAngle(
                        CurrentPoint.Coordinate,
                        PreviousPoint.Coordinate,
                        NextPoint.Coordinate
                        )));
                }
            }

            return PathQueue;
        }

        /// <summary>
        /// 重新取得最佳路線
        /// </summary>
        /// <param name="PreviousPoint"></param>
        /// <param name="CurrentBeacon"></param>
        /// <param name="EndPoint"></param>
        /// <returns></returns>
        public Queue<(BeaconGroupModel Next, int Angle)> RegainPath(
            BeaconGroupModel PreviousPoint,
            Beacon CurrentBeacon,
            BeaconGroupModel EndPoint)
        {
            var StartPoingKey = Map
                .Where(beaconGroup => beaconGroup.Item.Beacons
                .Contains(CurrentBeacon)).Select(c => c.Key).First();
            var EndPointKey = Map
                .Where(c => c.Item == EndPoint).Select(c => c.Key).First();

            //檢查上一個位置與現在位置是否連接
            if (this.LocationConnects.Where(c =>
            (c.BeaconA == Map[StartPoingKey] && c.BeaconB == PreviousPoint) ||
            (c.BeaconA == PreviousPoint && c.BeaconB == Map[StartPoingKey]))
            .Count() == 0)
                new ArgumentException(
                    "The current point is independent of the previous point."
                    );

            // 取得最佳路線
            var Path = Map.Dijkstra(StartPoingKey, EndPointKey).GetPath();

            Queue<(BeaconGroupModel Next, int Angle)> PathQueue =
                new Queue<(BeaconGroupModel Next, int Angle)>();

            // 計算走向下一個地點要旋轉的角度
            for (int i = 0; i < Path.Count() - 1; i++)
            {
                BeaconGroupModel CurrentPoint = Map[Path.ToList()[i]].Item;
                BeaconGroupModel NextPoint = Map[Path.ToList()[i + 1]].Item;

                if (i == 0)
                    PathQueue.Enqueue((NextPoint,
                        RotateAngle.GetRotateAngle(
                        CurrentPoint.Coordinate,
                        PreviousPoint.Coordinate,
                        NextPoint.Coordinate
                        )));
                else
                {
                    PreviousPoint = Map[Path.ToList()[i - 1]].Item;

                    PathQueue.Enqueue((NextPoint,
                        RotateAngle.GetRotateAngle(
                        CurrentPoint.Coordinate,
                        PreviousPoint.Coordinate,
                        NextPoint.Coordinate
                        )));
                }
            }

            return PathQueue;
        }
    }
}
