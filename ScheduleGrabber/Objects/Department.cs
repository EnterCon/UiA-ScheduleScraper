using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using ScheduleGrabber.Utilities;
using System.Threading.Tasks;
using System.Net.Http;
using System.Diagnostics;

namespace ScheduleGrabber.Objects
{
    public class Department
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public List<Week> Schedule { get; set; }
        public Stopwatch Timer = new Stopwatch();
        public Exception Exception { get; set; }

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
        public async Task<Department> GrabSchedule()
        {
            Timer.Start();
            try
            {
                var urlEncoded = Grabber.RequestData.UrlEncode(this);
                HttpResponseMessage response = await Grabber.Client.PostAsync(Grabber.URL, urlEncoded);
                string htmlStr = await response.Content.ReadAsStringAsync();

                HtmlDocument scheduleHtml = Utility.ToHtml(htmlStr);
                if (scheduleHtml == null || scheduleHtml.DocumentNode == null)
                    throw new ArgumentException("GrabSchedule + " + this.Id +
                    ": something went wrong during the POST-request!");
                var weeks = scheduleHtml.DocumentNode.Descendants("table");

                var title = scheduleHtml.DocumentNode.Descendants()
                    .Where(n => n.GetAttributeValue("class", null) == "title").FirstOrDefault();
                this.Name = Utility.Sanitize(title.InnerText);

                if (weeks.Count() == 0)
                    return this;

                foreach (var week in weeks)
                {
                    var days = week.SelectNodes("tr[@class='tr2']");
                    if (days != null && days.Count > 0)
                    {
                        Week theWeek = new Week();
                        List<Day> dayList = new List<Day>();
                        string weekString = Utility.Sanitize(week.SelectSingleNode("tr[@class='tr1']/td[@class='td1']")
                            .InnerText);
                        theWeek.WeekNumber = Week.GetWeekNumber(weekString);
                        theWeek.Year = Week.GetYear(weekString);
                        foreach (var dayActivity in days)
                        {
                            Activity activity = new Activity();
                            activity.ParseCourses(Utility.Sanitize(dayActivity.ChildNodes[3].InnerText));
                            activity.ParseRooms(Utility.Sanitize(dayActivity.ChildNodes[4].InnerText));
                            activity.Lecturer = Utility.Sanitize(dayActivity.ChildNodes[5].InnerText);
                            activity.Notice = Utility.Sanitize(dayActivity.ChildNodes[6].InnerText);
                            string dateStr = Utility.Sanitize(dayActivity.ChildNodes[1].InnerText);
                            string timeStr = Utility.Sanitize(dayActivity.ChildNodes[2].InnerText);
                            activity.ParseTimespan(theWeek.Year, dateStr, timeStr);
                            var currentDate = dayList.Where(d => d.Date.Date.Equals(activity.Start.Date));
                            if (currentDate.Count() == 0)
                            {
                                Day aday = new Day();
                                aday.Activities.Add(activity);
                                aday.Date = new DateTime(activity.Start.Year, activity.Start.Month, activity.Start.Day);
                                aday.DayOfWeek = aday.Date.DayOfWeek.ToString();
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
            catch (Exception e)
            {
                this.Exception = e;
                Grabber.RuntimeExceptions.Add(e);
            }
            return this;
        }


    }
}
