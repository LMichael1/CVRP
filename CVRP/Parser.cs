using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CVRP
{
    class Parser
    {
        private readonly string _filePath;

        public List<Point> Points { get; }
        public List<Vehicle> Vehicles { get; }

        public Parser(string filePath)
        {
            _filePath = filePath;
            Points = new List<Point>();
            Vehicles = new List<Vehicle>();
        }

        public async Task Parse()
        {
            using (StreamReader sr = new StreamReader(_filePath, Encoding.Default))
            {
                string line;

                var lineType = LineType.None;

                while ((line = await sr.ReadLineAsync()) != null)
                {
                    if (line.Contains("POINTS"))
                    {
                        lineType = LineType.Point;
                        continue;
                    }

                    if (line.Contains("CARS"))
                    {
                        lineType = LineType.Vehicle;
                        continue;
                    }

                    if (line.Contains("COMMON_PARAMETERS"))
                    {
                        lineType = LineType.None;
                        continue;
                    }

                    if (line.Contains("DISTANCE"))
                    {
                        lineType = LineType.Matrix;
                        continue;
                    }

                    if (lineType == LineType.Matrix && line[0] != '0')
                    {
                        break;
                    }

                    switch (lineType)
                    {
                        case LineType.Point:
                            ParsePoints(line);
                            break;
                        case LineType.Vehicle:
                            ParseVehicles(line);
                            break;
                        case LineType.Matrix:
                            ParseMatrix(line);
                            break;
                        default:
                            break;
                    }
                }
            }

            var pointsIDs = Points.Select(point => point.ID);

            foreach (var point in Points)
            {
                var uncommonIDs = pointsIDs.Except(point.Distances.Keys);

                foreach (var id in uncommonIDs)
                {
                    point.Distances.Add(id, 0.0);
                }
            }
        }

        private void ParsePoints(string line)
        {
            var splitted = line.Split(new[] { ',' });

            var id = Convert.ToInt32(splitted[1]);
            var latitude = Convert.ToDouble(splitted[2], CultureInfo.InvariantCulture);
            var longitude = Convert.ToDouble(splitted[3], CultureInfo.InvariantCulture);
            var firstVolume = Convert.ToInt32(splitted[4]);
            var secondVolume = Convert.ToInt32(splitted[5]);
            var thirdVolume = Convert.ToInt32(splitted[6]);

            if (firstVolume > 0)
            {
                var point = new Point(id, latitude, longitude, ProductType.First, firstVolume);
                Points.Add(point);

                return;
            }

            if (secondVolume > 0)
            {
                var point = new Point(id, latitude, longitude, ProductType.Second, secondVolume);
                Points.Add(point);

                return;
            }
            
            if (thirdVolume > 0)
            {
                var point = new Point(id, latitude, longitude, ProductType.Third, thirdVolume);
                Points.Add(point);

                return;
            }

            Points.Add(new Point(id, latitude, longitude, ProductType.None, 0));
        }

        private void ParseVehicles(string line)
        {
            var splitted = line.Split(new[] { ',' });

            var id = Convert.ToInt32(splitted[0]);

            var barrelsString = splitted[24].Trim(new[] { '(', ')' });
            var orderString = splitted[25].Split(new[] { ':' })[0].Trim(new[] { '(', ')' });

            var barrelsSplitted = barrelsString.Split(new[] { ';' });
            var orderSplitted = orderString.Split(new[] { ';' });

            var barrels = new List<Barrel>(barrelsSplitted.Length);

            for (int i = 0; i < barrelsSplitted.Length; i++)
            {
                var barrel = new Barrel(Convert.ToInt32(barrelsSplitted[i]), Convert.ToInt32(orderSplitted[i]));
                barrels.Add(barrel);
            }

            var vehicle = new Vehicle(id, barrels);
            Vehicles.Add(vehicle);
        }

        private void ParseMatrix(string line)
        {
            var splitted = line.Split(new[] { ',' });

            var firstIndex = Convert.ToInt32(splitted[1]);
            var secondIndex = Convert.ToInt32(splitted[2]);

            var distance = Convert.ToDouble(splitted[3], CultureInfo.InvariantCulture);

            Points[firstIndex].Distances.Add(Points[secondIndex].ID, distance);
        }
    }
}
