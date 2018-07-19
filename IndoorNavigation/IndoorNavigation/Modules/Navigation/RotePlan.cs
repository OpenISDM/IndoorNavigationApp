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
        private Graph<Guid, string> Nodes = new Graph<Guid, string>();

        public RotePlan(List<Guid> Locations, List<LocationAssociation> Associations)
        {
            foreach (var Location in Locations)
                Nodes.AddNode(Location);

            foreach (var Association in Associations)
            {
                var CoordinateA = Convert.ToCoordinate(Association.LocationA);
                var CoordinateB = Convert.ToCoordinate(Association.LocationB);
                int Distance = System.Convert.ToInt32(CoordinateA.GetDistanceTo(CoordinateB) * 100);

                Nodes.Connect(
                    Nodes.Where(c => c.Item == Association.LocationA).Select(c => c.Key).First(),
                    Nodes.Where(c => c.Item == Association.LocationB).Select(c => c.Key).First(),
                    Distance,
                    string.Empty
                );
            }

        }

        public Queue<(Guid Next, int Angle)> GetPath(Guid StartPoint, Guid EndPoint)
        {
            Queue<(Guid Next, int Angle)> PathQueue = new Queue<(Guid Next, int Angle)>();

            var dijkstra = new Dijkstra<Guid, string>(Nodes);
            var Path = dijkstra.Process(
                Nodes.Where(c => c.Item == StartPoint).Select(c => c.Key).First(),
                Nodes.Where(c => c.Item == EndPoint).Select(c => c.Key).First()).GetPath();

            for (int i = 0; i < Path.Count()- 1 ; i++)
            {
                if (i == 0)
                    PathQueue.Enqueue((Nodes[Path.ToList()[i + 1]].Item, int.MinValue));
                else
                    PathQueue.Enqueue((Nodes[Path.ToList()[i + 1]].Item, RotateAngle.GetRotateAngle(
                        Convert.ToCoordinate(Nodes[Path.ToList()[i]].Item),
                        Convert.ToCoordinate(Nodes[Path.ToList()[i-1]].Item),
                        Convert.ToCoordinate(Nodes[Path.ToList()[i+1]].Item)
                        )));
            }

            return PathQueue;
        }
    }
}
