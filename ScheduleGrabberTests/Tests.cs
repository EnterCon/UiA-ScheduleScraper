using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScheduleGrabber.Tests
{
    [TestFixture]
    public class Tests
    {
        [TestCase]
        public void Test_Get_Week_Number()
        {
            string testcase1 = "Uke 11, 2015";
            Assert.That(Week.GetWeekNumber(testcase1), Is.EqualTo(11));

            string testcase2 = "Uke 31, 2015 - Ingen undervisning denne uken";
            Assert.That(Week.GetWeekNumber(testcase2), Is.EqualTo(31));

            string testcase3 = "Uke 66, 2015 - Ingen undervisning denne uken";
            Assert.That(Week.GetWeekNumber(testcase3), Throws.ArgumentException);
        }
    }
}
