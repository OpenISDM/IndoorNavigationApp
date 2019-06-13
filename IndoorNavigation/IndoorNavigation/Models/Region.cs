/*
 * Copyright (c) 2019 Academia Sinica, Institude of Information Science
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
 *      This class has the mapping between waypoints and resources of the
 *      underlying IPS.
 *      
 * Version:
 *
 *      1.0.0, 20190605
 * 
 * File Name:
 *
 *      Region.cs
 *
 * Abstract:
 *
 *      Waypoint-based navigator is a mobile Bluetooth navigation application
 *      that runs on smart phones. It is structed to support
 *      anywhere navigation. Indoors in areas covered by different
 *      indoor positioning system (IPS) and outdoors covered by GPS.
 *      In particilar, it can rely on BeDIS (Building/environment Data and
 *      Information System) for indoor positioning. 
 *      Using this IPS, the navigator does not need to continuously monitor
 *      its own position, since the IPS broadcast to the navigator the
 *      location of each waypoint.
 *      This version makes use of Xamarin.Forms, which is a complete 
 *      cross-platform UI tookit that runs on both iOS and Android.
 *
 * Authors:
 * 
 *      Paul Chang, paulchang@iis.sinica.edu.tw
 *      Eric Lee, ericlee@iis.sinica.edu.tw
 *
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Dijkstra.NET.Model;
using IndoorNavigation.Models.NavigaionLayer;

namespace IndoorNavigation.Models
{
    /// <summary>
    /// The second level of the navigation graph within two-level hierarchy
    /// </summary>
    public class Region
    {
        /// <summary>
        /// Gets or sets the name of Region.
        /// e.g. 1F of NTUH
        /// </summary>
        [XmlElement("Name")]
        public string Name { get; set; }

        /// <summary>
        /// The list of waypoint objects (nodes)
        /// </summary>
        [XmlArray("Waypoints")]
        [XmlArrayItem("Waypoint", typeof(Waypoint))]
        public List<Waypoint> Waypoints { get; set; }

        /// <summary>
        /// Connection between waypoints (edges)
        /// </summary>
        [XmlArray("Edges")]
        [XmlArrayItem("Edge", typeof(Edge))]
        public List<Edge> Edges { get; set; }

        /// <summary>
        /// The navigation subgraph
        /// </summary>
        [XmlIgnore]
        public Graph<Waypoint, string> NavigationSubgraph =
                new Graph<Waypoint, string>();

        /// <summary>
        /// Initializes a navigation subgraph of the Region and combine all the
        /// waypoints and edges to NavigationSubgraph
        /// </summary>
        public Graph<Waypoint, string> GetNavigationSubgraph(int[] avoid)
        {

            // Add all the waypoints of each region into region graph
            foreach (Waypoint waypoint in Waypoints)
            {
                NavigationSubgraph.AddNode(waypoint);
            }

            // Set each path into region graph
            foreach (Edge edge in Edges)
            {
                int distance = System.Convert.ToInt32(edge.Distance);

                // In connectiontye: 
                // 0 is hall, 1 is stair, 2 is elevator, 3 is escalator
                int type = (int)edge.ConnectionType;

                for (int i = 0; i < avoid.Count(); i++)
                {
                    //find the method the user do not like and add its cost
                    if (type != (int)ConnectionType.NormalHallway && 
                        type == avoid[i])
                    {
                        distance += 100;
                        break;
                    }
                }

                // Get two connected waypoints's key value
                uint sourceWaypointKey = NavigationSubgraph.Where(waypoint =>
                        waypoint.Item.ID.Equals(edge.SourceWaypointUUID))
                        .Select(waypoint => waypoint.Key).First();
                uint targetWaypointKey = NavigationSubgraph.Where(waypoint =>
                        waypoint.Item.ID.Equals(edge.TargetWaypointUUID))
                        .Select(waypoint => waypoint.Key).First();

                // Connect the waypoints
                NavigationSubgraph.Connect(sourceWaypointKey, targetWaypointKey,
                        distance, string.Empty);
            }

            return NavigationSubgraph;
        }

        // TODO: Add methods for accessing data on IPS resources and 
        // other data on the region.
    }

}
