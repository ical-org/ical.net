using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.IO;
using System.Resources;
using System.Web;
using System.Web.UI;
using System.Reflection;
using System.Text.RegularExpressions;

using DDay.iCal.Components;
using DDay.iCal.DataTypes;
using DDay.iCal.Serialization;
using NUnit.Framework;
using System.Globalization;

namespace DDay.iCal.Test
{
    [TestFixture]
    public class RecurrenceTest
    {
        private TZID tzid;

        [TestFixtureSetUp]
        public void InitAll()
        {
            tzid = new TZID("US-Eastern");
        }

        /// <summary>
        /// See Page 45 of RFC 2445 - RRULE:FREQ=YEARLY;INTERVAL=2;BYMONTH=1;BYDAY=SU;BYHOUR=8,9;BYMINUTE=30
        /// </summary>
        [Test, Category("Recurrence")]
        public void RRULE1()
        {
            iCalendar iCal = iCalendar.LoadFromFile(@"Calendars\Recurrence\RRULE1.ics");
            ProgramTest.TestCal(iCal);
            Event evt = iCal.Events[0];
            List<Occurrence> occurrences = evt.GetOccurrences(
                new iCalDateTime(2006, 1, 1, tzid, iCal),
                new iCalDateTime(2011, 1, 1, tzid, iCal));

            iCalDateTime dt = new iCalDateTime(2006, 1, 1, 8, 30, 0, tzid, iCal);
            int i = 0;

            while (dt.Year < 2011)
            {
                if ((dt > evt.Start) &&
                    (dt.Year % 2 == 1) && // Every-other year from 2005
                    (dt.Month == 1) &&
                    (dt.DayOfWeek == DayOfWeek.Sunday))
                {
                    iCalDateTime dt1 = dt.AddHours(1);
                    Assert.AreEqual(dt, occurrences[i].Period.StartTime, "Event should occur at " + dt);
                    Assert.AreEqual(dt1, occurrences[i + 1].Period.StartTime, "Event should occur at " + dt);
                    i += 2;
                }

                dt = dt.AddDays(1);
            }
        }

        /// <summary>
        /// See Page 118 of RFC 2445 - RRULE:FREQ=DAILY;COUNT=10;INTERVAL=2
        /// </summary>
        [Test, Category("Recurrence")]
        public void RRULE2()
        {
            iCalendar iCal = iCalendar.LoadFromFile(@"Calendars\Recurrence\RRULE2.ics");
            ProgramTest.TestCal(iCal);
            Event evt = iCal.Events[0];

            List<Occurrence> occurrences = evt.GetOccurrences(
                new iCalDateTime(2006, 7, 1, tzid, iCal),
                new iCalDateTime(2006, 9, 1, tzid, iCal));

            iCalDateTime[] DateTimes = new iCalDateTime[]
            {
                new iCalDateTime(2006, 07, 18, 10, 00, 00, tzid, iCal),
                new iCalDateTime(2006, 07, 20, 10, 00, 00, tzid, iCal),
                new iCalDateTime(2006, 07, 22, 10, 00, 00, tzid, iCal),
                new iCalDateTime(2006, 07, 24, 10, 00, 00, tzid, iCal),
                new iCalDateTime(2006, 07, 26, 10, 00, 00, tzid, iCal),
                new iCalDateTime(2006, 07, 28, 10, 00, 00, tzid, iCal),
                new iCalDateTime(2006, 07, 30, 10, 00, 00, tzid, iCal),
                new iCalDateTime(2006, 08, 01, 10, 00, 00, tzid, iCal),
                new iCalDateTime(2006, 08, 03, 10, 00, 00, tzid, iCal),
                new iCalDateTime(2006, 08, 05, 10, 00, 00, tzid, iCal)
            };

            for (int i = 0; i < DateTimes.Length; i++)
                Assert.AreEqual(DateTimes[i], occurrences[i].Period.StartTime, "Event should occur on " + DateTimes[i]);

            Assert.AreEqual(
                DateTimes.Length,
                occurrences.Count,
                "There should be exactly " + DateTimes.Length + " occurrences; there were " + occurrences.Count);
        }

        /// <summary>
        /// See Page 118 of RFC 2445 - RRULE:FREQ=DAILY;UNTIL=19971224T000000Z
        /// </summary>
        [Test, Category("Recurrence")]
        public void RRULE3()
        {
            iCalendar iCal = iCalendar.LoadFromFile(@"Calendars\Recurrence\RRULE3.ics");
            ProgramTest.TestCal(iCal);
            Event evt = iCal.Events[0];

            List<Occurrence> occurrences = evt.GetOccurrences(
                new iCalDateTime(1997, 9, 1, tzid, iCal),
                new iCalDateTime(1998, 1, 1, tzid, iCal));

            iCalDateTime dt = new iCalDateTime(1997, 9, 2, 9, 0, 0, tzid, iCal);
            int i = 0;
            while (dt.Year < 1998)
            {
                if ((dt >= evt.Start) &&
                    (dt < new iCalDateTime(1997, 12, 24, 0, 0, 0, tzid, iCal)))
                {
                    Assert.AreEqual(dt, occurrences[i].Period.StartTime, "Event should occur at " + dt);
                    Assert.IsTrue(
                        (dt < new iCalDateTime(1997, 10, 26, tzid, iCal) && dt.TimeZoneInfo.TimeZoneName == "EDT") ||
                        (dt > new iCalDateTime(1997, 10, 26, tzid, iCal) && dt.TimeZoneInfo.TimeZoneName == "EST"),
                        "Event " + dt + " doesn't occur in the correct time zone (including Daylight & Standard time zones)");
                    i++;
                }

                dt = dt.AddDays(1);
            }
        }

        /// <summary>
        /// See Page 118 of RFC 2445 - RRULE:FREQ=DAILY;INTERVAL=2
        /// </summary>
        [Test, Category("Recurrence")]
        public void RRULE4()
        {
            iCalendar iCal = iCalendar.LoadFromFile(@"Calendars\Recurrence\RRULE4.ics");
            ProgramTest.TestCal(iCal);
            Event evt = iCal.Events[0];

            List<Occurrence> occurrences = evt.GetOccurrences(
                new iCalDateTime(1997, 9, 1, tzid, iCal),
                new iCalDateTime(1997, 12, 4, tzid, iCal));

            iCalDateTime[] DateTimes = new iCalDateTime[]
            {
                new iCalDateTime(1997, 9, 2, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 9, 4, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 9, 6, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 9, 8, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 9, 10, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 9, 12, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 9, 14, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 9, 16, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 9, 18, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 9, 20, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 9, 22, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 9, 24, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 9, 26, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 9, 28, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 9, 30, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 10, 2, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 10, 4, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 10, 6, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 10, 8, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 10, 10, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 10, 12, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 10, 14, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 10, 16, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 10, 18, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 10, 20, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 10, 22, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 10, 24, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 10, 26, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 10, 28, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 10, 30, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 11, 1, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 11, 3, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 11, 5, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 11, 7, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 11, 9, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 11, 11, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 11, 13, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 11, 15, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 11, 17, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 11, 19, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 11, 21, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 11, 23, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 11, 25, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 11, 27, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 11, 29, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 12, 1, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 12, 3, 9, 0, 0, tzid, iCal)                
            };

            string[] TimeZones = new string[]
            {
                "EDT",
                "EDT",
                "EDT",
                "EDT",
                "EDT",
                "EDT",
                "EDT",
                "EDT",
                "EDT",
                "EDT",
                "EDT",
                "EDT",
                "EDT",
                "EDT",
                "EDT",
                "EDT",
                "EDT",
                "EDT",
                "EDT",
                "EDT",
                "EDT",
                "EDT",
                "EDT",
                "EDT",
                "EDT",
                "EDT",
                "EDT",
                "EST",
                "EST",
                "EST",
                "EST",
                "EST",
                "EST",
                "EST",
                "EST",
                "EST",
                "EST",
                "EST",
                "EST",
                "EST",
                "EST",
                "EST",
                "EST",
                "EST",
                "EST",
                "EST",
                "EST"
            };

            for (int i = 0; i < DateTimes.Length; i++)
            {
                iCalDateTime dt = (iCalDateTime)DateTimes[i];
                Assert.AreEqual(dt, occurrences[i].Period.StartTime, "Event should occur on " + dt);
                Assert.IsTrue(dt.TimeZoneInfo.TimeZoneName == TimeZones[i], "Event " + dt + " should occur in the " + TimeZones[i] + " timezone");
            }

            Assert.AreEqual(
                DateTimes.Length,
                occurrences.Count,
                "There should be exactly " + DateTimes.Length + " occurrences; there were " + occurrences.Count);
        }

        /// <summary>
        /// See Page 119 of RFC 2445 - RRULE:FREQ=DAILY;INTERVAL=10;COUNT=5
        /// </summary>
        [Test, Category("Recurrence")]
        public void RRULE5()
        {
            iCalendar iCal = iCalendar.LoadFromFile(@"Calendars\Recurrence\RRULE5.ics");
            ProgramTest.TestCal(iCal);
            Event evt = iCal.Events[0];

            List<Occurrence> occurrences = evt.GetOccurrences(
                new iCalDateTime(1997, 9, 1, tzid, iCal),
                new iCalDateTime(1998, 1, 1, tzid, iCal));

            iCalDateTime[] DateTimes = new iCalDateTime[]
            {
                new iCalDateTime(1997, 9, 2, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 9, 12, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 9, 22, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 10, 2, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 10, 12, 9, 0, 0, tzid, iCal)
            };

            for (int i = 0; i < DateTimes.Length; i++)
                Assert.AreEqual(DateTimes[i], occurrences[i].Period.StartTime, "Event should occur on " + DateTimes[i]);

            Assert.AreEqual(
                DateTimes.Length,
                occurrences.Count,
                "There should be exactly " + DateTimes.Length + " occurrences; there were " + occurrences.Count);
        }

        /// <summary>
        /// See Page 119 of RFC 2445 - RRULE:FREQ=DAILY;UNTIL=20000131T090000Z;BYMONTH=1
        /// </summary>
        [Test, Category("Recurrence")]
        public void RRULE6()
        {
            iCalendar iCal = iCalendar.LoadFromFile(@"Calendars\Recurrence\RRULE6.ics");
            ProgramTest.TestCal(iCal);
            Event evt = iCal.Events[0];

            List<Occurrence> occurrences = evt.GetOccurrences(
                new iCalDateTime(1998, 1, 1, tzid, iCal),
                new iCalDateTime(2000, 12, 31, tzid, iCal));

            iCalDateTime dt = new iCalDateTime(1998, 1, 1, 9, 0, 0, tzid, iCal);
            int i = 0;
            while (dt.Year < 2001)
            {
                if (dt >= evt.Start &&
                    dt.Month == 1 &&
                    dt <= new iCalDateTime(2000, 1, 31, 9, 0, 0, tzid, iCal))
                {
                    Assert.AreEqual(dt, occurrences[i].Period.StartTime, "Event should occur at " + dt);
                    i++;
                }

                dt = dt.AddDays(1);
            }
        }

        /// <summary>
        /// See Page 119 of RFC 2445 - RRULE:FREQ=YEARLY;UNTIL=20000131T150000Z;BYMONTH=1;BYDAY=SU,MO,TU,WE,TH,FR,SA
        /// <note>
        ///     The example was slightly modified to fix a suspected flaw in the design of
        ///     the example RRULEs.  UNTIL is always UTC time, but it expected the actual
        ///     time to correspond to other time zones.  Odd.
        /// </note>        
        /// </summary>
        [Test, Category("Recurrence")]
        public void RRULE6_1()
        {
            iCalendar iCal1 = iCalendar.LoadFromFile(@"Calendars\Recurrence\RRULE6.ics");
            iCalendar iCal2 = iCalendar.LoadFromFile(@"Calendars\Recurrence\RRULE6_1.ics");
            ProgramTest.TestCal(iCal1);
            ProgramTest.TestCal(iCal2);
            Event evt1 = (Event)iCal1.Events[0];
            Event evt2 = (Event)iCal2.Events[0];

            List<Occurrence> evt1Occurrences = evt1.GetOccurrences(new iCalDateTime(1997, 9, 1), new iCalDateTime(2000, 12, 31));
            List<Occurrence> evt2Occurrences = evt2.GetOccurrences(new iCalDateTime(1997, 9, 1), new iCalDateTime(2000, 12, 31));
            Assert.IsTrue(evt1Occurrences.Count == evt2Occurrences.Count, "RRULE6_1 does not match RRULE6 as it should");
            for (int i = 0; i < evt1Occurrences.Count; i++)
                Assert.AreEqual(evt1Occurrences[i].Period, evt2Occurrences[i].Period, "PERIOD " + i + " from RRULE6 (" + evt1Occurrences[i].ToString() + ") does not match PERIOD " + i + " from RRULE6_1 (" + evt2Occurrences[i].ToString() + ")");
        }

        /// <summary>
        /// See Page 119 of RFC 2445 - RRULE:FREQ=WEEKLY;COUNT=10
        /// </summary>
        [Test, Category("Recurrence")]
        public void RRULE7()
        {
            iCalendar iCal = iCalendar.LoadFromFile(@"Calendars\Recurrence\RRULE7.ics");
            ProgramTest.TestCal(iCal);
            Event evt = iCal.Events[0];

            List<Occurrence> occurrences = evt.GetOccurrences(
                new iCalDateTime(1997, 9, 1, tzid, iCal),
                new iCalDateTime(1998, 1, 1, tzid, iCal));

            iCalDateTime[] DateTimes = new iCalDateTime[]
            {
                new iCalDateTime(1997, 9, 2, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 9, 9, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 9, 16, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 9, 23, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 9, 30, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 10, 7, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 10, 14, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 10, 21, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 10, 28, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 11, 4, 9, 0, 0, tzid, iCal)
            };

            string[] TimeZones = new string[]
            {
                "EDT",
                "EDT",
                "EDT",
                "EDT",
                "EDT",
                "EDT",
                "EDT",
                "EDT",
                "EST",
                "EST"
            };

            for (int i = 0; i < DateTimes.Length; i++)
            {
                iCalDateTime dt = (iCalDateTime)DateTimes[i];
                Assert.AreEqual(dt, occurrences[i].Period.StartTime, "Event should occur on " + dt);
                Assert.AreEqual(TimeZones[i], dt.TimeZoneInfo.TimeZoneName, "Event " + dt + " should occur in the " + TimeZones[i] + " timezone");
            }

            Assert.AreEqual(
                DateTimes.Length,
                occurrences.Count,
                "There should be exactly " + DateTimes.Length + " occurrences; there were " + occurrences.Count);
        }

        /// <summary>
        /// See Page 119 of RFC 2445 - RRULE:FREQ=WEEKLY;UNTIL=19971224T000000Z
        /// </summary>
        [Test, Category("Recurrence")]
        public void RRULE8()
        {
            iCalendar iCal = iCalendar.LoadFromFile(@"Calendars\Recurrence\RRULE8.ics");
            ProgramTest.TestCal(iCal);
            Event evt = iCal.Events[0];

            List<Occurrence> occurrences = evt.GetOccurrences(
                new iCalDateTime(1997, 9, 1, tzid, iCal),
                new iCalDateTime(1999, 1, 1, tzid, iCal));

            iCalDateTime[] DateTimes = new iCalDateTime[]
            {
                new iCalDateTime(1997, 9, 2, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 9, 9, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 9, 16, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 9, 23, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 9, 30, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 10, 7, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 10, 14, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 10, 21, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 10, 28, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 11, 4, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 11, 11, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 11, 18, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 11, 25, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 12, 2, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 12, 9, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 12, 16, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 12, 23, 9, 0, 0, tzid, iCal)
            };

            string[] TimeZones = new string[]
            {
                "EDT",
                "EDT",
                "EDT",
                "EDT",
                "EDT",
                "EDT",
                "EDT",
                "EDT",
                "EST",
                "EST",
                "EST",
                "EST",
                "EST",
                "EST",
                "EST",
                "EST",
                "EST"
            };

            for (int i = 0; i < DateTimes.Length; i++)
            {
                iCalDateTime dt = (iCalDateTime)DateTimes[i];
                Assert.AreEqual(dt, occurrences[i].Period.StartTime, "Event should occur on " + dt);
                Assert.AreEqual(TimeZones[i], dt.TimeZoneInfo.TimeZoneName, "Event " + dt + " should occur in the " + TimeZones[i] + " timezone");
            }

            Assert.AreEqual(
                DateTimes.Length,
                occurrences.Count,
                "There should be exactly " + DateTimes.Length + " occurrences; there were " + occurrences.Count);
        }

