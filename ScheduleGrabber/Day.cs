using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScheduleGrabber
{
    public class Day
    {
        public string Activity { get; set; }
        public DateTime Time { get; set; }
        public string Lecturer { get; set; }
        public string Notice { get; set; }
        public string Room { get; set; }
    }
}
