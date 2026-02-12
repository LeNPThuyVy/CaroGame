using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CaroGame.Models
{
    public class Statistics
    {
        public int Win { get; set; } = 0;
        public int Lose { get; set; } = 0;

        public long TotalTime { get; set; } = 0; 
        public int Moves { get; set; } = 0;

        public double AvgTime
        {
            get
            {
                if (Moves == 0) return 0;
                return (double)TotalTime / Moves;
            }
        }

    }
}