        /// <summary>
        /// See Page 119 of RFC 2445 - RRULE:FREQ=WEEKLY;INTERVAL=2;WKST=SU
        /// </summary>
        [Test, Category("Recurrence")]
        public void RRULE9()
        {
            iCalendar iCal = iCalendar.LoadFromFile(@"Calendars\Recurrence\RRULE9.ics");
            ProgramTest.TestCal(iCal);
            Event evt = iCal.Events[0];

            List<Occurrence> occurrences = evt.GetOccurrences(
                new iCalDateTime(1997, 9, 1, tzid, iCal),
                new iCalDateTime(1998, 1, 31, tzid, iCal));

            iCalDateTime[] DateTimes = new iCalDateTime[]
            {
                new iCalDateTime(1997, 9, 2, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 9, 16, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 9, 30, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 10, 14, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 10, 28, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 11, 11, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 11, 25, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 12, 9, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 12, 23, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1998, 1, 6, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1998, 1, 20, 9, 0, 0, tzid, iCal)
            };

            string[] TimeZones = new string[]
            {
                "EDT",
                "EDT",
                "EDT",
                "EDT",
                "EST",
                "EST",
                "EST",
                "EST",
                "EST",
                "EST",
                "EST"
            };

            for (int i = 0; i < DateTimes.Length; i++)
            {
                iCalDateTime dt = (iCalDateTime)DateTimes[i];
                Assert.AreEqual(dt, occurrences[i].Period.StartTime, "Event should occur on " + dt);
                Assert.AreEqual(TimeZones[i], dt.TimeZoneInfo.TimeZoneName, "Event " + dt + " should occur in the " + TimeZones[i] + " timezone");
            }

            Assert.AreEqual(
                DateTimes.Length,
                occurrences.Count,
                "There should be exactly " + DateTimes.Length + " occurrences; there were " + occurrences.Count);
        }

        /// <summary>
        /// See Page 119 of RFC 2445 - RRULE:FREQ=WEEKLY;UNTIL=19971007T000000Z;WKST=SU;BYDAY=TU,TH
        /// </summary>
        [Test, Category("Recurrence")]
        public void RRULE10()
        {
            iCalendar iCal = iCalendar.LoadFromFile(@"Calendars\Recurrence\RRULE10.ics");
            ProgramTest.TestCal(iCal);
            Event evt = iCal.Events[0];

            List<Occurrence> occurrences = evt.GetOccurrences(
                new iCalDateTime(1997, 9, 1, tzid, iCal),
                new iCalDateTime(1999, 1, 1, tzid, iCal));

            iCalDateTime[] DateTimes = new iCalDateTime[]
            {
                new iCalDateTime(1997, 9, 2, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 9, 4, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 9, 9, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 9, 11, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 9, 16, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 9, 18, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 9, 23, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 9, 25, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 9, 30, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 10, 2, 9, 0, 0, tzid, iCal)
            };

            for (int i = 0; i < DateTimes.Length; i++)
                Assert.AreEqual(DateTimes[i], occurrences[i].Period.StartTime, "Event should occur on " + DateTimes[i]);

            Assert.AreEqual(
                DateTimes.Length,
                occurrences.Count,
                "There should be exactly " + DateTimes.Length + " occurrences; there were " + occurrences.Count);
        }

        /// <summary>
        /// See Page 120 of RFC 2445 - RRULE:FREQ=WEEKLY;COUNT=10;WKST=SU;BYDAY=TU,TH
        /// </summary>
        [Test, Category("Recurrence")]
        public void RRULE11()
        {
            iCalendar iCal1 = iCalendar.LoadFromFile(@"Calendars\Recurrence\RRULE10.ics");
            iCalendar iCal2 = iCalendar.LoadFromFile(@"Calendars\Recurrence\RRULE11.ics");
            ProgramTest.TestCal(iCal1);
            ProgramTest.TestCal(iCal2);
            Event evt1 = (Event)iCal1.Events[0];
            Event evt2 = (Event)iCal2.Events[0];

            List<Occurrence> evt1occ = evt1.GetOccurrences(new iCalDateTime(1997, 9, 1), new iCalDateTime(1999, 1, 1));
            List<Occurrence> evt2occ = evt2.GetOccurrences(new iCalDateTime(1997, 9, 1), new iCalDateTime(1999, 1, 1));
            Assert.AreEqual(evt1occ.Count, evt2occ.Count, "RRULE11 does not match RRULE10 as it should");
            for (int i = 0; i < evt1occ.Count; i++)
                Assert.AreEqual(evt1occ[i].Period, evt2occ[i].Period, "PERIOD " + i + " from RRULE10 (" + evt1occ[i].Period.ToString() + ") does not match PERIOD " + i + " from RRULE11 (" + evt2occ[i].Period.ToString() + ")");
        }

        /// <summary>
        /// See Page 120 of RFC 2445 - RRULE:FREQ=WEEKLY;INTERVAL=2;UNTIL=19971224T000000Z;WKST=SU;BYDAY=MO,WE,FR
        /// </summary>
        [Test, Category("Recurrence")]
        public void RRULE12()
        {
            iCalendar iCal = iCalendar.LoadFromFile(@"Calendars\Recurrence\RRULE12.ics");
            ProgramTest.TestCal(iCal);
            Event evt = iCal.Events[0];

            List<Occurrence> occurrences = evt.GetOccurrences(
                new iCalDateTime(1996, 1, 1, tzid, iCal),
                new iCalDateTime(1999, 1, 1, tzid, iCal));

            iCalDateTime[] DateTimes = new iCalDateTime[]
            {
                new iCalDateTime(1997, 9, 2, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 9, 3, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 9, 5, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 9, 15, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 9, 17, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 9, 19, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 9, 29, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 10, 1, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 10, 3, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 10, 13, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 10, 15, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 10, 17, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 10, 27, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 10, 29, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 10, 31, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 11, 10, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 11, 12, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 11, 14, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 11, 24, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 11, 26, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 11, 28, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 12, 8, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 12, 10, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 12, 12, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 12, 22, 9, 0, 0, tzid, iCal)
            };

            string[] TimeZones = new string[]
            {
                "EDT",
                "EDT",
                "EDT",
                "EDT",
                "EDT",
                "EDT",
                "EDT",
                "EDT",
                "EDT",
                "EDT",
                "EDT",
                "EDT",
                "EST",
                "EST",
                "EST",
                "EST",
                "EST",
                "EST",
                "EST",
                "EST",
                "EST",
                "EST",
                "EST",
                "EST",                
                "EST"
            };

            for (int i = 0; i < DateTimes.Length; i++)
            {
                iCalDateTime dt = (iCalDateTime)DateTimes[i];
                Assert.AreEqual(dt, occurrences[i].Period.StartTime, "Event should occur on " + dt);
                Assert.AreEqual(TimeZones[i], dt.TimeZoneInfo.TimeZoneName, "Event " + dt + " should occur in the " + TimeZones[i] + " timezone");
            }

            Assert.AreEqual(
                DateTimes.Length,
                occurrences.Count,
                "There should be exactly " + DateTimes.Length + " occurrences; there were " + occurrences.Count);
        }

        /// <summary>
        /// Tests to ensure FREQUENCY=WEEKLY with INTERVAL=2 works when starting evaluation from an "off" week
        /// </summary>
        [Test, Category("Recurrence")]
        public void RRULE12_1()
        {
            iCalendar iCal = iCalendar.LoadFromFile(@"Calendars\Recurrence\RRULE12.ics");
            ProgramTest.TestCal(iCal);
            Event evt = iCal.Events[0];

            List<Occurrence> occurrences = evt.GetOccurrences(
                new iCalDateTime(1997, 9, 9, tzid, iCal),
                new iCalDateTime(1999, 1, 1, tzid, iCal));

            iCalDateTime[] DateTimes = new iCalDateTime[]
            {
                new iCalDateTime(1997, 9, 15, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 9, 17, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 9, 19, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 9, 29, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 10, 1, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 10, 3, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 10, 13, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 10, 15, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 10, 17, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 10, 27, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 10, 29, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 10, 31, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 11, 10, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 11, 12, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 11, 14, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 11, 24, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 11, 26, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 11, 28, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 12, 8, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 12, 10, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 12, 12, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 12, 22, 9, 0, 0, tzid, iCal)
            };

            string[] TimeZones = new string[]
            {
                "EDT",
                "EDT",
                "EDT",
                "EDT",
                "EDT",
                "EDT",
                "EDT",
                "EDT",
                "EDT",
                "EST",
                "EST",
                "EST",
                "EST",
                "EST",
                "EST",
                "EST",
                "EST",
                "EST",
                "EST",
                "EST",
                "EST",                
                "EST"
            };

            for (int i = 0; i < DateTimes.Length; i++)
            {
                iCalDateTime dt = (iCalDateTime)DateTimes[i];
                Assert.AreEqual(dt, occurrences[i].Period.StartTime, "Event should occur on " + dt);
                Assert.AreEqual(TimeZones[i], dt.TimeZoneInfo.TimeZoneName, "Event " + dt + " should occur in the " + TimeZones[i] + " timezone");
            }

            Assert.AreEqual(
                DateTimes.Length,
                occurrences.Count,
                "There should be exactly " + DateTimes.Length + " occurrences; there were " + occurrences.Count);
        }

        /// <summary>
        /// See Page 120 of RFC 2445 - RRULE:FREQ=WEEKLY;INTERVAL=2;COUNT=8;WKST=SU;BYDAY=TU,TH
        /// </summary>
        [Test, Category("Recurrence")]
        public void RRULE13()
        {
            iCalendar iCal = iCalendar.LoadFromFile(@"Calendars\Recurrence\RRULE13.ics");
            ProgramTest.TestCal(iCal);
            Event evt = iCal.Events[0];

            List<Occurrence> occurrences = evt.GetOccurrences(
                new iCalDateTime(1996, 1, 1, tzid, iCal),
                new iCalDateTime(1999, 1, 1, tzid, iCal));

            iCalDateTime[] DateTimes = new iCalDateTime[]
            {
                new iCalDateTime(1997, 9, 2, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 9, 4, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 9, 16, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 9, 18, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 9, 30, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 10, 2, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 10, 14, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 10, 16, 9, 0, 0, tzid, iCal)
            };

            for (int i = 0; i < DateTimes.Length; i++)
                Assert.AreEqual(DateTimes[i], occurrences[i].Period.StartTime, "Event should occur on " + DateTimes[i]);

            Assert.AreEqual(
                DateTimes.Length,
                occurrences.Count,
                "There should be exactly " + DateTimes.Length + " occurrences; there were " + occurrences.Count);
        }

        /// <summary>
        /// See Page 120 of RFC 2445 - RRULE:FREQ=MONTHLY;COUNT=10;BYDAY=1FR
        /// </summary>
        [Test, Category("Recurrence")]
        public void RRULE14()
        {
            iCalendar iCal = iCalendar.LoadFromFile(@"Calendars\Recurrence\RRULE14.ics");
            ProgramTest.TestCal(iCal);
            Event evt = iCal.Events[0];

            List<Occurrence> occurrences = evt.GetOccurrences(
                new iCalDateTime(1996, 1, 1, tzid, iCal),
                new iCalDateTime(1999, 1, 1, tzid, iCal));

            iCalDateTime[] DateTimes = new iCalDateTime[]
            {
                new iCalDateTime(1997, 9, 5, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 10, 3, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 11, 7, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 12, 5, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1998, 1, 2, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1998, 2, 6, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1998, 3, 6, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1998, 4, 3, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1998, 5, 1, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1998, 6, 5, 9, 0, 0, tzid, iCal)
            };

            string[] TimeZones = new string[]
            {
                "EDT",
                "EDT",
                "EST",
                "EST",
                "EST",
                "EST",
                "EST",
                "EST",
                "EDT",
                "EDT"
            };

            for (int i = 0; i < DateTimes.Length; i++)
            {
                iCalDateTime dt = (iCalDateTime)DateTimes[i];
                Assert.AreEqual(dt, occurrences[i].Period.StartTime, "Event should occur on " + dt);
                Assert.AreEqual(TimeZones[i], dt.TimeZoneInfo.TimeZoneName, "Event " + dt + " should occur in the " + TimeZones[i] + " timezone");
            }

            Assert.AreEqual(
                DateTimes.Length,
                occurrences.Count,
                "There should be exactly " + DateTimes.Length + " occurrences; there were " + occurrences.Count);
        }

        /// <summary>
        /// See Page 120 of RFC 2445 - RRULE:FREQ=MONTHLY;UNTIL=19971224T000000Z;BYDAY=1FR
        /// </summary>
        [Test, Category("Recurrence")]
        public void RRULE15()
        {
            iCalendar iCal = iCalendar.LoadFromFile(@"Calendars\Recurrence\RRULE15.ics");
            ProgramTest.TestCal(iCal);
            Event evt = iCal.Events[0];

            List<Occurrence> occurrences = evt.GetOccurrences(
                new iCalDateTime(1996, 1, 1, tzid, iCal),
                new iCalDateTime(1999, 1, 1, tzid, iCal));

            iCalDateTime[] DateTimes = new iCalDateTime[]
            {
                new iCalDateTime(1997, 9, 5, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 10, 3, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 11, 7, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 12, 5, 9, 0, 0, tzid, iCal)
            };

            string[] TimeZones = new string[]
            {
                "EDT",
                "EDT",
                "EST",
                "EST"
            };

            for (int i = 0; i < DateTimes.Length; i++)
            {
                iCalDateTime dt = (iCalDateTime)DateTimes[i];
                Assert.AreEqual(dt, occurrences[i].Period.StartTime, "Event should occur on " + dt);
                Assert.AreEqual(TimeZones[i], dt.TimeZoneInfo.TimeZoneName, "Event " + dt + " should occur in the " + TimeZones[i] + " timezone");
            }

            Assert.AreEqual(
                DateTimes.Length,
                occurrences.Count,
                "There should be exactly " + DateTimes.Length + " occurrences; there were " + occurrences.Count);
        }

        /// <summary>
        /// See Page 120 of RFC 2445 - RRULE:FREQ=MONTHLY;INTERVAL=2;COUNT=10;BYDAY=1SU,-1SU
        /// </summary>
        [Test, Category("Recurrence")]
        public void RRULE16()
        {
            iCalendar iCal = iCalendar.LoadFromFile(@"Calendars\Recurrence\RRULE16.ics");
            ProgramTest.TestCal(iCal);
            Event evt = iCal.Events[0];

            List<Occurrence> occurrences = evt.GetOccurrences(
                new iCalDateTime(1996, 1, 1, tzid, iCal),
                new iCalDateTime(1999, 1, 1, tzid, iCal));

            iCalDateTime[] DateTimes = new iCalDateTime[]
            {
                new iCalDateTime(1997, 9, 7, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 9, 28, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 11, 2, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 11, 30, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1998, 1, 4, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1998, 1, 25, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1998, 3, 1, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1998, 3, 29, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1998, 5, 3, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1998, 5, 31, 9, 0, 0, tzid, iCal)
            };

            string[] TimeZones = new string[]
            {
                "EDT",
                "EDT",
                "EST",
                "EST",
                "EST",
                "EST",
                "EST",
                "EST",
                "EDT",
                "EDT"
            };

            for (int i = 0; i < DateTimes.Length; i++)
            {
                iCalDateTime dt = (iCalDateTime)DateTimes[i];
                Assert.AreEqual(dt, occurrences[i].Period.StartTime, "Event should occur on " + dt);
                Assert.AreEqual(TimeZones[i], dt.TimeZoneInfo.TimeZoneName, "Event " + dt + " should occur in the " + TimeZones[i] + " timezone");
            }

            Assert.AreEqual(
                DateTimes.Length,
                occurrences.Count,
                "There should be exactly " + DateTimes.Length + " occurrences; there were " + occurrences.Count);
        }

        /// <summary>
        /// See Page 121 of RFC 2445 - RRULE:FREQ=MONTHLY;COUNT=6;BYDAY=-2MO
        /// </summary>
        [Test, Category("Recurrence")]
        public void RRULE17()
        {
            iCalendar iCal = iCalendar.LoadFromFile(@"Calendars\Recurrence\RRULE17.ics");
            ProgramTest.TestCal(iCal);
            Event evt = iCal.Events[0];

            List<Occurrence> occurrences = evt.GetOccurrences(
                new iCalDateTime(1996, 1, 1, tzid, iCal),
                new iCalDateTime(1999, 1, 1, tzid, iCal));

            iCalDateTime[] DateTimes = new iCalDateTime[]
            {
                new iCalDateTime(1997, 9, 22, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 10, 20, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 11, 17, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 12, 22, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1998, 1, 19, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1998, 2, 16, 9, 0, 0, tzid, iCal)
            };

            string[] TimeZones = new string[]
            {
                "EDT",
                "EDT",
                "EST",
                "EST",
                "EST",
                "EST"
            };

            for (int i = 0; i < DateTimes.Length; i++)
            {
                iCalDateTime dt = (iCalDateTime)DateTimes[i];
                Assert.AreEqual(dt, occurrences[i].Period.StartTime, "Event should occur on " + dt);
                Assert.AreEqual(TimeZones[i], dt.TimeZoneInfo.TimeZoneName, "Event " + dt + " should occur in the " + TimeZones[i] + " timezone");
            }

            Assert.AreEqual(
                DateTimes.Length,
                occurrences.Count,
                "There should be exactly " + DateTimes.Length + " occurrences; there were " + occurrences.Count);
        }

