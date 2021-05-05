using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace CVRP
{
    class Vehicle : IComparable<Vehicle>, ICloneable
    {
        public int ID { get; }
        public List<Barrel> Barrels { get; }
        public List<Barrel> VirtualBarrels { get; }
        public int FullCapacity => Barrels.Sum(barrel => barrel.FullCapacity);
        public int OccupiedCapacity => VirtualBarrels.Sum(barrel => barrel.OccupiedCapacity);
        public bool IsFull => FullCapacity == OccupiedCapacity;

        public Vehicle(int id, int productsCount, IEnumerable<Barrel> barrels)
        {
            ID = id;
            Barrels = barrels.OrderBy(barrel => barrel.FullCapacity).ToList();

            VirtualBarrels = new List<Barrel>();

            for (int i = 0; i < productsCount; i++)
            {
                VirtualBarrels.Add(new Barrel(FullCapacity, i));
            }
        }

        public void Fill(Point point)
        {
            var virtualBarrel = VirtualBarrels.First(barrel => barrel.ProductType == point.ProductType);
            virtualBarrel.Fill(point.Volume, point.ProductType);
        }

        public void Remove(Point point)
        {
            var virtualBarrel = VirtualBarrels.First(barrel => barrel.ProductType == point.ProductType);
            virtualBarrel.Remove(point.Volume);
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();

            result.AppendFormat("ID: {0}\nBarrels:\n", ID);

            foreach (var barrel in Barrels)
            {
                result.AppendLine(barrel.ToString());
            }

            return result.ToString();
        }

        public int CompareTo(Vehicle other)
        {
            if (FullCapacity > other.FullCapacity)
            {
                return 1;
            }

            if (FullCapacity < other.FullCapacity)
            {
                return -1;
            }

            return 0;
        }

        public object Clone()
        {
            var barrels = new List<Barrel>();

            foreach (var barrel in Barrels)
            {
                barrels.Add((Barrel)barrel.Clone());
            }

            return new Vehicle(ID, VirtualBarrels.Count, barrels);
        }
    }
}
