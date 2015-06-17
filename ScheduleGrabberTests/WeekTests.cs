using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScheduleGrabber.Tests
{
    [TestFixture]
    public class WeekTests
    {
        [TestCase]
        public void Test_GetWeekNumber()
        {
            string testcase = "Uke 11, 2015";
            Assert.That(Week.GetWeekNumber(testcase), Is.EqualTo(11));

            testcase = "Uke 31, 2015 - Ingen undervisning denne uken";
            Assert.That(Week.GetWeekNumber(testcase), Is.EqualTo(31));

            testcase = "Uke 66, 2015 - Ingen undervisning denne uken";
            Assert.Throws<ArgumentException>(delegate { Week.GetWeekNumber(testcase); });
        }

        [TestCase]
        public void Test_GetYear()
        {
            string testcase = "Uke 11, 2015";
            Assert.That(Week.GetYear(testcase), Is.EqualTo(2015));

            testcase = "Uke 31, 2014 - Ingen undervisning denne uken";
            Assert.That(Week.GetYear(testcase), Is.EqualTo(2014));

            testcase = "Uke 66, 1999 - Ingen undervisning denne uken";
            Assert.Throws<ArgumentException>(delegate { Week.GetYear(testcase); });
        }
    }
}
