using System;
using System.Collections.Generic;
using System.Text;

namespace CVRP
{
    class Barrel
    {
        public int Order { get; }
        public ProductType ProductType { get; private set; }
        public int FullCapacity { get; }
        public int OccupiedCapacity { get; private set; }
        public int FreeCapacity => FullCapacity - OccupiedCapacity;
        public bool IsFull => FreeCapacity == 0;
        
        public Barrel(int fullCapacity, int order)
        {
            Order = order;
            FullCapacity = fullCapacity;
            OccupiedCapacity = 0;
        }

        public bool Fill(ref int volume, ProductType type)
        {
            if ((ProductType == ProductType.None || ProductType == type) && !IsFull)
            {
                ProductType = type;

                if (volume >= FreeCapacity)
                {
                    volume -= FreeCapacity;
                    OccupiedCapacity += FreeCapacity;
                }
                else
                {
                    OccupiedCapacity += volume;
                    volume = 0;
                }

                return true;
            }

            return false;
        }

        public void Reset()
        {
            ProductType = ProductType.None;
            OccupiedCapacity = 0;
        }

        public override string ToString()
        {
            return string.Format("{0}, {1}/{2} ", Order, OccupiedCapacity, FullCapacity);
        }
    }
}
