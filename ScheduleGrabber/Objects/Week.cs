using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ScheduleGrabber
{
    public class Week
    {
        public int Number { get; set; }
        public int Year { get; set; }
        public List<Day> Days { get; set; }

        public Week()
        {
            this.Days = new List<Day>();
        }

        /// <summary>
        /// Get the weeknumber from a table header in the schedule website
        /// Example format: "Uke 34, 2015" -> where we want to return 34
        /// </summary>
        /// <param name="stringContainingWeekNumber">the string to parse</param>
        /// <returns>an integerer indicating the weeknumber</returns>
        public static int GetWeekNumber(string stringContainingWeekNumber)
        {
            Regex weekNumberReg = new Regex(@"(?<=Uke.*)\b([0-9]|[1-4][0-9]|5[0-2])\b", RegexOptions.IgnoreCase);
            Match weekNumberMatch = weekNumberReg.Match(stringContainingWeekNumber);
            int count = 0; int weekNumber = 0;
            while (weekNumberMatch.Success)
            {
                if (count > 1)
                    throw new ArgumentException("GetWeekNumber found more than 1 match for string: '" +
                    stringContainingWeekNumber + "'");

                weekNumber = int.Parse(weekNumberMatch.Groups[0].Value);
                count++;
                weekNumberMatch = weekNumberMatch.NextMatch();
            }
            if (weekNumber == 0)
                throw new ArgumentException("GetWeekNumber found no valid weeknumber for string: '" +
                    stringContainingWeekNumber + "'");
            return weekNumber;
        }

        /// <summary>
        /// Get the year from a string
        /// </summary>
        /// <param name="stringContainingYear">
        ///     a string containing a yearnumber
        ///     The year must be within the 21st century (2000 -> 2099)
        /// </param>
        /// <returns>an integer indicating the year</returns>
        public static int GetYear(string stringContainingYear)
        {
            Regex yearReg = new Regex(@"(20)\d{2}", RegexOptions.IgnoreCase);
            Match yearMatch = yearReg.Match(stringContainingYear);
            int count = 0; int year = 0;
            while (yearMatch.Success)
            {
                if (count > 1)
                    throw new ArgumentException("GetYear found more than 1 match for string: '"
                    + stringContainingYear + "'");

                year = int.Parse(yearMatch.Groups[0].Value);
                count++;
                yearMatch = yearMatch.NextMatch();
            }
            if (year == 0)
                throw new ArgumentException("GetYear found no valid year for string: '"
                    + stringContainingYear + "'");
            return year;
        }
    }
}
