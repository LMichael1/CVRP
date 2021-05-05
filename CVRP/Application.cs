using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CVRP
{
    class Application
    {
        private readonly string[] _args;

        private List<Point> _points;
        private List<Vehicle> _vehicles;

        public Application(string[] args)
        {
            _args = args;
        }

        public async Task RunAsync()
        {
            var parser = new Parser(_args[0]);
            await parser.Parse();

            _points = parser.Points;
            _vehicles = parser.Vehicles;
            _vehicles.Sort((a, b) => b.CompareTo(a));

            var solution = GetInitialSolution();

            Console.WriteLine("INITIAL\n");
            PrintSolution(solution);

            Process2opt(solution);
            Console.WriteLine("2-OPT\n");
            PrintSolution(solution);

            ProcessShift1_0(solution);
            Console.WriteLine("SHIFT (1, 0)\n");
            PrintSolution(solution);

            Process2opt(solution);
            Console.WriteLine("2-OPT\n");
            PrintSolution(solution);

            ProcessSwap1_1(solution);
            Console.WriteLine("SWAP (1, 1)\n");
            PrintSolution(solution);

            Process2opt(solution);
            Console.WriteLine("2-OPT\n");
            PrintSolution(solution);

            ProcessShift0_1(solution);
            Console.WriteLine("SHIFT (0, 1)\n");
            PrintSolution(solution);

            Process2opt(solution);
            Console.WriteLine("2-OPT\n");
            PrintSolution(solution);
        }

        private void ProcessShift0_1(List<Route> solution)
        {
            for (int i = 0; i < solution.Count - 1; i++)
            {
                for (int j = i + 1; j < solution.Count; j++)
                {
                    Shift(solution[j], solution[i]);
                }
            }
        }

        private void ProcessShift1_0(List<Route> solution)
        {
            for (int i = 0; i < solution.Count - 1; i++)
            {
                for (int j = i + 1; j < solution.Count; j++)
                {
                    Shift(solution[i], solution[j]);
                }
            }
        }

        private void Shift(Route first, Route second)
        {
            var shouldRestart = true;

            while (shouldRestart)
            {
                shouldRestart = false;

                var minLength = first.Length + second.Length;

                for (int i = 1; i < first.Points.Count - 1; i++)
                {
                    var minIndex = 0;

                    if (!second.CanBeAdded(first.Points[i])) continue;

                    for (int j = 1; j < second.Points.Count - 1; j++)
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

        private void ProcessSwap1_1(List<Route> solution)
        {
            for (int i = 0; i < solution.Count - 1; i++)
            {
                for (int j = i + 1; j < solution.Count; j++)
                {
                    Swap(solution[i], solution[j]);
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

        private void Process2opt(IEnumerable<Route> solution)
        {
            foreach (var route in solution)
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

        private List<Route> GetInitialSolution()
        {
            var depot = _points.FirstOrDefault(point => point.IsDepot);
            var routes = new List<Route>();

            foreach (var vehicle in _vehicles)
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

                    var destinationsIDs = _points.Where(point => !point.IsDepot && !point.IsEmpty && route.CanBeAdded(point)).Select(point => point.ID).ToList();

                    if (destinationsIDs.Count == 0) break;

                    var nearestDestinations = currentPoint.Distances.Where(item => destinationsIDs.Contains(item.Key)).OrderBy(item => item.Value);

                    var nextPointID = nearestDestinations.First().Key;
                    currentPoint = _points.First(point => point.ID == nextPointID);
                }

                routes.Add(route);

                if (_points.Where(point => !point.IsEmpty).Count() == 0)
                {
                    break;
                }
            }

            return routes;
        }

        private void PrintSolution(List<Route> solution)
        {
            var totalLength = 0.0;

            foreach (var item in solution)
            {
                totalLength += item.Length;
                Console.WriteLine(item);
            }

            var remainedPointsCount = _points.Where(point => solution.Count(route => !route.Points.Contains(point)) == solution.Count).Count();

            Console.WriteLine("Total Length: {0}\nRoutes count: {1}\nNot empty routes count: {2}\nRemained points count: {3}\n\n\n", totalLength, solution.Count, solution.Count(route => !route.IsEmpty), remainedPointsCount);
        }

        private void PrintData()
        {
            Console.WriteLine("======POINTS======\n");
            foreach (var point in _points)
            {
                Console.WriteLine(point);
                Console.WriteLine();
            }

            Console.WriteLine("======VEHICLES======\n");
            foreach (var vehicle in _vehicles)
            {
                Console.WriteLine(vehicle);
            }
        }
    }
}
