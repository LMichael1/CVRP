using System;
using System.Collections.Generic;
using System.Text;

namespace CVRP
{
    class Barrel : ICloneable
    {
        public int ProductType { get; private set; }
        public int FullCapacity { get; }
        public int OccupiedCapacity { get; set; }
        public int FreeCapacity => FullCapacity - OccupiedCapacity;
        public bool IsFull => FreeCapacity == 0;
        public bool IsEmpty => OccupiedCapacity == 0;
        
        public Barrel(int fullCapacity)
        {
            FullCapacity = fullCapacity;
            OccupiedCapacity = 0;
            ProductType = -1;
        }

        public Barrel(int fullCapacity, int productType)
        {
            FullCapacity = fullCapacity;
            OccupiedCapacity = 0;
            ProductType = productType;
        }

        public void Fill(int volume, int productType)
        {
            if (ProductType != -1 && ProductType != productType)
            {
                throw new ArgumentException("Incorrect product type.");
            }

            if (volume > FreeCapacity)
            {
                throw new ArgumentException("Incorrect volume.");
            }

            OccupiedCapacity += volume;
            ProductType = productType;
        }

        public void Remove(int volume)
        {
            if (volume > OccupiedCapacity)
            {
                throw new ArgumentException("Incorrect volume.");
            }

            OccupiedCapacity -= volume;
        }

        public void Reset()
        {
            ProductType = -1;
            OccupiedCapacity = 0;
        }

        public override string ToString()
        {
            return string.Format("{0}/{1} ", OccupiedCapacity, FullCapacity);
        }

        public object Clone()
        {
            var barrel = new Barrel(FullCapacity, ProductType);
            barrel.OccupiedCapacity = OccupiedCapacity;

            return barrel;
        }
    }
}