        /// <summary>
        /// See Page 121 of RFC 2445 - RRULE:FREQ=MONTHLY;BYMONTHDAY=-3
        /// </summary>
        [Test, Category("Recurrence")]
        public void RRULE18()
        {
            iCalendar iCal = iCalendar.LoadFromFile(@"Calendars\Recurrence\RRULE18.ics");
            ProgramTest.TestCal(iCal);
            Event evt = iCal.Events[0];

            List<Occurrence> occurrences = evt.GetOccurrences(
                new iCalDateTime(1996, 1, 1, tzid, iCal),
                new iCalDateTime(1998, 3, 1, tzid, iCal));

            iCalDateTime[] DateTimes = new iCalDateTime[]
            {
                new iCalDateTime(1997, 9, 28, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 10, 29, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 11, 28, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 12, 29, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1998, 1, 29, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1998, 2, 26, 9, 0, 0, tzid, iCal)
            };

            string[] TimeZones = new string[]
            {
                "EDT",                
                "EST",
                "EST",
                "EST",
                "EST",
                "EST"                
            };

            for (int i = 0; i < DateTimes.Length; i++)
            {
                iCalDateTime dt = (iCalDateTime)DateTimes[i];
                Assert.AreEqual(dt, occurrences[i].Period.StartTime, "Event should occur on " + dt);
                Assert.AreEqual(TimeZones[i], dt.TimeZoneInfo.TimeZoneName, "Event " + dt + " should occur in the " + TimeZones[i] + " timezone");
            }

            Assert.AreEqual(
                DateTimes.Length,
                occurrences.Count,
                "There should be exactly " + DateTimes.Length + " occurrences; there were " + occurrences.Count);
        }

        /// <summary>
        /// See Page 121 of RFC 2445 - RRULE:FREQ=MONTHLY;COUNT=10;BYMONTHDAY=2,15
        /// </summary>
        [Test, Category("Recurrence")]
        public void RRULE19()
        {
            iCalendar iCal = iCalendar.LoadFromFile(@"Calendars\Recurrence\RRULE19.ics");
            ProgramTest.TestCal(iCal);
            Event evt = iCal.Events[0];

            List<Occurrence> occurrences = evt.GetOccurrences(
                new iCalDateTime(1996, 1, 1, tzid, iCal),
                new iCalDateTime(1998, 3, 1, tzid, iCal));

            iCalDateTime[] DateTimes = new iCalDateTime[]
            {
                new iCalDateTime(1997, 9, 2, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 9, 15, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 10, 2, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 10, 15, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 11, 2, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 11, 15, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 12, 2, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 12, 15, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1998, 1, 2, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1998, 1, 15, 9, 0, 0, tzid, iCal)
            };

            string[] TimeZones = new string[]
            {
                "EDT",
                "EDT",
                "EDT",
                "EDT",
                "EST",
                "EST",
                "EST",
                "EST",
                "EST",
                "EST"
            };

            for (int i = 0; i < DateTimes.Length; i++)
            {
                iCalDateTime dt = (iCalDateTime)DateTimes[i];
                Assert.AreEqual(dt, occurrences[i].Period.StartTime, "Event should occur on " + dt);
                Assert.AreEqual(TimeZones[i], dt.TimeZoneInfo.TimeZoneName, "Event " + dt + " should occur in the " + TimeZones[i] + " timezone");
            }

            Assert.AreEqual(
                DateTimes.Length,
                occurrences.Count,
                "There should be exactly " + DateTimes.Length + " occurrences; there were " + occurrences.Count);
        }

        /// <summary>
        /// See Page 121 of RFC 2445 - RRULE:FREQ=MONTHLY;COUNT=10;BYMONTHDAY=1,-1
        /// </summary>
        [Test, Category("Recurrence")]
        public void RRULE20()
        {
            iCalendar iCal = iCalendar.LoadFromFile(@"Calendars\Recurrence\RRULE20.ics");
            ProgramTest.TestCal(iCal);
            Event evt = iCal.Events[0];

            List<Occurrence> occurrences = evt.GetOccurrences(
                new iCalDateTime(1996, 1, 1, tzid, iCal),
                new iCalDateTime(1998, 3, 1, tzid, iCal));

            iCalDateTime[] DateTimes = new iCalDateTime[]
            {
                new iCalDateTime(1997, 9, 30, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 10, 1, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 10, 31, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 11, 1, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 11, 30, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 12, 1, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 12, 31, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1998, 1, 1, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1998, 1, 31, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1998, 2, 1, 9, 0, 0, tzid, iCal)
            };

            string[] TimeZones = new string[]
            {
                "EDT",
                "EDT",                
                "EST",
                "EST",
                "EST",
                "EST",
                "EST",
                "EST",
                "EST",
                "EST"
            };

            for (int i = 0; i < DateTimes.Length; i++)
            {
                iCalDateTime dt = (iCalDateTime)DateTimes[i];
                Assert.AreEqual(dt, occurrences[i].Period.StartTime, "Event should occur on " + dt);
                Assert.AreEqual(TimeZones[i], dt.TimeZoneInfo.TimeZoneName, "Event " + dt + " should occur in the " + TimeZones[i] + " timezone");
            }

            Assert.AreEqual(
                DateTimes.Length,
                occurrences.Count,
                "There should be exactly " + DateTimes.Length + " occurrences; there were " + occurrences.Count);
        }

        /// <summary>
        /// See Page 121 of RFC 2445 - RRULE:FREQ=MONTHLY;INTERVAL=18;COUNT=10;BYMONTHDAY=10,11,12,13,14,15
        /// </summary>
        [Test, Category("Recurrence")]
        public void RRULE21()
        {
            iCalendar iCal = iCalendar.LoadFromFile(@"Calendars\Recurrence\RRULE21.ics");
            ProgramTest.TestCal(iCal);
            Event evt = iCal.Events[0];

            List<Occurrence> occurrences = evt.GetOccurrences(
                new iCalDateTime(1996, 1, 1, tzid, iCal),
                new iCalDateTime(2000, 1, 1, tzid, iCal));

            iCalDateTime[] DateTimes = new iCalDateTime[]
            {
                new iCalDateTime(1997, 9, 10, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 9, 11, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 9, 12, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 9, 13, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 9, 14, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 9, 15, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1999, 3, 10, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1999, 3, 11, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1999, 3, 12, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1999, 3, 13, 9, 0, 0, tzid, iCal),
            };

            string[] TimeZones = new string[]
            {
                "EDT",
                "EDT",
                "EDT",
                "EDT",
                "EDT",
                "EDT",
                "EST",
                "EST",
                "EST",
                "EST"                
            };

            for (int i = 0; i < DateTimes.Length; i++)
            {
                iCalDateTime dt = (iCalDateTime)DateTimes[i];
                Assert.AreEqual(dt, occurrences[i].Period.StartTime, "Event should occur on " + dt);
                Assert.AreEqual(TimeZones[i], dt.TimeZoneInfo.TimeZoneName, "Event " + dt + " should occur in the " + TimeZones[i] + " timezone");
            }

            Assert.AreEqual(
                DateTimes.Length,
                occurrences.Count,
                "There should be exactly " + DateTimes.Length + " occurrences; there were " + occurrences.Count);
        }

        /// <summary>
        /// See Page 122 of RFC 2445 - RRULE:FREQ=MONTHLY;INTERVAL=2;BYDAY=TU
        /// </summary>
        [Test, Category("Recurrence")]
        public void RRULE22()
        {
            iCalendar iCal = iCalendar.LoadFromFile(@"Calendars\Recurrence\RRULE22.ics");
            ProgramTest.TestCal(iCal);
            Event evt = iCal.Events[0];

            List<Occurrence> occurrences = evt.GetOccurrences(
                new iCalDateTime(1996, 1, 1, tzid, iCal),
                new iCalDateTime(1998, 4, 1, tzid, iCal));

            iCalDateTime[] DateTimes = new iCalDateTime[]
            {
                new iCalDateTime(1997, 9, 2, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 9, 9, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 9, 16, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 9, 23, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 9, 30, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 11, 4, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 11, 11, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 11, 18, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 11, 25, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1998, 1, 6, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1998, 1, 13, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1998, 1, 20, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1998, 1, 27, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1998, 3, 3, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1998, 3, 10, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1998, 3, 17, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1998, 3, 24, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1998, 3, 31, 9, 0, 0, tzid, iCal)
            };

            string[] TimeZones = new string[]
            {
                "EDT",
                "EDT",
                "EDT",
                "EDT",                
                "EDT",
                "EST",
                "EST",
                "EST",
                "EST",
                "EST",
                "EST",
                "EST",
                "EST",
                "EST",
                "EST",
                "EST",
                "EST",
                "EST"                
            };

            for (int i = 0; i < DateTimes.Length; i++)
            {
                iCalDateTime dt = (iCalDateTime)DateTimes[i];
                Assert.AreEqual(dt, occurrences[i].Period.StartTime, "Event should occur on " + dt);
                Assert.AreEqual(TimeZones[i], dt.TimeZoneInfo.TimeZoneName, "Event " + dt + " should occur in the " + TimeZones[i] + " timezone");
            }

            Assert.AreEqual(
                DateTimes.Length,
                occurrences.Count,
                "There should be exactly " + DateTimes.Length + " occurrences; there were " + occurrences.Count);
        }

        /// <summary>
        /// See Page 122 of RFC 2445 - RRULE:FREQ=YEARLY;COUNT=10;BYMONTH=6,7
        /// </summary>
        [Test, Category("Recurrence")]
        public void RRULE23()
        {
            iCalendar iCal = iCalendar.LoadFromFile(@"Calendars\Recurrence\RRULE23.ics");
            ProgramTest.TestCal(iCal);
            Event evt = iCal.Events[0];

            List<Occurrence> occurrences = evt.GetOccurrences(
                new iCalDateTime(1996, 1, 1, tzid, iCal),
                new iCalDateTime(2002, 1, 1, tzid, iCal));

            iCalDateTime[] DateTimes = new iCalDateTime[]
            {
                new iCalDateTime(1997, 6, 10, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 7, 10, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1998, 6, 10, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1998, 7, 10, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1999, 6, 10, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1999, 7, 10, 9, 0, 0, tzid, iCal),
                new iCalDateTime(2000, 6, 10, 9, 0, 0, tzid, iCal),
                new iCalDateTime(2000, 7, 10, 9, 0, 0, tzid, iCal),
                new iCalDateTime(2001, 6, 10, 9, 0, 0, tzid, iCal),
                new iCalDateTime(2001, 7, 10, 9, 0, 0, tzid, iCal)
            };

            for (int i = 0; i < DateTimes.Length; i++)
            {
                iCalDateTime dt = (iCalDateTime)DateTimes[i];
                Assert.AreEqual(dt, occurrences[i].Period.StartTime, "Event should occur on " + dt);
                Assert.AreEqual("EDT", dt.TimeZoneInfo.TimeZoneName, "Event " + dt + " should occur in the EDT timezone");
            }

            Assert.AreEqual(
                DateTimes.Length,
                occurrences.Count,
                "There should be exactly " + DateTimes.Length + " occurrences; there were " + occurrences.Count);
        }

        /// <summary>
        /// See Page 122 of RFC 2445 - RRULE:FREQ=YEARLY;INTERVAL=2;COUNT=10;BYMONTH=1,2,3
        /// </summary>
        [Test, Category("Recurrence")]
        public void RRULE24()
        {
            iCalendar iCal = iCalendar.LoadFromFile(@"Calendars\Recurrence\RRULE24.ics");
            ProgramTest.TestCal(iCal);
            Event evt = iCal.Events[0];

            List<Occurrence> occurrences = evt.GetOccurrences(
                new iCalDateTime(1996, 1, 1, tzid, iCal),
                new iCalDateTime(2003, 4, 1, tzid, iCal));

            iCalDateTime[] DateTimes = new iCalDateTime[]
            {
                new iCalDateTime(1997, 3, 10, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1999, 1, 10, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1999, 2, 10, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1999, 3, 10, 9, 0, 0, tzid, iCal),
                new iCalDateTime(2001, 1, 10, 9, 0, 0, tzid, iCal),
                new iCalDateTime(2001, 2, 10, 9, 0, 0, tzid, iCal),
                new iCalDateTime(2001, 3, 10, 9, 0, 0, tzid, iCal),
                new iCalDateTime(2003, 1, 10, 9, 0, 0, tzid, iCal),
                new iCalDateTime(2003, 2, 10, 9, 0, 0, tzid, iCal),
                new iCalDateTime(2003, 3, 10, 9, 0, 0, tzid, iCal)
            };

            for (int i = 0; i < DateTimes.Length; i++)
            {
                iCalDateTime dt = (iCalDateTime)DateTimes[i];
                Assert.AreEqual(dt, occurrences[i].Period.StartTime, "Event should occur on " + dt);
                Assert.AreEqual("EST", dt.TimeZoneInfo.TimeZoneName, "Event " + dt + " should occur in the EST timezone");
            }

            Assert.AreEqual(
                DateTimes.Length,
                occurrences.Count,
                "There should be exactly " + DateTimes.Length + " occurrences; there were " + occurrences.Count);
        }

        /// <summary>
        /// See Page 122 of RFC 2445 - RRULE:FREQ=YEARLY;INTERVAL=3;COUNT=10;BYYEARDAY=1,100,200
        /// </summary>
        [Test, Category("Recurrence")]
        public void RRULE25()
        {
            iCalendar iCal = iCalendar.LoadFromFile(@"Calendars\Recurrence\RRULE25.ics");
            ProgramTest.TestCal(iCal);
            Event evt = iCal.Events[0];

            List<Occurrence> occurrences = evt.GetOccurrences(
                new iCalDateTime(1996, 1, 1, tzid, iCal),
                new iCalDateTime(2007, 1, 1, tzid, iCal));

            iCalDateTime[] DateTimes = new iCalDateTime[]
            {
                new iCalDateTime(1997, 1, 1, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 4, 10, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 7, 19, 9, 0, 0, tzid, iCal),
                new iCalDateTime(2000, 1, 1, 9, 0, 0, tzid, iCal),
                new iCalDateTime(2000, 4, 9, 9, 0, 0, tzid, iCal),
                new iCalDateTime(2000, 7, 18, 9, 0, 0, tzid, iCal),
                new iCalDateTime(2003, 1, 1, 9, 0, 0, tzid, iCal),
                new iCalDateTime(2003, 4, 10, 9, 0, 0, tzid, iCal),
                new iCalDateTime(2003, 7, 19, 9, 0, 0, tzid, iCal),
                new iCalDateTime(2006, 1, 1, 9, 0, 0, tzid, iCal)
            };

            string[] TimeZones = new string[]
            {
                "EST",
                "EDT",
                "EDT",
                "EST",
                "EDT",
                "EDT",
                "EST",
                "EDT",
                "EDT",
                "EST"
            };

            for (int i = 0; i < DateTimes.Length; i++)
            {
                iCalDateTime dt = (iCalDateTime)DateTimes[i];
                Assert.AreEqual(dt, occurrences[i].Period.StartTime, "Event should occur on " + dt);
                Assert.AreEqual(TimeZones[i], dt.TimeZoneInfo.TimeZoneName, "Event " + dt + " should occur in the " + TimeZones[i] + " timezone");
            }

            Assert.AreEqual(
                DateTimes.Length,
                occurrences.Count,
                "There should be exactly " + DateTimes.Length + " occurrences; there were " + occurrences.Count);
        }

        /// <summary>
        /// See Page 123 of RFC 2445 - RRULE:FREQ=YEARLY;BYDAY=20MO
        /// </summary>
        [Test, Category("Recurrence")]
        public void RRULE26()
        {
            iCalendar iCal = iCalendar.LoadFromFile(@"Calendars\Recurrence\RRULE26.ics");
            ProgramTest.TestCal(iCal);
            Event evt = iCal.Events[0];

            List<Occurrence> occurrences = evt.GetOccurrences(
                new iCalDateTime(1996, 1, 1, tzid, iCal),
                new iCalDateTime(1999, 12, 31, tzid, iCal));

            iCalDateTime[] DateTimes = new iCalDateTime[]
            {
                new iCalDateTime(1997, 5, 19, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1998, 5, 18, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1999, 5, 17, 9, 0, 0, tzid, iCal)
            };

            for (int i = 0; i < DateTimes.Length; i++)
            {
                iCalDateTime dt = (iCalDateTime)DateTimes[i];
                Assert.AreEqual(dt, occurrences[i].Period.StartTime, "Event should occur on " + dt);
                Assert.AreEqual("EDT", dt.TimeZoneInfo.TimeZoneName, "Event " + dt + " should occur in the EDT timezone");
            }

            Assert.AreEqual(
                DateTimes.Length,
                occurrences.Count,
                "There should be exactly " + DateTimes.Length + " occurrences; there were " + occurrences.Count);
        }

