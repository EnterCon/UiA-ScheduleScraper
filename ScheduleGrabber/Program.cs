using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
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
    public static class Program
    {
        static IMongoClient _client;
        static IMongoDatabase _database;
        static HttpClient client = new HttpClient();
        static Stopwatch timer = new Stopwatch();

        /// <summary>
        /// Run the program
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            try
            {
                Console.BackgroundColor = ConsoleColor.Green;
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("ScheduleGrabber starting...");
                string url = "http://timeplan.uia.no/swsuiav/public/no/default.aspx";
                _client = new MongoClient();
                _database = _client.GetDatabase("uia-schedule");
                Console.WriteLine("Connected to database");

                HtmlDocument doc = new HtmlDocument();
                throw new Exception("Hello");
                var resp = client.GetAsync(url).Result;
                var respStr = resp.Content.ReadAsStringAsync().Result;
                doc = respStr.ToHtml();
                string __VIEWSTATE = doc.DocumentNode.Descendants().Where(d => d.Id == "__VIEWSTATE").First().GetAttributeValue("value", null);
                string __VIEWSTATEGENERATOR = doc.DocumentNode.Descendants().Where(d => d.Id == "__VIEWSTATEGENERATOR").First().GetAttributeValue("value", null);
                string __EVENTVALIDATION = doc.DocumentNode.Descendants().Where(d => d.Id == "__EVENTVALIDATION").First().GetAttributeValue("value", null);
                string tLinkType = "studentsets";
                string tWildCard = "";
                string lbWeeks = doc.DocumentNode.Descendants().Where(d => d.Id == "lbWeeks").First().Descendants("option").ElementAt(1).GetAttributeValue("value", null);
                string lbDays = "1-6";
                string RadioType = "XMLSpreadsheet;studentsetxmlurl;SWSCUST StudentSet XMLSpreadsheet";
                string bGetTimetable = "Vis+timeplan";
                Console.WriteLine("Got form data");

                HtmlNode selectBox = doc.DocumentNode.Descendants().Where(d => d.Id == "dlObject").First();
                List<string> departments = new List<string>();
                foreach (var option in selectBox.Descendants("option"))
                    departments.Add(option.GetAttributeValue("value", null));
                Console.WriteLine("Got all ID's");
                Console.WriteLine("");

                int amount = 0;
                timer.Start();
                Console.Write("\rInserted data for " + amount + " of " + departments.Count + " departments in " +
                    string.Format("{0:0.0}", timer.Elapsed.TotalSeconds) + " seconds (" +
                    string.Format("{0:0.00}", amount / timer.Elapsed.TotalSeconds) + " departments/second)");
                Parallel.ForEach(departments, department =>
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

                    var collection = _database.GetCollection<BsonDocument>("courses");
                    collection.InsertOneAsync(document);
                    amount++;
                    Console.Write("\rInserted data for " + amount + " of " + departments.Count + " departments in " +
                        string.Format("{0:0.0}", timer.Elapsed.TotalSeconds) + " seconds (" +
                        string.Format("{0:0.00}", amount / timer.Elapsed.TotalSeconds) + " departments/second)");
                });
                timer.Stop();
            }
            catch (Exception ex)
            {
                Console.BackgroundColor = ConsoleColor.Red;
                Console.WriteLine("");
                Console.WriteLine("EXCEPTION OCCURRED: \n " + ex.Message + " \n at: " + ex.LineNumber());
                Console.BackgroundColor = ConsoleColor.Green;
            }
            Console.WriteLine("");
            Console.WriteLine("ScheduleGrabber finished grabbing in " + string.Format("{0:0.0}", timer.Elapsed.TotalSeconds));
            Console.ReadLine();
        }

        /// <summary>
        /// Takes request data and a department ID to request it's schedule.
        /// </summary>
        /// <param name="requestData">the data for the POST request</param>
        /// <param name="department">the ID of the department</param>
        /// <returns>a BsonDocument to be stored somewhere</returns>
        public static BsonDocument GetScheduleData(Dictionary<string, string> requestData, string department)
        {
            var formdata = new FormUrlEncodedContent(requestData);
            var response = client.PostAsync("http://timeplan.uia.no/swsuiav/public/no/default.aspx", formdata).Result;
            string html = response.Content.ReadAsStringAsync().Result;
            HtmlDocument courseDoc = html.ToHtml();
            var weeks = courseDoc.DocumentNode.Descendants("table");
            string title = courseDoc.DocumentNode.Descendants().Where(n => n.GetAttributeValue("class", null) == "title").First().InnerText;
            var document = new BsonDocument
            {
                { "id", department },
                { "name", title.Sanitize() },
                { "schedule", new BsonDocument
                    {
                    }
                }
            };

            foreach (var week in weeks)
            {
                string theWeek = week.SelectSingleNode("tr[@class='tr1']/td[@class='td1']").InnerText.Sanitize();
                document["schedule"].AsBsonDocument.Add(theWeek, new BsonArray());
                var events = week.SelectNodes("tr[@class='tr2']");
                if (events != null && events.Count > 0)
                {
                    foreach (var ev in events)
                    {
                        document["schedule"].AsBsonDocument[theWeek].AsBsonArray.Add(new BsonDocument
                                {
                                    {"day", ev.ChildNodes[0].InnerText.Sanitize() },
                                    {"date", ev.ChildNodes[1].InnerText.Sanitize() },
                                    {"time", ev.ChildNodes[2].InnerText.Sanitize() },
                                    {"activity", ev.ChildNodes[3].InnerText.Sanitize() },
                                    {"room", ev.ChildNodes[4].InnerText.Sanitize() },
                                    {"lecturer", ev.ChildNodes[5].InnerText.Sanitize() },
                                    {"notice", ev.ChildNodes[6].InnerText.Sanitize() }
                                });
                    }
                }
            }
            return document;
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
