using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScheduleGrabber
{
    public class Activity
    {
        public string Title { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string Lecturer { get; set; }
        public string Notice { get; set; }
        public string Room { get; set; }
    }
}
