using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.IO;
using System.Resources;
using System.Web;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace DDay.iCal.Test
{
    [TestFixture]
    public class AlarmTest
    {
        private string tzid;

        [TestFixtureSetUp]
        public void InitAll()
        {
            tzid = "US-Eastern";
        }

        public void TestAlarm(string calendar, List<IDateTime> dates, iCalDateTime start, iCalDateTime end)
        {
            IICalendar iCal = iCalendar.LoadFromFile(@"Calendars\Alarm\" + calendar)[0];
            ProgramTest.TestCal(iCal);
            IEvent evt = iCal.Events[0];

            start.TZID = tzid;
            end.TZID = tzid;

            for (int i = 0; i < dates.Count; i++)
                dates[i].TZID = tzid;

            // Poll all alarms that occurred between Start and End
            IList<AlarmOccurrence> alarms = evt.PollAlarms(start, end);

            foreach (AlarmOccurrence alarm in alarms)
                Assert.IsTrue(dates.Contains(alarm.DateTime), "Alarm triggers at " + alarm.Period.StartTime + ", but it should not.");
            Assert.IsTrue(dates.Count == alarms.Count, "There were " + alarms.Count + " alarm occurrences; there should have been " + dates.Count + ".");
        }

        [Test, Category("Alarm")]
        public void ALARM1()
        {
            List<IDateTime> DateTimes = new List<IDateTime>();
            DateTimes.AddRange(new iCalDateTime[]
            {
                new iCalDateTime(2006, 7, 18, 9, 30, 0)
            });

            TestAlarm("ALARM1.ics", DateTimes, new iCalDateTime(2006, 7, 1), new iCalDateTime(2006, 9, 1));
        }

        [Test, Category("Alarm")]
        public void ALARM2()
        {
            List<IDateTime> DateTimes = new List<IDateTime>();
            DateTimes.AddRange(new iCalDateTime[]
            {
                new iCalDateTime(2006, 7, 18, 9, 30, 0),
                new iCalDateTime(2006, 7, 20, 9, 30, 0),
                new iCalDateTime(2006, 7, 22, 9, 30, 0),
                new iCalDateTime(2006, 7, 24, 9, 30, 0),
                new iCalDateTime(2006, 7, 26, 9, 30, 0),
                new iCalDateTime(2006, 7, 28, 9, 30, 0),
                new iCalDateTime(2006, 7, 30, 9, 30, 0),
                new iCalDateTime(2006, 8, 1, 9, 30, 0),
                new iCalDateTime(2006, 8, 3, 9, 30, 0),
                new iCalDateTime(2006, 8, 5, 9, 30, 0)
            });

            TestAlarm("ALARM2.ics", DateTimes, new iCalDateTime(2006, 7, 1), new iCalDateTime(2006, 9, 1));
        }

        [Test, Category("Alarm")]
        public void ALARM3()
        {
            List<IDateTime> DateTimes = new List<IDateTime>();
            DateTimes.AddRange(new iCalDateTime[]
            {
                new iCalDateTime(1998, 2, 11, 9, 0, 0),
                new iCalDateTime(1998, 3, 11, 9, 0, 0),
                new iCalDateTime(1998, 11, 11, 9, 0, 0),
                new iCalDateTime(1999, 8, 11, 9, 0, 0),
                new iCalDateTime(2000, 10, 11, 9, 0, 0)
            });

            TestAlarm("ALARM3.ics", DateTimes, new iCalDateTime(1997, 1, 1), new iCalDateTime(2000, 12, 31));
        }

        [Test, Category("Alarm")]
        public void ALARM4()
        {
            List<IDateTime> DateTimes = new List<IDateTime>();
            DateTimes.AddRange(new iCalDateTime[]
            {
                new iCalDateTime(1998, 2, 11, 9, 0, 0),
                new iCalDateTime(1998, 2, 11, 11, 0, 0),
                new iCalDateTime(1998, 2, 11, 13, 0, 0),
                new iCalDateTime(1998, 2, 11, 15, 0, 0),
                new iCalDateTime(1998, 3, 11, 9, 0, 0),
                new iCalDateTime(1998, 3, 11, 11, 0, 0),
                new iCalDateTime(1998, 3, 11, 13, 0, 0),
                new iCalDateTime(1998, 3, 11, 15, 0, 0),
                new iCalDateTime(1998, 11, 11, 9, 0, 0),
                new iCalDateTime(1998, 11, 11, 11, 0, 0),
                new iCalDateTime(1998, 11, 11, 13, 0, 0),
                new iCalDateTime(1998, 11, 11, 15, 0, 0),
                new iCalDateTime(1999, 8, 11, 9, 0, 0),
                new iCalDateTime(1999, 8, 11, 11, 0, 0),
                new iCalDateTime(1999, 8, 11, 13, 0, 0),
                new iCalDateTime(1999, 8, 11, 15, 0, 0),
                new iCalDateTime(2000, 10, 11, 9, 0, 0),
                new iCalDateTime(2000, 10, 11, 11, 0, 0),
                new iCalDateTime(2000, 10, 11, 13, 0, 0),
                new iCalDateTime(2000, 10, 11, 15, 0, 0)
            });

            TestAlarm("ALARM4.ics", DateTimes, new iCalDateTime(1997, 1, 1), new iCalDateTime(2000, 12, 31));
        }

        [Test, Category("Alarm")]
        public void ALARM5()
        {
            List<IDateTime> DateTimes = new List<IDateTime>();
            DateTimes.AddRange(new iCalDateTime[]
            {
                new iCalDateTime(1998, 1, 2, 8, 0, 0)
            });

            TestAlarm("ALARM5.ics", DateTimes, new iCalDateTime(1997, 7, 1), new iCalDateTime(2000, 12, 31));
        }

        [Test, Category("Alarm")]
        public void ALARM6()
        {
            List<IDateTime> DateTimes = new List<IDateTime>();
            DateTimes.AddRange(new iCalDateTime[]
            {
                new iCalDateTime(1998, 1, 2, 8, 0, 0),
                new iCalDateTime(1998, 1, 5, 8, 0, 0),
                new iCalDateTime(1998, 1, 8, 8, 0, 0),
                new iCalDateTime(1998, 1, 11, 8, 0, 0),
                new iCalDateTime(1998, 1, 14, 8, 0, 0),
                new iCalDateTime(1998, 1, 17, 8, 0, 0)
            });

            TestAlarm("ALARM6.ics", DateTimes, new iCalDateTime(1997, 7, 1), new iCalDateTime(2000, 12, 31));
        }

        [Test, Category("Alarm")]
        public void ALARM7()
        {
            List<IDateTime> DateTimes = new List<IDateTime>();
            DateTimes.AddRange(new iCalDateTime[]
            {
                new iCalDateTime(2006, 7, 18, 10, 30, 0),
                new iCalDateTime(2006, 7, 20, 10, 30, 0),
                new iCalDateTime(2006, 7, 22, 10, 30, 0),
                new iCalDateTime(2006, 7, 24, 10, 30, 0),
                new iCalDateTime(2006, 7, 26, 10, 30, 0),
                new iCalDateTime(2006, 7, 28, 10, 30, 0),
                new iCalDateTime(2006, 7, 30, 10, 30, 0),
                new iCalDateTime(2006, 8, 1, 10, 30, 0),
                new iCalDateTime(2006, 8, 3, 10, 30, 0),
                new iCalDateTime(2006, 8, 5, 10, 30, 0)
            });

            TestAlarm("ALARM7.ics", DateTimes, new iCalDateTime(2006, 7, 1), new iCalDateTime(2006, 9, 1));
        }
    }
}
