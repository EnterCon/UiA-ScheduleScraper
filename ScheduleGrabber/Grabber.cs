using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Diagnostics;
using NDesk.Options;

namespace ScheduleGrabber
{
    /// <summary>
    /// This is the ScheduleGrabber.
    /// It will gather all schedule information from the
    /// Timeplan website at UiA, by iterating every department ID
    /// and making POST requests.
    /// 
    /// The resulting data is stored in a database, to be
    /// made available through an API.
    /// </summary>
    public static class Grabber
    {
        public static HttpClient Client = new HttpClient();
        public static string URL = "http://timeplan.uia.no/swsuiav/public/no/default.aspx";
        public static PostData RequestData { get; set; }
        public static HtmlDocument SchedulePage { get; set; }
        static Stopwatch timer = new Stopwatch();

        /// <summary>
        /// Run the ScheduleGrabber!
        /// </summary>
        /// <param name="args">CLI arguments</param>
        static void Main(string[] args)
        {
            Utility.StandardConsole();
            Console.WriteLine();
            Console.WriteLine(@"   ____    __          __     __    _____         __   __          ");
            Console.WriteLine(@"  / __/___/ /  ___ ___/ /_ __/ /__ / ___/______ _/ /  / /  ___ ____");
            Console.WriteLine(@" _\ \/ __/ _ \/ -_) _  / // / / -_) (_ / __/ _ `/ _ \/ _ \/ -_) __/");
            Console.WriteLine(@"/___/\__/_//_/\__/\_,_/\_,_/_/\__/\___/_/  \_,_/_.__/_.__/\__/_/   ");
            Console.WriteLine();

            bool show_help = false;
            var options = new OptionSet()
                .Add("h|help",  "show this message and exit", v => show_help = v != null);

            try
            {
                List<string> extra = new List<string>();
                extra = options.Parse(args);
                SchedulePage = GetSchedulePage();
                RequestData = GetRequestData();
                List<Department> departments = GetDepartments();
            }
            catch (OptionException e)
            {
                Console.Write("UiA ScheduleGrabber: ");
                Console.WriteLine(e.Message);
                Console.WriteLine("Try `ScheduleGrabber --help' for more information.");
                return;
            }

            if (show_help)
            {
                ShowHelp(options);
                return;
            }
        }

        /// <summary>
        /// Display available options
        /// </summary>
        /// <param name="options">the NDesk.OptionSet</param>
        static void ShowHelp(OptionSet options)
        {
            Console.WriteLine("Usage: ScheduleGrabber [OPTIONS]");
            Console.WriteLine("Write UiA schedule data to JSON");
            Console.WriteLine("If no flags are given, departments.json is generated.");
            Console.WriteLine();
            Console.WriteLine("Options:");
            options.WriteOptionDescriptions(Console.Out);
        }

        /// <summary>
        /// Request the UiA-Schedule website, parse it,
        /// and load it into memory as an HmlDocument oject.
        /// </summary>
        /// <returns>the HtmlDocument</returns>
        private static HtmlDocument GetSchedulePage()
        {
            HtmlDocument doc = new HtmlDocument();
            var resp = Client.GetAsync(URL).Result;
            var respStr = resp.Content.ReadAsStringAsync().Result;
            doc = respStr.ToHtml();
            return doc;
        }

        /// <summary>
        /// Get the Form request data required to POST
        /// and retrieve department schedule information
        /// </summary>
        /// <returns></returns>
        public static PostData GetRequestData()
        {
            if(SchedulePage == null || SchedulePage.ToString().Length >= 0)
                throw new ArgumentException("SchedulePage document isn't loaded!");
            string __VIEWSTATE = SchedulePage.DocumentNode.Descendants()
                .Where(d => d.Id == "__VIEWSTATE").First().GetAttributeValue("value", null);
            string __VIEWSTATEGENERATOR = SchedulePage.DocumentNode.Descendants()
                .Where(d => d.Id == "__VIEWSTATEGENERATOR").First().GetAttributeValue("value", null);
            string __EVENTVALIDATION = SchedulePage.DocumentNode.Descendants()
                .Where(d => d.Id == "__EVENTVALIDATION").First().GetAttributeValue("value", null);
            string tLinkType = "studentsets";
            string tWildCard = "";
            string lbWeeks = SchedulePage.DocumentNode.Descendants()
                .Where(d => d.Id == "lbWeeks").First()
                .Descendants("option").ElementAt(1).GetAttributeValue("value", null);
            string lbDays = "1-6";
            string RadioType = "XMLSpreadsheet;studentsetxmlurl;SWSCUST StudentSet XMLSpreadsheet";
            string bGetTimetable = "Vis+timeplan";
            return new PostData()
                {
                    __EVENTARGUMENT = "",
                    __EVENTTARGET = "",
                    __LASTFOCUS = "",
                    __VIEWSTATE = __VIEWSTATE,
                    __VIEWSTATEGENERATOR = __VIEWSTATEGENERATOR,
                    __EVENTVALIDATION = __EVENTVALIDATION,
                    tLinkType = tLinkType,
                    tWildcard = tWildCard,
                    lbWeeks = lbWeeks,
                    lbDays = lbDays,
                    RadioType = RadioType,
                    bGetTimetable = bGetTimetable
                };
        }

        /// <summary>
        /// Place all the departments from the SchedulePage selectbox
        /// into memory, so that we can grab their schedule.
        /// </summary>
        /// <returns>a list of department objects</returns>
        public static List<Department> GetDepartments()
        {
            HtmlNode selectBox = SchedulePage.DocumentNode.Descendants().Where(d => d.Id == "dlObject").First();
            List<Department> departments = new List<Department>();
            foreach (var option in selectBox.Descendants("option"))
                departments.Add(new Department(option.GetAttributeValue("value", null)));
            return departments;
        }
    }
}
