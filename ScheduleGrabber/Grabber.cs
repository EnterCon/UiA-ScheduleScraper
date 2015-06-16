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
        public static HtmlDocument SchedulePage { get; set; }

        static Stopwatch timer = new Stopwatch();

        /// <summary>
        /// Run the ScheduleGrabber!
        /// </summary>
        /// <param name="args">CLI arguments</param>
        static void Main(string[] args)
        {
            try
            {
                Utility.StandardConsole();
                SchedulePage = GetSchedulePage();
                RequestData = GetRequestData();
                List<Department> departments = GetDepartments();
 
                timer.Start();

                timer.Stop();
            }
            catch (Exception ex)
            {
                Utility.ExceptionConsole(() =>
                {
                    Console.WriteLine("");
                    Console.WriteLine("EXCEPTION OCCURRED: \n " + ex.Message + 
                        " \n at: " + ex.LineNumber());
                });
            }

            Console.WriteLine("");
            Console.WriteLine("ScheduleGrabber finished grabbing in " + 
                string.Format("{0:0.0}", timer.Elapsed.TotalSeconds));
            Console.ReadLine();
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
