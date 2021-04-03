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
            PrintSolution(solution);

            Process2opt(solution);
            PrintSolution(solution);
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

                    if (shouldRestart)
                    {
                        break;
                    }
                }
            }
        }

        private List<Route> GetInitialSolution()
        {
            var depot = _points.FirstOrDefault(point => point.IsDepot);
            var routes = new List<Route>();

            foreach (var vehicle in _vehicles)
            {
                var route = new Route(vehicle);

                var currentPoint = depot;

                while (!vehicle.IsFull)
                {
                    route.Points.Add(currentPoint);

                    if (currentPoint != depot)
                    {
                        var volume = currentPoint.Product.CurrentVolume;
                        vehicle.FillBarrels(ref volume, ProductType.None); // None пока что
                        currentPoint.Product.CurrentVolume = volume;
                    }

                    var destinations = _points.Where(point => !point.Product.IsEmpty).Select(point => point.ID).ToList();

                    if (destinations.Count == 0) break;

                    var nearestDestinations = currentPoint.Distances.Where(item => destinations.Contains(item.Key)).OrderBy(item => item.Value);

                    var nextPointID = nearestDestinations.First().Key;
                    currentPoint = _points.First(point => point.ID == nextPointID);
                }

                route.Points.Add(depot);

                routes.Add(route);

                if (_points.Where(point => !point.Product.IsEmpty).Count() == 0)
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

            Console.WriteLine("Total Length: {0}", totalLength);
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
