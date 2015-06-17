using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        /// Draw a progress bar at the current cursor position.
        /// Be careful not to Console.WriteLine or anything whilst using this to show progress!
        /// Based on: https://gist.github.com/gabehesse/975472
        /// </summary>
        /// <param name="progress">The position of the bar</param>
        /// <param name="total">The amount it counts</param>
        public static void DrawTextProgressBar(int progress, int total, ref Stopwatch timer)
        {
            //draw empty progress bar
            Console.CursorLeft = 0;
            Console.Write("["); //start
            Console.CursorLeft = 32;
            Console.Write("]"); //end
            Console.CursorLeft = 1;
            float onechunk = 30.0f / total;

            //draw filled part
            int position = 1;
            for (int i = 0; i < onechunk * progress; i++)
            {
                Console.CursorLeft = position++;
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("=");
            }

            //draw unfilled part
            for (int i = position; i <= 31; i++)
            {
                Console.CursorLeft = position++;
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(" ");
            }

            string str = "";
            if(progress != 0) // Avoid infinity
            {
                double timePerIteration = timer.Elapsed.TotalSeconds / (double)progress;
                double timeleft = timePerIteration * ((double)total - (double)progress);
                TimeSpan time = TimeSpan.FromSeconds(timeleft);
                str = time.ToString(@"mm\:ss");
            }

            if (progress < 0 || progress >= total)
            {
                throw new InvalidOperationException("DrawProgressBar: Index out of range");
            }
            int percent = (100 * (progress + 1)) / total;

            //draw totals
            Console.CursorLeft = 35;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.Write(progress.ToString() + " of " + total.ToString() +
                " (" + percent + "% done) (" + str + " left) ");
        }
    }
}
