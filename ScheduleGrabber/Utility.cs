using System;
using System.Collections.Generic;
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
    }
}
