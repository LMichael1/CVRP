using System;
using System.Collections.Generic;
using System.Text;

namespace CVRP
{
    public enum LineType
    {
        None,
        Demand,
        TimeWindows,
        Point,
        Vehicle,
        DistancesMatrix,
        TimesMatrix
    }
}
