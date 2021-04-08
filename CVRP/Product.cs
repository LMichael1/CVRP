using System;
using System.Collections.Generic;
using System.Text;

namespace CVRP
{
    class Product
    {
        public ProductType Type { get; }
        public int FullVolume { get; }
        public int CurrentVolume { get; set; }
        public bool IsEmpty => CurrentVolume == 0;

        public Product(ProductType type, int volume)
        {
            Type = type;
            FullVolume = volume;
            CurrentVolume = volume;
        }

        public void Reset()
        {
            CurrentVolume = 0;
        }

        public override string ToString()
        {
            return string.Format("ProductType: {0}\nVolume: {1}/{2}", Type, CurrentVolume, FullVolume);
        }
    }
}
