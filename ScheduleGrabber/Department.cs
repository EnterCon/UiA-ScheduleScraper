using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

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

            string title = courseDoc.DocumentNode.Descendants().Where(n => n.GetAttributeValue("class", null) == "title").First().InnerText;
            this.Name = title.Sanitize();

            foreach (var week in weeks)
            {
                Week theWeek = new Week();

                string weekString = week.SelectSingleNode("tr[@class='tr1']/td[@class='td1']").InnerText.Sanitize();
                int weekNumber = Utility.GetWeekNumber(weekString);

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

                this.Schedule.Add(theWeek);
            }
            return document;
        }
    }
}