        /// <summary>
        /// See Page 123 of RFC 2445 - RRULE:FREQ=YEARLY;BYWEEKNO=20;BYDAY=MO
        /// </summary>
        [Test, Category("Recurrence")]
        public void RRULE27()
        {
            iCalendar iCal = iCalendar.LoadFromFile(@"Calendars\Recurrence\RRULE27.ics");
            ProgramTest.TestCal(iCal);
            Event evt = iCal.Events[0];

            List<Occurrence> occurrences = evt.GetOccurrences(
                new iCalDateTime(1996, 1, 1, tzid, iCal),
                new iCalDateTime(1999, 12, 31, tzid, iCal));

            iCalDateTime[] DateTimes = new iCalDateTime[]
            {
                new iCalDateTime(1997, 5, 12, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1998, 5, 11, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1999, 5, 17, 9, 0, 0, tzid, iCal)
            };

            for (int i = 0; i < DateTimes.Length; i++)
            {
                iCalDateTime dt = (iCalDateTime)DateTimes[i];
                Assert.AreEqual(dt, occurrences[i].Period.StartTime, "Event should occur on " + dt);
                Assert.AreEqual("EDT", dt.TimeZoneInfo.TimeZoneName, "Event " + dt + " should occur in the EDT timezone");
            }

            Assert.AreEqual(
                DateTimes.Length,
                occurrences.Count,
                "There should be exactly " + DateTimes.Length + " occurrences; there were " + occurrences.Count);
        }

        /// <summary>
        /// See Page 123 of RFC 2445 - RRULE:FREQ=YEARLY;BYMONTH=3;BYDAY=TH
        /// </summary>
        [Test, Category("Recurrence")]
        public void RRULE28()
        {
            iCalendar iCal = iCalendar.LoadFromFile(@"Calendars\Recurrence\RRULE28.ics");
            ProgramTest.TestCal(iCal);
            Event evt = iCal.Events[0];

            List<Occurrence> occurrences = evt.GetOccurrences(
                new iCalDateTime(1996, 1, 1, tzid, iCal),
                new iCalDateTime(1999, 12, 31, tzid, iCal));

            iCalDateTime[] DateTimes = new iCalDateTime[]
            {
                new iCalDateTime(1997, 3, 13, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 3, 20, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 3, 27, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1998, 3, 5, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1998, 3, 12, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1998, 3, 19, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1998, 3, 26, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1999, 3, 4, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1999, 3, 11, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1999, 3, 18, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1999, 3, 25, 9, 0, 0, tzid, iCal)
            };

            for (int i = 0; i < DateTimes.Length; i++)
            {
                iCalDateTime dt = (iCalDateTime)DateTimes[i];
                Assert.AreEqual(dt, occurrences[i].Period.StartTime, "Event should occur on " + dt);
                Assert.AreEqual("EST", dt.TimeZoneInfo.TimeZoneName, "Event " + dt + " should occur in the EST timezone");
            }

            Assert.AreEqual(
                DateTimes.Length,
                occurrences.Count,
                "There should be exactly " + DateTimes.Length + " occurrences; there were " + occurrences.Count);
        }

        /// <summary>
        /// See Page 123 of RFC 2445 - RRULE:FREQ=YEARLY;BYDAY=TH;BYMONTH=6,7,8
        /// </summary>
        [Test, Category("Recurrence")]
        public void RRULE29()
        {
            iCalendar iCal = iCalendar.LoadFromFile(@"Calendars\Recurrence\RRULE29.ics");
            ProgramTest.TestCal(iCal);
            Event evt = iCal.Events[0];

            List<Occurrence> occurrences = evt.GetOccurrences(
                new iCalDateTime(1996, 1, 1, tzid, iCal),
                new iCalDateTime(1999, 12, 31, tzid, iCal));

            iCalDateTime[] DateTimes = new iCalDateTime[]
            {
                new iCalDateTime(1997, 6, 5, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 6, 12, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 6, 19, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 6, 26, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 7, 3, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 7, 10, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 7, 17, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 7, 24, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 7, 31, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 8, 7, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 8, 14, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 8, 21, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 8, 28, 9, 0, 0, tzid, iCal),                
                new iCalDateTime(1998, 6, 4, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1998, 6, 11, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1998, 6, 18, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1998, 6, 25, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1998, 7, 2, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1998, 7, 9, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1998, 7, 16, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1998, 7, 23, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1998, 7, 30, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1998, 8, 6, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1998, 8, 13, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1998, 8, 20, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1998, 8, 27, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1999, 6, 3, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1999, 6, 10, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1999, 6, 17, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1999, 6, 24, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1999, 7, 1, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1999, 7, 8, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1999, 7, 15, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1999, 7, 22, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1999, 7, 29, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1999, 8, 5, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1999, 8, 12, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1999, 8, 19, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1999, 8, 26, 9, 0, 0, tzid, iCal)
            };

            for (int i = 0; i < DateTimes.Length; i++)
            {
                iCalDateTime dt = (iCalDateTime)DateTimes[i];
                Assert.AreEqual(dt, occurrences[i].Period.StartTime, "Event should occur on " + dt);
                Assert.AreEqual("EDT", dt.TimeZoneInfo.TimeZoneName, "Event " + dt + " should occur in the EDT timezone");
            }

            Assert.AreEqual(
                DateTimes.Length,
                occurrences.Count,
                "There should be exactly " + DateTimes.Length + " occurrences; there were " + occurrences.Count);
        }

        /// <summary>
        /// See Page 123 of RFC 2445:
        /// EXDATE;TZID=US-Eastern:19970902T090000
        /// RRULE:FREQ=MONTHLY;BYDAY=FR;BYMONTHDAY=13
        /// </summary>
        [Test, Category("Recurrence")]
        public void RRULE30()
        {
            iCalendar iCal = iCalendar.LoadFromFile(@"Calendars\Recurrence\RRULE30.ics");
            ProgramTest.TestCal(iCal);
            Event evt = iCal.Events[0];

            List<Occurrence> occurrences = evt.GetOccurrences(
                new iCalDateTime(1996, 1, 1, tzid, iCal),
                new iCalDateTime(2000, 12, 31, tzid, iCal));

            iCalDateTime[] DateTimes = new iCalDateTime[]
            {
                new iCalDateTime(1998, 2, 13, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1998, 3, 13, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1998, 11, 13, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1999, 8, 13, 9, 0, 0, tzid, iCal),
                new iCalDateTime(2000, 10, 13, 9, 0, 0, tzid, iCal)
            };

            string[] TimeZones = new string[]
            {
                "EST",
                "EST",
                "EST",
                "EDT",
                "EDT"                
            };

            for (int i = 0; i < DateTimes.Length; i++)
            {
                iCalDateTime dt = (iCalDateTime)DateTimes[i];
                Assert.AreEqual(dt, occurrences[i].Period.StartTime, "Event should occur on " + dt);
                Assert.AreEqual(TimeZones[i], dt.TimeZoneInfo.TimeZoneName, "Event " + dt + " should occur in the " + TimeZones[i] + " timezone");
            }

            Assert.AreEqual(
                DateTimes.Length,
                occurrences.Count,
                "There should be exactly " + DateTimes.Length + " occurrences; there were " + occurrences.Count);
        }

        /// <summary>
        /// See Page 124 of RFC 2445 - RRULE:FREQ=MONTHLY;BYDAY=SA;BYMONTHDAY=7,8,9,10,11,12,13
        /// </summary>
        [Test, Category("Recurrence")]
        public void RRULE31()
        {
            iCalendar iCal = iCalendar.LoadFromFile(@"Calendars\Recurrence\RRULE31.ics");
            ProgramTest.TestCal(iCal);
            Event evt = iCal.Events[0];

            List<Occurrence> occurrences = evt.GetOccurrences(
                new iCalDateTime(1996, 1, 1, tzid, iCal),
                new iCalDateTime(1998, 6, 30, tzid, iCal));

            iCalDateTime[] DateTimes = new iCalDateTime[]
            {
                new iCalDateTime(1997, 9, 13, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 10, 11, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 11, 8, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 12, 13, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1998, 1, 10, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1998, 2, 7, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1998, 3, 7, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1998, 4, 11, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1998, 5, 9, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1998, 6, 13, 9, 0, 0, tzid, iCal)
            };

            string[] TimeZones = new string[]
            {
                "EDT",
                "EDT",
                "EST",
                "EST",
                "EST",
                "EST",
                "EST",
                "EDT",
                "EDT",
                "EDT"
            };

            for (int i = 0; i < DateTimes.Length; i++)
            {
                iCalDateTime dt = (iCalDateTime)DateTimes[i];
                Assert.AreEqual(dt, occurrences[i].Period.StartTime, "Event should occur on " + dt);
                Assert.AreEqual(TimeZones[i], dt.TimeZoneInfo.TimeZoneName, "Event " + dt + " should occur in the " + TimeZones[i] + " timezone");
            }

            Assert.AreEqual(
                DateTimes.Length,
                occurrences.Count,
                "There should be exactly " + DateTimes.Length + " occurrences; there were " + occurrences.Count);
        }

        /// <summary>
        /// See Page 124 of RFC 2445 - RRULE:FREQ=YEARLY;INTERVAL=4;BYMONTH=11;BYDAY=TU;BYMONTHDAY=2,3,4,5,6,7,8
        /// </summary>
        [Test, Category("Recurrence")]
        public void RRULE32()
        {
            iCalendar iCal = iCalendar.LoadFromFile(@"Calendars\Recurrence\RRULE32.ics");
            ProgramTest.TestCal(iCal);
            Event evt = iCal.Events[0];

            List<Occurrence> occurrences = evt.GetOccurrences(
                new iCalDateTime(1996, 1, 1, tzid, iCal),
                new iCalDateTime(2004, 12, 31, tzid, iCal));

            iCalDateTime[] DateTimes = new iCalDateTime[]
            {
                new iCalDateTime(1996, 11, 5, 9, 0, 0, tzid, iCal),
                new iCalDateTime(2000, 11, 7, 9, 0, 0, tzid, iCal),
                new iCalDateTime(2004, 11, 2, 9, 0, 0, tzid, iCal)
            };

            for (int i = 0; i < DateTimes.Length; i++)
            {
                iCalDateTime dt = (iCalDateTime)DateTimes[i];
                Assert.AreEqual(dt, occurrences[i].Period.StartTime, "Event should occur on " + dt);
                Assert.AreEqual("EST", dt.TimeZoneInfo.TimeZoneName, "Event " + dt + " should occur in the EST timezone");
            }

            Assert.AreEqual(
                DateTimes.Length,
                occurrences.Count,
                "There should be exactly " + DateTimes.Length + " occurrences; there were " + occurrences.Count);
        }

        /// <summary>
        /// See Page 124 of RFC 2445 - RRULE:FREQ=MONTHLY;COUNT=3;BYDAY=TU,WE,TH;BYSETPOS=3
        /// </summary>
        [Test, Category("Recurrence")]
        public void RRULE33()
        {
            iCalendar iCal = iCalendar.LoadFromFile(@"Calendars\Recurrence\RRULE33.ics");
            ProgramTest.TestCal(iCal);
            Event evt = iCal.Events[0];

            List<Occurrence> occurrences = evt.GetOccurrences(
                new iCalDateTime(1996, 1, 1, tzid, iCal),
                new iCalDateTime(2004, 12, 31, tzid, iCal));

            iCalDateTime[] DateTimes = new iCalDateTime[]
            {
                new iCalDateTime(1997, 9, 4, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 10, 7, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 11, 6, 9, 0, 0, tzid, iCal)
            };

            string[] TimeZones = new string[]
            {
                "EDT",
                "EDT",
                "EST"
            };

            for (int i = 0; i < DateTimes.Length; i++)
            {
                iCalDateTime dt = (iCalDateTime)DateTimes[i];
                Assert.AreEqual(dt, occurrences[i].Period.StartTime, "Event should occur on " + dt);
                Assert.AreEqual(TimeZones[i], dt.TimeZoneInfo.TimeZoneName, "Event " + dt + " should occur in the " + TimeZones[i] + " timezone");
            }

            Assert.AreEqual(
                DateTimes.Length,
                occurrences.Count,
                "There should be exactly " + DateTimes.Length + " occurrences; there were " + occurrences.Count);
        }

        /// <summary>
        /// See Page 124 of RFC 2445 - RRULE:FREQ=MONTHLY;BYDAY=MO,TU,WE,TH,FR;BYSETPOS=-2
        /// </summary>
        [Test, Category("Recurrence")]
        public void RRULE34()
        {
            iCalendar iCal = iCalendar.LoadFromFile(@"Calendars\Recurrence\RRULE34.ics");
            ProgramTest.TestCal(iCal);
            Event evt = iCal.Events[0];

            List<Occurrence> occurrences = evt.GetOccurrences(
                new iCalDateTime(1996, 1, 1, tzid, iCal),
                new iCalDateTime(1998, 3, 31, tzid, iCal));

            iCalDateTime[] DateTimes = new iCalDateTime[]
            {
                new iCalDateTime(1997, 9, 29, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 10, 30, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 11, 27, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 12, 30, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1998, 1, 29, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1998, 2, 26, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1998, 3, 30, 9, 0, 0, tzid, iCal)
            };

            string[] TimeZones = new string[]
            {
                "EDT",                
                "EST",
                "EST",
                "EST",
                "EST",
                "EST",
                "EST"
            };

            for (int i = 0; i < DateTimes.Length; i++)
            {
                iCalDateTime dt = (iCalDateTime)DateTimes[i];
                Assert.AreEqual(dt, occurrences[i].Period.StartTime, "Event should occur on " + dt);
                Assert.AreEqual(TimeZones[i], dt.TimeZoneInfo.TimeZoneName, "Event " + dt + " should occur in the " + TimeZones[i] + " timezone");
            }

            Assert.AreEqual(
                DateTimes.Length,
                occurrences.Count,
                "There should be exactly " + DateTimes.Length + " occurrences; there were " + occurrences.Count);
        }

        /// <summary>
        /// See Page 125 of RFC 2445 - RRULE:FREQ=HOURLY;INTERVAL=3;UNTIL=19970902T170000Z        
        /// FIXME: The UNTIL time on this item has been altered to 19970902T190000Z to
        /// match the local EDT time occurrence of 3:00pm.  Is the RFC example incorrect?
        /// </summary>
        [Test, Category("Recurrence")]
        public void RRULE35()
        {
            iCalendar iCal = iCalendar.LoadFromFile(@"Calendars\Recurrence\RRULE35.ics");
            ProgramTest.TestCal(iCal);
            Event evt = iCal.Events[0];

            List<Occurrence> occurrences = evt.GetOccurrences(
                new iCalDateTime(1996, 1, 1, tzid, iCal),
                new iCalDateTime(1998, 3, 31, tzid, iCal));

            iCalDateTime[] DateTimes = new iCalDateTime[]
            {
                new iCalDateTime(1997, 9, 2, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 9, 2, 12, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 9, 2, 15, 0, 0, tzid, iCal)
            };

            for (int i = 0; i < DateTimes.Length; i++)
            {
                iCalDateTime dt = (iCalDateTime)DateTimes[i];
                Assert.AreEqual(dt, occurrences[i].Period.StartTime, "Event should occur on " + dt);
                Assert.AreEqual("EDT", dt.TimeZoneInfo.TimeZoneName, "Event " + dt + " should occur in the EDT timezone");
            }

            Assert.AreEqual(
                DateTimes.Length,
                occurrences.Count,
                "There should be exactly " + DateTimes.Length + " occurrences; there were " + occurrences.Count);
        }

        /// <summary>
        /// See Page 125 of RFC 2445 - RRULE:FREQ=MINUTELY;INTERVAL=15;COUNT=6
        /// </summary>
        [Test, Category("Recurrence")]
        public void RRULE36()
        {
            iCalendar iCal = iCalendar.LoadFromFile(@"Calendars\Recurrence\RRULE36.ics");
            ProgramTest.TestCal(iCal);
            Event evt = iCal.Events[0];

            List<Occurrence> occurrences = evt.GetOccurrences(
                new iCalDateTime(1997, 9, 2, tzid, iCal),
                new iCalDateTime(1997, 9, 3, tzid, iCal));

            iCalDateTime[] DateTimes = new iCalDateTime[]
            {
                new iCalDateTime(1997, 9, 2, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 9, 2, 9, 15, 0, tzid, iCal),
                new iCalDateTime(1997, 9, 2, 9, 30, 0, tzid, iCal),
                new iCalDateTime(1997, 9, 2, 9, 45, 0, tzid, iCal),
                new iCalDateTime(1997, 9, 2, 10, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 9, 2, 10, 15, 0, tzid, iCal)
            };

            for (int i = 0; i < DateTimes.Length; i++)
            {
                iCalDateTime dt = (iCalDateTime)DateTimes[i];
                Assert.AreEqual(dt, occurrences[i].Period.StartTime, "Event should occur on " + dt);
                Assert.AreEqual("EDT", dt.TimeZoneInfo.TimeZoneName, "Event " + dt + " should occur in the EDT timezone");
            }

            Assert.AreEqual(
                DateTimes.Length,
                occurrences.Count,
                "There should be exactly " + DateTimes.Length + " occurrences; there were " + occurrences.Count);
        }

