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
        public static Tuple<DateTime, DateTime> ParseTimespan(int year, string date, string time)
        {
            DateTime start;
            DateTime end;
            string[] times = time.Split('-');
            if (times.Count() != 2)
                throw new ArgumentException("ParseScheduleTimeColumns: time argument has invalid format: '"
                    + time + "'");
            string startStr = times[0];
            string endStr = times[1];

            try
            {
                string[] hoursAndMinutes = startStr.Split('.');
                for (int i = 0; i < hoursAndMinutes.Length; i++)
                    hoursAndMinutes[i] =
                        hoursAndMinutes[i].Length == 1 ? "0" + hoursAndMinutes[i] : hoursAndMinutes[i];
                object[] parameters = new object[] { date, year, hoursAndMinutes[0], hoursAndMinutes[1] };
                start = DateTime.ParseExact(String.Format("{0} {1} {2}.{3}.00", parameters), 
                    "dd MMM yyyy hh.mm.ss", CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {
                throw new ArgumentException("ParseScheduleTimeColumns: couldn't parse start of activity: '"
                    + startStr + "'", ex);
            }

            try
            {
                string[] hoursAndMinutes = endStr.Split('.');
                for (int i = 0; i < hoursAndMinutes.Length; i++)
                    hoursAndMinutes[i] =
                        hoursAndMinutes[i].Length == 1 ? "0" + hoursAndMinutes[i] : hoursAndMinutes[i];
                object[] parameters = new object[] { date, year, hoursAndMinutes[0], hoursAndMinutes[1] };
                end = DateTime.ParseExact(String.Format("{0} {1} {2}.{3}.00", parameters),
                    "dd MMM yyyy hh.mm.ss", CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {
                throw new ArgumentException("ParseScheduleTimeColumns: couldn't parse end of activity: '"
                    + endStr + "'", ex);
            }

            Tuple<DateTime, DateTime> result = new Tuple<DateTime, DateTime>(start, end);
            return result;
        }

    }

}
