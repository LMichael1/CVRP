using System;
using System.Collections.Generic;
using System.Text;

namespace CVRP
{
    class Point
    {
        public int ID { get; }
        public double Latitude { get; }
        public double Longitude { get; }
        public ProductType ProductType { get; }
        public int FullVolume { get; }
        public int CurrentVolume { get; set; }
        public bool IsEmpty => CurrentVolume == 0;
        public bool IsDepot => ID < 0;
        public Dictionary<int, double> Distances { get; set; }

        public Point(int id, double latitude, double longitude, ProductType type, int volume)
        {
            ID = id;
            Latitude = latitude;
            Longitude = longitude;
            Distances = new Dictionary<int, double>();
            ProductType = type;
            FullVolume = volume;
            CurrentVolume = volume;
        }

        public void Reset()
        {
            CurrentVolume = 0;
        }

        public override string ToString()
        {
            return string.Format("ID: {0}\nLatitude: {1}\nLongitude: {2}\nProductType: {3}\nVolume: {4}/{5}",
                ID, Latitude, Longitude, ProductType, CurrentVolume, FullVolume);
        }
    }
}
