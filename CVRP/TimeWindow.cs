using System;
using System.Collections.Generic;
using System.Text;

namespace CVRP
{
    class TimeWindow
    {
        public int Start { get; set; }
        public int End { get; set; }

        public TimeWindow(int start, int end)
        {
            Start = start;
            End = end;
        }
    }
}
