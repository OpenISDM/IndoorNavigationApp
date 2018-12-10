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
 *      The algorithm of tour planning
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
    /// rout plan
    /// Dijkstra algorithm
    /// </summary>
    public class RoutePlan
    {
        private Graph<BeaconGroupModel, string> map =
            new Graph<BeaconGroupModel, string>();
        private readonly List<LocationConnectModel> locationConnects;

        /// <summary>
        /// Initialize the element
        /// </summary>
        /// <param name="BeaconGroups">Location information </param>
        /// <param name="LocationConnects">the path information </param>
        public RoutePlan(List<BeaconGroupModel> BeaconGroups,
            List<LocationConnectModel> LocationConnects)
        {
            // Add a location
            foreach (var BeaconGroup in BeaconGroups)
                map.AddNode(BeaconGroup);

            // Set the path
            this.locationConnects = LocationConnects;
            foreach (var locationConnect in LocationConnects)
            {
                // Get the distance of two locations which in cm.
                int distance = System.Convert.ToInt32(
                    locationConnect.BeaconA.Coordinate
                    .GetDistanceTo(locationConnect.BeaconB.Coordinate) * 100);

                // Get two connected location's key vaule
                uint beaconAKey = map.Where(BeaconGroup =>
                        BeaconGroup.Item == locationConnect.BeaconA)
                        .Select(BeaconGroup => BeaconGroup.Key).First();
                uint beaconBKey = map.Where(BeaconGroup =>
                        BeaconGroup.Item == locationConnect.BeaconB)
                        .Select(BeaconGroup => BeaconGroup.Key).First();

                // Connect two locations
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
        /// Get the best path
        /// </summary>
        /// <param name="StartBeacon"></param>
        /// <param name="EndPoint"></param>
        /// <returns></returns>
        public Queue<NextInstructionModel> GetPath(
            Beacon StartBeacon, BeaconGroupModel EndPoint)
        {
            // Find where is the the start beacon and find the key vaule
            uint startPoingKey = map
                .Where(BeaconGroup => BeaconGroup.Item.Beacons
                .Contains(StartBeacon)).Select(c => c.Key).First();
            uint endPointKey = map
                .Where(c => c.Item == EndPoint).Select(c => c.Key).First();
            // Get the best path
            var path = map.Dijkstra(startPoingKey, endPointKey).GetPath();

            Queue<NextInstructionModel> pathQueue =
                new Queue<NextInstructionModel>();
            // Compute the angle to the next location
            for (int i = 0; i < path.Count() - 1; i++)
            {
                // Get the current location and next location
                BeaconGroupModel currentPoint = map[path.ToList()[i]].Item;
                BeaconGroupModel nextPoint = map[path.ToList()[i + 1]].Item;

                // If i=0, it represented that it needs to compute the initial
                // directino fo start point
                if (i == 0)
                {
                    // Don't need to compute the direction because user should
                    // get the wrong way first, and then calibrate the 
                    // direction
                    pathQueue.Enqueue(new NextInstructionModel
                    {
                        NextPoint = nextPoint,
                        Angle = int.MaxValue
                    });

                    // Reserved function
                    // According to the meeting before, there is a sticker on
                    // LBeacon to show the initial direction
                    //// Check the starting point if is under the LBeacon
                    //// There is a sign on LBeacon to show the user where is
                    //// the diresction to face first. 
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
                    // Last location
                    BeaconGroupModel previousPoint =
                        map[path.ToList()[i - 1]].Item;

                    // Compute the angle to turn. It is also be stored with
                    // next location to the path queue.
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
        /// Reacquire the best path
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
            // Find where is the the start beacon and find the key vaule
            var startPoingKey = map
                .Where(beaconGroup => beaconGroup.Item.Beacons
                .Contains(CurrentBeacon)).Select(c => c.Key).First();
            var endPointKey = map
                .Where(c => c.Item == EndPoint).Select(c => c.Key).First();

            // Check the current location if is connected to the last 
            // location.
            // If the user skips more than two location, the last location is
            // less meaningful.
            if (this.locationConnects.Where(c =>
            (c.BeaconA == map[startPoingKey].Item && 
            c.BeaconB == PreviousPoint) ||
            (c.BeaconA == PreviousPoint && 
            c.BeaconB == map[startPoingKey].Item))
            .Count() == 0)
                throw new ArgumentException(
                    "The current point is independent of the previous point."
                    );

            // Get the best path
            var path = map.Dijkstra(startPoingKey, endPointKey).GetPath();

            Queue<NextInstructionModel> pathQueue =
                new Queue<NextInstructionModel>();

            // Compute the angle to turn to next location
            for (int i = 0; i < path.Count() - 1; i++)
            {
                // Acquire the current and next location
                BeaconGroupModel currentPoint = map[path.ToList()[i]].Item;
                BeaconGroupModel nextPoint = map[path.ToList()[i + 1]].Item;


                if (i == 0)
                {
                    // The function of redirection can use last node to 
                    // compute the angle to turn
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
                    // Use the last node in the queue to compute the angle
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
