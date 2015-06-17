using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace ScheduleGrabber.Objects
{
    public class Activity
    {
        public List<string> Courses { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string Lecturer { get; set; }
        public string Notice { get; set; }
        public List<string> Rooms { get; set; }

        public Activity()
        {
            this.Rooms = new List<string>();
            this.Courses = new List<string>();
        }

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
        public void ParseTimespan(int year, string date, string time)
        {
            string[] times = time.Split('-');
            if (times.Count() != 2)
                throw new ArgumentException("ParseTimespan: time argument has invalid format: '"
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
                this.Start = DateTime.ParseExact(String.Format("{0} {1} {2}.{3}.00", parameters), 
                    "dd MMM yyyy HH.mm.ss", CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {
                throw new ArgumentException("ParseTimespan: couldn't parse start of activity: '"
                    + startStr + "'", ex);
            }

            try
            {
                string[] hoursAndMinutes = endStr.Split('.');
                for (int i = 0; i < hoursAndMinutes.Length; i++)
                    hoursAndMinutes[i] =
                        hoursAndMinutes[i].Length == 1 ? "0" + hoursAndMinutes[i] : hoursAndMinutes[i];
                object[] parameters = new object[] { date, year, hoursAndMinutes[0], hoursAndMinutes[1] };
                this.End = DateTime.ParseExact(String.Format("{0} {1} {2}.{3}.00", parameters),
                    "dd MMM yyyy HH.mm.ss", CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {
                throw new ArgumentException("ParseTimespan: couldn't parse end of activity: '"
                    + endStr + "'", ex);
            }
        }

        /// <summary>
        /// Parse a string of rooms into separate objects.
        /// </summary>
        /// <param name="stringOfRooms">
        ///     a string containing several rooms.
        ///     Example: "CK IK 023, CK IK 028, CK IU 054, CK IU 056, CK IU 057, CK IU 059"
        /// </param>
        public void ParseRooms(string stringOfRooms)
        {
            string[] rooms = stringOfRooms.Split(',');
            foreach(var room in rooms)
            {
                this.Rooms.Add(room.Trim());
            }
        }

        /// <summary>
        /// Parses a string and retrieves course-ID's from it.
        /// </summary>
        /// <param name="stringOfCourses">
        ///     A string containing courses. Example:
        ///     "KJ-111 BIO111 ML-112/BIO104 MU-139 Musikkdidaktikk, tirs -ERN100 for"
        ///     See: http://regexr.com/3b7hc
        /// </param>
        public void ParseCourses(string stringOfCourses)
        {
            Regex coursesReg = new Regex(@"(([A-Z]{2}|[A-Z]{3})(-)?\d{3})");
            Match coursesMatch = coursesReg.Match(stringOfCourses);
            while(coursesMatch.Success)
            {
                this.Courses.Add(coursesMatch.Value);
                coursesMatch = coursesMatch.NextMatch();
            }
        }

    }

}
