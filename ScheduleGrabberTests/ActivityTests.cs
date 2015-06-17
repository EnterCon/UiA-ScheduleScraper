using System;
using NUnit.Framework;
using ScheduleGrabber;

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
            Tuple<DateTime, DateTime> expected = new Tuple<DateTime, DateTime>(start, end);
            Assert.That(Activity.ParseTimespan(year, date, time), Is.EqualTo(expected));
        }
    }
}
