﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CVRP
{
    class Solution
    {
        public List<Route> Routes { get; set; }
        public List<Point> Points { get; set; } 
        public List<Vehicle> Vehicles { get; set; }
        public List<Point> RemainedPoints => Points.Where(point => !point.IsEmpty && !point.IsDepot).ToList();
        public double TotalLength => Routes.Sum(route => route.Length);
        public int NotEmptyRoutesCount => Routes.Count(route => route.Length > 2);

        public Solution()
        {
            Routes = new List<Route>();
        }

        public Solution(IEnumerable<Point> points, IEnumerable<Vehicle> vehicles, IEnumerable<Route> routes)
        {
            Points = points.ToList();
            Vehicles = vehicles.ToList();
            Routes = routes.ToList();
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();

            foreach (var route in Routes.Where(route => !route.IsEmpty))
            {
                stringBuilder.Append(route);
            }

            var lengthKm = Math.Round(TotalLength / 1000.0, 1);

            stringBuilder.AppendFormat("\nTotal Length: {0} km ({1} m)\nRoutes count: {2}\nNot empty routes count: {3}\nRemained points count: {4}\n\n\n", 
                lengthKm, TotalLength, Routes.Count, NotEmptyRoutesCount, RemainedPoints.Count);

            return stringBuilder.ToString();
        }
    }
}