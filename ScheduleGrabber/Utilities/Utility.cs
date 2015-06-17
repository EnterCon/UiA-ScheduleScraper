using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;

namespace ScheduleGrabber.Utilities
{
    public static class Utility
    {
        /// <summary>
        /// Set console settings to standard for this application
        /// </summary>
        public static void StandardConsole()
        {
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;
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
        public static void WriteException(Action block)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            block.Invoke();
            Utility.StandardConsole();
        }

        /// <summary>
        /// Load the string into an HtmlDocument object
        /// </summary>
        /// <param name="str">the string</param>
        /// <returns>an HtmlDocument object</returns>
        public static HtmlDocument ToHtml(string str)
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
        public static string Sanitize(string str)
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

        /// <summary>
        /// Get the directory of the executing assembly
        /// From: http://stackoverflow.com/a/283917
        /// </summary>
        public static string Directory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }

        /// <summary>
        /// Show progress as percentage
        /// From: http://geekswithblogs.net/abhijeetp/archive/2010/02/21/showing-progress-in-a-.net-console-application.aspx
        /// </summary>
        /// <param name="message"></param>
        /// <param name="currElementIndex"></param>
        /// <param name="totalElementCount"></param>
        public static void ShowPercentProgress(string message, int currElementIndex, int totalElementCount)
        {
            if (currElementIndex < 0 || currElementIndex >= totalElementCount)
            {
                throw new InvalidOperationException("currElement out of range");
            }
            int percent = (100 * (currElementIndex + 1)) / totalElementCount;
            Console.Write("\r{0} {1}% complete", message, percent);
            if (currElementIndex == totalElementCount - 1)
            {
                Console.WriteLine(Environment.NewLine);
            }
        }
    }
}
