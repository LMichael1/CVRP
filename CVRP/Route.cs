using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CVRP
{
    class Route : ICloneable
    {
        public Vehicle Vehicle { get; set; }
        public List<Point> Points { get; set; }
        public double Length
        {
            get
            {
                double length = 0.0;

                for (int i = 0; i < Points.Count - 1; i++)
                {
                    length += Points[i].Distances[Points[i + 1].ID];
                }

                return length;
            }
        }
        public bool IsEmpty => Points.Count == 2;

        public Route(Vehicle vehicle, Point depot)
        {
            Vehicle = vehicle;
            Points = new List<Point>();
            Points.Add(depot);
            Points.Add(depot);
        }

        public bool CanBeAdded(Point point)
        {
            if (point.IsDepot) return true;
            if (Vehicle.IsFull || Vehicle.OccupiedCapacity + point.Volume > Vehicle.FullCapacity) return false;

            var clone = (Vehicle)Vehicle.Clone();

            var currentBarrel = clone.VirtualBarrels.First(barrel => barrel.ProductType == point.ProductType);
            clone.Fill(point);

            foreach (var virtualBarrel in clone.VirtualBarrels.Where(barrel => !barrel.IsEmpty).OrderBy(barrel => barrel.OccupiedCapacity))
            {
                var suitableBarrel = clone.Barrels.Where(barrel => (barrel.ProductType == virtualBarrel.ProductType || barrel.ProductType == -1)
                    && barrel.FreeCapacity >= virtualBarrel.OccupiedCapacity)
                    .OrderBy(barrel => barrel.FreeCapacity).FirstOrDefault();

                if (suitableBarrel != null)
                {
                    suitableBarrel.Fill(virtualBarrel.OccupiedCapacity, virtualBarrel.ProductType);
                    virtualBarrel.OccupiedCapacity = 0;
                    continue;
                }

                foreach (var realBarrel in clone.Barrels.Where(barrel => !barrel.IsFull
                    && (barrel.ProductType == virtualBarrel.ProductType || barrel.ProductType == -1))
                    .OrderBy(barrel => barrel.FreeCapacity))
                {
                    if (virtualBarrel.OccupiedCapacity > realBarrel.FreeCapacity)
                    {
                        virtualBarrel.OccupiedCapacity -= realBarrel.FreeCapacity;
                        realBarrel.Fill(realBarrel.FreeCapacity, virtualBarrel.ProductType);
                    }
                    else
                    {
                        realBarrel.Fill(virtualBarrel.OccupiedCapacity, virtualBarrel.ProductType);
                        virtualBarrel.OccupiedCapacity = 0;
                        break;
                    }
                }
            }

            return clone.VirtualBarrels.Count(barrel => barrel.OccupiedCapacity > 0) == 0;
        }

        public void AddPoint(Point point)
        {
            Points.Insert(Points.Count - 1, point);
            Vehicle.Fill(point);
        }

        public void InsertPoint(int index, Point point)
        {
            Points.Insert(index, point);
            Vehicle.Fill(point);
        }

        public void RemovePoint(int index)
        {
            Vehicle.Remove(Points[index]);
            Points.RemoveAt(index);
        }

        public override string ToString()
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.Append("---------------------------\n");

            stringBuilder.Append("========= VEHICLE =========\n");
            stringBuilder.Append(Vehicle.ToString());

            stringBuilder.Append("========= ROUTE =========\n");

            stringBuilder.Append("Route: ");

            foreach (var point in Points)
            {
                stringBuilder.Append(point.ID);
                stringBuilder.Append(" ");
            }

            stringBuilder.AppendFormat("\nLength: {0}", Length);

            stringBuilder.Append("\n---------------------------\n");

            return stringBuilder.ToString();
        }

        public object Clone()
        {
            var clone = new Route((Vehicle)Vehicle.Clone(), Points[0]);

            for (int i = 1; i < Points.Count - 1; i++)
            {
                clone.AddPoint(Points[i]);
            }

            return clone;
        }
    }
}
