using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CVRP
{
    public class Point
    {
        public int ID { get; }
        public double Latitude { get; }
        public double Longitude { get; }
        public int ProductType { get; }
        public int Volume { get; }
        public bool IsEmpty { get; set; }
        public bool IsDepot => ID < 0;
        public List<TimeWindow> TimeWindows { get; }
        public int ServiceTime { get; }
        public int PenaltyLate { get; }
        public int PenaltyWait { get; }
        public Dictionary<int, double> Distances { get; set; }
        public Dictionary<int, int> Times { get; set; }

        public Point(int id, double latitude, double longitude, int type, int volume, 
            IEnumerable<TimeWindow> timeWindows, int serviceTime, int penaltyLate, int penaltyWait)
        {
            ID = id;
            Latitude = latitude;
            Longitude = longitude;
            Distances = new Dictionary<int, double>();
            Times = new Dictionary<int, int>();
            ProductType = type;
            Volume = volume;
            IsEmpty = false;
            TimeWindows = timeWindows.OrderBy(window => window.Start).ToList();
            ServiceTime = serviceTime;
            PenaltyLate = penaltyLate;
            PenaltyWait = penaltyWait;
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
