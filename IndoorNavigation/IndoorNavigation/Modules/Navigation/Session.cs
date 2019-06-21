using System;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Collections.Generic;
using System.Diagnostics;
using Dijkstra.NET.Model;
using Dijkstra.NET.Extensions;
using IndoorNavigation.Models.NavigaionLayer;
using IndoorNavigation.Modules.Navigation;
using IndoorNavigation.Modules;
using IndoorNavigation.Modules.SignalProcessingAlgorithms;
using IndoorNavigation.Models;
using System.IO;
using System.Xml.Serialization;
using System.Xml;
using Region = IndoorNavigation.Models.NavigaionLayer.Region;

namespace IndoorNavigation.Modules
{
    public class Session
    {
        List<Waypoint> AllCorrectWaypoint = new List<Waypoint>();
        private Graph<Region, string> regionGraph = new Graph<Region, string>();
        private Graph<Waypoint, string> subgraph = new Graph<Waypoint, string>();
        public List<Waypoint> AllCorrectWaypointGet = new List<Waypoint>();
        public List<Guid> AllNoneWrongWaypoint = new List<Guid>();
        public Waypoint finalwaypointInPath = new Waypoint();
        public Session(Navigraph map, Waypoint startwaypoint, Waypoint finalwaypoint, List<int> avoid)
        {
            Console.WriteLine("Start Way point is" + startwaypoint.Name + "\n");
            Console.WriteLine("Final Way point is" + finalwaypoint.Name + "\n");
            //read the xml file to get regions and edges information
            regionGraph = map.GetRegiongraph();
            //we only consider to one region situation,
            //therefore, we add different floors' waypoints in same regions
            subgraph = map.Regions[0].SetNavigationSubgraph(avoid);
            //get the best route through dijkstra
            AllCorrectWaypointGet = GetPath(startwaypoint, finalwaypoint, subgraph);
            finalwaypointInPath = finalwaypoint;
        }
        List<Waypoint> GetPath(
           Waypoint startwaypointInPath, Waypoint finalwaypointInPath, Graph<Waypoint, string> subgraph)
        {
            //get the keys of the start way point and destination waypoint and throw the two keys into dijkstra
            // to get the best route
            var startPoingKey = subgraph
                .Where(waypoint => waypoint.Item.ID
                .Equals(startwaypointInPath.ID)).Select(c => c.Key).First();
            var endPointKey = subgraph
                .Where(waypoint => waypoint.Item.ID
                .Equals(finalwaypointInPath.ID)).Select(c => c.Key).First();
            var path = subgraph.Dijkstra(startPoingKey,
                                           endPointKey).GetPath();

            for (int i = 0; i < path.Count(); i++)
            {
                Console.WriteLine("Path " + i + ", " + subgraph[path.ToList()[i]].Item.Name);
            }
            //List<Waypoint> AllNeighborWaypoint = new List<Waypoint>();
            //put the correct waypoint into List all_correct_waaypoint
            for (int i = 0; i < path.Count(); i++)
            {
                AllCorrectWaypoint.Add(subgraph[path.ToList()[i]].Item);
            }

            //put all correct waypoints and their neighbors in list AllNoneWrongWaypoint
             for (int i = 0; i < AllCorrectWaypoint.Count; i++)
            {
                AllNoneWrongWaypoint.Add(AllCorrectWaypoint[i].ID);
                for (int j = 0; j < AllCorrectWaypoint[i].Neighbors.Count; j++)
                {
                    AllNoneWrongWaypoint.Add(AllCorrectWaypoint[i].Neighbors[j].TargetWaypointUUID);
                }

            }

            for (int i = 0; i < AllCorrectWaypoint.Count; i++)
            {
                Console.WriteLine("All correct Waypoint : " + AllCorrectWaypoint[i].Name + "\n");
            }
            //remove the elements that are repeated
            for (int i = 0; i < AllNoneWrongWaypoint.Count; i++)
            {
                for (int j = AllNoneWrongWaypoint.Count - 1; j > i; j--)
                {

                    if (AllNoneWrongWaypoint[i] == AllNoneWrongWaypoint[j])
                    {
                        AllNoneWrongWaypoint.RemoveAt(j);
                    }

                }
            }
            for (int i = 0; i < AllNoneWrongWaypoint.Count; i++)
            {
                Console.WriteLine("All Accept Guid : " + AllNoneWrongWaypoint[i] + "\n");
            }
            return AllCorrectWaypoint;
        }

