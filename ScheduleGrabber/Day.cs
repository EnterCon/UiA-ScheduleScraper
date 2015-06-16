using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScheduleGrabber
{
    public class Day
    {
        private Activity activity;
        public DateTime Date { get; set; }
        public List<Activity> Activities { get; set; }
    }
}
