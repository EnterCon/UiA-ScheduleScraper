using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Diagnostics;
using NDesk.Options;
using Newtonsoft.Json;

namespace ScheduleGrabber
{
    /// <summary>
    /// This is the UiA ScheduleGrabber.
    /// It will gather all schedule information from the
    /// Timeplan website at UiA, by iterating every department ID
    /// and making POST requests.
    /// 
    /// TODO:
    ///     * From LINQ to XPath when querying the HtmlDocument for nodes (prettier)
    ///     * Consider XML support
    ///     * Consider No-SQL support
    /// </summary>
    public static class Grabber
    {
        public static HttpClient Client = new HttpClient();
        public static string URL = "http://timeplan.uia.no/swsuiah/public/no/default.aspx";
        public static PostData RequestData { get; set; }
        public static HtmlDocument SchedulePage { get; set; }
        public static List<Department> Departments { get; set; }
        private static Stopwatch timer = new Stopwatch();

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
            string file = null;
            string id = null;
            var options = new OptionSet()
                .Add("h|help", "show this message and exit", v => show_help = v != null)
                .Add("f|file=", "write JSON to specified file", input => file = input)
                .Add("i|id=", "write only the specified ID to JSON file", input => id = input);

            try
            {
                List<string> extra = new List<string>();
                extra = options.Parse(args);

                if (show_help)
                {
                    ShowHelp(options);
                    return;
                }

                SchedulePage = GetSchedulePage();
                RequestData = GetRequestData();
                Departments = GetDepartments(id);
                ToFile(GrabSchedules(Departments), file);
            }
            catch (OptionException e)
            {
                Utility.WriteException(() =>
                {
                    Console.Write("UiA ScheduleGrabber: ");
                    Console.WriteLine(e.Message);
                    Console.WriteLine("Try `ScheduleGrabber --help' for more information.");
                });
                return;
            }
            catch(Exception e)
            {
                Utility.WriteException(() =>
                {
                    Console.WriteLine();
                    Console.WriteLine("Exception occurred: " + e.Message);
                    Console.WriteLine("At line number: " + e.LineNumber());
                    Console.WriteLine();
                });
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
            HtmlDocument document = new HtmlDocument();
            var response = Client.GetAsync(URL).Result;
            var responseContent = response.Content.ReadAsStringAsync().Result;
            document = responseContent.ToHtml();
            return document;
        }

        /// <summary>
        /// Get the Form request data required to POST
        /// and retrieve department schedule information
        /// </summary>
        /// <returns></returns>
        public static PostData GetRequestData()
        {
            if(SchedulePage == null || SchedulePage.ToString().Length <= 0)
                throw new ArgumentException("GetRequestData: SchedulePage document isn't loaded!");
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
        public static List<Department> GetDepartments(string id = null)
        {
            if (SchedulePage == null || SchedulePage.DocumentNode == null || SchedulePage.DocumentNode.Descendants().Count() == 0)
                throw new ArgumentException("GetDepartments: the ScheduledPage hasn't been loaded into memory.");
            HtmlNode selectBox = SchedulePage.DocumentNode.Descendants().Where(d => d.Id == "dlObject").First();
            List<Department> departments = new List<Department>();
            if (id != null)
            {
                departments.Add(new Department(selectBox.Descendants("option")
                    .Where(o => o.GetAttributeValue("value", null) == id).First().GetAttributeValue("value", null)));
            }
            else
            {
                foreach (var option in selectBox.Descendants("option"))
                    departments.Add(new Department(option.GetAttributeValue("value", null)));
            }
            return departments;
        }

        /// <summary>
        /// Grab schedules for a list of departments, and serialize it to JSON
        /// </summary>
        /// <param name="departments">A list of departments</param>
        /// <returns>a JSON-formatted string</returns>
        public static string GrabSchedules(List<Department> departments)
        {
            for (int i = 0; i < departments.Count; i++)
            {
                departments[i].GrabSchedule(RequestData);
                Utility.ShowPercentProgress("Grabbing schedule information", i, departments.Count);
            }
            return ToJson(departments);
        }

        /// <summary>
        /// Convert object to JSON
        /// </summary>
        /// <returns>JSON-formatted string</returns>
        public static string ToJson(object value)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(value, Formatting.Indented);
        }

        /// <summary>
        /// Write the JSON to file. Departments by default
        /// </summary>
        /// <param name="file">the file to write to</param>
        /// <param name="json">JSON-formatted representation of department schedules</param>
        public static void ToFile(string json, string file = null)
        {
            if (file == null || file.Length < 1)
                file = "departments.json";
            System.IO.File.WriteAllText(Utility.Directory + "\\" + file, json);
        }
    }
}
