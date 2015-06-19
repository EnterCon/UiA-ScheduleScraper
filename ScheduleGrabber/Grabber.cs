using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Diagnostics;
using NDesk.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Converters;
using ScheduleGrabber.Objects;
using ScheduleGrabber.Utilities;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Threading;
using System.Net;

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
    ///     * Implement better progress notifications
    ///     * Create tests for important functionality
    ///     * Consider XML support
    ///     * Consider No-SQL support
    /// </summary>
    public static class Grabber
    {
        public static HttpClient Client = new HttpClient();
        public static string URL = "http://timeplan.uia.no/swsuiah/public/no/default.aspx";
        public static PostData RequestData { get; set; }
        public static HtmlDocument SchedulePage { get; set; }
        public static List<Department> Departments = new List<Department>();
        public static Stopwatch Timer = new Stopwatch();
        public static Queue<string> Latest = new Queue<string>();
        public static List<Exception> RuntimeExceptions = new List<Exception>();

        /// <summary>
        /// Run the ScheduleGrabber!
        /// </summary>
        /// <param name="args">CLI arguments</param>
        static void Main(string[] args)
        {
            Client.Timeout = new TimeSpan(1, 0, 0); // Never quit

            Utility.StandardConsole();
            Console.WriteLine();
            Console.WriteLine(@"   ____    __          __     __    _____         __   __          ");
            Console.WriteLine(@"  / __/___/ /  ___ ___/ /_ __/ /__ / ___/______ _/ /  / /  ___ ____");
            Console.WriteLine(@" _\ \/ __/ _ \/ -_) _  / // / / -_) (_ / __/ _ `/ _ \/ _ \/ -_) __/");
            Console.WriteLine(@"/___/\__/_//_/\__/\_,_/\_,_/_/\__/\___/_/  \_,_/_.__/_.__/\__/_/   ");
            Console.WriteLine();
            Console.WriteLine(@"___________________________________________________________________");
            Console.WriteLine();
            Console.WriteLine();

            bool show_help = false;
            string file = null;
            string id = null;
            var options = new OptionSet()
                .Add("h|help", "show this message and exit", v => show_help = v != null)
                .Add("f|file=", "write JSON to specified file", input => file = input)
                .Add("i|id=", "write only the schedule data related to a specific department ID to JSON file", input => id = input);

            try
            {
                List<string> extra = new List<string>();
                extra = options.Parse(args);

                if (show_help)
                {
                    ShowHelp(options);
                    Console.WriteLine("Press any key to continue...");
                    Console.ReadKey();
                }
                Task.Run( async () =>
                { 
                    SchedulePage = GetSchedulePage();
                    RequestData = GetRequestData();
                    GetDepartments(id);
                    await GrabSchedules();
                    ToFile(ToJson(Departments), file);
                }).Wait();
            }
            catch (OptionException e)
            {
                Utility.WriteException(() =>
                {
                    Console.Write("UiA ScheduleGrabber: ");
                    Console.WriteLine(e.Message);
                    Console.WriteLine("Try `ScheduleGrabber --help' for more information.");
                });
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
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
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
            }
            Console.WriteLine();
            Console.WriteLine("ScheduleGrabber finished grabbing!");
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        /// <summary>
        /// Display available options
        /// </summary>
        /// <param name="options">the NDesk.OptionSet</param>
        static void ShowHelp(OptionSet options)
        {
            Console.WriteLine("Usage: ScheduleGrabber [OPTIONS]");
            Console.WriteLine("Write UiA schedule data to JSON.");
            Console.WriteLine("If no flags are given, departments.json is generated\n" +
                "from the schedule data of all departments.");
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
            document = Utility.ToHtml(responseContent);
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
        public static void GetDepartments(string id = null)
        {
            if (SchedulePage == null || SchedulePage.DocumentNode == null 
                || SchedulePage.DocumentNode.Descendants().Count() == 0)
                throw new ArgumentException("GetDepartments: the ScheduledPage hasn't been loaded into memory.");
            HtmlNode selectBox = SchedulePage.DocumentNode.Descendants().Where(d => d.Id == "dlObject").First();
            if (id != null)
            {
                var elem = selectBox.Descendants("option")
                    .Where(o => o.GetAttributeValue("value", null) == id).First();
                string idStr = elem.GetAttributeValue("value", null);
                string name = elem.InnerText;
                Department department = new Department(idStr, name);
                Departments.Add(department);
            }
            else
            {
                foreach (var option in selectBox.Descendants("option"))
                {
                    string idStr = option.GetAttributeValue("value", null);
                    if (idStr == null)
                    {
                        Department dep = new Department(new Exception("GetDepartments: no ID was found for element"));
                        Departments.Add(dep);
                        continue;
                    }
                    string name = option.NextSibling.InnerText;
                    Department department = new Department(idStr, name);
                    Departments.Add(department);
                }
            }
        }

        /// <summary>
        /// Grab schedules for a list of departments
        /// </summary>
        public static async Task GrabSchedules()
        {
            Timer.Start();
            int counter = 0;

            var tasks = Departments.Select(department => department.GrabSchedule()
                .ContinueWith(async dep => UpdateConsole(ref counter, await dep))).ToArray();
            await Task.WhenAll(tasks);
            Timer.Stop();
        }

        /// <summary>
        /// Update the current status of ScheduleGrabber,
        /// Inform the user!
        /// </summary>
        /// <param name="counter">Departments iteration counter</param>
        /// <param name="dep">the department that was just finished</param>
        /// <param name="time">how long it took to finish that department in milliseconds</param>
        public static void UpdateConsole(ref int counter, Department department)
        {
            Interlocked.Increment(ref counter);
            Utility.DrawTextProgressBar(counter, Departments.Count, ref Timer);
            Console.SetCursorPosition(0, 8);
            string timeStr = " (" + department.Timer.ElapsedMilliseconds + " ms)";
            string departmentStr = department.Name;
            departmentStr = (departmentStr.Length >= Console.WindowWidth - timeStr.Length)
                            ? departmentStr.Remove(Console.WindowWidth - timeStr.Length - 1)
                            : departmentStr;
            Latest.Enqueue(departmentStr + timeStr);
            if (Latest.Count > 5)
                Latest.Dequeue();

            int i = 0;
            foreach (string dep in Latest)
            {
                Utility.ClearCurrentConsoleLine();
                Console.WriteLine("\r" + dep);
                i++;
            }
        }

        /// <summary>
        /// Convert object to JSON
        /// </summary>
        /// <returns>JSON-formatted string</returns>
        public static string ToJson(object value)
        {
            var settings = new JsonSerializerSettings();
            settings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            settings.Converters.Add(new IsoDateTimeConverter() { DateTimeFormat = "dd-MM-yyyy HH:mm:ss" });
            return Newtonsoft.Json.JsonConvert.SerializeObject(
                value,
                Formatting.Indented,
                settings
            );
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
