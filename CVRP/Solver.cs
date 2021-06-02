using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CVRP
{
    public class Solver
    {
        public Solution Solution { get; private set; }

        public Solver(IEnumerable<Point> points, IEnumerable<Vehicle> vehicles)
        {
            var pointsList = points.ToList();
            var vehiclesList = vehicles.ToList();

            vehiclesList.Sort((a, b) => b.CompareTo(a));

            Solution = GetInitialSolution(pointsList, vehiclesList);
        }

        public void RunOptimization()
        {
            Process2opt();
            ProcessShift1_0();
            Process2opt();
            ProcessSwap1_1();
            Process2opt();
            ProcessShift0_1();
            Process2opt();

            foreach (var route in Solution.Routes)
            {
                route.ManageBarrels(route.Vehicle);
            }
        }

        private void ProcessShift0_1()
        {
            for (int i = 0; i < Solution.Routes.Count - 1; i++)
            {
                for (int j = i + 1; j < Solution.Routes.Count; j++)
                {
                    Shift(Solution.Routes[j], Solution.Routes[i]);
                }
            }
        }

        private void ProcessShift1_0()
        {
            for (int i = 0; i < Solution.Routes.Count - 1; i++)
            {
                for (int j = i + 1; j < Solution.Routes.Count; j++)
                {
                    Shift(Solution.Routes[i], Solution.Routes[j]);
                }
            }
        }

        private void Shift(Route first, Route second)
        {
            var shouldRestart = true;

            if (second.IsEmpty)
            {
                Console.WriteLine();
            }

            while (shouldRestart)
            {
                shouldRestart = false;

                var minLength = first.Length + second.Length;

                for (int i = 1; i < first.Points.Count - 1; i++)
                {
                    var minIndex = 0;

                    if (!second.CanBeAdded(first.Points[i])) continue;

                    for (int j = 1; j < second.Points.Count; j++)
                    {
                        var newFirst = (Route)first.Clone();
                        var newSecond = (Route)second.Clone();

                        newSecond.InsertPoint(j, newFirst.Points[i]);
                        newFirst.RemovePoint(i);

                        var newLength = newFirst.Length + newSecond.Length;

                        if (newLength < minLength)
                        {
                            minLength = newLength;
                            minIndex = j;
                        }
                    }

                    if (minIndex > 0)
                    {
                        second.InsertPoint(minIndex, first.Points[i]);
                        first.RemovePoint(i);
                        shouldRestart = true;
                        break;
                    }
                }
            }
        }

        private void ProcessSwap1_1()
        {
            for (int i = 0; i < Solution.Routes.Count - 1; i++)
            {
                for (int j = i + 1; j < Solution.Routes.Count; j++)
                {
                    Swap(Solution.Routes[i], Solution.Routes[j]);
                }
            }
        }

        private void Swap(Route first, Route second)
        {
            if (first.IsEmpty || second.IsEmpty) return;

            var shouldRestart = true;

            while (shouldRestart)
            {
                shouldRestart = false;

                var totalLength = first.Length + second.Length;

                for (int i = 1; i < first.Points.Count - 1; i++)
                {
                    for (int j = 1; j < second.Points.Count - 1; j++)
                    {
                        var newFirst = (Route)first.Clone();
                        var newSecond = (Route)second.Clone();

                        var firstPoint = newFirst.Points[i];
                        var secondPoint = newSecond.Points[j];

                        newFirst.RemovePoint(i);
                        newSecond.RemovePoint(j);

                        if (!newFirst.CanBeAdded(secondPoint) || !newSecond.CanBeAdded(firstPoint)) continue;

                        newFirst.InsertPoint(i, secondPoint);
                        newSecond.InsertPoint(j, firstPoint);

                        var newLength = newFirst.Length + newSecond.Length;

                        if (newLength < totalLength)
                        {
                            first.RemovePoint(i);
                            second.RemovePoint(j);

                            first.InsertPoint(i, secondPoint);
                            second.InsertPoint(j, firstPoint);

                            shouldRestart = true;
                            break;
                        }
                    }

                    if (shouldRestart) break;
                }
            }
        }

        private void Process2opt()
        {
            foreach (var route in Solution.Routes)
            {
                Process2opt(route);
            }
        }

        private void Process2opt(Route route)
        {
            if (route.Points.Count < 4) return;

            var shouldRestart = true;

            while (shouldRestart)
            {
                var bestDistance = route.Length;

                for (int i = 1; i <= route.Points.Count - 3; i++)
                {
                    shouldRestart = false;

                    for (int k = i + 1; k <= route.Points.Count - 2; k++)
                    {
                        var newRoute = (Route)route.Clone();
                        newRoute.Points.Reverse(i, k - i + 1);

                        var newDistance = newRoute.Length;

                        if (newDistance < bestDistance)
                        {
                            route.Points.Reverse(i, k - i + 1);
                            shouldRestart = true;
                            break;
                        }
                    }

                    if (shouldRestart) break;
                }
            }
        }

        private Solution GetInitialSolution(List<Point> points, List<Vehicle> vehicles)
        {
            var depot = points.FirstOrDefault(point => point.IsDepot);
            var routes = new List<Route>();

            foreach (var vehicle in vehicles)
            {
                var route = new Route(vehicle, depot);

                var currentPoint = depot;

                while (route.CanBeAdded(currentPoint))
                {
                    if (currentPoint != depot)
                    {
                        route.AddPoint(currentPoint);
                        currentPoint.IsEmpty = true;
                    }

                    var destinationsIDs = points.Where(point => !point.IsDepot && !point.IsEmpty && route.CanBeAdded(point)).Select(point => point.ID).ToList();

                    if (destinationsIDs.Count == 0) break;

                    var nearestDestinationsIDs = currentPoint.Distances.Where(item => destinationsIDs.Contains(item.Key)).OrderBy(item => item.Value);

                    if (route.StartTime == -1)
                    {
                        var nextPointID = nearestDestinationsIDs.First().Key;
                        var nextPoint = points.First(point => point.ID == nextPointID);

                        route.StartTime = nextPoint.TimeWindows[0].Start - currentPoint.Times[nextPointID];

                        currentPoint = nextPoint;
                    }
                    else
                    {
                        var lengths = new Dictionary<Point, double>();

                        foreach (var id in nearestDestinationsIDs.Select(item => item.Key))
                        {
                            var point = points.First(point => point.ID == id);

                            route.AddPoint(point);
                            lengths[point] = route.Length;
                            route.RemovePoint(route.Points.Count - 2);
                        }

                        var nextPoint = lengths.OrderBy(item => item.Value).Select(item => item.Key).First();

                        currentPoint = nextPoint;
                    }
                }

                routes.Add(route);

                if (points.Where(point => !point.IsEmpty).Count() == 0)
                {
                    break;
                }
            }

            return new Solution(points, vehicles, routes);
        }
    }
}
