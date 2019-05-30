 /*
  Copyright (c) 2018 Academia Sinica, Institude of Information Science
 
    License:
        GPL 3.0 : The content of this file is subject to the terms and
        conditions defined in file 'COPYING.txt', which is part of this source
        code package.
 
    Project Name:
 
        IndoorNavigation
 
    File Description:
 
        The algorithm of route planning

    File Name:

        WaypointRoutePlan.cs

    Abstract:

        

    Authors:
 
        Kenneth Tang, kenneth@gm.nssh.ntpc.edu.tw
        Paul Chang, paulchang@iis.sinica.edu.tw       
 
 */

//TODO: DUPLICATE

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
        /// Connect each to waypoint into navigation graph
        /// </summary>
        /// <param name="Waypoints">Location information </param>
        /// <param name="LocationConnects">the path information </param>
        public WaypointRoutePlan(List<WaypointModel> Waypoints,
            List<LocationConnectModel> LocationConnects)
        {
            // Add all the waypoints
            foreach (var waypoint in Waypoints)
                navigraph.AddNode(waypoint);

            // Set the path
            this.locationConnects = LocationConnects;
            foreach (var locationConnect in LocationConnects)
            {
                // Get the distance of two locations which in centimeter
                int distance = System.Convert.ToInt32(
                    locationConnect.SourceWaypoint.Coordinates
                    .GetDistanceTo(locationConnect.TargetWaypoint.Coordinates)*100);

                // Get two connected location's key value
                uint beaconAKey = navigraph.Where(BeaconGroup =>
                        BeaconGroup.Item == locationConnect.SourceWaypoint)
                        .Select(BeaconGroup => BeaconGroup.Key).First();
                uint beaconBKey = navigraph.Where(BeaconGroup =>
                        BeaconGroup.Item == locationConnect.TargetWaypoint)
                        .Select(BeaconGroup => BeaconGroup.Key).First();

                // Connect the waypoint
                navigraph.Connect(beaconAKey, beaconBKey,
                    distance, string.Empty);
            }
        }

        /// <summary>
        /// Get the optimal path by using Dijkstra's algorithm
        /// </summary>
        /// <param name="StartBeacon"></param>
        /// <param name="EndWaypoint"></param>
        /// <returns></returns>
        public Queue<NextStepModel> GetPath(
            Beacon StartBeacon, WaypointModel EndWaypoint)
        {
            // Find where is the the start beacon and find the key vaule
            uint startPointKey = navigraph
                .Where(BeaconGroup => BeaconGroup.Item.Beacons
                .Contains(StartBeacon)).Select(c => c.Key).First();
            uint endPointKey = navigraph
                .Where(c => c.Item == EndWaypoint).Select(c => c.Key).First();
            // Get the optimal path
            var path = navigraph.Dijkstra(startPointKey,
                                            endPointKey).GetPath();

            Queue<NextStepModel> pathQueue =
                new Queue<NextStepModel>();
            // Compute the angle to the next location
            for (int i = 0; i < path.Count() - 1; i++)
            {
                // Get the current location and next location
                WaypointModel currentWaypoint =
                                navigraph[path.ToList()[i]].Item;
                WaypointModel nextWaypoint =
                                navigraph[path.ToList()[i + 1]].Item;

                // If i=0, it represented that it needs to compute the initial
                // direction fo start point
                if (i == 0)
                {
                    // Don't need to compute the direction because user 
                    // should get the wrong way first, 
                    // and then calibrate the direction.
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
                    //// the direction to face first. 
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

                    pathQueue.Enqueue(new NextStepModel
                    {
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

            // Check the current location whether is connected to the
            // previous location. If the user skips more than two location,
            // the previous location is useless.
            if (!locationConnects.Any(c =>
            (c.SourceWaypoint == navigraph[startPoingKey].Item &&
            c.TargetWaypoint == PreviousWaypoint) ||
            (c.SourceWaypoint == PreviousWaypoint &&
            c.TargetWaypoint == navigraph[startPoingKey].Item)))
            {
                throw new ArgumentException("The current waypoint is " +
                    "independent of the previous waypoint.");
            }

            // Get the optimal path
            var path = navigraph.Dijkstra(startPoingKey,
                                            endPointKey).GetPath();

            Queue<NextStepModel> pathQueue = new Queue<NextStepModel>();

            // Compute the angle to turn to the next waypoint
            for (int i = 0; i < path.Count() - 1; i++)
            {
                // Acquire the current and next waypoint
                WaypointModel currentWaypoint =
                                navigraph[path.ToList()[i]].Item;
                WaypointModel nextWaypoint =
                                navigraph[path.ToList()[i + 1]].Item;

                if (i == 0)
                {
                    // The function of redirection can use last node to 
                    // compute angle to the next waypoint
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
                    // Use the previous node in the queue to compute the angle
                    PreviousWaypoint = navigraph[path.ToList()[i - 1]].Item;
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
            }

            return pathQueue;
        }

        /* Find error point by using tree search within threshold distance */
        /*
        /// <summary>
        /// Get the beacon list for beacon scanning 
        /// </summary>
        public List<List<Guid>> GetScanList(
                                    List<NextStepModel> NavigationPath,
                                    double ThresholdDistance)
        {
            List<List<Guid>> returnedList = new List<List<Guid>>();

            List<Guid> _tempGuidList;
            for (int i = 0; i < NavigationPath.Count - 1; i++)
            {
                _tempGuidList = new List<Guid>();

                //find the branch that is both connect with current step node
                //but not connect with next step node
                var adjacentBeacons = GetConnectedBeacons(NavigationPath[i].NextWaypoint.Beacons,
                                                          NavigationPath, i);

                //add the previous beacon to _tempGuidList
                if (i != 0)
                    _tempGuidList.Add(NavigationPath[i - 1].NextWaypoint.Beacons[0].UUID);

                for (int j = 0; j < adjacentBeacons.Count(); j++)
                {
                    if (NavigationPath[i].NextWaypoint.Beacons[0].GetCoordinates().GetDistanceTo(
                        adjacentBeacons.ElementAt(j)[0].GetCoordinates()) >= ThresholdDistance)
                    {
                        _tempGuidList.AddRange(from beacon in adjacentBeacons.ElementAt(j)
                                               select beacon.UUID);
                    }
                    else
                    {
                        IEnumerable<List<Beacon>> connectedBeacon = GetConnectedBeacons(adjacentBeacons.ElementAt(j), NavigationPath, i);

                        if (connectedBeacon.Any())
                        {
                            //remove the connected beacons that included in 
                            //the adjacentBeacons list
                            adjacentBeacons = adjacentBeacons.Concat(from beaconList in connectedBeacon
                                                                     where !beaconList.Intersect(adjacentBeacons.SelectMany(list => list)).Any()
                                                                     select beaconList);
                        }
                    }
                }

                returnedList.Add(_tempGuidList.Distinct().ToList());
            }

            return returnedList;
        }

        /// <summary>
        /// Get the beacon which is connect with
        /// </summary>
        private IEnumerable<List<Beacon>> GetConnectedBeacons(List<Beacon> previousConnectedBeacon, 
                                                              List<NextStepModel> navigationPath, int indexOfPath)
        {
            var returnedList = (from locationConnect in Utility.LocationConnects
                                where locationConnect.SourceWaypoint.Beacons.Intersect(previousConnectedBeacon).Any() &&
                                      !locationConnect.TargetWaypoint.Beacons.Intersect(navigationPath[indexOfPath].NextWaypoint.Beacons).Any() &&
                                      !locationConnect.TargetWaypoint.Beacons.Intersect(navigationPath[indexOfPath + 1].NextWaypoint.Beacons).Any()
                                select locationConnect.TargetWaypoint.Beacons).
                               Concat(from locationConnect in Utility.LocationConnects
                                      where locationConnect.TargetWaypoint.Beacons.Intersect(previousConnectedBeacon).Any() &&
                                            !locationConnect.SourceWaypoint.Beacons.Intersect(navigationPath[indexOfPath].NextWaypoint.Beacons).Any() &&
                                            !locationConnect.SourceWaypoint.Beacons.Intersect(navigationPath[indexOfPath + 1].NextWaypoint.Beacons).Any()
                                      select locationConnect.SourceWaypoint.Beacons);

            //filter the beacon which is from previous step
            if (indexOfPath != 0)
                returnedList = returnedList.Where(b => !b.Intersect(navigationPath[indexOfPath - 1].NextWaypoint.Beacons).Any());

            return returnedList;
        }
        */

    }
}
