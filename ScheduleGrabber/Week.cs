using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScheduleGrabber
{
    public class Week
    {
        public string Name { get; set; }
        public int Number { get; set; }
        public int Year { get; set; }
        public List<Day> Days { get; set; }
    }
}
