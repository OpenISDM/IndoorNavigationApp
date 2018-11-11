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
 *      RotePlan.cs
 * 
 * Abstract:
 *      
 *      規劃導航路徑的演算法
 *
 * Authors:
 * 
 *      Kenneth Tang, kenneth@gm.nssh.ntpc.edu.tw
 * 
 */

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
    /// 使用Dijkstra演算法
    /// </summary>
    public class RoutePlan
    {
        private Graph<BeaconGroupModel, string> map =
            new Graph<BeaconGroupModel, string>();
        private readonly List<LocationConnectModel> locationConnects;

        /// <summary>
        /// 初始化路徑規劃物件
        /// </summary>
        /// <param name="BeaconGroups">地點資料</param>
        /// <param name="LocationConnects">連接兩個地點的道路資料</param>
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

                // 找出要連接兩個地點的key值
                uint beaconAKey = map.Where(BeaconGroup =>
                        BeaconGroup.Item == locationConnect.BeaconA)
                        .Select(BeaconGroup => BeaconGroup.Key).First();
                uint beaconBKey = map.Where(BeaconGroup =>
                        BeaconGroup.Item == locationConnect.BeaconB)
                        .Select(BeaconGroup => BeaconGroup.Key).First();

                // 將兩個地點連接起來
                if (locationConnect.IsTwoWay)
                {
                    map.Connect(beaconAKey,beaconBKey,distance,string.Empty);
                    map.Connect(beaconBKey,beaconAKey,distance,string.Empty);
                }
                else
                    map.Connect(beaconAKey,beaconBKey,distance,string.Empty);
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
            // 先找出start beacon在哪個地點
            // 再找出這個地點的key值
            uint startPoingKey = map
                .Where(BeaconGroup => BeaconGroup.Item.Beacons
                .Contains(StartBeacon)).Select(c => c.Key).First();
            uint endPointKey = map
                .Where(c => c.Item == EndPoint).Select(c => c.Key).First();
            // 取得最佳路線
            var path = map.Dijkstra(startPoingKey, endPointKey).GetPath();

            Queue<NextInstructionModel> pathQueue =
                new Queue<NextInstructionModel>();
            // 計算走向下一個地點要旋轉的角度
            for (int i = 0; i < path.Count() - 1; i++)
            {
                // 取出現在的地點和下一個地點
                BeaconGroupModel currentPoint = map[path.ToList()[i]].Item;
                BeaconGroupModel nextPoint = map[path.ToList()[i + 1]].Item;

                // i=0代表要計算起點的方向
                if (i == 0)
                {
                    // 因為要先讓使用者走錯路，再校正方向。所以，不計算方向
                    pathQueue.Enqueue(new NextInstructionModel
                    {
                        NextPoint = nextPoint,
                        Angle = int.MaxValue
                    });

                    // 保留功能
                    // 之前開會討論要在LBeacon上貼標籤來校正起始方向
                    //// 檢查起始點是否為LBeacon
                    //// LBeacon上方有提示使用者一開始要面向的方向
                    //if (StartBeacon.GetType() == typeof(LBeaconModel))
                    //    pathQueue.Enqueue(new NextInstructionModel
                    //    {
                    //        NextPoint = nextPoint,
                    //        Angle = RotateAngle.GetRotateAngle(
                    //                StartBeacon.GetCoordinate(),
                    //                (StartBeacon as LBeaconModel)
                    //                .MarkCoordinate,
                    //                nextPoint.Coordinate)
                    //    });
                    //else
                    //    pathQueue.Enqueue(new NextInstructionModel
                    //    {
                    //        NextPoint = nextPoint,
                    //        Angle = int.MaxValue
                    //    });
                }
                else
                {
                    // 上一個地點
                    BeaconGroupModel previousPoint = 
                        map[path.ToList()[i - 1]].Item;

                    // 計算要轉向的角度，連同下一個位置一併放到路線佇列
                    int angle = RotateAngle.GetRotateAngle(
                        currentPoint.Coordinate,
                        previousPoint.Coordinate,
                        nextPoint.Coordinate);

                    pathQueue.Enqueue(new NextInstructionModel {
                        NextPoint = nextPoint,
                        Angle = angle
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
            // 先找出start beacon在哪個地點
            // 再找出這個地點的key值
            var startPoingKey = map
                .Where(beaconGroup => beaconGroup.Item.Beacons
                .Contains(CurrentBeacon)).Select(c => c.Key).First();
            var endPointKey = map
                .Where(c => c.Item == EndPoint).Select(c => c.Key).First();

            // 檢查上一個位置與現在位置是否連接
            // 假如使用者跳過2個地點以上，上一個位置就沒有參考價值
            if (this.locationConnects.Where(c =>
            (c.BeaconA == map[startPoingKey] && c.BeaconB == PreviousPoint) ||
            (c.BeaconA == PreviousPoint && c.BeaconB == map[startPoingKey]))
            .Count() == 0)
                throw new ArgumentException(
                    "The current point is independent of the previous point."
                    );

            // 取得最佳路線
            var path = map.Dijkstra(startPoingKey, endPointKey).GetPath();

            Queue<NextInstructionModel> pathQueue =
                new Queue<NextInstructionModel>();

            // 計算走向下一個地點要旋轉的角度
            for (int i = 0; i < path.Count() - 1; i++)
            {
                // 取出現在的地點和下一個地點
                BeaconGroupModel currentPoint = map[path.ToList()[i]].Item;
                BeaconGroupModel nextPoint = map[path.ToList()[i + 1]].Item;


                if (i == 0)
                {
                    // 重新導航的第一個點可以使用上一個位置計算轉向角度
                    int angle = RotateAngle.GetRotateAngle(
                        currentPoint.Coordinate,
                        PreviousPoint.Coordinate,
                        nextPoint.Coordinate
                    );
                    pathQueue.Enqueue(new NextInstructionModel
                    {
                        NextPoint = nextPoint,
                        Angle = angle
                    });
                }
                else
                {
                    // 使用路線佇列中的上一個點計算轉向角度
                    PreviousPoint = map[path.ToList()[i - 1]].Item;
                    int angle = RotateAngle.GetRotateAngle(
                        currentPoint.Coordinate,
                        PreviousPoint.Coordinate,
                        nextPoint.Coordinate
                        );
                    pathQueue.Enqueue(new NextInstructionModel {
                        NextPoint = nextPoint,
                        Angle = angle
                    });
                }
            }

            return pathQueue;
        }
    }
}
