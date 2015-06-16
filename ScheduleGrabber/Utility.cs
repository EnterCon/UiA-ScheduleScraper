using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ScheduleGrabber
{
    public static class Utility
    {
        public static void StandardConsole()
        {
            Console.BackgroundColor = ConsoleColor.Green;
        }

        public static void ExceptionConsole(Action block)
        {
            Console.BackgroundColor = ConsoleColor.Red;
            block.Invoke();
            Utility.StandardConsole();
        }

        public static int GetWeekNumber(string stringContainingWeekNumber)
        {
            Regex weekNumberReg = new Regex(@"(?<=Uke.*)\b([0-9]|[1-4][0-9]|5[0-2])\b", RegexOptions.IgnoreCase);
            Match weekNumberMatch = weekNumberReg.Match(stringContainingWeekNumber);
            int count = 0; int weekNumber = 0;
            while(weekNumberMatch.Success)
            {
                if (count >= 1)
                    throw new ArgumentException("GetWeekNumber found more than 1 match");

                weekNumber = int.Parse(weekNumberMatch.Groups[1].Value);
                count++;
                weekNumberMatch = weekNumberMatch.NextMatch();
            }
            if (weekNumber == 0)
                throw new ArgumentException("GetWeekNumber found no valid weeknumber for string: '" +
                    stringContainingWeekNumber + "'");
            return weekNumber;
        }

        public static Tuple<DateTime, DateTime> ParseScheduleTimeColumns(string date, string time)
        {
            DateTime start;
            DateTime end;
            try
            {
                start = DateTime.ParseExact(date, "dd-MMM", CultureInfo.InvariantCulture);
                end = DateTime.ParseExact(date, "dd-MMM", CultureInfo.InvariantCulture);
            }
            catch(Exception ex)
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
            catch(Exception ex)
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
            catch(Exception ex)
            {
                throw new ArgumentException("ParseScheduleTimeColumns: couldn't parse end of day: '"
                    + endStr + "'", ex);
            }

            Tuple<DateTime, DateTime> result = new Tuple<DateTime, DateTime>(start, end);
            return result;
        }
    }
}
