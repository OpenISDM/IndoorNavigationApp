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
 * 
 *      The algorithm of route planning.
 * 
 * File Name:
 *
 *      RoutePlan.cs
 *
 * Abstract:
 *
 *      
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
    /// route planning
    /// Dijkstra algorithm
    /// </summary>
    public class WaypointRoutePlan
    {
        private Graph<WaypointModel, string> navigraph =
            new Graph<WaypointModel, string>();
        private readonly List<LocationConnectModel> locationConnects;

        /// <summary>
        /// Initialize the element
        /// </summary>
        /// <param name="Waypoints">Location information </param>
        /// <param name="LocationConnects">the path information </param>
        public WaypointRoutePlan(List<WaypointModel> Waypoints,
            List<LocationConnectModel> LocationConnects)
        {
            // Add a waypoint
            foreach (var waypoint in Waypoints)
                navigraph.AddNode(waypoint);

            // Set the path
            this.locationConnects = LocationConnects;
            foreach (var locationConnect in LocationConnects)
            {
                // Get the distance of two locations which in cm.
                int distance = System.Convert.ToInt32(
                    locationConnect.BeaconA.Coordinates
                    .GetDistanceTo(locationConnect.BeaconB.Coordinates) * 100);

                // Get two connected location's key vaule
                uint beaconAKey = navigraph.Where(BeaconGroup =>
                        BeaconGroup.Item == locationConnect.BeaconA)
                        .Select(BeaconGroup => BeaconGroup.Key).First();
                uint beaconBKey = navigraph.Where(BeaconGroup =>
                        BeaconGroup.Item == locationConnect.BeaconB)
                        .Select(BeaconGroup => BeaconGroup.Key).First();

                // Connect two waypoints
                if (locationConnect.IsTwoWay)
                {
                    navigraph.Connect(beaconAKey, beaconBKey,
                                        distance, string.Empty);
                    navigraph.Connect(beaconBKey, beaconAKey,
                                        distance, string.Empty);
                }
                else
                    navigraph.Connect(beaconAKey, beaconBKey,
                                        distance, string.Empty);
            }
        }

        /// <summary>
        /// Get the optimal path
        /// </summary>
        /// <param name="StartBeacon"></param>
        /// <param name="EndWaypoint"></param>
        /// <returns></returns>
        public Queue<NextStepModel> GetPath(
            Beacon StartBeacon, WaypointModel EndWaypoint)
        {
            // Find where is the the start beacon and find the key vaule
            uint startPoingKey = navigraph
                .Where(BeaconGroup => BeaconGroup.Item.Beacons
                .Contains(StartBeacon)).Select(c => c.Key).First();
            uint endPointKey = navigraph
                .Where(c => c.Item == EndWaypoint).Select(c => c.Key).First();
            // Get the optimal path
            var path = navigraph.Dijkstra(startPoingKey, endPointKey).GetPath();

            Queue<NextStepModel> pathQueue =
                new Queue<NextStepModel>();
            // Compute the angle to the next location
            for (int i = 0; i < path.Count() - 1; i++)
            {
                // Get the current location and next location
                WaypointModel currentWaypoint = navigraph[path.ToList()[i]].Item;
                WaypointModel nextWaypoint = navigraph[path.ToList()[i + 1]].Item;

                // If i=0, it represented that it needs to compute the initial
                // direction fo start point
                if (i == 0)
                {
                    // Don't need to compute the direction because user should
                    // get the wrong way first, and then calibrate the direction
                    pathQueue.Enqueue(new NextStepModel
                    {
                        NextWaypoint = nextWaypoint,
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
                    WaypointModel previousWaypoint =
                        navigraph[path.ToList()[i - 1]].Item;

                    // Compute the angle to turn. It is also be stored with
                    // next location to the path queue.
                    int angle = RotateAngle.GetRotateAngle(
                        currentWaypoint.Coordinates,
                        previousWaypoint.Coordinates,
                        nextWaypoint.Coordinates);

                    pathQueue.Enqueue(new NextStepModel {
                        NextWaypoint = nextWaypoint,
                        Angle = angle
                    });
                }
            }

            return pathQueue;
        }

        /// <summary>
        /// Reacquire the optimal path
        /// </summary>
        /// <param name="PreviousWaypoint"></param>
        /// <param name="CurrentBeacon"></param>
        /// <param name="EndWaypoint"></param>
        /// <returns></returns>
        public Queue<NextStepModel> RegainPath(
            WaypointModel PreviousWaypoint,
            Beacon CurrentBeacon,
            WaypointModel EndWaypoint)
        {
            // Find where is the the start beacon and find the key vaule
            var startPoingKey = navigraph
                .Where(beaconGroup => beaconGroup.Item.Beacons
                .Contains(CurrentBeacon)).Select(c => c.Key).First();
            var endPointKey = navigraph
                .Where(c => c.Item == EndWaypoint).Select(c => c.Key).First();

            // Check the current location if is connected to the last 
            // location.
            // If the user skips more than two location, the last location is
            // less meaningful.
            if (this.locationConnects.Where(c =>
            (c.BeaconA == navigraph[startPoingKey].Item && 
            c.BeaconB == PreviousWaypoint) ||
            (c.BeaconA == PreviousWaypoint && 
            c.BeaconB == navigraph[startPoingKey].Item))
            .Count() == 0)
                throw new ArgumentException(
                    "The current point is independent of the previous point."
                    );

            // Get the optimal path
            var path = navigraph.Dijkstra(startPoingKey, endPointKey).GetPath();

            Queue<NextStepModel> pathQueue =
                new Queue<NextStepModel>();

            // Compute the angle to turn to next location
            for (int i = 0; i < path.Count() - 1; i++)
            {
                // Acquire the current and next location
                WaypointModel currentWaypoint = navigraph[path.ToList()[i]].Item;
                WaypointModel nextWaypoint = navigraph[path.ToList()[i + 1]].Item;


                if (i == 0)
                {
                    // The function of redirection can use last node to 
                    // compute the angle to turn
                    int angle = RotateAngle.GetRotateAngle(
                        currentWaypoint.Coordinates,
                        PreviousWaypoint.Coordinates,
                        nextWaypoint.Coordinates
                    );
                    pathQueue.Enqueue(new NextStepModel
                    {
                        NextWaypoint = nextWaypoint,
                        Angle = angle
                    });
                }
                else
                {
                    // Use the last node in the queue to compute the angle
                    PreviousWaypoint = navigraph[path.ToList()[i - 1]].Item;
                    int angle = RotateAngle.GetRotateAngle(
                        currentWaypoint.Coordinates,
                        PreviousWaypoint.Coordinates,
                        nextWaypoint.Coordinates
                        );
                    pathQueue.Enqueue(new NextStepModel {
                        NextWaypoint = nextWaypoint,
                        Angle = angle
                    });
                }
            }

            return pathQueue;
        }
    }
}
