using System.Collections.Generic;
using Dijkstra.NET.Model;
using Dijkstra.NET.Extensions;
using IndoorNavigation.Models;
using System.Linq;
using System;

namespace IndoorNavigation.Modules.Navigation
{
    /// <summary>
    /// 路徑規劃
    /// </summary>
    public class RoutePlan
    {
        private Graph<BeaconGroupModel, string> map =
            new Graph<BeaconGroupModel, string>();
        private List<LocationConnectModel> locationConnects;

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
                map.AddNode(BeaconGroup);

            // 設定道路
            this.locationConnects = LocationConnects;
            foreach (var locationConnect in LocationConnects)
            {
                // 取得兩個地點間的距離
                // 距離以公分為單位
                int distance = System.Convert.ToInt32(
                    locationConnect.BeaconA.Coordinate
                    .GetDistanceTo(locationConnect.BeaconB.Coordinate) * 100);

                uint beaconA = map.Where(BeaconGroup =>
                        BeaconGroup.Item == locationConnect.BeaconA)
                        .Select(BeaconGroup => BeaconGroup.Key).First();
                uint beaconB = map.Where(BeaconGroup =>
                        BeaconGroup.Item == locationConnect.BeaconB)
                        .Select(BeaconGroup => BeaconGroup.Key).First();

                // 將兩個地點連接起來
                if (locationConnect.IsTwoWay)
                {
                    map.Connect(beaconA,beaconB,distance,string.Empty);
                    map.Connect(beaconB,beaconA,distance,string.Empty);
                }
                else
                    map.Connect(beaconA,beaconB,distance,string.Empty);
            }
        }

        /// <summary>
        /// 取得最佳路線
        /// </summary>
        /// <param name="StartBeacon"></param>
        /// <param name="EndPoint"></param>
        /// <returns></returns>
        public Queue<NextInstructionModel> GetPath(
            Beacon StartBeacon, BeaconGroupModel EndPoint)
        {
            var startPoingKey = map
                .Where(BeaconGroup => BeaconGroup.Item.Beacons
                .Contains(StartBeacon)).Select(c => c.Key).First();
            var endPointKey = map
                .Where(c => c.Item == EndPoint).Select(c => c.Key).First();
            // 取得最佳路線
            var path = map.Dijkstra(startPoingKey, endPointKey).GetPath();

            Queue<NextInstructionModel> pathQueue =
                new Queue<NextInstructionModel>();
            // 計算走向下一個地點要旋轉的角度
            for (int i = 0; i < path.Count() - 1; i++)
            {
                BeaconGroupModel currentPoint = map[path.ToList()[i]].Item;
                BeaconGroupModel nextPoint = map[path.ToList()[i + 1]].Item;

                if (i == 0)
                    // 檢查起始點是否為LBeacon
                    // LBeacon上方有提示使用者一開始要面向的方向
                    if (StartBeacon.GetType() == typeof(LBeaconModel))
                        pathQueue.Enqueue(new NextInstructionModel {
                            NextPoint = nextPoint,
                            Angle = RotateAngle.GetRotateAngle(
                                    StartBeacon.GetCoordinate(),
                                    (StartBeacon as LBeaconModel)
                                    .MarkCoordinate,
                                    nextPoint.Coordinate)
                        });
                    else
                        pathQueue.Enqueue(new NextInstructionModel {
                            NextPoint = nextPoint, Angle = int.MaxValue});
                else
                {
                    BeaconGroupModel previousPoint = 
                        map[path.ToList()[i - 1]].Item;

                    pathQueue.Enqueue(new NextInstructionModel {
                        NextPoint = nextPoint,
                        Angle = RotateAngle.GetRotateAngle(
                        currentPoint.Coordinate,
                        previousPoint.Coordinate,
                        nextPoint.Coordinate)
                    });
                }
            }

            return pathQueue;
        }

        /// <summary>
        /// 重新取得最佳路線
        /// </summary>
        /// <param name="PreviousPoint"></param>
        /// <param name="CurrentBeacon"></param>
        /// <param name="EndPoint"></param>
        /// <returns></returns>
        public Queue<NextInstructionModel> RegainPath(
            BeaconGroupModel PreviousPoint,
            Beacon CurrentBeacon,
            BeaconGroupModel EndPoint)
        {
            var startPoingKey = map
                .Where(beaconGroup => beaconGroup.Item.Beacons
                .Contains(CurrentBeacon)).Select(c => c.Key).First();
            var endPointKey = map
                .Where(c => c.Item == EndPoint).Select(c => c.Key).First();

            //檢查上一個位置與現在位置是否連接
            if (this.locationConnects.Where(c =>
            (c.BeaconA == map[startPoingKey] && c.BeaconB == PreviousPoint) ||
            (c.BeaconA == PreviousPoint && c.BeaconB == map[startPoingKey]))
            .Count() == 0)
                new ArgumentException(
                    "The current point is independent of the previous point."
                    );

            // 取得最佳路線
            var path = map.Dijkstra(startPoingKey, endPointKey).GetPath();

            Queue<NextInstructionModel> pathQueue =
                new Queue<NextInstructionModel>();

            // 計算走向下一個地點要旋轉的角度
            for (int i = 0; i < path.Count() - 1; i++)
            {
                BeaconGroupModel currentPoint = map[path.ToList()[i]].Item;
                BeaconGroupModel nextPoint = map[path.ToList()[i + 1]].Item;

                if (i == 0)
                    pathQueue.Enqueue(new NextInstructionModel {
                        NextPoint = nextPoint,
                        Angle = RotateAngle.GetRotateAngle(
                        currentPoint.Coordinate,
                        PreviousPoint.Coordinate,
                        nextPoint.Coordinate
                        )
                    });
                else
                {
                    PreviousPoint = map[path.ToList()[i - 1]].Item;

                    pathQueue.Enqueue(new NextInstructionModel {
                        NextPoint = nextPoint,
                        Angle = RotateAngle.GetRotateAngle(
                        currentPoint.Coordinate,
                        PreviousPoint.Coordinate,
                        nextPoint.Coordinate
                        )
                    });
                }
            }

            return pathQueue;
        }
    }
}