        /// <summary>
        /// See Page 125 of RFC 2445 - RRULE:FREQ=MINUTELY;INTERVAL=90;COUNT=4
        /// </summary>
        [Test, Category("Recurrence")]
        public void RRULE37()
        {
            iCalendar iCal = iCalendar.LoadFromFile(@"Calendars\Recurrence\RRULE37.ics");
            ProgramTest.TestCal(iCal);
            Event evt = iCal.Events[0];

            List<Occurrence> occurrences = evt.GetOccurrences(
                new iCalDateTime(1996, 1, 1, tzid, iCal),
                new iCalDateTime(1998, 12, 31, tzid, iCal));

            iCalDateTime[] DateTimes = new iCalDateTime[]
            {
                new iCalDateTime(1997, 9, 2, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 9, 2, 10, 30, 0, tzid, iCal),
                new iCalDateTime(1997, 9, 2, 12, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 9, 2, 13, 30, 0, tzid, iCal)
            };

            for (int i = 0; i < DateTimes.Length; i++)
            {
                iCalDateTime dt = (iCalDateTime)DateTimes[i];
                Assert.AreEqual(dt, occurrences[i].Period.StartTime, "Event should occur on " + dt);
                Assert.AreEqual("EDT", dt.TimeZoneInfo.TimeZoneName, "Event " + dt + " should occur in the EDT timezone");
            }

            Assert.AreEqual(
                DateTimes.Length,
                occurrences.Count,
                "There should be exactly " + DateTimes.Length + " occurrences; there were " + occurrences.Count);
        }

        /// <summary>
        /// See Page 125 of RFC 2445 - RRULE:FREQ=DAILY;BYHOUR=9,10,11,12,13,14,15,16;BYMINUTE=0,20,40
        /// </summary>
        [Test, Category("Recurrence")]
        public void RRULE38()
        {
            iCalendar iCal = iCalendar.LoadFromFile(@"Calendars\Recurrence\RRULE38.ics");
            ProgramTest.TestCal(iCal);
            Event evt = iCal.Events[0];

            List<Occurrence> occurrences = evt.GetOccurrences(
                new iCalDateTime(1997, 9, 2, tzid, iCal),
                new iCalDateTime(1997, 9, 4, tzid, iCal));

            iCalDateTime[] DateTimes = new iCalDateTime[]
            {
                new iCalDateTime(1997, 9, 2, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 9, 2, 9, 20, 0, tzid, iCal),
                new iCalDateTime(1997, 9, 2, 9, 40, 0, tzid, iCal),
                new iCalDateTime(1997, 9, 2, 10, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 9, 2, 10, 20, 0, tzid, iCal),
                new iCalDateTime(1997, 9, 2, 10, 40, 0, tzid, iCal),
                new iCalDateTime(1997, 9, 2, 11, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 9, 2, 11, 20, 0, tzid, iCal),
                new iCalDateTime(1997, 9, 2, 11, 40, 0, tzid, iCal),
                new iCalDateTime(1997, 9, 2, 12, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 9, 2, 12, 20, 0, tzid, iCal),
                new iCalDateTime(1997, 9, 2, 12, 40, 0, tzid, iCal),
                new iCalDateTime(1997, 9, 2, 13, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 9, 2, 13, 20, 0, tzid, iCal),
                new iCalDateTime(1997, 9, 2, 13, 40, 0, tzid, iCal),
                new iCalDateTime(1997, 9, 2, 14, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 9, 2, 14, 20, 0, tzid, iCal),
                new iCalDateTime(1997, 9, 2, 14, 40, 0, tzid, iCal),
                new iCalDateTime(1997, 9, 2, 15, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 9, 2, 15, 20, 0, tzid, iCal),
                new iCalDateTime(1997, 9, 2, 15, 40, 0, tzid, iCal),
                new iCalDateTime(1997, 9, 2, 16, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 9, 2, 16, 20, 0, tzid, iCal),
                new iCalDateTime(1997, 9, 2, 16, 40, 0, tzid, iCal),
                new iCalDateTime(1997, 9, 3, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 9, 3, 9, 20, 0, tzid, iCal),
                new iCalDateTime(1997, 9, 3, 9, 40, 0, tzid, iCal),
                new iCalDateTime(1997, 9, 3, 10, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 9, 3, 10, 20, 0, tzid, iCal),
                new iCalDateTime(1997, 9, 3, 10, 40, 0, tzid, iCal),
                new iCalDateTime(1997, 9, 3, 11, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 9, 3, 11, 20, 0, tzid, iCal),
                new iCalDateTime(1997, 9, 3, 11, 40, 0, tzid, iCal),
                new iCalDateTime(1997, 9, 3, 12, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 9, 3, 12, 20, 0, tzid, iCal),
                new iCalDateTime(1997, 9, 3, 12, 40, 0, tzid, iCal),
                new iCalDateTime(1997, 9, 3, 13, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 9, 3, 13, 20, 0, tzid, iCal),
                new iCalDateTime(1997, 9, 3, 13, 40, 0, tzid, iCal),
                new iCalDateTime(1997, 9, 3, 14, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 9, 3, 14, 20, 0, tzid, iCal),
                new iCalDateTime(1997, 9, 3, 14, 40, 0, tzid, iCal),
                new iCalDateTime(1997, 9, 3, 15, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 9, 3, 15, 20, 0, tzid, iCal),
                new iCalDateTime(1997, 9, 3, 15, 40, 0, tzid, iCal),
                new iCalDateTime(1997, 9, 3, 16, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 9, 3, 16, 20, 0, tzid, iCal),
                new iCalDateTime(1997, 9, 3, 16, 40, 0, tzid, iCal)
            };

            for (int i = 0; i < DateTimes.Length; i++)
            {
                iCalDateTime dt = (iCalDateTime)DateTimes[i];
                Assert.AreEqual(dt, occurrences[i].Period.StartTime, "Event should occur on " + dt);
                Assert.AreEqual("EDT", dt.TimeZoneInfo.TimeZoneName, "Event " + dt + " should occur in the EDT timezone");
            }

            Assert.AreEqual(
                DateTimes.Length,
                occurrences.Count,
                "There should be exactly " + DateTimes.Length + " occurrences; there were " + occurrences.Count);
        }

        /// <summary>
        /// See Page 125 of RFC 2445 - RRULE:FREQ=MINUTELY;INTERVAL=20;BYHOUR=9,10,11,12,13,14,15,16
        /// </summary>
        [Test, Category("Recurrence")]
        public void RRULE39()
        {
            iCalendar iCal1 = iCalendar.LoadFromFile(@"Calendars\Recurrence\RRULE38.ics");
            iCalendar iCal2 = iCalendar.LoadFromFile(@"Calendars\Recurrence\RRULE39.ics");
            ProgramTest.TestCal(iCal1);
            ProgramTest.TestCal(iCal2);
            Event evt1 = (Event)iCal1.Events[0];
            Event evt2 = (Event)iCal2.Events[0];

            List<Occurrence> evt1occ = evt1.GetOccurrences(new iCalDateTime(1997, 9, 1, tzid, iCal1), new iCalDateTime(1997, 9, 3, tzid, iCal1));
            List<Occurrence> evt2occ = evt2.GetOccurrences(new iCalDateTime(1997, 9, 1, tzid, iCal2), new iCalDateTime(1997, 9, 3, tzid, iCal2));
            Assert.IsTrue(evt1occ.Count == evt2occ.Count, "RRULE39 does not match RRULE38 as it should");
            for (int i = 0; i < evt1occ.Count; i++)
                Assert.AreEqual(evt1occ[i].Period, evt2occ[i].Period, "PERIOD " + i + " from RRULE38 (" + evt1occ[i].Period.ToString() + ") does not match PERIOD " + i + " from RRULE39 (" + evt2occ[i].Period.ToString() + ")");
        }

        /// <summary>
        /// See Page 125 of RFC 2445 - RRULE:FREQ=WEEKLY;INTERVAL=2;COUNT=4;BYDAY=TU,SU;WKST=MO
        /// </summary>
        [Test, Category("Recurrence")]
        public void RRULE40()
        {
            iCalendar iCal = iCalendar.LoadFromFile(@"Calendars\Recurrence\RRULE40.ics");
            ProgramTest.TestCal(iCal);
            Event evt = iCal.Events[0];

            List<Occurrence> occurrences = evt.GetOccurrences(
                new iCalDateTime(1996, 1, 1, tzid, iCal),
                new iCalDateTime(1998, 12, 31, tzid, iCal));

            iCalDateTime[] DateTimes = new iCalDateTime[]
            {
                new iCalDateTime(1997, 8, 5, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 8, 10, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 8, 19, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 8, 24, 9, 0, 0, tzid, iCal)
            };

            for (int i = 0; i < DateTimes.Length; i++)
            {
                iCalDateTime dt = (iCalDateTime)DateTimes[i];
                Assert.AreEqual(dt, occurrences[i].Period.StartTime, "Event should occur on " + dt);
                Assert.AreEqual("EDT", dt.TimeZoneInfo.TimeZoneName, "Event " + dt + " should occur in the EDT timezone");
            }

            Assert.AreEqual(
                DateTimes.Length,
                occurrences.Count,
                "There should be exactly " + DateTimes.Length + " occurrences; there were " + occurrences.Count);
        }

        /// <summary>
        /// See Page 125 of RFC 2445 - RRULE:FREQ=WEEKLY;INTERVAL=2;COUNT=4;BYDAY=TU,SU;WKST=SU
        /// This is the same as RRULE40, except WKST is SU, which changes the results.
        /// </summary>
        [Test, Category("Recurrence")]
        public void RRULE41()
        {
            iCalendar iCal = iCalendar.LoadFromFile(@"Calendars\Recurrence\RRULE41.ics");
            ProgramTest.TestCal(iCal);
            Event evt = iCal.Events[0];

            List<Occurrence> occurrences = evt.GetOccurrences(
                new iCalDateTime(1996, 1, 1, tzid, iCal),
                new iCalDateTime(1998, 12, 31, tzid, iCal));

            iCalDateTime[] DateTimes = new iCalDateTime[]
            {
                new iCalDateTime(1997, 8, 5, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 8, 17, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 8, 19, 9, 0, 0, tzid, iCal),
                new iCalDateTime(1997, 8, 31, 9, 0, 0, tzid, iCal)
            };

            for (int i = 0; i < DateTimes.Length; i++)
            {
                iCalDateTime dt = (iCalDateTime)DateTimes[i];
                Assert.AreEqual(dt, occurrences[i].Period.StartTime, "Event should occur on " + dt);
                Assert.AreEqual("EDT", dt.TimeZoneInfo.TimeZoneName, "Event " + dt + " should occur in the EDT timezone");
            }

            Assert.AreEqual(
                DateTimes.Length,
                occurrences.Count,
                "There should be exactly " + DateTimes.Length + " occurrences; there were " + occurrences.Count);
        }

        /// <summary>
        /// Tests WEEKLY Frequencies to ensure that those with an INTERVAL > 1
        /// are correctly handled.  See Bug #1741093 - WEEKLY frequency eval behaves strangely.
        /// </summary>
        [Test, Category("Recurrence")]
        public void RRULE42()
        {
            iCalendar iCal = iCalendar.LoadFromFile(@"Calendars\Recurrence\RRULE42.ics");
            ProgramTest.TestCal(iCal);
            Event evt = iCal.Events[0];

            List<Occurrence> occurrences = evt.GetOccurrences(
                new iCalDateTime(2007, 7, 1, tzid, iCal),
                new iCalDateTime(2007, 8, 1, tzid, iCal));

            iCalDateTime[] DateTimes = new iCalDateTime[]
            {
                new iCalDateTime(2007, 7, 2, 8, 0, 0, tzid, iCal),
                new iCalDateTime(2007, 7, 3, 8, 0, 0, tzid, iCal),
                new iCalDateTime(2007, 7, 4, 8, 0, 0, tzid, iCal),
                new iCalDateTime(2007, 7, 5, 8, 0, 0, tzid, iCal),
                new iCalDateTime(2007, 7, 6, 8, 0, 0, tzid, iCal),
                new iCalDateTime(2007, 7, 16, 8, 0, 0, tzid, iCal),
                new iCalDateTime(2007, 7, 17, 8, 0, 0, tzid, iCal),
                new iCalDateTime(2007, 7, 18, 8, 0, 0, tzid, iCal),
                new iCalDateTime(2007, 7, 19, 8, 0, 0, tzid, iCal),
                new iCalDateTime(2007, 7, 20, 8, 0, 0, tzid, iCal),
                new iCalDateTime(2007, 7, 30, 8, 0, 0, tzid, iCal),
                new iCalDateTime(2007, 7, 31, 8, 0, 0, tzid, iCal)
            };

            Assert.AreEqual(
                DateTimes.Length,
                occurrences.Count,
                "There should be exactly " + DateTimes.Length + " occurrences; there were " + occurrences.Count);

            for (int i = 0; i < DateTimes.Length; i++)
            {
                iCalDateTime dt = (iCalDateTime)DateTimes[i];
                Assert.AreEqual(dt, occurrences[i].Period.StartTime, "Event should occur on " + dt);
                Assert.AreEqual("EDT", dt.TimeZoneInfo.TimeZoneName, "Event " + dt + " should occur in the EDT timezone");
            }
        }

