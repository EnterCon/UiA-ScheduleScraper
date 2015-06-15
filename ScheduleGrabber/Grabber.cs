using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Diagnostics;

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
        public static HtmlDocument SchedulePage { get; set }

        static Stopwatch timer = new Stopwatch();

        /// <summary>
        /// Run the program
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            try
            {
                Utility.StandardConsole();
                SchedulePage = GetSchedulePage();
                List<Department> departments = GetDepartments();
                

                int amount = 0;
                timer.Start();
                Console.Write("\rInserted data for " + amount + " of " + departments.Count + " departments in " +
                    string.Format("{0:0.0}", timer.Elapsed.TotalSeconds) + " seconds (" +
                    string.Format("{0:0.00}", amount / timer.Elapsed.TotalSeconds) + " departments/second)");
                foreach(Department department in departments)
                {
                    var requestData = new Dictionary<string, string>
                    {
                    { "__EVENTTARGET", "" },
                    { "__EVENTARGUMENT", ""},
                    { "__LASTFOCUS", ""},
                    { "__VIEWSTATE", __VIEWSTATE },
                    { "__VIEWSTATEGENERATOR", __VIEWSTATEGENERATOR },
                    { "__EVENTVALIDATION", __EVENTVALIDATION },
                    { "tLinkType",  tLinkType},
                    { "tWildcard", tWildCard},
                    { "dlObject", department },
                    { "lbWeeks", lbWeeks},
                    { "lbDays", lbDays},
                    { "RadioType", RadioType},
                    { "bGetTimetable", bGetTimetable}
                    };

                    BsonDocument document = GetScheduleData(requestData, department);

                    batch.Add(document);
                    amount++;
                    Console.Write("\rGot data for " + amount + " of " + departments.Count + " departments in " +
                        string.Format("{0:0.0}", timer.Elapsed.TotalSeconds) + " seconds (" +
                        string.Format("{0:0.00}", amount / timer.Elapsed.TotalSeconds) + " departments/second)");
                }
                timer.Stop();
            }
            catch (Exception ex)
            {
                Utility.ExceptionConsole(() =>
                {
                    Console.WriteLine("");
                    Console.WriteLine("EXCEPTION OCCURRED: \n " + ex.Message + " \n at: " + ex.LineNumber());
                });
            }



            Console.WriteLine("");
            Console.WriteLine("ScheduleGrabber finished grabbing in " + string.Format("{0:0.0}", timer.Elapsed.TotalSeconds));
            Console.ReadLine();
        }

        private static HtmlDocument GetSchedulePage()
        {
            HtmlDocument doc = new HtmlDocument();
            var resp = Client.GetAsync(URL).Result;
            var respStr = resp.Content.ReadAsStringAsync().Result;
            doc = respStr.ToHtml();
            return doc;
        }

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
                .Where(d => d.Id == "lbWeeks").First().Descendants("option").ElementAt(1).GetAttributeValue("value", null);
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


        public static List<Department> GetDepartments()
        {
            HtmlNode selectBox = doc.DocumentNode.Descendants().Where(d => d.Id == "dlObject").First();
            List<Department> departments = new List<Department>();
            foreach (var option in selectBox.Descendants("option"))
                departments.Add(new Department(option.GetAttributeValue("value", null)));
            return departments;
        }

        /// <summary>
        /// Takes request data and a department ID to request it's schedule.
        /// </summary>
        /// <param name="requestData">the data for the POST request</param>
        /// <param name="department">the ID of the department</param>
        /// <returns>a BsonDocument to be stored somewhere</returns>
        public static Department GetScheduleData(Dictionary<string, string> requestData, string department)
        {
            
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
