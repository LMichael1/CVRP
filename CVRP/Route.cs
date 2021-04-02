using System;
using System.Collections.Generic;
using System.Text;

namespace CVRP
{
    class Route
    {
        public Vehicle Vehicle { get; set; }
        public List<Point> Points { get; set; }
        public double Length
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

        public Route(Vehicle vehicle)
        {
            Vehicle = vehicle;
            Points = new List<Point>();
        }

        public override string ToString()
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.Append("---------------------------\n");

            stringBuilder.Append("========= VEHICLE =========\n");
            stringBuilder.Append(Vehicle.ToString());

            stringBuilder.Append("========= ROUTE =========\n");

            stringBuilder.Append("Route: ");

            foreach (var point in Points)
            {
                stringBuilder.Append(point.ID);
                stringBuilder.Append(" ");
            }

            stringBuilder.AppendFormat("\nLength: {0}", Length);

            stringBuilder.Append("\n---------------------------\n");

            return stringBuilder.ToString();
        }
    }
}
