using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.IO;
using System.Resources;
using System.Web;
using System.Web.UI;
using DDay.iCal.Components;
using DDay.iCal.DataTypes;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace DDay.iCal.Test
{
    [TestFixture]
    public class Alarm
    {
        private TZID tzid;

        [TestFixtureSetUp]
        public void InitAll()
        {
            tzid = new TZID("US-Eastern");
        }

        static public void DoTests()
        {
            Alarm a = new Alarm();
            a.InitAll();
            a.ALARM1();
            a.ALARM2();
            a.ALARM3();
            a.ALARM4();
            a.ALARM5();
            a.ALARM6();
            a.ALARM7();
        }

        public void TestAlarm(string Calendar, List<Date_Time> Dates, Date_Time Start, Date_Time End)
        {
            iCalendar iCal = iCalendar.LoadFromFile(@"Calendars\Alarm\" + Calendar);
            Program.TestCal(iCal);
            Event evt = iCal.Events[0];

            Start.iCalendar = iCal;
            Start.TZID = tzid;
            End.iCalendar = iCal;
            End.TZID = tzid;

            for (int i = 0; i < Dates.Count; i++)
            {
                Dates[i].TZID = tzid;
                Dates[i].iCalendar = iCal;
            }

            // Poll all alarms that occurred between Start and End
            List<AlarmOccurrence> alarms = evt.PollAlarms(Start, End);

            foreach (AlarmOccurrence alarm in alarms)
                Assert.IsTrue(Dates.Contains(alarm.DateTime), "Alarm triggers at " + alarm.Period.StartTime + ", but it should not.");
            Assert.IsTrue(Dates.Count == alarms.Count, "There were " + alarms.Count + " alarm occurrences; there should have been " + Dates.Count + ".");
        }

        [Test, Category("Alarm")]
        public void ALARM1()
        {
            List<Date_Time> DateTimes = new List<Date_Time>();
            DateTimes.AddRange(new Date_Time[]
            {
                new Date_Time(2006, 7, 18, 9, 30, 0)
            });

            TestAlarm("ALARM1.ics", DateTimes, new Date_Time(2006, 7, 1), new Date_Time(2006, 9, 1));
        }

        [Test, Category("Alarm")]
        public void ALARM2()
        {
            List<Date_Time> DateTimes = new List<Date_Time>();
            DateTimes.AddRange(new Date_Time[]
            {
                new Date_Time(2006, 7, 18, 9, 30, 0),
                new Date_Time(2006, 7, 20, 9, 30, 0),
                new Date_Time(2006, 7, 22, 9, 30, 0),
                new Date_Time(2006, 7, 24, 9, 30, 0),
                new Date_Time(2006, 7, 26, 9, 30, 0),
                new Date_Time(2006, 7, 28, 9, 30, 0),
                new Date_Time(2006, 7, 30, 9, 30, 0),
                new Date_Time(2006, 8, 1, 9, 30, 0),
                new Date_Time(2006, 8, 3, 9, 30, 0),
                new Date_Time(2006, 8, 5, 9, 30, 0)
            });

            TestAlarm("ALARM2.ics", DateTimes, new Date_Time(2006, 7, 1), new Date_Time(2006, 9, 1));
        }

        [Test, Category("Alarm")]
        public void ALARM3()
        {
            List<Date_Time> DateTimes = new List<Date_Time>();
            DateTimes.AddRange(new Date_Time[]
            {
                new Date_Time(1998, 2, 11, 9, 0, 0),
                new Date_Time(1998, 3, 11, 9, 0, 0),
                new Date_Time(1998, 11, 11, 9, 0, 0),
                new Date_Time(1999, 8, 11, 9, 0, 0),
                new Date_Time(2000, 10, 11, 9, 0, 0)
            });

            TestAlarm("ALARM3.ics", DateTimes, new Date_Time(1997, 7, 1), new Date_Time(2000, 12, 31));
        }

        [Test, Category("Alarm")]
        public void ALARM4()
        {
            List<Date_Time> DateTimes = new List<Date_Time>();
            DateTimes.AddRange(new Date_Time[]
            {
                new Date_Time(1998, 2, 11, 9, 0, 0),
                new Date_Time(1998, 2, 11, 11, 0, 0),
                new Date_Time(1998, 2, 11, 13, 0, 0),
                new Date_Time(1998, 2, 11, 15, 0, 0),
                new Date_Time(1998, 3, 11, 9, 0, 0),
                new Date_Time(1998, 3, 11, 11, 0, 0),
                new Date_Time(1998, 3, 11, 13, 0, 0),
                new Date_Time(1998, 3, 11, 15, 0, 0),
                new Date_Time(1998, 11, 11, 9, 0, 0),
                new Date_Time(1998, 11, 11, 11, 0, 0),
                new Date_Time(1998, 11, 11, 13, 0, 0),
                new Date_Time(1998, 11, 11, 15, 0, 0),
                new Date_Time(1999, 8, 11, 9, 0, 0),
                new Date_Time(1999, 8, 11, 11, 0, 0),
                new Date_Time(1999, 8, 11, 13, 0, 0),
                new Date_Time(1999, 8, 11, 15, 0, 0),
                new Date_Time(2000, 10, 11, 9, 0, 0),
                new Date_Time(2000, 10, 11, 11, 0, 0),
                new Date_Time(2000, 10, 11, 13, 0, 0),
                new Date_Time(2000, 10, 11, 15, 0, 0)
            });

            TestAlarm("ALARM4.ics", DateTimes, new Date_Time(1997, 7, 1), new Date_Time(2000, 12, 31));
        }

        [Test, Category("Alarm")]
        public void ALARM5()
        {
            List<Date_Time> DateTimes = new List<Date_Time>();
            DateTimes.AddRange(new Date_Time[]
            {
                new Date_Time(1998, 1, 2, 8, 0, 0)
            });

            TestAlarm("ALARM5.ics", DateTimes, new Date_Time(1997, 7, 1), new Date_Time(2000, 12, 31));
        }

        [Test, Category("Alarm")]
        public void ALARM6()
        {
            List<Date_Time> DateTimes = new List<Date_Time>();
            DateTimes.AddRange(new Date_Time[]
            {
                new Date_Time(1998, 1, 2, 8, 0, 0),
                new Date_Time(1998, 1, 5, 8, 0, 0),
                new Date_Time(1998, 1, 8, 8, 0, 0),
                new Date_Time(1998, 1, 11, 8, 0, 0),
                new Date_Time(1998, 1, 14, 8, 0, 0),
                new Date_Time(1998, 1, 17, 8, 0, 0)
            });

            TestAlarm("ALARM6.ics", DateTimes, new Date_Time(1997, 7, 1), new Date_Time(2000, 12, 31));
        }

        [Test, Category("Alarm")]
        public void ALARM7()
        {
            List<Date_Time> DateTimes = new List<Date_Time>();
            DateTimes.AddRange(new Date_Time[]
            {
                new Date_Time(2006, 7, 18, 10, 30, 0),
                new Date_Time(2006, 7, 20, 10, 30, 0),
                new Date_Time(2006, 7, 22, 10, 30, 0),
                new Date_Time(2006, 7, 24, 10, 30, 0),
                new Date_Time(2006, 7, 26, 10, 30, 0),
                new Date_Time(2006, 7, 28, 10, 30, 0),
                new Date_Time(2006, 7, 30, 10, 30, 0),
                new Date_Time(2006, 8, 1, 10, 30, 0),
                new Date_Time(2006, 8, 3, 10, 30, 0),
                new Date_Time(2006, 8, 5, 10, 30, 0)
            });

            TestAlarm("ALARM7.ics", DateTimes, new Date_Time(2006, 7, 1), new Date_Time(2006, 9, 1));
        }
    }
}
