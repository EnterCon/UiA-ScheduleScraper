using System;
using System.Collections.Generic;

namespace ScheduleGrabber.Objects
{
    public class Day
    {
        public DateTime Date { get; set; }
        public string DayOfWeek { get; set; }
        public List<Activity> Activities { get; set; }

        public Day()
        {
            this.Activities = new List<Activity>();
        }
    }
}
