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
        public Product Product { get; }
        public bool IsDepot => ID < 0;
        public Dictionary<int, double> Distances { get; set; }

        public Point(int id, double latitude, double longitude, ProductType type, int volume)
        {
            ID = id;
            Latitude = latitude;
            Longitude = longitude;
            Distances = new Dictionary<int, double>();
            Product = new Product(type, volume);
        }

        public override string ToString()
        {
            return string.Format("ID: {0}\nLatitude: {1}\nLongitude: {2}\n{3}",
                ID, Latitude, Longitude, Product);
        }
    }
}
