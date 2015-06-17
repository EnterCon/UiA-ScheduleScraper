using System;
using NUnit.Framework;
using ScheduleGrabber.Objects;
using System.Collections.Generic;

namespace ScheduleGrabberTests
{
    [TestFixture]
    public class ActivityTests
    {
        [TestCase]
        public void Test_ParseTimespan()
        {
            int year = 2015; string date = "17 Aug"; string time = "09.15-11.00";
            DateTime start = new DateTime(2015, 08, 17, 09, 15, 00);
            DateTime end = new DateTime(2015, 08, 17, 11, 00, 00);
            Activity activity = new Activity();
            activity.ParseTimespan(year, date, time);
            Assert.That(activity.Start, Is.EqualTo(start));
            Assert.That(activity.End, Is.EqualTo(end));
        }

        [TestCase]
        public void Test_ParseCourses()
        {
            string coursesString = "KJ-111 BIO111 ML-112/BIO104 MU-139 Musikkdidaktikk, tirs -ERN100 for";
            List<string> courses = new List<string> {"KJ-111", "BIO111", "ML-112", "BIO104", "MU-139", "ERN100" };
            Activity activity = new Activity();
            activity.ParseCourses(coursesString);
            Assert.That(activity.Courses, Is.EquivalentTo(courses));
        }
    }
}
