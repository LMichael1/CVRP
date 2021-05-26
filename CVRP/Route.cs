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
        public int StartTime { get; set; }
        public double RealLength
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
        public double Length
        {
            get
            {
                var length = 0.0;
                var time = StartTime;

                for (int i = 0; i < Points.Count - 1; i++)
                {
                    length += Points[i].Distances[Points[i + 1].ID];

                    if (Points[i].Times[Points[i + 1].ID] >= 800000000) // костыль)
                    {
                        length = 800000000;
                        return length;
                    }

                    time += Points[i].Times[Points[i + 1].ID];

                    // Если попадает в окно

                    var window = Points[i + 1].TimeWindows.FirstOrDefault(window => time >= window.Start
                        && time + Points[i + 1].ServiceTime <= window.End);

                    if (window != null)
                    {
                        time += Points[i + 1].ServiceTime;
                        continue;
                    }

                    // Если попадает в окно, но не успевает загрузиться

                    window = Points[i + 1].TimeWindows.FirstOrDefault(window => time >= window.Start
                        && time <= window.End);

                    if (window != null)
                    {
                        time += Points[i + 1].ServiceTime;

                        var penalty = (time - window.End) * Points[i + 1].PenaltyLate / 60;
                        length += penalty;

                        continue;
                    }

                    // Если приехал раньше или позже

                    var minTime = int.MaxValue;
                    var nearestWindow = Points[i + 1].TimeWindows.FirstOrDefault();

                    foreach (var timeWindow in Points[i + 1].TimeWindows)
                    {
                        if (Math.Abs(time - timeWindow.Start) < minTime)
                        {
                            minTime = time - timeWindow.Start;
                            nearestWindow = timeWindow;
                        }

                        if (Math.Abs(time - timeWindow.End) < minTime)
                        {
                            minTime = time - timeWindow.End;
                            nearestWindow = timeWindow;
                        }
                    }

                    if (minTime > 0) // опоздал
                    {
                        time += Points[i + 1].ServiceTime;

                        var penalty = minTime * Points[i + 1].PenaltyLate / 60;
                        length += penalty;

                        continue;
                    }
                    else // приехал раньше
                    {
                        time += Math.Abs(minTime); // ждёт открытия
                        time += Points[i + 1].ServiceTime;

                        var penalty = Math.Abs(minTime) * Points[i + 1].PenaltyWait / 60;
                        length += penalty;
                    }
                }

                return length;
            }
        }
        public bool IsEmpty => Points.Count == 2;
        public Dictionary<Point, int> PenaltyPoints
        {
            get
            {
                var penaltyPoints = new Dictionary<Point, int>();

                var time = StartTime;

                for (int i = 0; i < Points.Count - 1; i++)
                {
                    time += Points[i].Times[Points[i + 1].ID];
                    time += Points[i + 1].ServiceTime;

                    var windows = Points[i + 1].TimeWindows.Select(window => new TimeWindow(window.Start + Points[i + 1].ServiceTime, window.End));

                    if (windows.Count(window => time >= window.Start && time <= window.End) == 0)
                    {
                        penaltyPoints[Points[i + 1]] = time;
                    }
                }

                return penaltyPoints;
            }
        }

        public Route(Vehicle vehicle, Point depot)
        {
            Vehicle = vehicle;
            Points = new List<Point>();
            Points.Add(depot);
            Points.Add(depot);
            StartTime = -1;
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

                // бочка с минимальным свободным местом, в которую можно залить всё из виртуальной

                var suitableBarrel = vehicle.Barrels.Where(barrel => (barrel.ProductType == virtualBarrel.ProductType || barrel.ProductType == -1)
                    && barrel.FreeCapacity >= volume)
                    .OrderBy(barrel => barrel.FreeCapacity).FirstOrDefault();

                if (suitableBarrel != null)
                {
                    suitableBarrel.Fill(volume, virtualBarrel.ProductType);
                    volume = 0;
                    continue;
                }

                // для каждой подходящей бочки по возрастанию объема

                foreach (var realBarrel in vehicle.Barrels.Where(barrel => !barrel.IsFull
                    && (barrel.ProductType == virtualBarrel.ProductType || barrel.ProductType == -1))
                    .OrderBy(barrel => barrel.FreeCapacity))
                {
                    if (volume > realBarrel.FreeCapacity) // если объем сырья виртуальной больше свободного места
                    {
                        volume -= realBarrel.FreeCapacity;
                        realBarrel.Fill(realBarrel.FreeCapacity, virtualBarrel.ProductType);
                    }
                    else // если меньше либо равен
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

            StartTime = Points[1].TimeWindows[0].Start - Points[0].Times[Points[1].ID];
        }

        public void RemovePoint(int index)
        {
            Vehicle.Remove(Points[index]);
            Points.RemoveAt(index);

            if (Points.Count > 2)
            {
                StartTime = Points[1].TimeWindows[0].Start - Points[0].Times[Points[1].ID];
            }
            else
            {
                StartTime = -1;
            }
        }

        public override string ToString()
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.Append("------------------------------------------------------\n");

            stringBuilder.Append("Route: \n");

            var startTime = TimeSpan.FromSeconds(StartTime);
            stringBuilder.AppendFormat("\nStartTime: {0}\n\n", startTime.ToString(@"dd\.hh\:mm\:ss"));

            foreach (var point in Points)
            {
                if (point.IsDepot)
                {
                    stringBuilder.AppendFormat("ID: {0} [Depot]\n", point.ID);
                    continue;
                }

                stringBuilder.AppendFormat("ID: {0}, Product: (Type {1}: {2}), Windows: [", point.ID, point.ProductType, point.Volume);

                foreach (var window in point.TimeWindows)
                {
                    var start = TimeSpan.FromSeconds(window.Start);
                    var end = TimeSpan.FromSeconds(window.End);

                    stringBuilder.AppendFormat(" <{0} - {1}> ", start.ToString(@"dd\.hh\:mm"), end.ToString(@"dd\.hh\:mm"));
                }

                if (PenaltyPoints.ContainsKey(point))
                {
                    var time = TimeSpan.FromSeconds(PenaltyPoints[point]);

                    stringBuilder.AppendFormat("], Arrived (penalty): [{0}", time.ToString(@"dd\.hh\:mm"));
                }

                stringBuilder.Append("]\n");
            }

            var penaltiesLengthKm = Math.Round(Length / 1000.0, 1);
            var lengthKm = Math.Round(RealLength / 1000.0, 1);

            stringBuilder.AppendFormat("\nLength: {0} km", lengthKm);
            stringBuilder.AppendFormat("\nLength with penalties: {0} km\n\n", penaltiesLengthKm);

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

            clone.StartTime = StartTime;

            return clone;
        }
    }
}
