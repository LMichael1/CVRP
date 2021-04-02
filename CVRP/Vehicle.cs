using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace CVRP
{
    class Vehicle : IComparable<Vehicle>
    {
        public int ID { get; }
        public List<Barrel> Barrels { get; }
        public int FullCapacity
        {
            get
            {
                var sum = 0;

                foreach (var barrel in Barrels)
                {
                    sum += barrel.FullCapacity;
                }

                return sum;
            }
        }
        public bool IsFull => Barrels.Where(barrel => barrel.IsFull).Count() == Barrels.Count;

        public Vehicle(int id, IEnumerable<Barrel> barrels)
        {
            ID = id;
            Barrels = barrels.OrderBy(barrel => barrel.Order).ToList();
        }

        public void FillBarrels(ref int volume, ProductType productType)
        {
            foreach (var barrel in Barrels)
            {
                if (volume == 0)
                {
                    break;
                }

                barrel.Fill(ref volume, productType);
            }
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
    }
}