        /// <summary>
        /// Tests recurrence rule issue noted in
        /// Bug #1821721 - Recur for every-other-month doesn't evaluate correctly
        /// </summary>
        [Test, Category("Recurrence")]
        public void RRULE43()
        {
            iCalendar iCal = new iCalendar();

            iCalTimeZone tz = iCal.Create<iCalTimeZone>();

            tz.TZID = "US-Eastern";
            tz.Last_Modified = new DateTime(1987, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            iCalTimeZoneInfo standard = new iCalTimeZoneInfo(iCalTimeZone.STANDARD, tz);
            standard.Start = new DateTime(1967, 10, 29, 2, 0, 0, DateTimeKind.Utc);
            standard.AddRecurrencePattern(new RecurrencePattern("FREQ=YEARLY;BYDAY=-1SU;BYMONTH=10"));
            standard.TZOffsetFrom = new UTC_Offset("-0400");
            standard.TZOffsetTo = new UTC_Offset("-0500");
            standard.TimeZoneName = "EST";

            iCalTimeZoneInfo daylight = new iCalTimeZoneInfo(iCalTimeZone.DAYLIGHT, tz);
            daylight.Start = new DateTime(1987, 4, 5, 2, 0, 0, DateTimeKind.Utc);
            daylight.AddRecurrencePattern(new RecurrencePattern("FREQ=YEARLY;BYDAY=1SU;BYMONTH=4"));
            daylight.TZOffsetFrom = new UTC_Offset("-0500");
            daylight.TZOffsetTo = new UTC_Offset("-0400");
            daylight.TimeZoneName = "EDT";

            Event evt = iCal.Create<Event>();
            evt.Summary = "Test event";
            evt.Start = new iCalDateTime(2007, 1, 24, 8, 0, 0, tzid, iCal);
            evt.Duration = TimeSpan.FromHours(1);
            evt.End = new iCalDateTime(2007, 1, 24, 9, 0, 0, tzid, iCal);
            RecurrencePattern recur = new RecurrencePattern("FREQ=MONTHLY;INTERVAL=2;BYDAY=4WE");
            evt.AddRecurrencePattern(recur);

            List<Occurrence> occurrences = evt.GetOccurrences(
                new DateTime(2007, 1, 24),
                new DateTime(2007, 12, 31));

            iCalDateTime[] DateTimes = new iCalDateTime[]
            {                
                new iCalDateTime(2007, 1, 24, 8, 0, 0, tzid, iCal),
                new iCalDateTime(2007, 3, 28, 8, 0, 0, tzid, iCal),
                new iCalDateTime(2007, 5, 23, 8, 0, 0, tzid, iCal),
                new iCalDateTime(2007, 7, 25, 8, 0, 0, tzid, iCal),
                new iCalDateTime(2007, 9, 26, 8, 0, 0, tzid, iCal),
                new iCalDateTime(2007, 11, 28, 8, 0, 0, tzid, iCal)
            };

            for (int i = 0; i < DateTimes.Length; i++)
                Assert.AreEqual(DateTimes[i], occurrences[i].Period.StartTime, "Event should occur on " + DateTimes[i]);

            Assert.AreEqual(
                DateTimes.Length,
                occurrences.Count,
                "There should be exactly " + DateTimes.Length +
                " occurrences; there were " + occurrences.Count);
        }

        /// <summary>
        /// Ensures that, by default, SECONDLY recurrence rules are not allowed.
        /// </summary>
        [Test, Category("Recurrence"), ExpectedException(typeof(EvaluationEngineException))]
        public void RRULE44()
        {
            iCalendar iCal = iCalendar.LoadFromFile(@"Calendars\Recurrence\RRULE44.ics");
            List<Occurrence> occurrences = iCal.GetOccurrences(
                new iCalDateTime(2007, 6, 21, 8, 0, 0, tzid, iCal),
                new iCalDateTime(2007, 7, 21, 8, 0, 0, tzid, iCal));
        }

        /// <summary>
        /// Ensures that the proper behavior occurs when the evaluation
        /// mode is set to adjust automatically for SECONDLY evaluation
        /// </summary>
        [Test, Category("Recurrence")]
        public void RRULE44_1()
        {
            iCalendar iCal = iCalendar.LoadFromFile(@"Calendars\Recurrence\RRULE44.ics");
            iCal.RecurrenceEvaluationMode = RecurrenceEvaluationModeType.AdjustAutomatically;

            List<Occurrence> occurrences = iCal.GetOccurrences(
                new iCalDateTime(2007, 6, 21, 8, 0, 0, tzid, iCal),
                new iCalDateTime(2007, 6, 21, 8, 10, 0, tzid, iCal));

            iCalDateTime[] DateTimes = new iCalDateTime[]
            {
                new iCalDateTime(2007, 6, 21, 8, 0, 0, tzid, iCal),
                new iCalDateTime(2007, 6, 21, 8, 1, 0, tzid, iCal),
                new iCalDateTime(2007, 6, 21, 8, 2, 0, tzid, iCal),
                new iCalDateTime(2007, 6, 21, 8, 3, 0, tzid, iCal),
                new iCalDateTime(2007, 6, 21, 8, 4, 0, tzid, iCal),
                new iCalDateTime(2007, 6, 21, 8, 5, 0, tzid, iCal),
                new iCalDateTime(2007, 6, 21, 8, 6, 0, tzid, iCal),
                new iCalDateTime(2007, 6, 21, 8, 7, 0, tzid, iCal),
                new iCalDateTime(2007, 6, 21, 8, 8, 0, tzid, iCal),
                new iCalDateTime(2007, 6, 21, 8, 9, 0, tzid, iCal),
                new iCalDateTime(2007, 6, 21, 8, 10, 0, tzid, iCal)
            };

            for (int i = 0; i < DateTimes.Length; i++)
                Assert.AreEqual(DateTimes[i], occurrences[i].Period.StartTime, "Event should occur on " + DateTimes[i]);

            Assert.AreEqual(
                DateTimes.Length,
                occurrences.Count,
                "There should be exactly " + DateTimes.Length +
                " occurrences; there were " + occurrences.Count);

        }

        /// <summary>
        /// Ensures that if configured, MINUTELY recurrence rules are not allowed.
        /// </summary>
        [Test, Category("Recurrence"), ExpectedException(typeof(EvaluationEngineException))]
        public void RRULE45()
        {
            iCalendar iCal = iCalendar.LoadFromFile(@"Calendars\Recurrence\RRULE45.ics");
            iCal.RecurrenceRestriction = RecurrenceRestrictionType.RestrictMinutely;
            List<Occurrence> occurrences = iCal.GetOccurrences(
                new iCalDateTime(2007, 6, 21, 8, 0, 0, tzid, iCal),
                new iCalDateTime(2007, 7, 21, 8, 0, 0, tzid, iCal));
        }

        /// <summary>
        /// Ensures that the proper behavior occurs when the evaluation
        /// mode is set to adjust automatically for MINUTELY evaluation
        /// </summary>
        [Test, Category("Recurrence")]
        public void RRULE45_1()
        {
            iCalendar iCal = iCalendar.LoadFromFile(@"Calendars\Recurrence\RRULE45.ics");
            iCal.RecurrenceRestriction = RecurrenceRestrictionType.RestrictMinutely;
            iCal.RecurrenceEvaluationMode = RecurrenceEvaluationModeType.AdjustAutomatically;

            List<Occurrence> occurrences = iCal.GetOccurrences(
                new iCalDateTime(2007, 6, 21, 8, 0, 0, tzid, iCal),
                new iCalDateTime(2007, 6, 21, 12, 0, 0, tzid, iCal));

            iCalDateTime[] DateTimes = new iCalDateTime[]
            {
                new iCalDateTime(2007, 6, 21, 8, 0, 0, tzid, iCal),
                new iCalDateTime(2007, 6, 21, 9, 0, 0, tzid, iCal),
                new iCalDateTime(2007, 6, 21, 10, 0, 0, tzid, iCal),
                new iCalDateTime(2007, 6, 21, 11, 0, 0, tzid, iCal),
                new iCalDateTime(2007, 6, 21, 12, 0, 0, tzid, iCal)
            };

            for (int i = 0; i < DateTimes.Length; i++)
                Assert.AreEqual(DateTimes[i], occurrences[i].Period.StartTime, "Event should occur on " + DateTimes[i]);

            Assert.AreEqual(
                DateTimes.Length,
                occurrences.Count,
                "There should be exactly " + DateTimes.Length +
                " occurrences; there were " + occurrences.Count);

        }

        /// <summary>
        /// Ensures that if configured, HOURLY recurrence rules are not allowed.
        /// </summary>
        [Test, Category("Recurrence"), ExpectedException(typeof(EvaluationEngineException))]
        public void RRULE46()
        {
            iCalendar iCal = iCalendar.LoadFromFile(@"Calendars\Recurrence\RRULE46.ics");
            iCal.RecurrenceRestriction = RecurrenceRestrictionType.RestrictHourly;
            List<Occurrence> occurrences = iCal.GetOccurrences(
                new iCalDateTime(2007, 6, 21, 8, 0, 0, tzid, iCal),
                new iCalDateTime(2007, 7, 21, 8, 0, 0, tzid, iCal));
        }

        /// <summary>
        /// Ensures that the proper behavior occurs when the evaluation
        /// mode is set to adjust automatically for HOURLY evaluation
        /// </summary>
        [Test, Category("Recurrence")]
        public void RRULE46_1()
        {
            iCalendar iCal = iCalendar.LoadFromFile(@"Calendars\Recurrence\RRULE46.ics");
            iCal.RecurrenceRestriction = RecurrenceRestrictionType.RestrictHourly;
            iCal.RecurrenceEvaluationMode = RecurrenceEvaluationModeType.AdjustAutomatically;

            List<Occurrence> occurrences = iCal.GetOccurrences(
                new iCalDateTime(2007, 6, 21, 8, 0, 0, tzid, iCal),
                new iCalDateTime(2007, 6, 25, 8, 0, 0, tzid, iCal));

            iCalDateTime[] DateTimes = new iCalDateTime[]
            {
                new iCalDateTime(2007, 6, 21, 8, 0, 0, tzid, iCal),
                new iCalDateTime(2007, 6, 22, 8, 0, 0, tzid, iCal),
                new iCalDateTime(2007, 6, 23, 8, 0, 0, tzid, iCal),
                new iCalDateTime(2007, 6, 24, 8, 0, 0, tzid, iCal),
                new iCalDateTime(2007, 6, 25, 8, 0, 0, tzid, iCal)
            };

            for (int i = 0; i < DateTimes.Length; i++)
                Assert.AreEqual(DateTimes[i], occurrences[i].Period.StartTime, "Event should occur on " + DateTimes[i]);

            Assert.AreEqual(
                DateTimes.Length,
                occurrences.Count,
                "There should be exactly " + DateTimes.Length +
                " occurrences; there were " + occurrences.Count);

        }

        /// <summary>
        /// Ensures that "off-month" calculation works correctly
        /// </summary>
        [Test, Category("Recurrence")]
        public void RRULE47()
        {
            iCalendar iCal = iCalendar.LoadFromFile(@"Calendars\Recurrence\RRULE47.ics");

            List<Occurrence> occurrences = iCal.GetOccurrences(
                new iCalDateTime(2008, 1, 1, 7, 0, 0, tzid, iCal),
                new iCalDateTime(2008, 2, 29, 7, 0, 0, tzid, iCal));

            iCalDateTime[] DateTimes = new iCalDateTime[]
            {
                new iCalDateTime(2008, 2, 11, 7, 0, 0, tzid, iCal),
                new iCalDateTime(2008, 2, 12, 7, 0, 0, tzid, iCal)
            };

            for (int i = 0; i < DateTimes.Length; i++)
                Assert.AreEqual(DateTimes[i], occurrences[i].Period.StartTime, "Event should occur on " + DateTimes[i]);

            Assert.AreEqual(
                DateTimes.Length,
                occurrences.Count,
                "There should be exactly " + DateTimes.Length +
                " occurrences; there were " + occurrences.Count);

        }

        /// <summary>
        /// Ensures that "off-year" calculation works correctly
        /// </summary>
        [Test, Category("Recurrence")]
        public void RRULE48()
        {
            iCalendar iCal = iCalendar.LoadFromFile(@"Calendars\Recurrence\RRULE48.ics");

            List<Occurrence> occurrences = iCal.GetOccurrences(
                new iCalDateTime(2006, 1, 1, 7, 0, 0, tzid, iCal),
                new iCalDateTime(2007, 1, 31, 7, 0, 0, tzid, iCal));

            iCalDateTime[] DateTimes = new iCalDateTime[]
            {
                new iCalDateTime(2007, 1, 8, 7, 0, 0, tzid, iCal),
                new iCalDateTime(2007, 1, 9, 7, 0, 0, tzid, iCal)
            };

            Assert.AreEqual(
                DateTimes.Length,
                occurrences.Count,
                "There should be exactly " + DateTimes.Length +
                " occurrences; there were " + occurrences.Count);

            for (int i = 0; i < DateTimes.Length; i++)
                Assert.AreEqual(DateTimes[i], occurrences[i].Period.StartTime, "Event should occur on " + DateTimes[i]);
        }

        /// <summary>
        /// Ensures that "off-day" calcuation works correctly
        /// </summary>
        [Test, Category("Recurrence")]
        public void RRULE49()
        {
            iCalendar iCal = iCalendar.LoadFromFile(@"Calendars\Recurrence\RRULE49.ics");

            List<Occurrence> occurrences = iCal.GetOccurrences(
                new iCalDateTime(2007, 4, 11, 7, 0, 0, tzid, iCal),
                new iCalDateTime(2007, 4, 16, 7, 0, 0, tzid, iCal));

            iCalDateTime[] DateTimes = new iCalDateTime[]
            {
                new iCalDateTime(2007, 4, 12, 7, 0, 0, tzid, iCal),
                new iCalDateTime(2007, 4, 15, 7, 0, 0, tzid, iCal)
            };

            Assert.AreEqual(
                DateTimes.Length,
                occurrences.Count,
                "There should be exactly " + DateTimes.Length +
                " occurrences; there were " + occurrences.Count);

            for (int i = 0; i < DateTimes.Length; i++)
                Assert.AreEqual(DateTimes[i], occurrences[i].Period.StartTime, "Event should occur on " + DateTimes[i]);
        }

        /// <summary>
        /// Ensures that "off-hour" calculation works correctly
        /// </summary>
        [Test, Category("Recurrence")]
        public void RRULE50()
        {
            iCalendar iCal = iCalendar.LoadFromFile(@"Calendars\Recurrence\RRULE50.ics");

            List<Occurrence> occurrences = iCal.GetOccurrences(
                new iCalDateTime(2007, 4, 9, 10, 0, 0, tzid, iCal),
                new iCalDateTime(2007, 4, 10, 20, 0, 0, tzid, iCal));

            iCalDateTime[] DateTimes = new iCalDateTime[]
            {
                new iCalDateTime(2007, 4, 10, 1, 0, 0, tzid, iCal),
                new iCalDateTime(2007, 4, 10, 19, 0, 0, tzid, iCal)
            };

            Assert.AreEqual(
                DateTimes.Length,
                occurrences.Count,
                "There should be exactly " + DateTimes.Length +
                " occurrences; there were " + occurrences.Count);


            for (int i = 0; i < DateTimes.Length; i++)
                Assert.AreEqual(DateTimes[i], occurrences[i].Period.StartTime, "Event should occur on " + DateTimes[i]);
        }

        /// <summary>
        /// Ensures that the following recurrence functions properly.
        /// The desired result is "The last Weekend-day of September for the next 10 years."
        /// This specifically tests the BYSETPOS=-1 to accomplish this.
        /// </summary>
        [Test, Category("Recurrence")]
        public void RRULE51()
        {
            iCalendar iCal = iCalendar.LoadFromFile(@"Calendars\Recurrence\RRULE51.ics");

            List<Occurrence> occurrences = iCal.GetOccurrences(
                new iCalDateTime(2009, 1, 1, 0, 0, 0, tzid, iCal),
                new iCalDateTime(2020, 1, 1, 0, 0, 0, tzid, iCal));            

            iCalDateTime[] DateTimes = new iCalDateTime[]
            {
                new iCalDateTime(2009, 9, 27, 5, 30, 0),
                new iCalDateTime(2010, 9, 26, 5, 30, 0),
                new iCalDateTime(2011, 9, 25, 5, 30, 0),
                new iCalDateTime(2012, 9, 30, 5, 30, 0),
                new iCalDateTime(2013, 9, 29, 5, 30, 0),
                new iCalDateTime(2014, 9, 28, 5, 30, 0),
                new iCalDateTime(2015, 9, 27, 5, 30, 0),
                new iCalDateTime(2016, 9, 25, 5, 30, 0),
                new iCalDateTime(2017, 9, 30, 5, 30, 0),
                new iCalDateTime(2018, 9, 30, 5, 30, 0)                
            };

            Assert.AreEqual(
                DateTimes.Length,
                occurrences.Count,
                "There should be exactly " + DateTimes.Length +
                " occurrences; there were " + occurrences.Count);
            
            for (int i = 0; i < DateTimes.Length; i++)
                Assert.AreEqual(DateTimes[i], occurrences[i].Period.StartTime, "Event should occur on " + DateTimes[i]);
        }

        /// <summary>
        /// Ensures that GetOccurrences() always returns a single occurrence
        /// for a non-recurring event.
        /// </summary>
        [Test, Category("Recurrence")]
        public void RRULE52()
        {
            iCalendar iCal = iCalendar.LoadFromFile(@"Calendars\Recurrence\RRULE52.ics");

            List<Occurrence> occurrences = iCal.GetOccurrences(
                new iCalDateTime(2009, 1, 1, 0, 0, 0, tzid, iCal),
                new iCalDateTime(2010, 1, 1, 0, 0, 0, tzid, iCal));

            iCalDateTime[] DateTimes = new iCalDateTime[]
            {
                new iCalDateTime(2009, 9, 27, 5, 30, 0)
            };

            Assert.AreEqual(
                DateTimes.Length,
                occurrences.Count,
                "There should be exactly " + DateTimes.Length +
                " occurrences; there were " + occurrences.Count);

            for (int i = 0; i < DateTimes.Length; i++)
                Assert.AreEqual(DateTimes[i], occurrences[i].Period.StartTime, "Event should occur on " + DateTimes[i]);
        }

        /// <summary>
        /// Ensures that RecurrencePattern.GetNextOccurrence() functions properly for an HOURLY frequency.
        /// </summary>
        [Test, Category("Recurrence")]
        public void RRULE53()
        {
            iCalendar iCal = iCalendar.LoadFromFile(@"Calendars\Recurrence\RRULE53.ics");

            List<Occurrence> occurrences = iCal.GetOccurrences(
                new iCalDateTime(2007, 4, 9, 7, 0, 0),
                new iCalDateTime(2007, 4, 10, 23, 0, 0)
            );

            iCalDateTime[] DateTimes = new iCalDateTime[]
            {
                new iCalDateTime(2007, 4, 9, 7, 0, 0),
                new iCalDateTime(2007, 4, 9, 11, 0, 0),
                new iCalDateTime(2007, 4, 9, 15, 0, 0),
                new iCalDateTime(2007, 4, 9, 19, 0, 0),
                new iCalDateTime(2007, 4, 9, 23, 0, 0),
                new iCalDateTime(2007, 4, 10, 3, 0, 0),
                new iCalDateTime(2007, 4, 10, 7, 0, 0),
                new iCalDateTime(2007, 4, 10, 11, 0, 0),
                new iCalDateTime(2007, 4, 10, 15, 0, 0),
                new iCalDateTime(2007, 4, 10, 19, 0, 0),
                new iCalDateTime(2007, 4, 10, 23, 0, 0)
            };

            Assert.AreEqual(
                DateTimes.Length,
                occurrences.Count,
                "There should be exactly " + DateTimes.Length +
                " occurrences; there were " + occurrences.Count);

            for (int i = 0; i < DateTimes.Length; i++)
                Assert.AreEqual(DateTimes[i], occurrences[i].Period.StartTime, "Event should occur on " + DateTimes[i]);

            // Now, verify that GetNextOccurrence() returns accurate results.
            Assert.AreEqual(1, iCal.Events.Count);
            Event evt = iCal.Events[0];
            Assert.AreEqual(1, evt.RRule.Length);
            RecurrencePattern rp = evt.RRule[0];

            for (int i = 0; i < DateTimes.Length - 1; i++)
            {
                iCalDateTime nextOccurrence = rp.GetNextOccurrence(DateTimes[i]);
                Assert.AreEqual(DateTimes[i + 1], nextOccurrence, "Next occurence should be " + DateTimes[i]);
            }
        }

        /// <summary>
        /// Ensures that RecurrencePattern.GetNextOccurrence() functions properly for an MINUTELY frequency.
        /// </summary>
        [Test, Category("Recurrence")]
        public void RRULE54()
        {
            iCalendar iCal = iCalendar.LoadFromFile(@"Calendars\Recurrence\RRULE54.ics");

            List<Occurrence> occurrences = iCal.GetOccurrences(
                new iCalDateTime(2007, 4, 9, 7, 0, 0),
                new iCalDateTime(2007, 4, 9, 12, 0, 0)
            );

            iCalDateTime[] DateTimes = new iCalDateTime[]
            {
                new iCalDateTime(2007, 4, 9, 7, 0, 0),
                new iCalDateTime(2007, 4, 9, 7, 30, 0),
                new iCalDateTime(2007, 4, 9, 8, 0, 0),
                new iCalDateTime(2007, 4, 9, 8, 30, 0),
                new iCalDateTime(2007, 4, 9, 9, 0, 0),
                new iCalDateTime(2007, 4, 9, 9, 30, 0),
                new iCalDateTime(2007, 4, 9, 10, 0, 0),
                new iCalDateTime(2007, 4, 9, 10, 30, 0),
                new iCalDateTime(2007, 4, 9, 11, 0, 0),
                new iCalDateTime(2007, 4, 9, 11, 30, 0),
                new iCalDateTime(2007, 4, 9, 12, 0, 0),
            };

            Assert.AreEqual(
                DateTimes.Length,
                occurrences.Count,
                "There should be exactly " + DateTimes.Length +
                " occurrences; there were " + occurrences.Count);

            for (int i = 0; i < DateTimes.Length; i++)
                Assert.AreEqual(DateTimes[i], occurrences[i].Period.StartTime, "Event should occur on " + DateTimes[i]);

            // Now, verify that GetNextOccurrence() returns accurate results.
            Assert.AreEqual(1, iCal.Events.Count);
            Event evt = iCal.Events[0];
            Assert.AreEqual(1, evt.RRule.Length);
            RecurrencePattern rp = evt.RRule[0];

            for (int i = 0; i < DateTimes.Length - 1; i++)
            {
                iCalDateTime nextOccurrence = rp.GetNextOccurrence(DateTimes[i]);
                Assert.AreEqual(DateTimes[i + 1], nextOccurrence, "Next occurence should be " + DateTimes[i]);
            }
        }

        /// <summary>
        /// Ensures that RecurrencePattern.GetNextOccurrence() functions properly for an DAILY frequency with an INTERVAL.
        /// </summary>
        [Test, Category("Recurrence")]
        public void RRULE55()
        {
            iCalendar iCal = iCalendar.LoadFromFile(@"Calendars\Recurrence\RRULE55.ics");

            List<Occurrence> occurrences = iCal.GetOccurrences(
                new iCalDateTime(2007, 4, 9, 7, 0, 0),
                new iCalDateTime(2007, 4, 27, 7, 0, 0)
            );

            iCalDateTime[] DateTimes = new iCalDateTime[]
            {
                new iCalDateTime(2007, 4, 9, 7, 0, 0),
                new iCalDateTime(2007, 4, 11, 7, 0, 0),
                new iCalDateTime(2007, 4, 13, 7, 0, 0),
                new iCalDateTime(2007, 4, 15, 7, 0, 0),
                new iCalDateTime(2007, 4, 17, 7, 0, 0),
                new iCalDateTime(2007, 4, 19, 7, 0, 0),
                new iCalDateTime(2007, 4, 21, 7, 0, 0),
                new iCalDateTime(2007, 4, 23, 7, 0, 0),
                new iCalDateTime(2007, 4, 25, 7, 0, 0),
                new iCalDateTime(2007, 4, 27, 7, 0, 0)
            };

            Assert.AreEqual(
                DateTimes.Length,
                occurrences.Count,
                "There should be exactly " + DateTimes.Length +
                " occurrences; there were " + occurrences.Count);

            for (int i = 0; i < DateTimes.Length; i++)
                Assert.AreEqual(DateTimes[i], occurrences[i].Period.StartTime, "Event should occur on " + DateTimes[i]);

            // Now, verify that GetNextOccurrence() returns accurate results.
            Assert.AreEqual(1, iCal.Events.Count);
            Event evt = iCal.Events[0];
            Assert.AreEqual(1, evt.RRule.Length);
            RecurrencePattern rp = evt.RRule[0];

            for (int i = 0; i < DateTimes.Length - 1; i++)
            {
                iCalDateTime nextOccurrence = rp.GetNextOccurrence(DateTimes[i]);
                Assert.AreEqual(DateTimes[i + 1], nextOccurrence, "Next occurence should be " + DateTimes[i]);
            }
        }

        /// <summary>
        /// Ensures that RecurrencePattern.GetNextOccurrence() functions properly for an DAILY frequency with a BYDAY value.
        /// </summary>
        [Test, Category("Recurrence")]
        public void RRULE56()
        {
            iCalendar iCal = iCalendar.LoadFromFile(@"Calendars\Recurrence\RRULE56.ics");

            List<Occurrence> occurrences = iCal.GetOccurrences(
                new iCalDateTime(2007, 9, 10, 7, 0, 0),
                new iCalDateTime(2007, 9, 27, 7, 0, 0)
            );

            iCalDateTime[] DateTimes = new iCalDateTime[]
            {
                new iCalDateTime(2007, 9, 10, 7, 0, 0),
                new iCalDateTime(2007, 9, 13, 7, 0, 0),
                new iCalDateTime(2007, 9, 17, 7, 0, 0),
                new iCalDateTime(2007, 9, 20, 7, 0, 0),
                new iCalDateTime(2007, 9, 24, 7, 0, 0),
                new iCalDateTime(2007, 9, 27, 7, 0, 0)
            };

            Assert.AreEqual(
                DateTimes.Length,
                occurrences.Count,
                "There should be exactly " + DateTimes.Length +
                " occurrences; there were " + occurrences.Count);

            for (int i = 0; i < DateTimes.Length; i++)
                Assert.AreEqual(DateTimes[i], occurrences[i].Period.StartTime, "Event should occur on " + DateTimes[i]);

            // Now, verify that GetNextOccurrence() returns accurate results.
            Assert.AreEqual(1, iCal.Events.Count);
            Event evt = iCal.Events[0];
            Assert.AreEqual(1, evt.RRule.Length);
            RecurrencePattern rp = evt.RRule[0];

            for (int i = 0; i < DateTimes.Length - 1; i++)
            {
                iCalDateTime nextOccurrence = rp.GetNextOccurrence(DateTimes[i]);
                Assert.AreEqual(DateTimes[i + 1], nextOccurrence, "Next occurence should be " + DateTimes[i]);
            }
        }

        /// <summary>
        /// Ensures that RecurrencePattern.GetNextOccurrence() functions properly for a WEEKLY frequency with an INTERVAL.
        /// </summary>
        [Test, Category("Recurrence")]
        public void RRULE57()
        {
            iCalendar iCal = iCalendar.LoadFromFile(@"Calendars\Recurrence\RRULE57.ics");

            List<Occurrence> occurrences = iCal.GetOccurrences(
                new iCalDateTime(2007, 9, 10, 7, 0, 0),
                new iCalDateTime(2007, 12, 31, 11, 59, 59)
            );

            iCalDateTime[] DateTimes = new iCalDateTime[]
            {
                new iCalDateTime(2007, 9, 10, 7, 0, 0),
                new iCalDateTime(2007, 9, 24, 7, 0, 0),
                new iCalDateTime(2007, 10, 8, 7, 0, 0),
                new iCalDateTime(2007, 10, 22, 7, 0, 0),
                new iCalDateTime(2007, 11, 5, 7, 0, 0),
                new iCalDateTime(2007, 11, 19, 7, 0, 0),
                new iCalDateTime(2007, 12, 3, 7, 0, 0),
                new iCalDateTime(2007, 12, 17, 7, 0, 0),
                new iCalDateTime(2007, 12, 31, 7, 0, 0),
            };

            Assert.AreEqual(
                DateTimes.Length,
                occurrences.Count,
                "There should be exactly " + DateTimes.Length +
                " occurrences; there were " + occurrences.Count);

            for (int i = 0; i < DateTimes.Length; i++)
                Assert.AreEqual(DateTimes[i], occurrences[i].Period.StartTime, "Event should occur on " + DateTimes[i]);

            // Now, verify that GetNextOccurrence() returns accurate results.
            Assert.AreEqual(1, iCal.Events.Count);
            Event evt = iCal.Events[0];
            Assert.AreEqual(1, evt.RRule.Length);
            RecurrencePattern rp = evt.RRule[0];

            for (int i = 0; i < DateTimes.Length - 1; i++)
            {
                iCalDateTime nextOccurrence = rp.GetNextOccurrence(DateTimes[i]);
                Assert.AreEqual(DateTimes[i + 1], nextOccurrence, "Next occurence should be " + DateTimes[i]);
            }
        }

        /// <summary>
        /// Ensures that RecurrencePattern.GetNextOccurrence() functions properly for a MONTHLY frequency.
        /// </summary>
        [Test, Category("Recurrence")]
        public void RRULE58()
        {
            iCalendar iCal = iCalendar.LoadFromFile(@"Calendars\Recurrence\RRULE58.ics");

            List<Occurrence> occurrences = iCal.GetOccurrences(
                new iCalDateTime(2007, 9, 10, 7, 0, 0),
                new iCalDateTime(2008, 9, 10, 7, 0, 0)
            );

            iCalDateTime[] DateTimes = new iCalDateTime[]
            {
                new iCalDateTime(2007, 9, 10, 7, 0, 0),
                new iCalDateTime(2007, 10, 10, 7, 0, 0),
                new iCalDateTime(2007, 11, 10, 7, 0, 0),
                new iCalDateTime(2007, 12, 10, 7, 0, 0),
                new iCalDateTime(2008, 1, 10, 7, 0, 0),
                new iCalDateTime(2008, 2, 10, 7, 0, 0),
                new iCalDateTime(2008, 3, 10, 7, 0, 0),
                new iCalDateTime(2008, 4, 10, 7, 0, 0),
                new iCalDateTime(2008, 5, 10, 7, 0, 0),
                new iCalDateTime(2008, 6, 10, 7, 0, 0),
                new iCalDateTime(2008, 7, 10, 7, 0, 0),
                new iCalDateTime(2008, 8, 10, 7, 0, 0),
                new iCalDateTime(2008, 9, 10, 7, 0, 0)
            };

            Assert.AreEqual(
                DateTimes.Length,
                occurrences.Count,
                "There should be exactly " + DateTimes.Length +
                " occurrences; there were " + occurrences.Count);

            for (int i = 0; i < DateTimes.Length; i++)
                Assert.AreEqual(DateTimes[i], occurrences[i].Period.StartTime, "Event should occur on " + DateTimes[i]);

            // Now, verify that GetNextOccurrence() returns accurate results.
            Assert.AreEqual(1, iCal.Events.Count);
            Event evt = iCal.Events[0];
            Assert.AreEqual(1, evt.RRule.Length);
            RecurrencePattern rp = evt.RRule[0];

            for (int i = 0; i < DateTimes.Length - 1; i++)
            {
                iCalDateTime nextOccurrence = rp.GetNextOccurrence(DateTimes[i]);
                Assert.AreEqual(DateTimes[i + 1], nextOccurrence, "Next occurence should be " + DateTimes[i]);
            }
        }

        /// <summary>
        /// Ensures that RecurrencePattern.GetNextOccurrence() functions properly for a YEARLY frequency.
        /// </summary>
        [Test, Category("Recurrence")]
        public void RRULE59()
        {
            iCalendar iCal = iCalendar.LoadFromFile(@"Calendars\Recurrence\RRULE59.ics");

            List<Occurrence> occurrences = iCal.GetOccurrences(
                new iCalDateTime(2007, 9, 10, 7, 0, 0),
                new iCalDateTime(2020, 9, 10, 7, 0, 0)
            );

            iCalDateTime[] DateTimes = new iCalDateTime[]
            {
                new iCalDateTime(2007, 9, 10, 7, 0, 0),
                new iCalDateTime(2008, 9, 10, 7, 0, 0),
                new iCalDateTime(2009, 9, 10, 7, 0, 0),
                new iCalDateTime(2010, 9, 10, 7, 0, 0),
                new iCalDateTime(2011, 9, 10, 7, 0, 0),
                new iCalDateTime(2012, 9, 10, 7, 0, 0),
                new iCalDateTime(2013, 9, 10, 7, 0, 0),
                new iCalDateTime(2014, 9, 10, 7, 0, 0),
                new iCalDateTime(2015, 9, 10, 7, 0, 0),
                new iCalDateTime(2016, 9, 10, 7, 0, 0),
                new iCalDateTime(2017, 9, 10, 7, 0, 0),
                new iCalDateTime(2018, 9, 10, 7, 0, 0),
                new iCalDateTime(2019, 9, 10, 7, 0, 0),
                new iCalDateTime(2020, 9, 10, 7, 0, 0)
            };

            Assert.AreEqual(
                DateTimes.Length,
                occurrences.Count,
                "There should be exactly " + DateTimes.Length +
                " occurrences; there were " + occurrences.Count);

            for (int i = 0; i < DateTimes.Length; i++)
                Assert.AreEqual(DateTimes[i], occurrences[i].Period.StartTime, "Event should occur on " + DateTimes[i]);

            // Now, verify that GetNextOccurrence() returns accurate results.
            Assert.AreEqual(1, iCal.Events.Count);
            Event evt = iCal.Events[0];
            Assert.AreEqual(1, evt.RRule.Length);
            RecurrencePattern rp = evt.RRule[0];

            for (int i = 0; i < DateTimes.Length - 1; i++)
            {
                iCalDateTime nextOccurrence = rp.GetNextOccurrence(DateTimes[i]);
                Assert.AreEqual(DateTimes[i + 1], nextOccurrence, "Next occurence should be " + DateTimes[i]);
            }
        }

        /// <summary>
        /// Tests the iCal holidays downloaded from apple.com
        /// </summary>
        [Test, Category("Recurrence")]
        public void USHOLIDAYS()
        {
            iCalendar iCal = iCalendar.LoadFromFile(@"Calendars\General\USHolidays.ics");

            Assert.IsNotNull(iCal, "iCalendar was not loaded.");
            Hashtable items = new Hashtable();
            items["Christmas"] = new iCalDateTime(2006, 12, 25);
            items["Thanksgiving"] = new iCalDateTime(2006, 11, 23);
            items["Veteran's Day"] = new iCalDateTime(2006, 11, 11);
            items["Halloween"] = new iCalDateTime(2006, 10, 31);
            items["Daylight Saving Time Ends"] = new iCalDateTime(2006, 10, 29);
            items["Columbus Day"] = new iCalDateTime(2006, 10, 9);
            items["Labor Day"] = new iCalDateTime(2006, 9, 4);
            items["Independence Day"] = new iCalDateTime(2006, 7, 4);
            items["Father's Day"] = new iCalDateTime(2006, 6, 18);
            items["Flag Day"] = new iCalDateTime(2006, 6, 14);
            items["John F. Kennedy's Birthday"] = new iCalDateTime(2006, 5, 29);
            items["Memorial Day"] = new iCalDateTime(2006, 5, 29);
            items["Mother's Day"] = new iCalDateTime(2006, 5, 14);
            items["Cinco de Mayo"] = new iCalDateTime(2006, 5, 5);
            items["Earth Day"] = new iCalDateTime(2006, 4, 22);
            items["Easter"] = new iCalDateTime(2006, 4, 16);
            items["Tax Day"] = new iCalDateTime(2006, 4, 15);
            items["Daylight Saving Time Begins"] = new iCalDateTime(2006, 4, 2);
            items["April Fool's Day"] = new iCalDateTime(2006, 4, 1);
            items["St. Patrick's Day"] = new iCalDateTime(2006, 3, 17);
            items["Washington's Birthday"] = new iCalDateTime(2006, 2, 22);
            items["President's Day"] = new iCalDateTime(2006, 2, 20);
            items["Valentine's Day"] = new iCalDateTime(2006, 2, 14);
            items["Lincoln's Birthday"] = new iCalDateTime(2006, 2, 12);
            items["Groundhog Day"] = new iCalDateTime(2006, 2, 2);
            items["Martin Luther King, Jr. Day"] = new iCalDateTime(2006, 1, 16);
            items["New Year's Day"] = new iCalDateTime(2006, 1, 1);

            List<Occurrence> occurrences = iCal.GetOccurrences(
                new iCalDateTime(2006, 1, 1),
                new iCalDateTime(2006, 12, 31));

            Assert.AreEqual(items.Count, occurrences.Count, "The number of holidays did not evaluate correctly.");
            foreach (Occurrence o in occurrences)
            {
                Assert.IsTrue(items.ContainsKey(o.Component.Summary.ToString()), "Holiday text did not match known holidays.");
                Assert.AreEqual(items[o.Component.Summary.ToString()], o.Period.StartTime, "Date/time of holiday '" + o.Component.Summary.ToString() + "' did not match.");
            }
        }

        /// <summary>
        /// Tests recurrence rule parsing in English.
        /// </summary>
        [Test, Category("Recurrence")]
        public void RECURPARSE1()
        {
            iCalendar iCal = new iCalendar();

            Event evt = iCal.Create<Event>();
            evt.Summary = "Test event";
            evt.Start = new iCalDateTime(2006, 10, 1, 9, 0, 0);
            evt.Duration = new TimeSpan(1, 0, 0);
            evt.AddRecurrencePattern(new RecurrencePattern("Every 3rd month on the last tuesday and wednesday"));

            List<Occurrence> occurrences = evt.GetOccurrences(
                new iCalDateTime(2006, 10, 1),
                new iCalDateTime(2007, 4, 30));

            iCalDateTime[] DateTimes = new iCalDateTime[]
            {
                new iCalDateTime(2006, 10, 1, 9, 0, 0),
                new iCalDateTime(2006, 10, 25, 9, 0, 0),
                new iCalDateTime(2006, 10, 31, 9, 0, 0),
                new iCalDateTime(2007, 1, 30, 9, 0, 0),
                new iCalDateTime(2007, 1, 31, 9, 0, 0),
                new iCalDateTime(2007, 4, 24, 9, 0, 0),
                new iCalDateTime(2007, 4, 25, 9, 0, 0)
            };

            for (int i = 0; i < DateTimes.Length; i++)
                Assert.AreEqual(DateTimes[i], occurrences[i].Period.StartTime, "Event should occur on " + DateTimes[i]);

            Assert.AreEqual(
                DateTimes.Length,
                occurrences.Count,
                "There should be exactly " + DateTimes.Length +
                " occurrences; there were " + occurrences.Count);
        }

        /// <summary>
        /// Tests recurrence rule parsing in English.
        /// </summary>
        [Test, Category("Recurrence")]
        public void RECURPARSE2()
        {
            iCalendar iCal = new iCalendar();

            Event evt = iCal.Create<Event>();
            evt.Summary = "Test event";
            evt.Start = new iCalDateTime(2006, 10, 1, 9, 0, 0);
            evt.Duration = new TimeSpan(1, 0, 0);
            evt.AddRecurrencePattern(new RecurrencePattern("Every day at 6:00PM"));

            List<Occurrence> occurrences = evt.GetOccurrences(
                new iCalDateTime(2006, 10, 1),
                new iCalDateTime(2006, 10, 6));

            iCalDateTime[] DateTimes = new iCalDateTime[]
            {
                new iCalDateTime(2006, 10, 1, 9, 0, 0),
                new iCalDateTime(2006, 10, 1, 18, 0, 0),
                new iCalDateTime(2006, 10, 2, 18, 0, 0),
                new iCalDateTime(2006, 10, 3, 18, 0, 0),
                new iCalDateTime(2006, 10, 4, 18, 0, 0),
                new iCalDateTime(2006, 10, 5, 18, 0, 0),
            };

            for (int i = 0; i < DateTimes.Length; i++)
                Assert.AreEqual(DateTimes[i], occurrences[i].Period.StartTime, "Event should occur on " + DateTimes[i]);

            Assert.AreEqual(
                DateTimes.Length,
                occurrences.Count,
                "There should be exactly " + DateTimes.Length +
                " occurrences; there were " + occurrences.Count);
        }

        /// <summary>
        /// Tests recurrence rule parsing in English.
        /// </summary>
        [Test, Category("Recurrence")]
        public void RECURPARSE3()
        {
            iCalendar iCal = new iCalendar();

            Event evt = iCal.Create<Event>();
            evt.Summary = "Test event";
            evt.Start = new iCalDateTime(2006, 1, 1, 9, 0, 0);
            evt.Duration = new TimeSpan(1, 0, 0);
            evt.AddRecurrencePattern(new RecurrencePattern("Every other month, on day 21"));

            List<Occurrence> occurrences = evt.GetOccurrences(
                new iCalDateTime(2006, 1, 1),
                new iCalDateTime(2006, 12, 31));

            iCalDateTime[] DateTimes = new iCalDateTime[]
            {
                new iCalDateTime(2006, 1, 1, 9, 0, 0),
                new iCalDateTime(2006, 1, 21, 9, 0, 0),
                new iCalDateTime(2006, 3, 21, 9, 0, 0),
                new iCalDateTime(2006, 5, 21, 9, 0, 0),
                new iCalDateTime(2006, 7, 21, 9, 0, 0),
                new iCalDateTime(2006, 9, 21, 9, 0, 0),
                new iCalDateTime(2006, 11, 21, 9, 0, 0)                
            };

            for (int i = 0; i < DateTimes.Length; i++)
                Assert.AreEqual(DateTimes[i], occurrences[i].Period.StartTime, "Event should occur on " + DateTimes[i]);

            Assert.AreEqual(
                DateTimes.Length,
                occurrences.Count,
                "There should be exactly " + DateTimes.Length +
                " occurrences; there were " + occurrences.Count);
        }

        /// <summary>
        /// Tests recurrence rule parsing in English.
        /// </summary>
        [Test, Category("Recurrence")]
        public void RECURPARSE4()
        {
            iCalendar iCal = new iCalendar();

            Event evt = iCal.Create<Event>();
            evt.Summary = "Test event";
            evt.Start = new iCalDateTime(2006, 1, 1, 9, 0, 0);
            evt.Duration = new TimeSpan(1, 0, 0);
            evt.AddRecurrencePattern(new RecurrencePattern("Every 10 minutes for 5 occurrences"));

            List<Occurrence> occurrences = evt.GetOccurrences(
                new iCalDateTime(2006, 1, 1),
                new iCalDateTime(2006, 1, 31));

            iCalDateTime[] DateTimes = new iCalDateTime[]
            {
                new iCalDateTime(2006, 1, 1, 9, 0, 0),
                new iCalDateTime(2006, 1, 1, 9, 10, 0),
                new iCalDateTime(2006, 1, 1, 9, 20, 0),
                new iCalDateTime(2006, 1, 1, 9, 30, 0),
                new iCalDateTime(2006, 1, 1, 9, 40, 0)
            };

            for (int i = 0; i < DateTimes.Length; i++)
                Assert.AreEqual(DateTimes[i], occurrences[i].Period.StartTime, "Event should occur on " + DateTimes[i]);

            Assert.AreEqual(
                DateTimes.Length,
                occurrences.Count,
                "There should be exactly " + DateTimes.Length +
                " occurrences; there were " + occurrences.Count);
        }

        /// <summary>
        /// Tests recurrence rule parsing in English.        
        /// </summary>
        [Test, Category("Recurrence")]
        public void RECURPARSE5()
        {
            iCalendar iCal = new iCalendar();

            Event evt = iCal.Create<Event>();
            evt.Summary = "Test event";
            evt.Start = new iCalDateTime(2006, 1, 1, 9, 0, 0);
            evt.Duration = new TimeSpan(1, 0, 0);
            evt.AddRecurrencePattern(new RecurrencePattern("Every 10 minutes until 1/1/2006 9:50"));

            List<Occurrence> occurrences = evt.GetOccurrences(
                new iCalDateTime(2006, 1, 1),
                new iCalDateTime(2006, 1, 31));

            iCalDateTime[] DateTimes = new iCalDateTime[]
            {
                new iCalDateTime(2006, 1, 1, 9, 0, 0),
                new iCalDateTime(2006, 1, 1, 9, 10, 0),
                new iCalDateTime(2006, 1, 1, 9, 20, 0),
                new iCalDateTime(2006, 1, 1, 9, 30, 0),
                new iCalDateTime(2006, 1, 1, 9, 40, 0),
                new iCalDateTime(2006, 1, 1, 9, 50, 0)
            };

            for (int i = 0; i < DateTimes.Length; i++)
                Assert.AreEqual(DateTimes[i], occurrences[i].Period.StartTime, "Event should occur on " + DateTimes[i]);

            Assert.AreEqual(
                DateTimes.Length,
                occurrences.Count,
                "There should be exactly " + DateTimes.Length +
                " occurrences; there were " + occurrences.Count);
        }

        /// <summary>
        /// Tests recurrence rule parsing in English.        
        /// </summary>
        [Test, Category("Recurrence")]
        public void RECURPARSE6()
        {
            iCalendar iCal = new iCalendar();

            Event evt = iCal.Create<Event>();
            evt.Summary = "Test event";
            evt.Start = new iCalDateTime(2006, 1, 1, 9, 0, 0);
            evt.Duration = new TimeSpan(1, 0, 0);
            evt.AddRecurrencePattern(new RecurrencePattern("Every month on the first sunday, at 5:00PM, and at 7:00PM"));

            List<Occurrence> occurrences = evt.GetOccurrences(
                new iCalDateTime(2006, 1, 1),
                new iCalDateTime(2006, 3, 31));

            iCalDateTime[] DateTimes = new iCalDateTime[]
            {
                new iCalDateTime(2006, 1, 1, 9, 0, 0),
                new iCalDateTime(2006, 1, 1, 17, 0, 0),
                new iCalDateTime(2006, 1, 1, 19, 0, 0),
                new iCalDateTime(2006, 2, 5, 17, 0, 0),
                new iCalDateTime(2006, 2, 5, 19, 0, 0),
                new iCalDateTime(2006, 3, 5, 17, 0, 0),
                new iCalDateTime(2006, 3, 5, 19, 0, 0)
            };

            for (int i = 0; i < DateTimes.Length; i++)
                Assert.AreEqual(DateTimes[i], occurrences[i].Period.StartTime, "Event should occur on " + DateTimes[i]);

            Assert.AreEqual(
                DateTimes.Length,
                occurrences.Count,
                "There should be exactly " + DateTimes.Length +
                " occurrences; there were " + occurrences.Count);
        }

        /// <summary>
        /// Ensures that the StartTime and EndTime of periods have
        /// HasTime set to true if the beginning time had HasTime set
        /// to false.
        /// </summary>
        [Test, Category("Recurrence")]
        public void EVALUATE1()
        {
            iCalendar iCal = new iCalendar();
            Event evt = iCal.Create<Event>();
            evt.Summary = "Event summary";

            // Start at midnight, UTC time
            evt.Start = DateTime.SpecifyKind(DateTime.Today, DateTimeKind.Utc);

            evt.AddRecurrencePattern(new RecurrencePattern("FREQ=MINUTELY;INTERVAL=10;COUNT=5"));
            List<Occurrence> occurrences = evt.GetOccurrences(DateTime.Today.AddDays(1), DateTime.Today.AddDays(2));

            foreach (Occurrence o in occurrences)
                Assert.IsTrue(o.Period.StartTime.HasTime, "All recurrences of this event should have a time set.");
        }

        [Test, Category("Recurrence")]
        public void RECURRENCEPATTERN1()
        {
            // NOTE: recurrence patterns are not generally meant to be used directly like this.
            // However, this does make a good test to ensure they behave as they should.
            RecurrencePattern pattern = new
            RecurrencePattern("FREQ=SECONDLY;INTERVAL=10");
            pattern.RestrictionType = RecurrenceRestrictionType.NoRestriction;

			CultureInfo us = CultureInfo.CreateSpecificCulture("en-US");

            DateTime startDate = DateTime.Parse("3/30/08 11:59:40 PM", us);
            DateTime fromDate = DateTime.Parse("3/30/08 11:59:40 PM", us);
			DateTime toDate = DateTime.Parse("3/31/08 12:00:10 AM", us);			

            List<iCalDateTime> occurrences = pattern.Evaluate(startDate, fromDate, toDate);
            Assert.AreEqual(4, occurrences.Count);
			Assert.AreEqual(DateTime.Parse("03/30/08 11:59:40 PM", us), occurrences[0].Value);
			Assert.AreEqual(DateTime.Parse("03/30/08 11:59:50 PM", us), occurrences[1].Value);
			Assert.AreEqual(DateTime.Parse("03/31/08 12:00:00 AM", us), occurrences[2].Value);
			Assert.AreEqual(DateTime.Parse("03/31/08 12:00:10 AM", us), occurrences[3].Value);
        }

        [Test, Category("Recurrence")]
        public void RECURRENCEPATTERN2()
        {
            // NOTE: recurrence patterns are generally not meant to be used directly like this.
            // However, this does make a good test to ensure they behave as they should.
            RecurrencePattern pattern = new RecurrencePattern("FREQ=MINUTELY;INTERVAL=1");

			CultureInfo us = CultureInfo.CreateSpecificCulture("en-US");

            DateTime startDate = DateTime.Parse("3/31/2008 12:00:10 AM", us);
			DateTime fromDate = DateTime.Parse("4/1/2008 10:08:10 AM", us);
			DateTime toDate = DateTime.Parse("4/1/2008 10:43:23 AM", us);			

            List<iCalDateTime> occurrences = pattern.Evaluate(startDate, fromDate, toDate);
            Assert.AreNotEqual(0, occurrences.Count);
        }

        [Test, Category("Recurrence")]
        public void TEST1()
        {
            iCalendar iCal = new iCalendar();
            Event evt = iCal.Create<Event>();
            evt.Summary = "Event summary";
            evt.Start = DateTime.SpecifyKind(DateTime.Today, DateTimeKind.Utc);

            RecurrencePattern recur = new RecurrencePattern();
            evt.AddRecurrencePattern(recur);

            try
            {
                List<Occurrence> occurrences = evt.GetOccurrences(DateTime.Today.AddDays(1), DateTime.Today.AddDays(2));                
                Assert.Fail("An exception should be thrown when evaluating a recurrence with no specified FREQUENCY");
            }
            catch { }
        }

        [Test, Category("Recurrence")]
        public void TEST2()
        {
            iCalendar iCal = new iCalendar();
            Event evt = iCal.Create<Event>();
            evt.Summary = "Event summary";
            evt.Start = DateTime.SpecifyKind(DateTime.Today, DateTimeKind.Utc);

            RecurrencePattern recur = new RecurrencePattern();
            recur.Frequency = FrequencyType.Daily;
            recur.Count = 3;
            recur.ByDay.Add(new DaySpecifier(DayOfWeek.Monday));
            recur.ByDay.Add(new DaySpecifier(DayOfWeek.Wednesday));
            recur.ByDay.Add(new DaySpecifier(DayOfWeek.Friday));
            evt.AddRecurrencePattern(recur);

            DDay.iCal.Serialization.iCalendar.DataTypes.RecurrencePatternSerializer serializer =
                new DDay.iCal.Serialization.iCalendar.DataTypes.RecurrencePatternSerializer(recur);
            Assert.IsTrue(string.Compare(serializer.SerializeToString(), "FREQ=DAILY;COUNT=3;BYDAY=MO,WE,FR") == 0,
                "Serialized recurrence string is incorrect");
        }

        [Test, Category("Recurrence")]
        public void TEST3()
        {
            iCalendar iCal = new iCalendar();
            Event evt = iCal.Create<Event>();

            evt.Start = new iCalDateTime(2008, 10, 18, 10, 30, 0);
            evt.Summary = "Test Event";
            evt.Duration = TimeSpan.FromHours(1);
            evt.AddRecurrencePattern(new RecurrencePattern("RRULE:FREQ=WEEKLY;BYDAY=MO,TU,WE,TH"));

            iCalDateTime doomsdayDate = new iCalDateTime(2010, 12, 31, 10, 30, 0);
            List<Occurrence> allOcc = evt.GetOccurrences(evt.Start, doomsdayDate);

            foreach (Occurrence occ in allOcc)
                Console.WriteLine(occ.Period.StartTime.ToString("d") + " " + occ.Period.StartTime.ToString("t"));
        }

        [Test, Category("Recurrence")]
        public void TEST4()
        {
            RecurrencePattern rpattern = new RecurrencePattern();
            rpattern.ByDay.Add(new DaySpecifier(DayOfWeek.Saturday));
            rpattern.ByDay.Add(new DaySpecifier(DayOfWeek.Sunday));

            rpattern.Frequency = FrequencyType.Weekly;

            DateTime evtStart = new DateTime(2006, 12, 1);
            DateTime evtEnd = new DateTime(2007, 1, 1);

            // Add the exception dates
            List<iCalDateTime> listOfDateTime = rpattern.Evaluate(evtStart, evtStart, evtEnd);
            Assert.AreEqual(10, listOfDateTime.Count);
            Assert.AreEqual(2, listOfDateTime[0].Day);
            Assert.AreEqual(3, listOfDateTime[1].Day);
            Assert.AreEqual(9, listOfDateTime[2].Day);
            Assert.AreEqual(10, listOfDateTime[3].Day);
            Assert.AreEqual(16, listOfDateTime[4].Day);
            Assert.AreEqual(17, listOfDateTime[5].Day);
            Assert.AreEqual(23, listOfDateTime[6].Day);
            Assert.AreEqual(24, listOfDateTime[7].Day);
            Assert.AreEqual(30, listOfDateTime[8].Day);
            Assert.AreEqual(31, listOfDateTime[9].Day);
        }

        [Test, Category("Recurrence")]
        public void TEST5()
        {
            RecurrencePattern rpattern = new RecurrencePattern();
            rpattern.ByHour.Add(8);
            rpattern.ByHour.Add(17);

            rpattern.Frequency = FrequencyType.Daily;

            iCalDateTime lastOccurrence = new iCalDateTime(2006, 10, 1, 11, 15, 0);

            for (int i = 0; i < 20; i++)
            {
                iCalDateTime nextOccurrence = rpattern.GetNextOccurrence(lastOccurrence);
                iCalDateTime expectedNextOccurrence;
                if (lastOccurrence.Hour > 17)
                    expectedNextOccurrence = DateTime.SpecifyKind(lastOccurrence.Date, DateTimeKind.Local).AddDays(1).AddHours(8).AddMinutes(15);
                else
                    expectedNextOccurrence = DateTime.SpecifyKind(lastOccurrence.Date, DateTimeKind.Local).Date.AddHours(17).AddMinutes(15);

                Assert.AreEqual(expectedNextOccurrence, nextOccurrence);
                lastOccurrence = lastOccurrence.AddHours(12);
            }
        }
    }
}
