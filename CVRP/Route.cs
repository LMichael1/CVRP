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

            ManageBarrels(clone);

            return clone.VirtualBarrels.Sum(barrel => barrel.OccupiedCapacity) == clone.Barrels.Sum(barrel => barrel.OccupiedCapacity);
        }

        private void ManageBarrels(Vehicle vehicle)
        {
            foreach (var barrel in vehicle.Barrels)
            {
                barrel.Reset();
            }

            foreach (var virtualBarrel in vehicle.VirtualBarrels.Where(barrel => !barrel.IsEmpty).OrderBy(barrel => barrel.OccupiedCapacity))
            {
                var volume = virtualBarrel.OccupiedCapacity;

                var suitableBarrel = vehicle.Barrels.Where(barrel => (barrel.ProductType == virtualBarrel.ProductType || barrel.ProductType == -1)
                    && barrel.FreeCapacity >= volume)
                    .OrderBy(barrel => barrel.FreeCapacity).FirstOrDefault();

                if (suitableBarrel != null)
                {
                    suitableBarrel.Fill(volume, virtualBarrel.ProductType);
                    volume = 0;
                    continue;
                }

                foreach (var realBarrel in vehicle.Barrels.Where(barrel => !barrel.IsFull
                    && (barrel.ProductType == virtualBarrel.ProductType || barrel.ProductType == -1))
                    .OrderBy(barrel => barrel.FreeCapacity))
                {
                    if (volume > realBarrel.FreeCapacity)
                    {
                        volume -= realBarrel.FreeCapacity;
                        realBarrel.Fill(realBarrel.FreeCapacity, virtualBarrel.ProductType);
                    }
                    else
                    {
                        realBarrel.Fill(volume, virtualBarrel.ProductType);
                        volume = 0;
                        break;
                    }
                }
            }
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

            stringBuilder.Append("------------------------------------------------------\n");

            stringBuilder.Append("Route: ");

            foreach (var point in Points)
            {
                stringBuilder.AppendFormat("{0}({1}:{2}) ", point.ID, point.ProductType, point.Volume);
            }

            var lengthKm = Math.Round(Length / 1000.0, 1);

            stringBuilder.AppendFormat("\nLength: {0} km ({1} m)\n\n", lengthKm, Length);

            var clone = (Vehicle)Vehicle.Clone();
            ManageBarrels(clone);

            stringBuilder.Append(clone.ToString());

            stringBuilder.Append("\n------------------------------------------------------\n");

            return stringBuilder.ToString();
        }

        public object Clone()
        {
            var clone = new Route((Vehicle)Vehicle.Clone(), Points[0]);

            clone.Points = new List<Point>();

            foreach (var point in Points)
            {
                clone.Points.Add(point);
            }

            return clone;
        }
    }
}
