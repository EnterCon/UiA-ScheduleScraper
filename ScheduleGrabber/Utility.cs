using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ScheduleGrabber
{
    public static class Utility
    {
        /// <summary>
        /// Set console settings to standard for this application
        /// </summary>
        public static void StandardConsole()
        {
            Console.BackgroundColor = ConsoleColor.Black;
        }

        /// <summary>
        /// A utility method for writing exceptions
        /// to the console.
        /// See main for usage.
        /// </summary>
        /// <param name="block">
        ///     a block of code either in the form
        ///     of a method, or lambda expression.
        /// </param>
        public static void ExceptionConsole(Action block)
        {
            Console.BackgroundColor = ConsoleColor.Red;
            block.Invoke();
            Utility.StandardConsole();
        }

        /// <summary>
        /// Load the string into an HtmlDocument object
        /// </summary>
        /// <param name="str">the string</param>
        /// <returns>an HtmlDocument object</returns>
        public static HtmlDocument ToHtml(this string str)
        {
            str = WebUtility.HtmlDecode(str);
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(str);
            return doc;
        }

        /// <summary>
        /// Trim whitespaces and remove escape sequences
        /// </summary>
        /// <param name="str">the string</param>
        /// <returns>result of sanitation</returns>
        public static string Sanitize(this string str)
        {
            StringBuilder res = new StringBuilder();
            List<char> escapeSequences = new List<char>
            {
                '\r', '\n', '\t', '\\', '\f', '\v', '\a', '\b', '\n', '\'', '\"'
            };
            for (int i = 0; i < str.Length; i++)
            {
                if (escapeSequences.Contains(str[i]))
                    continue;
                if (i == 0 && char.IsWhiteSpace(str[i]))
                    continue;
                if (i != 0 && char.IsWhiteSpace(str[i]) && char.IsWhiteSpace(str[i - 1]))
                    continue;
                res.Append(str[i]);
            }
            return res.ToString().TrimEnd();
        }

        /// <summary>
        /// Extension method for getting exception line number.
        /// Source: http://stackoverflow.com/a/11362875
        /// </summary>
        /// <param name="e">the exception</param>
        /// <returns>the linenumber</returns>
        public static int LineNumber(this Exception e)
        {
            int linenum = 0;
            try
            {
                //linenum = Convert.ToInt32(e.StackTrace.Substring(e.StackTrace.LastIndexOf(":line") + 5));
                //For Localized Visual Studio ... In other languages stack trace  doesn't end with ":Line 12"
                linenum = Convert.ToInt32(e.StackTrace.Substring(e.StackTrace.LastIndexOf(' ')));
            }
            catch
            {
                //Stack trace is not available!
            }
            return linenum;
        }
    }
}
