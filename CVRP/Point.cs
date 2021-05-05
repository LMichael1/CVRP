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
        public int ProductType { get; }
        public int Volume { get; }
        public bool IsEmpty { get; set; }
        public bool IsDepot => ID < 0;
        public Dictionary<int, double> Distances { get; set; }

        public Point(int id, double latitude, double longitude, int type, int volume)
        {
            ID = id;
            Latitude = latitude;
            Longitude = longitude;
            Distances = new Dictionary<int, double>();
            ProductType = type;
            Volume = volume;
            IsEmpty = false;
        }

        public void Reset()
        {
            IsEmpty = false;
        }

        public override string ToString()
        {
            return string.Format("ID: {0}\nLatitude: {1}\nLongitude: {2}\nProductType: {3}\nVolume: {4}",
                ID, Latitude, Longitude, ProductType, Volume);
        }
    }
}
