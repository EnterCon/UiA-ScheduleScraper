using System;
using System.Collections.Generic;
using System.Globalization;
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

        /// <summary>
        /// This is a method for parsing rows (activities) in a schedulepage week table.
        /// </summary>
        /// <param name="date">
        ///     The date of the activity. Must have the following format;
        ///     date : "17 Aug"
        /// </param>
        /// <param name="time">
        ///     The time column of the activity. Must have the following format;
        ///     time : "09.15-11.00"
        /// </param>
        /// <returns>a tuple of the start and end of an activity</returns>
        public static Tuple<DateTime, DateTime> ParseTimespan(string date, string time)
        {
            DateTime start;
            DateTime end;
            try
            {
                start = DateTime.ParseExact(date, "dd-MMM", CultureInfo.InvariantCulture);
                end = DateTime.ParseExact(date, "dd-MMM", CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {
                throw new ArgumentException("ParseScheduleTimeColumns: can't parse date argument: '"
                    + date + "'", ex);
            }
            string[] times = time.Split('-');
            if (times.Count() != 2)
                throw new ArgumentException("ParseScheduleTimeColumns: time argument has invalid format: '"
                    + time + "'");
            string startStr = times[0];
            string endStr = times[1];

            try
            {
                string[] hoursAndMinutes = startStr.Split('.');
                int startHours = int.Parse(hoursAndMinutes[0]);
                int startMinutes = int.Parse(hoursAndMinutes[1]);
                TimeSpan startTime = new TimeSpan(startHours, startMinutes, 0);
                start.Add(startTime);
            }
            catch (Exception ex)
            {
                throw new ArgumentException("ParseScheduleTimeColumns: couldn't parse start of day: '"
                    + startStr + "'", ex);
            }

            try
            {
                string[] hoursAndMinutes = endStr.Split('.');
                int endHours = int.Parse(times[1].Split('.')[0]);
                int endMinutes = int.Parse(times[1].Split('.')[1]);
                TimeSpan endTime = new TimeSpan(endHours, endMinutes, 0);
                end.Add(endTime);
            }
            catch (Exception ex)
            {
                throw new ArgumentException("ParseScheduleTimeColumns: couldn't parse end of day: '"
                    + endStr + "'", ex);
            }

            Tuple<DateTime, DateTime> result = new Tuple<DateTime, DateTime>(start, end);
            return result;
        }

    }

}
