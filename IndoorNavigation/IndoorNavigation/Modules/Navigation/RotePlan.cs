using System;
using System.Collections.Generic;
using Dijkstra.NET.Model;
using Dijkstra.NET.ShortestPath;
using IndoorNavigation.Models;
using System.Linq;
using IndoorNavigation.Modules.Utility;

namespace IndoorNavigation.Modules.Navigation
{
    public class RotePlan
    {
        private Graph<BeaconGroup, string> Nodes = new Graph<BeaconGroup, string>();

        public RotePlan(List<BeaconGroup> Locations, List<LocationAssociation> Associations)
        {
            foreach (var Location in Locations)
                Nodes.AddNode(Location);

            foreach (var Association in Associations)
            {
                int Distance = System.Convert.ToInt32(Association.BeaconA.Coordinate.GetDistanceTo(Association.BeaconB.Coordinate) * 100);

                Nodes.Connect(
                    Nodes.Where(c => c.Item == Association.BeaconA).Select(c => c.Key).First(),
                    Nodes.Where(c => c.Item == Association.BeaconB).Select(c => c.Key).First(),
                    Distance,
                    string.Empty
                );
            }

        }

        public Queue<(BeaconGroup Next, int Angle)> GetPath(Beacon StartPoint, BeaconGroup EndPoint)
        {
            Queue<(BeaconGroup Next, int Angle)> PathQueue = new Queue<(BeaconGroup Next, int Angle)>();

            var dijkstra = new Dijkstra<BeaconGroup, string>(Nodes);
            var Path = dijkstra.Process(
                Nodes.Where(beaconGroup => beaconGroup.Item.Locations.Contains(StartPoint)).Select(c => c.Key).First(),
                Nodes.Where(c => c.Item == EndPoint).Select(c => c.Key).First()).GetPath();

            for (int i = 0; i < Path.Count()- 1 ; i++)
            {
                if (i == 0)
                    PathQueue.Enqueue((Nodes[Path.ToList()[i + 1]].Item, RotateAngle.GetRotateAngle(
                        Convert.ToCoordinate(StartPoint.UUID),
                        StartPoint.MarkCoordinate,
                        Nodes[Path.ToList()[i + 1]].Item.Coordinate
                        )));
                else
                    PathQueue.Enqueue((Nodes[Path.ToList()[i + 1]].Item, RotateAngle.GetRotateAngle(
                        Nodes[Path.ToList()[i]].Item.Coordinate,
                        Nodes[Path.ToList()[i-1]].Item.Coordinate,
                        Nodes[Path.ToList()[i+1]].Item.Coordinate
                        )));
            }

            return PathQueue;
        }
    }
}
