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

        public Department()
        {
            this.Schedule = new List<Week>();
        }

        public Department(string id)
        {
            this.Schedule = new List<Week>();
            this.Id = id;
        }

        /// <summary>
        /// This method grabs the schedule for the department
        /// of this object, and stores the schedule
        /// in it's schedule property: a list of weeks
        /// which contain days with activities.
        /// </summary>
        /// <param name="requestData">
        ///     the request data from the HttpClient
        ///     required for posting to the website.
        /// </param>
        public void GrabSchedule(PostData requestData)
        {
            var urlEncoded = requestData.UrlEncode(this);
            var response = Grabber.Client.PostAsync(Grabber.URL, urlEncoded).Result;
            string htmlStr = response.Content.ReadAsStringAsync().Result;
            HtmlDocument scheduleHtml = htmlStr.ToHtml();
            if (scheduleHtml == null || scheduleHtml.DocumentNode == null)
                throw new ArgumentException("GrabSchedule + " + this.Id +
                ": something went wrong during the POST-request!");
            var weeks = scheduleHtml.DocumentNode.Descendants("table");

            var title = scheduleHtml.DocumentNode.Descendants()
                .Where(n => n.GetAttributeValue("class", null) == "title").FirstOrDefault();
            this.Name = title.InnerText.Sanitize();

            if (weeks.Count() == 0)
                return;

            foreach (var week in weeks)
            {
                var days = week.SelectNodes("tr[@class='tr2']");
                if (days != null && days.Count > 0)
                {
                    Week theWeek = new Week();
                    List<Day> dayList = new List<Day>();
                    string weekString = week.SelectSingleNode("tr[@class='tr1']/td[@class='td1']")
                        .InnerText.Sanitize();
                    theWeek.Number = Week.GetWeekNumber(weekString);
                    theWeek.Year = Week.GetYear(weekString);
                    foreach (var dayActivity in days)
                    {
                        Activity activity = new Activity();
                        activity.Title = dayActivity.ChildNodes[3].InnerText.Sanitize();
                        activity.Room = dayActivity.ChildNodes[4].InnerText.Sanitize();
                        activity.Lecturer = dayActivity.ChildNodes[5].InnerText.Sanitize();
                        activity.Notice = dayActivity.ChildNodes[6].InnerText.Sanitize();
                        string dateStr = dayActivity.ChildNodes[1].InnerText.Sanitize();
                        string timeStr = dayActivity.ChildNodes[2].InnerText.Sanitize();
                        Tuple<DateTime, DateTime> startAndEnd = Activity.ParseTimespan(theWeek.Year, dateStr, timeStr);
                        activity.Start = startAndEnd.Item1;
                        activity.End = startAndEnd.Item2;
                        var currentDate = dayList.Where(d => d.Date.Date.Equals(activity.Start.Date));
                        if (currentDate.Count() == 0)
                        {
                            Day aday = new Day();
                            aday.Activities.Add(activity);
                            aday.Date = new DateTime(activity.Start.Year, activity.Start.Month, activity.Start.Day);
                            dayList.Add(aday);
                        }
                        else if (currentDate.Count() == 1)
                        {
                            currentDate.First().Activities.Add(activity);
                        }
                    }

                    theWeek.Days.AddRange(dayList.ToArray());
                    this.Schedule.Add(theWeek);
                }
            }
        }


    }
}