       public NavigationInstruction DetectRoute(Waypoint currentwaypoint)
        {
            //returnInformation is a structure that contains five elements that need to be passed to the main and UI
            NavigationInstruction navigationInstruction = new NavigationInstruction();
            //getWaypoint is used to check where the current way point is in the Allcorrectwaypoint
            int getWaypoint = 0;

            Console.WriteLine("current Way Point ; " + currentwaypoint.Name);

            if (AllCorrectWaypoint.Contains(currentwaypoint))
            {
                //ReturnInformation.Status status = new ReturnInformation.Status();
                navigationInstruction.Status = NavigationResult.Correct;
                //check where the current point is in the AllcorrectWaypoint, we need this information
                //to get the direction, the progress and the next way point
                for (int i = 0; i < AllCorrectWaypoint.Count; i++)
                {
                    if (AllCorrectWaypoint[i].ID == currentwaypoint.ID)
                    {
                        getWaypoint = i;
                        break;
                    }
                }
                //get nextwaypoint
                navigationInstruction.nextwaypoint = AllCorrectWaypoint[getWaypoint + 1];
                //if the floor information is different, it means that the users need to change the floor
                //therefore, we can detemine the users need to go up or down by compare which floor is bigger
                if (currentwaypoint.Floor != AllCorrectWaypoint[getWaypoint + 1].Floor)
                {
                    if (currentwaypoint.Floor > AllCorrectWaypoint[getWaypoint + 1].Floor)
                    {
                        navigationInstruction.direction = TurnDirection.Down;
                    }
                    else
                    {
                        navigationInstruction.direction = TurnDirection.Up;
                    }
                    navigationInstruction.distance = 0;
                }
                //if the fllor information between current way point and next way point are same,
                //we need to tell the use go straight or turn direction, therefore, we need to 
                //have three informations, previous, current and next waypoints
                else
                {
                    Console.WriteLine("Nextwaypoint : " + navigationInstruction.nextwaypoint.Name);
                    uint sourceKey = subgraph
                                    .Where(WaypointList => WaypointList.Item.ID
                                    .Equals(AllCorrectWaypoint[getWaypoint].ID)).Select(c => c.Key)
                                     .First();
                    uint targetKey = subgraph
                                    .Where(WaypointList => WaypointList.Item.ID
                                    .Equals(AllCorrectWaypoint[getWaypoint + 1].ID)).Select(c => c.Key)
                                    .First();
                    navigationInstruction.distance = subgraph.Dijkstra(sourceKey, targetKey).Distance;
                    Console.WriteLine("Distance : " + navigationInstruction.distance);
                    Console.WriteLine("Get way Point : " + getWaypoint);
                    Console.WriteLine("all correct way point count : " + AllCorrectWaypoint.Count);
                    //getwaypoint=0 means that we are now in the starting point, we do not have the previous waypoint 
                    //information, we give the previous waypoint 0
                    if (getWaypoint == 0)
                    {
                        CardinalDirection currentDirection = 0;
                        CardinalDirection nextDirection = AllCorrectWaypoint[getWaypoint].Neighbors
                                                        .Where(neighbors =>
                                                            neighbors.TargetWaypointUUID
                                                            .Equals(AllCorrectWaypoint[getWaypoint + 1].ID))
                                                        .Select(c => c.Direction).First();

                        // Calculate the turning direction by cardinal direction
                        int nextTurnDirection = (int)nextDirection - (int)currentDirection;
                        nextTurnDirection = nextTurnDirection < 0 ?
                                            nextTurnDirection + 8 : nextTurnDirection;
                        navigationInstruction.direction = (TurnDirection)nextTurnDirection;
                    }
                    else
                    {
                        CardinalDirection currentDirection = AllCorrectWaypoint[getWaypoint - 1].Neighbors
                                                .Where(neighbors =>
                                                    neighbors.TargetWaypointUUID
                                                    .Equals(AllCorrectWaypoint[getWaypoint].ID))
                                                .Select(c => c.Direction).First();
                        CardinalDirection nextDirection = AllCorrectWaypoint[getWaypoint].Neighbors
                                                        .Where(neighbors =>
                                                            neighbors.TargetWaypointUUID
                                                            .Equals(AllCorrectWaypoint[getWaypoint + 1].ID))
                                                        .Select(c => c.Direction).First();
                        int nextTurnDirection = (int)nextDirection - (int)currentDirection;
                        nextTurnDirection = nextTurnDirection < 0 ?
                                            nextTurnDirection + 8 : nextTurnDirection;
                        navigationInstruction.direction = (TurnDirection)nextTurnDirection;
                        Console.WriteLine("Direction : " + nextTurnDirection + nextDirection + currentDirection);
                    }
                }
                navigationInstruction.progress = (double)Math.Round((decimal)getWaypoint / AllCorrectWaypoint.Count, 3);
            }
            else if (AllNoneWrongWaypoint.Contains(currentwaypoint.ID) == false)
            {
                //if the waypoint is wrong, we initial the correct waypoint and its neighbors
                //and rerun the GetPath function and reget the navigationInstruction
                AllCorrectWaypoint = new List<Waypoint>();
                AllNoneWrongWaypoint = new List<Guid>();
                AllCorrectWaypoint = GetPath(currentwaypoint, finalwaypointInPath, subgraph);
                for (int i = 0; i < AllCorrectWaypoint.Count; i++)
                {
                    Console.WriteLine("Renew Route : " + AllCorrectWaypoint[i].Name);
                }
                Console.WriteLine("out of this loop");
                navigationInstruction = DetectRoute(currentwaypoint);
                navigationInstruction.Status = NavigationResult.Wrong;
                Console.WriteLine("check");
            }

            return navigationInstruction;
        }
        public enum NavigationResult
        {
            Correct = 0,
            Wrong,
            Contonue,
        }
        public enum TurnDirection
        {
            FirstDirection = -1, // Exception: used only in the first step
            Forward = 0,
            Forward_Right,
            Right,
            Backward_Right,
            Backward,
            Backward_Left,
            Left,
            Forward_Left,
            Up,
            Down
        }
        public class NavigationInstruction
        {
            public Waypoint nextwaypoint;
            public double distance;
            public double progress;
            public NavigationResult Status;
            public TurnDirection direction;
        }
    }
}
