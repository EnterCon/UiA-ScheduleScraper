using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Globalization;

namespace ScheduleGrabber
{
    public class Department
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public List<Week> Schedule { get; set; }

        public Department(string id)
        {
            this.Id = id;
        }


        public void GrabSchedule(PostData requestData)
        {
            var response = Grabber.Client.PostAsync(Grabber.URL, requestData.UrlEncode(this)).Result;
            string html = response.Content.ReadAsStringAsync().Result;
            HtmlDocument courseDoc = html.ToHtml();
            var weeks = courseDoc.DocumentNode.Descendants("table");

            string title = courseDoc.DocumentNode.Descendants()
                .Where(n => n.GetAttributeValue("class", null) == "title").First().InnerText;
            this.Name = title.Sanitize();

            foreach (var week in weeks)
            {
                string weekString = week.SelectSingleNode("tr[@class='tr1']/td[@class='td1']")
                    .InnerText.Sanitize();

                var days = week.SelectNodes("tr[@class='tr2']");
                if (days != null && days.Count > 0)
                {
                    Week theWeek = new Week();
                    Day theDay = new Day();
                    theWeek.Number = Utility.GetWeekNumber(weekString);
                    foreach (var dayActivity in days)
                    {
                        Activity activity = new Activity();
                        activity.Title = dayActivity.ChildNodes[3].InnerText.Sanitize();
                        activity.Room = dayActivity.ChildNodes[4].InnerText.Sanitize();
                        activity.Lecturer = dayActivity.ChildNodes[5].InnerText.Sanitize();
                        activity.Notice = dayActivity.ChildNodes[6].InnerText.Sanitize();
                        string dateStr = dayActivity.ChildNodes[1].InnerText.Sanitize();
                        string timeStr = dayActivity.ChildNodes[2].InnerText.Sanitize();
                        Tuple<DateTime, DateTime> startAndEnd = Utility.ParseScheduleTimeColumns(dateStr, timeStr);
                        activity.Start = startAndEnd.Item1;
                        activity.End = startAndEnd.Item2;
                        theWeek.Days.Add(theDay);
                    }

                    theWeek.Days.Add(theDay);
                    this.Schedule.Add(theWeek);
                }
            }
        }
    }
}
