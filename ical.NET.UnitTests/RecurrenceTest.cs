using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using Ical.Net.DataTypes;
using Ical.Net.Evaluation;
using Ical.Net.Exceptions;
using Ical.Net.Interfaces;
using Ical.Net.Interfaces.Components;
using Ical.Net.Interfaces.DataTypes;
using Ical.Net.Interfaces.Evaluation;
using Ical.Net.Serialization.iCalendar.Serializers.DataTypes;

using Ical.Net.Utility;
using NUnit.Framework;

namespace Ical.Net.UnitTests
{
    [TestFixture]
    public class RecurrenceTest
    {
        private string _tzid;

        [OneTimeSetUp]
        public void InitAll()
        {
            _tzid = "US-Eastern";
        }

        private void EventOccurrenceTest(
            ICalendar cal,
            IDateTime fromDate,
            IDateTime toDate,
            IDateTime[] dateTimes,
            string[] timeZones,
            int eventIndex
        )
        {
            var evt = cal.Events.Skip(eventIndex).First();
            fromDate.AssociatedObject = cal;
            toDate.AssociatedObject = cal;

            var occurrences = evt.GetOccurrences(
                fromDate,
                toDate).OrderBy(o => o.Period.StartTime).ToList();

            Assert.AreEqual(
                dateTimes.Length,
                occurrences.Count,
                "There should be exactly " + dateTimes.Length + " occurrences; there were " + occurrences.Count);

            if (evt.RecurrenceRules.Count > 0)
            {
                Assert.AreEqual(1, evt.RecurrenceRules.Count);
            }

            for (var i = 0; i < dateTimes.Length; i++)
            {
                // Associate each incoming date/time with the calendar.
                dateTimes[i].AssociatedObject = cal;

                var dt = dateTimes[i];
                Assert.AreEqual(dt, occurrences[i].Period.StartTime, "Event should occur on " + dt);
                if (timeZones != null)
                    Assert.AreEqual(timeZones[i], dt.TimeZoneName, "Event " + dt + " should occur in the " + timeZones[i] + " timezone");
            }            
        }

        private void EventOccurrenceTest(
            ICalendar cal,
            IDateTime fromDate,
            IDateTime toDate,
            IDateTime[] dateTimes,
            string[] timeZones
        )
        {
            EventOccurrenceTest(cal, fromDate, toDate, dateTimes, timeZones, 0);
        }

        /// <summary>
        /// See Page 45 of RFC 2445 - RRULE:FREQ=YEARLY;INTERVAL=2;BYMONTH=1;BYDAY=SU;BYHOUR=8,9;BYMINUTE=30
        /// </summary>
        [Test, Category("Recurrence")]
        public void YearlyComplex1()
        {
            var iCal = Calendar.LoadFromStream(new StringReader(IcsFiles.YearlyComplex1))[0];
            ProgramTest.TestCal(iCal);
            var evt = iCal.Events.First();
            var occurrences = evt.GetOccurrences(
                new CalDateTime(2006, 1, 1, _tzid),
                new CalDateTime(2011, 1, 1, _tzid)).OrderBy(o => o.Period.StartTime).ToList();

            IDateTime dt = new CalDateTime(2007, 1, 1, 8, 30, 0, _tzid);
            var i = 0;

            while (dt.Year < 2011)
            {
                if (dt.GreaterThan(evt.Start) &&
                    (dt.Year % 2 == 1) && // Every-other year from 2005
                    (dt.Month == 1) &&
                    (dt.DayOfWeek == DayOfWeek.Sunday))
                {
                    var dt1 = dt.AddHours(1);
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
        public void DailyCount1()
        {
            var iCal = Calendar.LoadFromStream(new StringReader(IcsFiles.DailyCount1))[0];
            EventOccurrenceTest(
                iCal,
                new CalDateTime(2006, 7, 1, _tzid),
                new CalDateTime(2006, 9, 1, _tzid),
                new[]
                {
                    new CalDateTime(2006, 07, 18, 10, 00, 00, _tzid),
                    new CalDateTime(2006, 07, 20, 10, 00, 00, _tzid),
                    new CalDateTime(2006, 07, 22, 10, 00, 00, _tzid),
                    new CalDateTime(2006, 07, 24, 10, 00, 00, _tzid),
                    new CalDateTime(2006, 07, 26, 10, 00, 00, _tzid),
                    new CalDateTime(2006, 07, 28, 10, 00, 00, _tzid),
                    new CalDateTime(2006, 07, 30, 10, 00, 00, _tzid),
                    new CalDateTime(2006, 08, 01, 10, 00, 00, _tzid),
                    new CalDateTime(2006, 08, 03, 10, 00, 00, _tzid),
                    new CalDateTime(2006, 08, 05, 10, 00, 00, _tzid)
                },
                null
            );
        }

        /// <summary>
        /// See Page 118 of RFC 2445 - RRULE:FREQ=DAILY;UNTIL=19971224T000000Z
        /// </summary>
        [Test, Category("Recurrence")]
        public void DailyUntil1()
        {
            var iCal = Calendar.LoadFromStream(new StringReader(IcsFiles.DailyUntil1))[0];
            ProgramTest.TestCal(iCal);
            var evt = iCal.Events.First();

            var occurrences = evt.GetOccurrences(
                new CalDateTime(1997, 9, 1, _tzid),
                new CalDateTime(1998, 1, 1, _tzid)).OrderBy(o => o.Period.StartTime).ToList();

            IDateTime dt = new CalDateTime(1997, 9, 2, 9, 0, 0, _tzid);
            var i = 0;
            while (dt.Year < 1998)
            {
                if (dt.GreaterThanOrEqual(evt.Start) &&
                    dt.LessThan(new CalDateTime(1997, 12, 24, 0, 0, 0, _tzid)))
                {
                    Assert.AreEqual(dt, occurrences[i].Period.StartTime, "Event should occur at " + dt);
                    Assert.IsTrue(
                        (dt.LessThan(new CalDateTime(1997, 10, 26, _tzid)) && dt.TimeZoneName == "US-Eastern") ||
                        (dt.GreaterThan(new CalDateTime(1997, 10, 26, _tzid)) && dt.TimeZoneName == "US-Eastern"),
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
        public void Daily1()
        {
            var iCal = Calendar.LoadFromStream(new StringReader(IcsFiles.Daily1))[0];
            EventOccurrenceTest(
                iCal,
                new CalDateTime(1997, 9, 1, _tzid),
                new CalDateTime(1997, 12, 4, _tzid),
                new[]
                {
                    new CalDateTime(1997, 9, 2, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 9, 4, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 9, 6, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 9, 8, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 9, 10, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 9, 12, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 9, 14, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 9, 16, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 9, 18, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 9, 20, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 9, 22, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 9, 24, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 9, 26, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 9, 28, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 9, 30, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 10, 2, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 10, 4, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 10, 6, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 10, 8, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 10, 10, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 10, 12, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 10, 14, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 10, 16, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 10, 18, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 10, 20, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 10, 22, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 10, 24, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 10, 26, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 10, 28, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 10, 30, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 11, 1, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 11, 3, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 11, 5, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 11, 7, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 11, 9, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 11, 11, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 11, 13, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 11, 15, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 11, 17, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 11, 19, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 11, 21, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 11, 23, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 11, 25, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 11, 27, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 11, 29, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 12, 1, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 12, 3, 9, 0, 0, _tzid)
                },
                new[]
                {
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern"
                }
            );
        }

        /// <summary>
        /// See Page 119 of RFC 2445 - RRULE:FREQ=DAILY;INTERVAL=10;COUNT=5
        /// </summary>
        [Test, Category("Recurrence")]
        public void DailyCount2()
        {
            var iCal = Calendar.LoadFromStream(new StringReader(IcsFiles.DailyCount2))[0];
            EventOccurrenceTest(
                iCal,
                new CalDateTime(1997, 9, 1, _tzid),
                new CalDateTime(1998, 1, 1, _tzid),
                new[]
                {
                    new CalDateTime(1997, 9, 2, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 9, 12, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 9, 22, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 10, 2, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 10, 12, 9, 0, 0, _tzid)
                },
                null
            );
        }

        /// <summary>
        /// See Page 119 of RFC 2445 - RRULE:FREQ=DAILY;UNTIL=20000131T090000Z;BYMONTH=1
        /// </summary>
        [Test, Category("Recurrence")]
        public void ByMonth1()
        {
            var iCal = Calendar.LoadFromStream(new StringReader(IcsFiles.ByMonth1))[0];
            ProgramTest.TestCal(iCal);
            var evt = iCal.Events.First();

            var occurrences = evt.GetOccurrences(
                new CalDateTime(1998, 1, 1, _tzid),
                new CalDateTime(2000, 12, 31, _tzid)).OrderBy(o => o.Period.StartTime).ToList();

            IDateTime dt = new CalDateTime(1998, 1, 1, 9, 0, 0, _tzid);
            var i = 0;
            while (dt.Year < 2001)
            {
                if (dt.GreaterThanOrEqual(evt.Start) &&
                    dt.Month == 1 &&
                    dt.LessThanOrEqual(new CalDateTime(2000, 1, 31, 9, 0, 0, _tzid)))
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
        public void ByMonth2()
        {
            var iCal1 = Calendar.LoadFromStream(new StringReader(IcsFiles.ByMonth1))[0];
            var iCal2 = Calendar.LoadFromStream(new StringReader(IcsFiles.ByMonth2))[0];
            ProgramTest.TestCal(iCal1);
            ProgramTest.TestCal(iCal2);
            IEvent evt1 = (Event)iCal1.Events.First();
            IEvent evt2 = (Event)iCal2.Events.First();

            var evt1Occurrences = evt1.GetOccurrences(new CalDateTime(1997, 9, 1), new CalDateTime(2000, 12, 31)).OrderBy(o => o.Period.StartTime).ToList();
            var evt2Occurrences = evt2.GetOccurrences(new CalDateTime(1997, 9, 1), new CalDateTime(2000, 12, 31)).OrderBy(o => o.Period.StartTime).ToList();
            Assert.IsTrue(evt1Occurrences.Count == evt2Occurrences.Count, "ByMonth1 does not match ByMonth2 as it should");
            for (var i = 0; i < evt1Occurrences.Count; i++)
                Assert.AreEqual(evt1Occurrences[i].Period, evt2Occurrences[i].Period, "PERIOD " + i + " from ByMonth1 (" + evt1Occurrences[i] + ") does not match PERIOD " + i + " from ByMonth2 (" + evt2Occurrences[i] + ")");
        }

        /// <summary>
        /// See Page 119 of RFC 2445 - RRULE:FREQ=WEEKLY;COUNT=10
        /// </summary>
        [Test, Category("Recurrence")]
        public void WeeklyCount1()
        {
            var iCal = Calendar.LoadFromStream(new StringReader(IcsFiles.WeeklyCount1))[0];
            EventOccurrenceTest(
                iCal,
                new CalDateTime(1997, 9, 1, _tzid),
                new CalDateTime(1998, 1, 1, _tzid),
                new[]
                {
                    new CalDateTime(1997, 9, 2, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 9, 9, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 9, 16, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 9, 23, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 9, 30, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 10, 7, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 10, 14, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 10, 21, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 10, 28, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 11, 4, 9, 0, 0, _tzid)
                },
                new[]
                {
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern"
                }
            );
        }

        /// <summary>
        /// See Page 119 of RFC 2445 - RRULE:FREQ=WEEKLY;UNTIL=19971224T000000Z
        /// </summary>
        [Test, Category("Recurrence")]
        public void WeeklyUntil1()
        {
            var iCal = Calendar.LoadFromStream(new StringReader(IcsFiles.WeeklyUntil1))[0];
            EventOccurrenceTest(
                iCal,
                new CalDateTime(1997, 9, 1, _tzid),
                new CalDateTime(1999, 1, 1, _tzid),
                new[]
                {
                    new CalDateTime(1997, 9, 2, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 9, 9, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 9, 16, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 9, 23, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 9, 30, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 10, 7, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 10, 14, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 10, 21, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 10, 28, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 11, 4, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 11, 11, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 11, 18, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 11, 25, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 12, 2, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 12, 9, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 12, 16, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 12, 23, 9, 0, 0, _tzid)
                },
                new[]
                {
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern"
                }
            );
        }

        /// <summary>
        /// See Page 119 of RFC 2445 - RRULE:FREQ=WEEKLY;INTERVAL=2;WKST=SU
        /// </summary>
        [Test, Category("Recurrence")]
        public void WeeklyWkst1()
        {
            var iCal = Calendar.LoadFromStream(new StringReader(IcsFiles.WeeklyWkst1))[0];
            EventOccurrenceTest(
                iCal,
                new CalDateTime(1997, 9, 1, _tzid),
                new CalDateTime(1998, 1, 31, _tzid),
                new[]
                {
                    new CalDateTime(1997, 9, 2, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 9, 16, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 9, 30, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 10, 14, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 10, 28, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 11, 11, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 11, 25, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 12, 9, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 12, 23, 9, 0, 0, _tzid),
                    new CalDateTime(1998, 1, 6, 9, 0, 0, _tzid),
                    new CalDateTime(1998, 1, 20, 9, 0, 0, _tzid)
                },
                new[]
                {
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern"
                }
            );
        }

        /// <summary>
        /// See Page 119 of RFC 2445 - RRULE:FREQ=WEEKLY;UNTIL=19971007T000000Z;WKST=SU;BYDAY=TU,TH
        /// </summary>
        [Test, Category("Recurrence")]
        public void WeeklyUntilWkst1()
        {
            var iCal = Calendar.LoadFromStream(new StringReader(IcsFiles.WeeklyUntilWkst1))[0];
            EventOccurrenceTest(
                iCal,
                new CalDateTime(1997, 9, 1, _tzid),
                new CalDateTime(1999, 1, 1, _tzid),
                new[]
                {
                    new CalDateTime(1997, 9, 2, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 9, 4, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 9, 9, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 9, 11, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 9, 16, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 9, 18, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 9, 23, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 9, 25, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 9, 30, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 10, 2, 9, 0, 0, _tzid)
                },
                null
            );
        }

        /// <summary>
        /// See Page 120 of RFC 2445 - RRULE:FREQ=WEEKLY;COUNT=10;WKST=SU;BYDAY=TU,TH
        /// </summary>
        [Test, Category("Recurrence")]
        public void WeeklyCountWkst1()
        {
            var iCal1 = Calendar.LoadFromStream(new StringReader(IcsFiles.WeeklyUntilWkst1))[0];
            var iCal2 = Calendar.LoadFromStream(new StringReader(IcsFiles.WeeklyCountWkst1))[0];
            ProgramTest.TestCal(iCal1);
            ProgramTest.TestCal(iCal2);
            var evt1 = iCal1.Events.First();
            var evt2 = iCal2.Events.First();

            var evt1Occ = evt1.GetOccurrences(new CalDateTime(1997, 9, 1), new CalDateTime(1999, 1, 1)).OrderBy(o => o.Period.StartTime).ToList();
            var evt2Occ = evt2.GetOccurrences(new CalDateTime(1997, 9, 1), new CalDateTime(1999, 1, 1)).OrderBy(o => o.Period.StartTime).ToList();
            Assert.AreEqual(evt1Occ.Count, evt2Occ.Count, "WeeklyCountWkst1() does not match WeeklyUntilWkst1() as it should");
            for (var i = 0; i < evt1Occ.Count; i++)
                Assert.AreEqual(evt1Occ[i].Period, evt2Occ[i].Period, "PERIOD " + i + " from WeeklyUntilWkst1 (" + evt1Occ[i].Period + ") does not match PERIOD " + i + " from WeeklyCountWkst1 (" + evt2Occ[i].Period + ")");
        }

        /// <summary>
        /// See Page 120 of RFC 2445 - RRULE:FREQ=WEEKLY;INTERVAL=2;UNTIL=19971224T000000Z;WKST=SU;BYDAY=MO,WE,FR
        /// </summary>
        [Test, Category("Recurrence")]
        public void WeeklyUntilWkst2()
        {
            var iCal = Calendar.LoadFromStream(new StringReader(IcsFiles.WeeklyUntilWkst2))[0];
            EventOccurrenceTest(
                iCal,
                new CalDateTime(1996, 1, 1, _tzid),
                new CalDateTime(1999, 1, 1, _tzid),
                new[]
                {
                    new CalDateTime(1997, 9, 2, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 9, 3, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 9, 5, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 9, 15, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 9, 17, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 9, 19, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 9, 29, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 10, 1, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 10, 3, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 10, 13, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 10, 15, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 10, 17, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 10, 27, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 10, 29, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 10, 31, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 11, 10, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 11, 12, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 11, 14, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 11, 24, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 11, 26, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 11, 28, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 12, 8, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 12, 10, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 12, 12, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 12, 22, 9, 0, 0, _tzid)
                },
                new[]
                {
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern"
                }
            );
        }

        /// <summary>
        /// Tests to ensure FREQUENCY=WEEKLY with INTERVAL=2 works when starting evaluation from an "off" week
        /// </summary>
        [Test, Category("Recurrence")]
        public void WeeklyUntilWkst2_1()
        {
            var iCal = Calendar.LoadFromStream(new StringReader(IcsFiles.WeeklyUntilWkst2))[0];
            EventOccurrenceTest(
                iCal,
                new CalDateTime(1997, 9, 9, _tzid),
                new CalDateTime(1999, 1, 1, _tzid),
                new[]
                {
                    new CalDateTime(1997, 9, 15, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 9, 17, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 9, 19, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 9, 29, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 10, 1, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 10, 3, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 10, 13, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 10, 15, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 10, 17, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 10, 27, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 10, 29, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 10, 31, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 11, 10, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 11, 12, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 11, 14, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 11, 24, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 11, 26, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 11, 28, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 12, 8, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 12, 10, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 12, 12, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 12, 22, 9, 0, 0, _tzid)
                },
                new[]
                {
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern"
                }
            );
        }

        /// <summary>
        /// See Page 120 of RFC 2445 - RRULE:FREQ=WEEKLY;INTERVAL=2;COUNT=8;WKST=SU;BYDAY=TU,TH
        /// </summary>
        [Test, Category("Recurrence")]
        public void WeeklyCountWkst2()
        {
            var iCal = Calendar.LoadFromStream(new StringReader(IcsFiles.WeeklyCountWkst2))[0];
            EventOccurrenceTest(
                iCal,
                new CalDateTime(1996, 1, 1, _tzid),
                new CalDateTime(1999, 1, 1, _tzid),
                new[]
                {
                    new CalDateTime(1997, 9, 2, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 9, 4, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 9, 16, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 9, 18, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 9, 30, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 10, 2, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 10, 14, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 10, 16, 9, 0, 0, _tzid)
                },
                null
            );
        }

        /// <summary>
        /// See Page 120 of RFC 2445 - RRULE:FREQ=MONTHLY;COUNT=10;BYDAY=1FR
        /// </summary>
        [Test, Category("Recurrence")]
        public void MonthlyCountByDay1()
        {
            var iCal = Calendar.LoadFromStream(new StringReader(IcsFiles.MonthlyCountByDay1))[0];
            EventOccurrenceTest(
                iCal,
                new CalDateTime(1996, 1, 1, _tzid),
                new CalDateTime(1999, 1, 1, _tzid),
                new[]
                {
                    new CalDateTime(1997, 9, 5, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 10, 3, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 11, 7, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 12, 5, 9, 0, 0, _tzid),
                    new CalDateTime(1998, 1, 2, 9, 0, 0, _tzid),
                    new CalDateTime(1998, 2, 6, 9, 0, 0, _tzid),
                    new CalDateTime(1998, 3, 6, 9, 0, 0, _tzid),
                    new CalDateTime(1998, 4, 3, 9, 0, 0, _tzid),
                    new CalDateTime(1998, 5, 1, 9, 0, 0, _tzid),
                    new CalDateTime(1998, 6, 5, 9, 0, 0, _tzid)
                },
                new[]
                {
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern"
                }
            );
        }

        /// <summary>
        /// See Page 120 of RFC 2445 - RRULE:FREQ=MONTHLY;UNTIL=19971224T000000Z;BYDAY=1FR
        /// </summary>
        [Test, Category("Recurrence")]
        public void MonthlyUntilByDay1()
        {
            var iCal = Calendar.LoadFromStream(new StringReader(IcsFiles.MonthlyUntilByDay1))[0];
            EventOccurrenceTest(
                iCal,
                new CalDateTime(1996, 1, 1, _tzid),
                new CalDateTime(1999, 1, 1, _tzid),
                new[]
                {
                    new CalDateTime(1997, 9, 5, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 10, 3, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 11, 7, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 12, 5, 9, 0, 0, _tzid)
                },
                new[]
                {
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern"
                }
            );
        }

        /// <summary>
        /// See Page 120 of RFC 2445 - RRULE:FREQ=MONTHLY;INTERVAL=2;COUNT=10;BYDAY=1SU,-1SU
        /// </summary>
        [Test, Category("Recurrence")]
        public void MonthlyCountByDay2()
        {
            var iCal = Calendar.LoadFromStream(new StringReader(IcsFiles.MonthlyCountByDay2))[0];
            EventOccurrenceTest(
                iCal,
                new CalDateTime(1996, 1, 1, _tzid),
                new CalDateTime(1999, 1, 1, _tzid),
                new[]
                {
                    new CalDateTime(1997, 9, 7, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 9, 28, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 11, 2, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 11, 30, 9, 0, 0, _tzid),
                    new CalDateTime(1998, 1, 4, 9, 0, 0, _tzid),
                    new CalDateTime(1998, 1, 25, 9, 0, 0, _tzid),
                    new CalDateTime(1998, 3, 1, 9, 0, 0, _tzid),
                    new CalDateTime(1998, 3, 29, 9, 0, 0, _tzid),
                    new CalDateTime(1998, 5, 3, 9, 0, 0, _tzid),
                    new CalDateTime(1998, 5, 31, 9, 0, 0, _tzid)
                },
                new[]
                {
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern"
                }
            );
        }

        /// <summary>
        /// See Page 121 of RFC 2445 - RRULE:FREQ=MONTHLY;COUNT=6;BYDAY=-2MO
        /// </summary>
        [Test, Category("Recurrence")]
        public void MonthlyCountByDay3()
        {
            var iCal = Calendar.LoadFromStream(new StringReader(IcsFiles.MonthlyCountByDay3))[0];
            EventOccurrenceTest(
                iCal,
                new CalDateTime(1996, 1, 1, _tzid),
                new CalDateTime(1999, 1, 1, _tzid),
                new[]
                {
                    new CalDateTime(1997, 9, 22, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 10, 20, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 11, 17, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 12, 22, 9, 0, 0, _tzid),
                    new CalDateTime(1998, 1, 19, 9, 0, 0, _tzid),
                    new CalDateTime(1998, 2, 16, 9, 0, 0, _tzid)
                },
                new[]
                {
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern"
                }
            );
        }

        /// <summary>
        /// See Page 121 of RFC 2445 - RRULE:FREQ=MONTHLY;BYMONTHDAY=-3
        /// </summary>
        [Test, Category("Recurrence")]
        public void ByMonthDay1()
        {
            var iCal = Calendar.LoadFromStream(new StringReader(IcsFiles.ByMonthDay1))[0];
            EventOccurrenceTest(
                iCal,
                new CalDateTime(1996, 1, 1, _tzid),
                new CalDateTime(1998, 3, 1, _tzid),
                new[]
                {
                    new CalDateTime(1997, 9, 28, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 10, 29, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 11, 28, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 12, 29, 9, 0, 0, _tzid),
                    new CalDateTime(1998, 1, 29, 9, 0, 0, _tzid),
                    new CalDateTime(1998, 2, 26, 9, 0, 0, _tzid)
                },
                new[]
                {
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern"
                }
            );
        }

        /// <summary>
        /// See Page 121 of RFC 2445 - RRULE:FREQ=MONTHLY;COUNT=10;BYMONTHDAY=2,15
        /// </summary>
        [Test, Category("Recurrence")]
        public void MonthlyCountByMonthDay1()
        {
            var iCal = Calendar.LoadFromStream(new StringReader(IcsFiles.MonthlyCountByMonthDay1))[0];

            EventOccurrenceTest(
                iCal,
                new CalDateTime(1996, 1, 1, _tzid),
                new CalDateTime(1998, 3, 1, _tzid),
                new[]
                {
                    new CalDateTime(1997, 9, 2, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 9, 15, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 10, 2, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 10, 15, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 11, 2, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 11, 15, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 12, 2, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 12, 15, 9, 0, 0, _tzid),
                    new CalDateTime(1998, 1, 2, 9, 0, 0, _tzid),
                    new CalDateTime(1998, 1, 15, 9, 0, 0, _tzid)
                },
                new[]
                {
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern"
                }
            );
        }

        /// <summary>
        /// See Page 121 of RFC 2445 - RRULE:FREQ=MONTHLY;COUNT=10;BYMONTHDAY=1,-1
        /// </summary>
        [Test, Category("Recurrence")]
        public void MonthlyCountByMonthDay2()
        {
            var iCal = Calendar.LoadFromStream(new StringReader(IcsFiles.MonthlyCountByMonthDay2))[0];
            EventOccurrenceTest(
                iCal,
                new CalDateTime(1996, 1, 1, _tzid),
                new CalDateTime(1998, 3, 1, _tzid),
                new[]
                {
                    new CalDateTime(1997, 9, 30, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 10, 1, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 10, 31, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 11, 1, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 11, 30, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 12, 1, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 12, 31, 9, 0, 0, _tzid),
                    new CalDateTime(1998, 1, 1, 9, 0, 0, _tzid),
                    new CalDateTime(1998, 1, 31, 9, 0, 0, _tzid),
                    new CalDateTime(1998, 2, 1, 9, 0, 0, _tzid)
                },
                new[]
                {
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern"
                }
            );
        }

        /// <summary>
        /// See Page 121 of RFC 2445 - RRULE:FREQ=MONTHLY;INTERVAL=18;COUNT=10;BYMONTHDAY=10,11,12,13,14,15
        /// </summary>
        [Test, Category("Recurrence")]
        public void MonthlyCountByMonthDay3()
        {
            var iCal = Calendar.LoadFromStream(new StringReader(IcsFiles.MonthlyCountByMonthDay3))[0];
            EventOccurrenceTest(
                iCal,
                new CalDateTime(1996, 1, 1, _tzid),
                new CalDateTime(2000, 1, 1, _tzid),
                new[]
                {
                    new CalDateTime(1997, 9, 10, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 9, 11, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 9, 12, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 9, 13, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 9, 14, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 9, 15, 9, 0, 0, _tzid),
                    new CalDateTime(1999, 3, 10, 9, 0, 0, _tzid),
                    new CalDateTime(1999, 3, 11, 9, 0, 0, _tzid),
                    new CalDateTime(1999, 3, 12, 9, 0, 0, _tzid),
                    new CalDateTime(1999, 3, 13, 9, 0, 0, _tzid)
                },
                new[]
                {
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern"
                }
            );
        }

        /// <summary>
        /// See Page 122 of RFC 2445 - RRULE:FREQ=MONTHLY;INTERVAL=2;BYDAY=TU
        /// </summary>
        [Test, Category("Recurrence")]
        public void MonthlyByDay1()
        {
            var iCal = Calendar.LoadFromStream(new StringReader(IcsFiles.MonthlyByDay1))[0];
            EventOccurrenceTest(
                iCal,
                new CalDateTime(1996, 1, 1, _tzid),
                new CalDateTime(1998, 4, 1, _tzid),
                new[]
                {
                    new CalDateTime(1997, 9, 2, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 9, 9, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 9, 16, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 9, 23, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 9, 30, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 11, 4, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 11, 11, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 11, 18, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 11, 25, 9, 0, 0, _tzid),
                    new CalDateTime(1998, 1, 6, 9, 0, 0, _tzid),
                    new CalDateTime(1998, 1, 13, 9, 0, 0, _tzid),
                    new CalDateTime(1998, 1, 20, 9, 0, 0, _tzid),
                    new CalDateTime(1998, 1, 27, 9, 0, 0, _tzid),
                    new CalDateTime(1998, 3, 3, 9, 0, 0, _tzid),
                    new CalDateTime(1998, 3, 10, 9, 0, 0, _tzid),
                    new CalDateTime(1998, 3, 17, 9, 0, 0, _tzid),
                    new CalDateTime(1998, 3, 24, 9, 0, 0, _tzid),
                    new CalDateTime(1998, 3, 31, 9, 0, 0, _tzid)
                },
                new[]
                {
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern"
                }
            );
        }

        /// <summary>
        /// See Page 122 of RFC 2445 - RRULE:FREQ=YEARLY;COUNT=10;BYMONTH=6,7
        /// </summary>
        [Test, Category("Recurrence")]
        public void YearlyByMonth1()
        {
            var iCal = Calendar.LoadFromStream(new StringReader(IcsFiles.YearlyByMonth1))[0];
            EventOccurrenceTest(
                iCal,
                new CalDateTime(1996, 1, 1, _tzid),
                new CalDateTime(2002, 1, 1, _tzid),
                new[]
                {
                    new CalDateTime(1997, 6, 10, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 7, 10, 9, 0, 0, _tzid),
                    new CalDateTime(1998, 6, 10, 9, 0, 0, _tzid),
                    new CalDateTime(1998, 7, 10, 9, 0, 0, _tzid),
                    new CalDateTime(1999, 6, 10, 9, 0, 0, _tzid),
                    new CalDateTime(1999, 7, 10, 9, 0, 0, _tzid),
                    new CalDateTime(2000, 6, 10, 9, 0, 0, _tzid),
                    new CalDateTime(2000, 7, 10, 9, 0, 0, _tzid),
                    new CalDateTime(2001, 6, 10, 9, 0, 0, _tzid),
                    new CalDateTime(2001, 7, 10, 9, 0, 0, _tzid)
                },
                null
            );
        }

        /// <summary>
        /// See Page 122 of RFC 2445 - RRULE:FREQ=YEARLY;INTERVAL=2;COUNT=10;BYMONTH=1,2,3
        /// </summary>
        [Test, Category("Recurrence")]
        public void YearlyCountByMonth1()
        {
            var iCal = Calendar.LoadFromStream(new StringReader(IcsFiles.YearlyCountByMonth1))[0];
            EventOccurrenceTest(
                iCal,
                new CalDateTime(1996, 1, 1, _tzid),
                new CalDateTime(2003, 4, 1, _tzid),
                new[]
                {
                    new CalDateTime(1997, 3, 10, 9, 0, 0, _tzid),
                    new CalDateTime(1999, 1, 10, 9, 0, 0, _tzid),
                    new CalDateTime(1999, 2, 10, 9, 0, 0, _tzid),
                    new CalDateTime(1999, 3, 10, 9, 0, 0, _tzid),
                    new CalDateTime(2001, 1, 10, 9, 0, 0, _tzid),
                    new CalDateTime(2001, 2, 10, 9, 0, 0, _tzid),
                    new CalDateTime(2001, 3, 10, 9, 0, 0, _tzid),
                    new CalDateTime(2003, 1, 10, 9, 0, 0, _tzid),
                    new CalDateTime(2003, 2, 10, 9, 0, 0, _tzid),
                    new CalDateTime(2003, 3, 10, 9, 0, 0, _tzid)
                },
                null
            );
        }

        /// <summary>
        /// See Page 122 of RFC 2445 - RRULE:FREQ=YEARLY;INTERVAL=3;COUNT=10;BYYEARDAY=1,100,200
        /// </summary>
        [Test, Category("Recurrence")]
        public void YearlyCountByYearDay1()
        {
            var iCal = Calendar.LoadFromStream(new StringReader(IcsFiles.YearlyCountByYearDay1))[0];
            EventOccurrenceTest(
                iCal,
                new CalDateTime(1996, 1, 1, _tzid),
                new CalDateTime(2007, 1, 1, _tzid),
                new[]
                {
                    new CalDateTime(1997, 1, 1, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 4, 10, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 7, 19, 9, 0, 0, _tzid),
                    new CalDateTime(2000, 1, 1, 9, 0, 0, _tzid),
                    new CalDateTime(2000, 4, 9, 9, 0, 0, _tzid),
                    new CalDateTime(2000, 7, 18, 9, 0, 0, _tzid),
                    new CalDateTime(2003, 1, 1, 9, 0, 0, _tzid),
                    new CalDateTime(2003, 4, 10, 9, 0, 0, _tzid),
                    new CalDateTime(2003, 7, 19, 9, 0, 0, _tzid),
                    new CalDateTime(2006, 1, 1, 9, 0, 0, _tzid)
                },
                new[]
                {
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern"
                }
            );
        }

        /// <summary>
        /// See Page 123 of RFC 2445 - RRULE:FREQ=YEARLY;BYDAY=20MO
        /// </summary>
        [Test, Category("Recurrence")]
        public void YearlyByDay1()
        {
            var iCal = Calendar.LoadFromStream(new StringReader(IcsFiles.YearlyByDay1))[0];
            EventOccurrenceTest(
                iCal,
                new CalDateTime(1996, 1, 1, _tzid),
                new CalDateTime(1999, 12, 31, _tzid),
                new[]
                {
                    new CalDateTime(1997, 5, 19, 9, 0, 0, _tzid),
                    new CalDateTime(1998, 5, 18, 9, 0, 0, _tzid),
                    new CalDateTime(1999, 5, 17, 9, 0, 0, _tzid)
                },
                null
            );
        }

        /// <summary>
        /// See Page 123 of RFC 2445 - RRULE:FREQ=YEARLY;BYWEEKNO=20;BYDAY=MO
        /// </summary>
        [Test, Category("Recurrence")]
        public void YearlyByWeekNo1()
        {
            var iCal = Calendar.LoadFromStream(new StringReader(IcsFiles.YearlyByWeekNo1))[0];
            EventOccurrenceTest(
                iCal,
                new CalDateTime(1996, 1, 1, _tzid),
                new CalDateTime(1999, 12, 31, _tzid),
                new[]
                {
                    new CalDateTime(1997, 5, 12, 9, 0, 0, _tzid),
                    new CalDateTime(1998, 5, 11, 9, 0, 0, _tzid),
                    new CalDateTime(1999, 5, 17, 9, 0, 0, _tzid)
                },
                null
            );
        }

        /// <summary>
        /// DTSTART;TZID=US-Eastern:19970512T090000
        /// RRULE:FREQ=YEARLY;BYWEEKNO=20
        /// Includes Monday in week 20 (since 19970512 is a Monday)
        /// of each year.
        /// See http://lists.calconnect.org/pipermail/caldeveloper-l/2010-April/000042.html
        /// and related threads for a fairly in-depth discussion about this topic.
        /// </summary>
        [Test, Category("Recurrence")]
        public void YearlyByWeekNo2()
        {
            var iCal = Calendar.LoadFromStream(new StringReader(IcsFiles.YearlyByWeekNo2))[0];
            EventOccurrenceTest(
                iCal,
                new CalDateTime(1996, 1, 1, _tzid),
                new CalDateTime(1999, 12, 31, _tzid),
                new[]
                {
                    new CalDateTime(1997, 5, 12, 9, 0, 0, _tzid),
                    new CalDateTime(1998, 5, 11, 9, 0, 0, _tzid),                    
                    new CalDateTime(1999, 5, 17, 9, 0, 0, _tzid)                    
                },
                null
            );
        }

        /// <summary>
        /// DTSTART;TZID=US-Eastern:20020101T100000
        /// RRULE:FREQ=YEARLY;BYWEEKNO=1
        /// Ensures that 20021230 part of week 1 in 2002.
        /// See http://lists.calconnect.org/pipermail/caldeveloper-l/2010-April/000042.html
        /// and related threads for a fairly in-depth discussion about this topic.
        /// </summary>
        [Test, Category("Recurrence")]
        public void YearlyByWeekNo3()
        {
            var iCal = Calendar.LoadFromStream(new StringReader(IcsFiles.YearlyByWeekNo3))[0];
            EventOccurrenceTest(
                iCal,
                new CalDateTime(2001, 1, 1, _tzid),
                new CalDateTime(2003, 1, 31, _tzid),
                new[]
                {
                    new CalDateTime(2002, 1, 1, 10, 0, 0, _tzid),
                    new CalDateTime(2002, 12, 31, 10, 0, 0, _tzid)
                },
                null
            );
        }

        /// <summary>
        /// RRULE:FREQ=YEARLY;BYWEEKNO=20;BYDAY=MO,TU,WE,TH,FR,SA,SU
        /// Includes every day in week 20.
        /// See http://lists.calconnect.org/pipermail/caldeveloper-l/2010-April/000042.html
        /// and related threads for a fairly in-depth discussion about this topic.
        /// </summary>
        [Test, Category("Recurrence")]
        public void YearlyByWeekNo4()
        {
            var iCal = Calendar.LoadFromStream(new StringReader(IcsFiles.YearlyByWeekNo4))[0];
            EventOccurrenceTest(
                iCal,
                new CalDateTime(1996, 1, 1, _tzid),
                new CalDateTime(1999, 12, 31, _tzid),
                new[]
                {
                    new CalDateTime(1997, 5, 12, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 5, 13, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 5, 14, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 5, 15, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 5, 16, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 5, 17, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 5, 18, 9, 0, 0, _tzid),
                    new CalDateTime(1998, 5, 11, 9, 0, 0, _tzid),
                    new CalDateTime(1998, 5, 12, 9, 0, 0, _tzid),
                    new CalDateTime(1998, 5, 13, 9, 0, 0, _tzid),
                    new CalDateTime(1998, 5, 14, 9, 0, 0, _tzid),
                    new CalDateTime(1998, 5, 15, 9, 0, 0, _tzid),
                    new CalDateTime(1998, 5, 16, 9, 0, 0, _tzid),
                    new CalDateTime(1998, 5, 17, 9, 0, 0, _tzid),
                    new CalDateTime(1999, 5, 17, 9, 0, 0, _tzid),
                    new CalDateTime(1999, 5, 18, 9, 0, 0, _tzid),
                    new CalDateTime(1999, 5, 19, 9, 0, 0, _tzid),
                    new CalDateTime(1999, 5, 20, 9, 0, 0, _tzid),
                    new CalDateTime(1999, 5, 21, 9, 0, 0, _tzid),
                    new CalDateTime(1999, 5, 22, 9, 0, 0, _tzid),
                    new CalDateTime(1999, 5, 23, 9, 0, 0, _tzid)
                },
                null
            );
        }

        /// <summary>
        /// DTSTART;TZID=US-Eastern:20020101T100000
        /// RRULE:FREQ=YEARLY;BYWEEKNO=1;BYDAY=MO,TU,WE,TH,FR,SA,SU
        /// Ensures that 20021230 and 20021231 are in week 1.
        /// Also ensures 20011231 is NOT in the result.
        /// See http://lists.calconnect.org/pipermail/caldeveloper-l/2010-April/000042.html
        /// and related threads for a fairly in-depth discussion about this topic.
        /// </summary>
        [Test, Category("Recurrence")]
        public void YearlyByWeekNo5()
        {
            var iCal = Calendar.LoadFromStream(new StringReader(IcsFiles.YearlyByWeekNo5))[0];
            EventOccurrenceTest(
                iCal,
                new CalDateTime(2001, 1, 1, _tzid),
                new CalDateTime(2003, 1, 31, _tzid),
                new[]
                {
                    new CalDateTime(2002, 1, 1, 10, 0, 0, _tzid),
                    new CalDateTime(2002, 1, 2, 10, 0, 0, _tzid),
                    new CalDateTime(2002, 1, 3, 10, 0, 0, _tzid),
                    new CalDateTime(2002, 1, 4, 10, 0, 0, _tzid),
                    new CalDateTime(2002, 1, 5, 10, 0, 0, _tzid),
                    new CalDateTime(2002, 1, 6, 10, 0, 0, _tzid),                    
                    new CalDateTime(2002, 12, 30, 10, 0, 0, _tzid),
                    new CalDateTime(2002, 12, 31, 10, 0, 0, _tzid),
                    new CalDateTime(2003, 1, 1, 10, 0, 0, _tzid),
                    new CalDateTime(2003, 1, 2, 10, 0, 0, _tzid),
                    new CalDateTime(2003, 1, 3, 10, 0, 0, _tzid),
                    new CalDateTime(2003, 1, 4, 10, 0, 0, _tzid),
                    new CalDateTime(2003, 1, 5, 10, 0, 0, _tzid)
                },
                null
            );
        }

        /// <summary>
        /// See Page 123 of RFC 2445 - RRULE:FREQ=YEARLY;BYMONTH=3;BYDAY=TH
        /// </summary>
        [Test, Category("Recurrence")]
        public void YearlyByMonth2()
        {
            var iCal = Calendar.LoadFromStream(new StringReader(IcsFiles.YearlyByMonth2))[0];
            EventOccurrenceTest(
                iCal,
                new CalDateTime(1996, 1, 1, _tzid),
                new CalDateTime(1999, 12, 31, _tzid),
                new[]
                {
                    new CalDateTime(1997, 3, 13, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 3, 20, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 3, 27, 9, 0, 0, _tzid),
                    new CalDateTime(1998, 3, 5, 9, 0, 0, _tzid),
                    new CalDateTime(1998, 3, 12, 9, 0, 0, _tzid),
                    new CalDateTime(1998, 3, 19, 9, 0, 0, _tzid),
                    new CalDateTime(1998, 3, 26, 9, 0, 0, _tzid),
                    new CalDateTime(1999, 3, 4, 9, 0, 0, _tzid),
                    new CalDateTime(1999, 3, 11, 9, 0, 0, _tzid),
                    new CalDateTime(1999, 3, 18, 9, 0, 0, _tzid),
                    new CalDateTime(1999, 3, 25, 9, 0, 0, _tzid)
                },
                null
            );
        }

        /// <summary>
        /// See Page 123 of RFC 2445 - RRULE:FREQ=YEARLY;BYDAY=TH;BYMONTH=6,7,8
        /// </summary>
        [Test, Category("Recurrence")]
        public void YearlyByMonth3()
        {
            var iCal = Calendar.LoadFromStream(new StringReader(IcsFiles.YearlyByMonth3))[0];
            EventOccurrenceTest(
                iCal,
                new CalDateTime(1996, 1, 1, _tzid),
                new CalDateTime(1999, 12, 31, _tzid),
                new[]
                {
                    new CalDateTime(1997, 6, 5, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 6, 12, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 6, 19, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 6, 26, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 7, 3, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 7, 10, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 7, 17, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 7, 24, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 7, 31, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 8, 7, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 8, 14, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 8, 21, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 8, 28, 9, 0, 0, _tzid),
                    new CalDateTime(1998, 6, 4, 9, 0, 0, _tzid),
                    new CalDateTime(1998, 6, 11, 9, 0, 0, _tzid),
                    new CalDateTime(1998, 6, 18, 9, 0, 0, _tzid),
                    new CalDateTime(1998, 6, 25, 9, 0, 0, _tzid),
                    new CalDateTime(1998, 7, 2, 9, 0, 0, _tzid),
                    new CalDateTime(1998, 7, 9, 9, 0, 0, _tzid),
                    new CalDateTime(1998, 7, 16, 9, 0, 0, _tzid),
                    new CalDateTime(1998, 7, 23, 9, 0, 0, _tzid),
                    new CalDateTime(1998, 7, 30, 9, 0, 0, _tzid),
                    new CalDateTime(1998, 8, 6, 9, 0, 0, _tzid),
                    new CalDateTime(1998, 8, 13, 9, 0, 0, _tzid),
                    new CalDateTime(1998, 8, 20, 9, 0, 0, _tzid),
                    new CalDateTime(1998, 8, 27, 9, 0, 0, _tzid),
                    new CalDateTime(1999, 6, 3, 9, 0, 0, _tzid),
                    new CalDateTime(1999, 6, 10, 9, 0, 0, _tzid),
                    new CalDateTime(1999, 6, 17, 9, 0, 0, _tzid),
                    new CalDateTime(1999, 6, 24, 9, 0, 0, _tzid),
                    new CalDateTime(1999, 7, 1, 9, 0, 0, _tzid),
                    new CalDateTime(1999, 7, 8, 9, 0, 0, _tzid),
                    new CalDateTime(1999, 7, 15, 9, 0, 0, _tzid),
                    new CalDateTime(1999, 7, 22, 9, 0, 0, _tzid),
                    new CalDateTime(1999, 7, 29, 9, 0, 0, _tzid),
                    new CalDateTime(1999, 8, 5, 9, 0, 0, _tzid),
                    new CalDateTime(1999, 8, 12, 9, 0, 0, _tzid),
                    new CalDateTime(1999, 8, 19, 9, 0, 0, _tzid),
                    new CalDateTime(1999, 8, 26, 9, 0, 0, _tzid)
                },
                null
            );
        }

        /// <summary>
        /// See Page 123 of RFC 2445:
        /// EXDATE;TZID=US-Eastern:19970902T090000
        /// RRULE:FREQ=MONTHLY;BYDAY=FR;BYMONTHDAY=13
        /// </summary>
        [Test, Category("Recurrence")]
        public void MonthlyByMonthDay1()
        {
            var iCal = Calendar.LoadFromStream(new StringReader(IcsFiles.MonthlyByMonthDay1))[0];
            EventOccurrenceTest(
                iCal,
                new CalDateTime(1996, 1, 1, _tzid),
                new CalDateTime(2000, 12, 31, _tzid),
                new[]
                {
                    new CalDateTime(1998, 2, 13, 9, 0, 0, _tzid),
                    new CalDateTime(1998, 3, 13, 9, 0, 0, _tzid),
                    new CalDateTime(1998, 11, 13, 9, 0, 0, _tzid),
                    new CalDateTime(1999, 8, 13, 9, 0, 0, _tzid),
                    new CalDateTime(2000, 10, 13, 9, 0, 0, _tzid)
                },
                new[]
                {
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern"
                }
            );
        }

        /// <summary>
        /// See Page 124 of RFC 2445 - RRULE:FREQ=MONTHLY;BYDAY=SA;BYMONTHDAY=7,8,9,10,11,12,13
        /// </summary>
        [Test, Category("Recurrence")]
        public void MonthlyByMonthDay2()
        {
            var iCal = Calendar.LoadFromStream(new StringReader(IcsFiles.MonthlyByMonthDay2))[0];
            EventOccurrenceTest(
                iCal,
                new CalDateTime(1996, 1, 1, _tzid),
                new CalDateTime(1998, 6, 30, _tzid),
                new[]
                {
                    new CalDateTime(1997, 9, 13, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 10, 11, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 11, 8, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 12, 13, 9, 0, 0, _tzid),
                    new CalDateTime(1998, 1, 10, 9, 0, 0, _tzid),
                    new CalDateTime(1998, 2, 7, 9, 0, 0, _tzid),
                    new CalDateTime(1998, 3, 7, 9, 0, 0, _tzid),
                    new CalDateTime(1998, 4, 11, 9, 0, 0, _tzid),
                    new CalDateTime(1998, 5, 9, 9, 0, 0, _tzid),
                    new CalDateTime(1998, 6, 13, 9, 0, 0, _tzid)
                },
                new[]
                {
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern"
                }
            );
        }

        /// <summary>
        /// See Page 124 of RFC 2445 - RRULE:FREQ=YEARLY;INTERVAL=4;BYMONTH=11;BYDAY=TU;BYMONTHDAY=2,3,4,5,6,7,8
        /// </summary>
        [Test, Category("Recurrence")]
        public void YearlyByMonthDay1()
        {
            var iCal = Calendar.LoadFromStream(new StringReader(IcsFiles.YearlyByMonthDay1))[0];
            EventOccurrenceTest(
                iCal,
                new CalDateTime(1996, 1, 1, _tzid),
                new CalDateTime(2004, 12, 31, _tzid),
                new[]
                {
                    new CalDateTime(1996, 11, 5, 9, 0, 0, _tzid),
                    new CalDateTime(2000, 11, 7, 9, 0, 0, _tzid),
                    new CalDateTime(2004, 11, 2, 9, 0, 0, _tzid)
                },
                null
            );
        }

        /// <summary>
        /// See Page 124 of RFC 2445 - RRULE:FREQ=MONTHLY;COUNT=3;BYDAY=TU,WE,TH;BYSETPOS=3
        /// </summary>
        [Test, Category("Recurrence")]
        public void MonthlyBySetPos1()
        {
            var iCal = Calendar.LoadFromStream(new StringReader(IcsFiles.MonthlyBySetPos1))[0];
            EventOccurrenceTest(
                iCal,
                new CalDateTime(1996, 1, 1, _tzid),
                new CalDateTime(2004, 12, 31, _tzid),
                new[]
                {
                    new CalDateTime(1997, 9, 4, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 10, 7, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 11, 6, 9, 0, 0, _tzid)
                },
                new[]
                {
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern"
                }
            );
        }

        /// <summary>
        /// See Page 124 of RFC 2445 - RRULE:FREQ=MONTHLY;BYDAY=MO,TU,WE,TH,FR;BYSETPOS=-2
        /// </summary>
        [Test, Category("Recurrence")]
        public void MonthlyBySetPos2()
        {
            var iCal = Calendar.LoadFromStream(new StringReader(IcsFiles.MonthlyBySetPos2))[0];
            EventOccurrenceTest(
                iCal,
                new CalDateTime(1996, 1, 1, _tzid),
                new CalDateTime(1998, 3, 31, _tzid),
                new[]
                {
                    new CalDateTime(1997, 9, 29, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 10, 30, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 11, 27, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 12, 30, 9, 0, 0, _tzid),
                    new CalDateTime(1998, 1, 29, 9, 0, 0, _tzid),
                    new CalDateTime(1998, 2, 26, 9, 0, 0, _tzid),
                    new CalDateTime(1998, 3, 30, 9, 0, 0, _tzid)
                },
                new[]
                {
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern",
                    "US-Eastern"
                }
            );
        }

        /// <summary>
        /// See Page 125 of RFC 2445 - RRULE:FREQ=HOURLY;INTERVAL=3;UNTIL=19970902T170000Z        
        /// FIXME: The UNTIL time on this item has been altered to 19970902T190000Z to
        /// match the local EDT time occurrence of 3:00pm.  Is the RFC example incorrect?
        /// </summary>
        [Test, Category("Recurrence")]
        public void HourlyUntil1()
        {
            var iCal = Calendar.LoadFromStream(new StringReader(IcsFiles.HourlyUntil1))[0];
            EventOccurrenceTest(
                iCal,
                new CalDateTime(1996, 1, 1, _tzid),
                new CalDateTime(1998, 3, 31, _tzid),
                new[]
                {
                    new CalDateTime(1997, 9, 2, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 9, 2, 12, 0, 0, _tzid),
                    new CalDateTime(1997, 9, 2, 15, 0, 0, _tzid)
                },
                null
            );
        }

        /// <summary>
        /// See Page 125 of RFC 2445 - RRULE:FREQ=MINUTELY;INTERVAL=15;COUNT=6
        /// </summary>
        [Test, Category("Recurrence")]
        public void MinutelyCount1()
        {
            var iCal = Calendar.LoadFromStream(new StringReader(IcsFiles.MinutelyCount1))[0];
            EventOccurrenceTest(
                iCal,
                new CalDateTime(1997, 9, 2, _tzid),
                new CalDateTime(1997, 9, 3, _tzid),
                new[]
                {
                    new CalDateTime(1997, 9, 2, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 9, 2, 9, 15, 0, _tzid),
                    new CalDateTime(1997, 9, 2, 9, 30, 0, _tzid),
                    new CalDateTime(1997, 9, 2, 9, 45, 0, _tzid),
                    new CalDateTime(1997, 9, 2, 10, 0, 0, _tzid),
                    new CalDateTime(1997, 9, 2, 10, 15, 0, _tzid)
                },
                null
            );
        }

        /// <summary>
        /// See Page 125 of RFC 2445 - RRULE:FREQ=MINUTELY;INTERVAL=90;COUNT=4
        /// </summary>
        [Test, Category("Recurrence")]
        public void MinutelyCount2()
        {
            var iCal = Calendar.LoadFromStream(new StringReader(IcsFiles.MinutelyCount2))[0];
            EventOccurrenceTest(
                iCal,
                new CalDateTime(1996, 1, 1, _tzid),
                new CalDateTime(1998, 12, 31, _tzid),
                new[]
                {
                    new CalDateTime(1997, 9, 2, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 9, 2, 10, 30, 0, _tzid),
                    new CalDateTime(1997, 9, 2, 12, 0, 0, _tzid),
                    new CalDateTime(1997, 9, 2, 13, 30, 0, _tzid)
                },
                null
            );
        }

        /// <summary>
        /// See https://sourceforge.net/projects/dday-ical/forums/forum/656447/topic/3827441
        /// </summary>
        [Test, Category("Recurrence")]
        public void MinutelyCount3()
        {
            var iCal = Calendar.LoadFromStream(new StringReader(IcsFiles.MinutelyCount3))[0];
            EventOccurrenceTest(
                iCal,
                new CalDateTime(2010, 8, 27, _tzid),
                new CalDateTime(2010, 8, 28, _tzid),
                new[]
                {
                    new CalDateTime(2010, 8, 27, 11, 0, 0, _tzid),
                    new CalDateTime(2010, 8, 27, 11, 1, 0, _tzid),
                    new CalDateTime(2010, 8, 27, 11, 2, 0, _tzid),
                    new CalDateTime(2010, 8, 27, 11, 3, 0, _tzid),
                    new CalDateTime(2010, 8, 27, 11, 4, 0, _tzid),
                    new CalDateTime(2010, 8, 27, 11, 5, 0, _tzid),
                    new CalDateTime(2010, 8, 27, 11, 6, 0, _tzid),
                    new CalDateTime(2010, 8, 27, 11, 7, 0, _tzid),
                    new CalDateTime(2010, 8, 27, 11, 8, 0, _tzid),
                    new CalDateTime(2010, 8, 27, 11, 9, 0, _tzid)
                },
                null
            );
        }

        /// <summary>
        /// See https://sourceforge.net/projects/dday-ical/forums/forum/656447/topic/3827441
        /// </summary>
        [Test, Category("Recurrence")]
        public void MinutelyCount4()
        {
            var iCal = Calendar.LoadFromStream(new StringReader(IcsFiles.MinutelyCount4))[0];
            EventOccurrenceTest(
                iCal,
                new CalDateTime(2010, 8, 27, _tzid),
                new CalDateTime(2010, 8, 28, _tzid),
                new[]
                {
                    new CalDateTime(2010, 8, 27, 11, 0, 0, _tzid),
                    new CalDateTime(2010, 8, 27, 11, 7, 0, _tzid),
                    new CalDateTime(2010, 8, 27, 11, 14, 0, _tzid),
                    new CalDateTime(2010, 8, 27, 11, 21, 0, _tzid),
                    new CalDateTime(2010, 8, 27, 11, 28, 0, _tzid),
                    new CalDateTime(2010, 8, 27, 11, 35, 0, _tzid),
                    new CalDateTime(2010, 8, 27, 11, 42, 0, _tzid),
                    new CalDateTime(2010, 8, 27, 11, 49, 0, _tzid),
                    new CalDateTime(2010, 8, 27, 11, 56, 0, _tzid),
                    new CalDateTime(2010, 8, 27, 12, 3, 0, _tzid)
                },
                null
            );
        }

        /// <summary>
        /// See Page 125 of RFC 2445 - RRULE:FREQ=DAILY;BYHOUR=9,10,11,12,13,14,15,16;BYMINUTE=0,20,40
        /// </summary>
        [Test, Category("Recurrence")]
        public void DailyByHourMinute1()
        {
            var iCal = Calendar.LoadFromStream(new StringReader(IcsFiles.DailyByHourMinute1))[0];
            EventOccurrenceTest(
                iCal,
                new CalDateTime(1997, 9, 2, _tzid),
                new CalDateTime(1997, 9, 4, _tzid),
                new[]
                {
                    new CalDateTime(1997, 9, 2, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 9, 2, 9, 20, 0, _tzid),
                    new CalDateTime(1997, 9, 2, 9, 40, 0, _tzid),
                    new CalDateTime(1997, 9, 2, 10, 0, 0, _tzid),
                    new CalDateTime(1997, 9, 2, 10, 20, 0, _tzid),
                    new CalDateTime(1997, 9, 2, 10, 40, 0, _tzid),
                    new CalDateTime(1997, 9, 2, 11, 0, 0, _tzid),
                    new CalDateTime(1997, 9, 2, 11, 20, 0, _tzid),
                    new CalDateTime(1997, 9, 2, 11, 40, 0, _tzid),
                    new CalDateTime(1997, 9, 2, 12, 0, 0, _tzid),
                    new CalDateTime(1997, 9, 2, 12, 20, 0, _tzid),
                    new CalDateTime(1997, 9, 2, 12, 40, 0, _tzid),
                    new CalDateTime(1997, 9, 2, 13, 0, 0, _tzid),
                    new CalDateTime(1997, 9, 2, 13, 20, 0, _tzid),
                    new CalDateTime(1997, 9, 2, 13, 40, 0, _tzid),
                    new CalDateTime(1997, 9, 2, 14, 0, 0, _tzid),
                    new CalDateTime(1997, 9, 2, 14, 20, 0, _tzid),
                    new CalDateTime(1997, 9, 2, 14, 40, 0, _tzid),
                    new CalDateTime(1997, 9, 2, 15, 0, 0, _tzid),
                    new CalDateTime(1997, 9, 2, 15, 20, 0, _tzid),
                    new CalDateTime(1997, 9, 2, 15, 40, 0, _tzid),
                    new CalDateTime(1997, 9, 2, 16, 0, 0, _tzid),
                    new CalDateTime(1997, 9, 2, 16, 20, 0, _tzid),
                    new CalDateTime(1997, 9, 2, 16, 40, 0, _tzid),
                    new CalDateTime(1997, 9, 3, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 9, 3, 9, 20, 0, _tzid),
                    new CalDateTime(1997, 9, 3, 9, 40, 0, _tzid),
                    new CalDateTime(1997, 9, 3, 10, 0, 0, _tzid),
                    new CalDateTime(1997, 9, 3, 10, 20, 0, _tzid),
                    new CalDateTime(1997, 9, 3, 10, 40, 0, _tzid),
                    new CalDateTime(1997, 9, 3, 11, 0, 0, _tzid),
                    new CalDateTime(1997, 9, 3, 11, 20, 0, _tzid),
                    new CalDateTime(1997, 9, 3, 11, 40, 0, _tzid),
                    new CalDateTime(1997, 9, 3, 12, 0, 0, _tzid),
                    new CalDateTime(1997, 9, 3, 12, 20, 0, _tzid),
                    new CalDateTime(1997, 9, 3, 12, 40, 0, _tzid),
                    new CalDateTime(1997, 9, 3, 13, 0, 0, _tzid),
                    new CalDateTime(1997, 9, 3, 13, 20, 0, _tzid),
                    new CalDateTime(1997, 9, 3, 13, 40, 0, _tzid),
                    new CalDateTime(1997, 9, 3, 14, 0, 0, _tzid),
                    new CalDateTime(1997, 9, 3, 14, 20, 0, _tzid),
                    new CalDateTime(1997, 9, 3, 14, 40, 0, _tzid),
                    new CalDateTime(1997, 9, 3, 15, 0, 0, _tzid),
                    new CalDateTime(1997, 9, 3, 15, 20, 0, _tzid),
                    new CalDateTime(1997, 9, 3, 15, 40, 0, _tzid),
                    new CalDateTime(1997, 9, 3, 16, 0, 0, _tzid),
                    new CalDateTime(1997, 9, 3, 16, 20, 0, _tzid),
                    new CalDateTime(1997, 9, 3, 16, 40, 0, _tzid)
                },
                null
            );
        }

        /// <summary>
        /// See Page 125 of RFC 2445 - RRULE:FREQ=MINUTELY;INTERVAL=20;BYHOUR=9,10,11,12,13,14,15,16
        /// </summary>
        [Test, Category("Recurrence")]
        public void MinutelyByHour1()
        {
            var iCal1 = Calendar.LoadFromStream(new StringReader(IcsFiles.DailyByHourMinute1))[0];
            var iCal2 = Calendar.LoadFromStream(new StringReader(IcsFiles.MinutelyByHour1))[0];
            ProgramTest.TestCal(iCal1);
            ProgramTest.TestCal(iCal2);
            var evt1 = iCal1.Events.First();
            var evt2 = iCal2.Events.First();

            var evt1Occ = evt1.GetOccurrences(new CalDateTime(1997, 9, 1, _tzid), new CalDateTime(1997, 9, 3, _tzid)).OrderBy(o => o.Period.StartTime).ToList();
            var evt2Occ = evt2.GetOccurrences(new CalDateTime(1997, 9, 1, _tzid), new CalDateTime(1997, 9, 3, _tzid)).OrderBy(o => o.Period.StartTime).ToList();
            Assert.IsTrue(evt1Occ.Count == evt2Occ.Count, "MinutelyByHour1() does not match DailyByHourMinute1() as it should");
            for (var i = 0; i < evt1Occ.Count; i++)
                Assert.AreEqual(evt1Occ[i].Period, evt2Occ[i].Period, "PERIOD " + i + " from DailyByHourMinute1 (" + evt1Occ[i].Period + ") does not match PERIOD " + i + " from MinutelyByHour1 (" + evt2Occ[i].Period + ")");
        }

        /// <summary>
        /// See Page 125 of RFC 2445 - RRULE:FREQ=WEEKLY;INTERVAL=2;COUNT=4;BYDAY=TU,SU;WKST=MO
        /// </summary>
        [Test, Category("Recurrence")]
        public void WeeklyCountWkst3()
        {
            var iCal = Calendar.LoadFromStream(new StringReader(IcsFiles.WeeklyCountWkst3))[0];
            EventOccurrenceTest(
                iCal,
                new CalDateTime(1996, 1, 1, _tzid),
                new CalDateTime(1998, 12, 31, _tzid),
                new[]
                {
                    new CalDateTime(1997, 8, 5, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 8, 10, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 8, 19, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 8, 24, 9, 0, 0, _tzid)
                },
                null
            );
        }

        /// <summary>
        /// See Page 125 of RFC 2445 - RRULE:FREQ=WEEKLY;INTERVAL=2;COUNT=4;BYDAY=TU,SU;WKST=SU
        /// This is the same as WeeklyCountWkst3, except WKST is SU, which changes the results.
        /// </summary>
        [Test, Category("Recurrence")]
        public void WeeklyCountWkst4()
        {
            var iCal = Calendar.LoadFromStream(new StringReader(IcsFiles.WeeklyCountWkst4))[0];
            EventOccurrenceTest(
                iCal,
                new CalDateTime(1996, 1, 1, _tzid),
                new CalDateTime(1998, 12, 31, _tzid),
                new[]
                {
                    new CalDateTime(1997, 8, 5, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 8, 17, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 8, 19, 9, 0, 0, _tzid),
                    new CalDateTime(1997, 8, 31, 9, 0, 0, _tzid)
                },
                null
            );
        }

        /// <summary>
        /// Tests WEEKLY Frequencies to ensure that those with an INTERVAL > 1
        /// are correctly handled.  See Bug #1741093 - WEEKLY frequency eval behaves strangely.
        /// </summary>
        [Test, Category("Recurrence")]
        public void Bug1741093()
        {
            var iCal = Calendar.LoadFromStream(new StringReader(IcsFiles.Bug1741093))[0];
            EventOccurrenceTest(
                iCal,
                new CalDateTime(2007, 7, 1, _tzid),
                new CalDateTime(2007, 8, 1, _tzid),
                new[]
                {
                    new CalDateTime(2007, 7, 2, 8, 0, 0, _tzid),
                    new CalDateTime(2007, 7, 3, 8, 0, 0, _tzid),
                    new CalDateTime(2007, 7, 4, 8, 0, 0, _tzid),
                    new CalDateTime(2007, 7, 5, 8, 0, 0, _tzid),
                    new CalDateTime(2007, 7, 6, 8, 0, 0, _tzid),
                    new CalDateTime(2007, 7, 16, 8, 0, 0, _tzid),
                    new CalDateTime(2007, 7, 17, 8, 0, 0, _tzid),
                    new CalDateTime(2007, 7, 18, 8, 0, 0, _tzid),
                    new CalDateTime(2007, 7, 19, 8, 0, 0, _tzid),
                    new CalDateTime(2007, 7, 20, 8, 0, 0, _tzid),
                    new CalDateTime(2007, 7, 30, 8, 0, 0, _tzid),
                    new CalDateTime(2007, 7, 31, 8, 0, 0, _tzid)
                },
                null
            );
        }

        /// <summary>
        /// Ensures that, by default, SECONDLY recurrence rules are not allowed.
        /// </summary>
        [Test, Category("Recurrence")]
        public void Secondly1()
        {
            var evt = new AutoResetEvent(false);

            try
            {
                var iCal = Calendar.LoadFromStream(new StringReader(IcsFiles.Secondly1))[0];
                var occurrences = iCal.GetOccurrences(new CalDateTime(2007, 6, 21, 8, 0, 0, _tzid), new CalDateTime(2007, 7, 21, 8, 0, 0, _tzid));
            }
            catch (EvaluationEngineException)
            {
                evt.Set();
            }

            Assert.IsTrue(evt.WaitOne(2000), "Evaluation engine should have failed.");
        }

        /// <summary>
        /// Ensures that the proper behavior occurs when the evaluation
        /// mode is set to adjust automatically for SECONDLY evaluation
        /// </summary>
        [Test, Category("Recurrence")]
        public void Secondly1_1()
        {
            var iCal = Calendar.LoadFromStream(new StringReader(IcsFiles.Secondly1))[0];
            iCal.RecurrenceEvaluationMode = RecurrenceEvaluationModeType.AdjustAutomatically;

            EventOccurrenceTest(
                iCal,
                new CalDateTime(2007, 6, 21, 8, 0, 0, _tzid),
                new CalDateTime(2007, 6, 21, 8, 10, 1, _tzid), // End period is exclusive, not inclusive.
                new[]
                {
                    new CalDateTime(2007, 6, 21, 8, 0, 0, _tzid),
                    new CalDateTime(2007, 6, 21, 8, 1, 0, _tzid),
                    new CalDateTime(2007, 6, 21, 8, 2, 0, _tzid),
                    new CalDateTime(2007, 6, 21, 8, 3, 0, _tzid),
                    new CalDateTime(2007, 6, 21, 8, 4, 0, _tzid),
                    new CalDateTime(2007, 6, 21, 8, 5, 0, _tzid),
                    new CalDateTime(2007, 6, 21, 8, 6, 0, _tzid),
                    new CalDateTime(2007, 6, 21, 8, 7, 0, _tzid),
                    new CalDateTime(2007, 6, 21, 8, 8, 0, _tzid),
                    new CalDateTime(2007, 6, 21, 8, 9, 0, _tzid),
                    new CalDateTime(2007, 6, 21, 8, 10, 0, _tzid)
                },
                null
            );
        }

        /// <summary>
        /// Ensures that if configured, MINUTELY recurrence rules are not allowed.
        /// </summary>
        [Test, Category("Recurrence")/*, ExpectedException(typeof(EvaluationEngineException))*/]
        public void Minutely1()
        {
            try
            {
                var iCal = Calendar.LoadFromStream(new StringReader(IcsFiles.Minutely1))[0];
                iCal.RecurrenceRestriction = RecurrenceRestrictionType.RestrictMinutely;
                var occurrences = iCal.GetOccurrences(
                    new CalDateTime(2007, 6, 21, 8, 0, 0, _tzid),
                    new CalDateTime(2007, 7, 21, 8, 0, 0, _tzid));
            }
            catch (Exception e)
            {
                Assert.IsInstanceOf<EvaluationEngineException>(e);
            }
        }

        /// <summary>
        /// Ensures that the proper behavior occurs when the evaluation
        /// mode is set to adjust automatically for MINUTELY evaluation
        /// </summary>
        [Test, Category("Recurrence")]
        public void Minutely1_1()
        {
            var iCal = Calendar.LoadFromStream(new StringReader(IcsFiles.Minutely1))[0];
            iCal.RecurrenceRestriction = RecurrenceRestrictionType.RestrictMinutely;
            iCal.RecurrenceEvaluationMode = RecurrenceEvaluationModeType.AdjustAutomatically;

            EventOccurrenceTest(
                iCal,
                new CalDateTime(2007, 6, 21, 8, 0, 0, _tzid),
                new CalDateTime(2007, 6, 21, 12, 0, 1, _tzid), // End period is exclusive, not inclusive.
                new[]
                {
                    new CalDateTime(2007, 6, 21, 8, 0, 0, _tzid),
                    new CalDateTime(2007, 6, 21, 9, 0, 0, _tzid),
                    new CalDateTime(2007, 6, 21, 10, 0, 0, _tzid),
                    new CalDateTime(2007, 6, 21, 11, 0, 0, _tzid),
                    new CalDateTime(2007, 6, 21, 12, 0, 0, _tzid)
                },
                null
            );
        }

        /// <summary>
        /// Ensures that if configured, HOURLY recurrence rules are not allowed.
        /// </summary>
        [Test, Category("Recurrence")/*, ExpectedException(typeof(EvaluationEngineException))*/]
        public void Hourly1()
        {
            try
            {
                var iCal = Calendar.LoadFromStream(new StringReader(IcsFiles.Hourly1))[0];
                iCal.RecurrenceRestriction = RecurrenceRestrictionType.RestrictHourly;
                var occurrences = iCal.GetOccurrences(new CalDateTime(2007, 6, 21, 8, 0, 0, _tzid), new CalDateTime(2007, 7, 21, 8, 0, 0, _tzid));
            }
            catch (Exception e)
            {
                Assert.IsInstanceOf<EvaluationEngineException>(e);
            }
            
        }

        /// <summary>
        /// Ensures that the proper behavior occurs when the evaluation
        /// mode is set to adjust automatically for HOURLY evaluation
        /// </summary>
        [Test, Category("Recurrence")]
        public void Hourly1_1()
        {
            var iCal = Calendar.LoadFromStream(new StringReader(IcsFiles.Hourly1))[0];
            iCal.RecurrenceRestriction = RecurrenceRestrictionType.RestrictHourly;
            iCal.RecurrenceEvaluationMode = RecurrenceEvaluationModeType.AdjustAutomatically;

            EventOccurrenceTest(
                iCal,
                new CalDateTime(2007, 6, 21, 8, 0, 0, _tzid),
                new CalDateTime(2007, 6, 25, 8, 0, 1, _tzid), // End period is exclusive, not inclusive.
                new[]
                {
                    new CalDateTime(2007, 6, 21, 8, 0, 0, _tzid),
                    new CalDateTime(2007, 6, 22, 8, 0, 0, _tzid),
                    new CalDateTime(2007, 6, 23, 8, 0, 0, _tzid),
                    new CalDateTime(2007, 6, 24, 8, 0, 0, _tzid),
                    new CalDateTime(2007, 6, 25, 8, 0, 0, _tzid)
                },
                null
            );
        }

        /// <summary>
        /// Ensures that "off-month" calculation works correctly
        /// </summary>
        [Test, Category("Recurrence")]
        public void MonthlyInterval1()
        {
            var iCal = Calendar.LoadFromStream(new StringReader(IcsFiles.MonthlyInterval1))[0];
            EventOccurrenceTest(
                iCal,
                new CalDateTime(2008, 1, 1, 7, 0, 0, _tzid),
                new CalDateTime(2008, 2, 29, 7, 0, 0, _tzid),
                new[]
                {
                    new CalDateTime(2008, 2, 11, 7, 0, 0, _tzid),
                    new CalDateTime(2008, 2, 12, 7, 0, 0, _tzid)
                },
                null
            );
        }

        /// <summary>
        /// Ensures that "off-year" calculation works correctly
        /// </summary>
        [Test, Category("Recurrence")]
        public void YearlyInterval1()
        {
            var iCal = Calendar.LoadFromStream(new StringReader(IcsFiles.YearlyInterval1))[0];
            EventOccurrenceTest(
                iCal,
                new CalDateTime(2006, 1, 1, 7, 0, 0, _tzid),
                new CalDateTime(2007, 1, 31, 7, 0, 0, _tzid),
                new[]
                {
                    new CalDateTime(2007, 1, 8, 7, 0, 0, _tzid),
                    new CalDateTime(2007, 1, 9, 7, 0, 0, _tzid)
                },
                null
            );
        }

        /// <summary>
        /// Ensures that "off-day" calcuation works correctly
        /// </summary>
        [Test, Category("Recurrence")]
        public void DailyInterval1()
        {
            var iCal = Calendar.LoadFromStream(new StringReader(IcsFiles.DailyInterval1))[0];
            EventOccurrenceTest(
                iCal,
                new CalDateTime(2007, 4, 11, 7, 0, 0, _tzid),
                new CalDateTime(2007, 4, 16, 7, 0, 0, _tzid),
                new[]
                {
                    new CalDateTime(2007, 4, 12, 7, 0, 0, _tzid),
                    new CalDateTime(2007, 4, 15, 7, 0, 0, _tzid)
                },
                null
            );
        }

        /// <summary>
        /// Ensures that "off-hour" calculation works correctly
        /// </summary>
        [Test, Category("Recurrence")]
        public void HourlyInterval1()
        {
            var iCal = Calendar.LoadFromStream(new StringReader(IcsFiles.HourlyInterval1))[0];
            EventOccurrenceTest(
                iCal,
                new CalDateTime(2007, 4, 9, 10, 0, 0, _tzid),
                new CalDateTime(2007, 4, 10, 20, 0, 0, _tzid),
                new[]
                {
                    // NOTE: this instance is included in the result set because it ends
                    // after the start of the evaluation period.
                    // See bug #3007244.
                    // https://sourceforge.net/tracker/?func=detail&aid=3007244&group_id=187422&atid=921236
                    new CalDateTime(2007, 4, 9, 7, 0, 0, _tzid), 
                    new CalDateTime(2007, 4, 10, 1, 0, 0, _tzid),
                    new CalDateTime(2007, 4, 10, 19, 0, 0, _tzid)
                },
                null
            );
        }

        /// <summary>
        /// Ensures that the following recurrence functions properly.
        /// The desired result is "The last Weekend-day of September for the next 10 years."
        /// This specifically tests the BYSETPOS=-1 to accomplish this.
        /// </summary>
        [Test, Category("Recurrence")]
        public void YearlyBySetPos1()
        {
            var iCal = Calendar.LoadFromStream(new StringReader(IcsFiles.YearlyBySetPos1))[0];
            EventOccurrenceTest(
                iCal,
                new CalDateTime(2009, 1, 1, 0, 0, 0, _tzid),
                new CalDateTime(2020, 1, 1, 0, 0, 0, _tzid),
                new[]
                {
                    new CalDateTime(2009, 9, 27, 5, 30, 0),
                    new CalDateTime(2010, 9, 26, 5, 30, 0),
                    new CalDateTime(2011, 9, 25, 5, 30, 0),
                    new CalDateTime(2012, 9, 30, 5, 30, 0),
                    new CalDateTime(2013, 9, 29, 5, 30, 0),
                    new CalDateTime(2014, 9, 28, 5, 30, 0),
                    new CalDateTime(2015, 9, 27, 5, 30, 0),
                    new CalDateTime(2016, 9, 25, 5, 30, 0),
                    new CalDateTime(2017, 9, 30, 5, 30, 0),
                    new CalDateTime(2018, 9, 30, 5, 30, 0)
                },
                null
            );
        }

        /// <summary>
        /// Ensures that GetOccurrences() always returns a single occurrence
        /// for a non-recurring event.
        /// </summary>
        [Test, Category("Recurrence")]
        public void Empty1()
        {
            var iCal = Calendar.LoadFromStream(new StringReader(IcsFiles.Empty1))[0];
            EventOccurrenceTest(
                iCal,
                new CalDateTime(2009, 1, 1, 0, 0, 0, _tzid),
                new CalDateTime(2010, 1, 1, 0, 0, 0, _tzid),
                new[]
                {
                    new CalDateTime(2009, 9, 27, 5, 30, 0)
                },
                null
            );
        }

        /// <summary>
        /// Ensures that RecurrencePattern.GetNextOccurrence() functions properly for an HOURLY frequency.
        /// </summary>
        [Test, Category("Recurrence")]
        public void HourlyInterval2()
        {
            var iCal = Calendar.LoadFromStream(new StringReader(IcsFiles.HourlyInterval2))[0];
            EventOccurrenceTest(
                iCal,
                new CalDateTime(2007, 4, 9, 7, 0, 0),
                new CalDateTime(2007, 4, 10, 23, 0, 1), // End time is exclusive, not inclusive
                new[]
                {
                    new CalDateTime(2007, 4, 9, 7, 0, 0),
                    new CalDateTime(2007, 4, 9, 11, 0, 0),
                    new CalDateTime(2007, 4, 9, 15, 0, 0),
                    new CalDateTime(2007, 4, 9, 19, 0, 0),
                    new CalDateTime(2007, 4, 9, 23, 0, 0),
                    new CalDateTime(2007, 4, 10, 3, 0, 0),
                    new CalDateTime(2007, 4, 10, 7, 0, 0),
                    new CalDateTime(2007, 4, 10, 11, 0, 0),
                    new CalDateTime(2007, 4, 10, 15, 0, 0),
                    new CalDateTime(2007, 4, 10, 19, 0, 0),
                    new CalDateTime(2007, 4, 10, 23, 0, 0)
                },
                null
            );            
        }

        /// <summary>
        /// Ensures that RecurrencePattern.GetNextOccurrence() functions properly for an MINUTELY frequency.
        /// </summary>
        [Test, Category("Recurrence")]
        public void MinutelyInterval1()
        {
            var iCal = Calendar.LoadFromStream(new StringReader(IcsFiles.MinutelyInterval1))[0];
            EventOccurrenceTest(
                iCal,
                new CalDateTime(2007, 4, 9, 7, 0, 0),
                new CalDateTime(2007, 4, 9, 12, 0, 1), // End time is exclusive, not inclusive
                new[]
                {
                    new CalDateTime(2007, 4, 9, 7, 0, 0),
                    new CalDateTime(2007, 4, 9, 7, 30, 0),
                    new CalDateTime(2007, 4, 9, 8, 0, 0),
                    new CalDateTime(2007, 4, 9, 8, 30, 0),
                    new CalDateTime(2007, 4, 9, 9, 0, 0),
                    new CalDateTime(2007, 4, 9, 9, 30, 0),
                    new CalDateTime(2007, 4, 9, 10, 0, 0),
                    new CalDateTime(2007, 4, 9, 10, 30, 0),
                    new CalDateTime(2007, 4, 9, 11, 0, 0),
                    new CalDateTime(2007, 4, 9, 11, 30, 0),
                    new CalDateTime(2007, 4, 9, 12, 0, 0)
                },
                null
            );
        }

        /// <summary>
        /// Ensures that RecurrencePattern.GetNextOccurrence() functions properly for an DAILY frequency with an INTERVAL.
        /// </summary>
        [Test, Category("Recurrence")]
        public void DailyInterval2()
        {
            var iCal = Calendar.LoadFromStream(new StringReader(IcsFiles.DailyInterval2))[0];
            EventOccurrenceTest(
                iCal,
                new CalDateTime(2007, 4, 9, 7, 0, 0),
                new CalDateTime(2007, 4, 27, 7, 0, 1), // End time is exclusive, not inclusive
                new[]
                {
                    new CalDateTime(2007, 4, 9, 7, 0, 0),
                    new CalDateTime(2007, 4, 11, 7, 0, 0),
                    new CalDateTime(2007, 4, 13, 7, 0, 0),
                    new CalDateTime(2007, 4, 15, 7, 0, 0),
                    new CalDateTime(2007, 4, 17, 7, 0, 0),
                    new CalDateTime(2007, 4, 19, 7, 0, 0),
                    new CalDateTime(2007, 4, 21, 7, 0, 0),
                    new CalDateTime(2007, 4, 23, 7, 0, 0),
                    new CalDateTime(2007, 4, 25, 7, 0, 0),
                    new CalDateTime(2007, 4, 27, 7, 0, 0)
                },
                null
            );
        }

        /// <summary>
        /// Ensures that RecurrencePattern.GetNextOccurrence() functions properly for an DAILY frequency with a BYDAY value.
        /// </summary>
        [Test, Category("Recurrence")]
        public void DailyByDay1()
        {
            var iCal = Calendar.LoadFromStream(new StringReader(IcsFiles.DailyByDay1))[0];
            EventOccurrenceTest(
                iCal,
                new CalDateTime(2007, 9, 10, 7, 0, 0),
                new CalDateTime(2007, 9, 27, 7, 0, 1), // End time is exclusive, not inclusive
                new[]
                {
                    new CalDateTime(2007, 9, 10, 7, 0, 0),
                    new CalDateTime(2007, 9, 13, 7, 0, 0),
                    new CalDateTime(2007, 9, 17, 7, 0, 0),
                    new CalDateTime(2007, 9, 20, 7, 0, 0),
                    new CalDateTime(2007, 9, 24, 7, 0, 0),
                    new CalDateTime(2007, 9, 27, 7, 0, 0)
                },
                null
            );
        }

        /// <summary>
        /// Ensures that DateUtil.AddWeeks works properly when week number is for previous year for selected date.
        /// </summary>
        [Test, Category("Recurrence")]
        public void WeeklyWeekStartsLastYear()
        {
            var iCal = Calendar.LoadFromStream(new StringReader(IcsFiles.WeeklyWeekStartsLastYear))[0];
            EventOccurrenceTest(
                iCal,
                new CalDateTime(2012, 1, 1, 7, 0, 0),
                new CalDateTime(2012, 1, 15, 11, 59, 59),
                new[]
                {
                    new CalDateTime(2012, 1, 2, 7, 0, 0),
                    new CalDateTime(2012, 1, 3, 7, 0, 0),
                    new CalDateTime(2012, 1, 4, 7, 0, 0),
                    new CalDateTime(2012, 1, 5, 7, 0, 0),
                    new CalDateTime(2012, 1, 6, 7, 0, 0),
                    new CalDateTime(2012, 1, 9, 7, 0, 0),
                    new CalDateTime(2012, 1, 10, 7, 0, 0),
                    new CalDateTime(2012, 1, 11, 7, 0, 0),
                    new CalDateTime(2012, 1, 12, 7, 0, 0),
                    new CalDateTime(2012, 1, 13, 7, 0, 0)
                },
                null
            );
        }

        /// <summary>
        /// Ensures that RecurrencePattern.GetNextOccurrence() functions properly for a WEEKLY frequency with an INTERVAL.
        /// </summary>
        [Test, Category("Recurrence")]
        public void WeeklyInterval1()
        {
            var iCal = Calendar.LoadFromStream(new StringReader(IcsFiles.WeeklyInterval1))[0];
            EventOccurrenceTest(
                iCal,
                new CalDateTime(2007, 9, 10, 7, 0, 0),
                new CalDateTime(2007, 12, 31, 11, 59, 59),
                new[]
                {
                    new CalDateTime(2007, 9, 10, 7, 0, 0),
                    new CalDateTime(2007, 9, 24, 7, 0, 0),
                    new CalDateTime(2007, 10, 8, 7, 0, 0),
                    new CalDateTime(2007, 10, 22, 7, 0, 0),
                    new CalDateTime(2007, 11, 5, 7, 0, 0),
                    new CalDateTime(2007, 11, 19, 7, 0, 0),
                    new CalDateTime(2007, 12, 3, 7, 0, 0),
                    new CalDateTime(2007, 12, 17, 7, 0, 0),
                    new CalDateTime(2007, 12, 31, 7, 0, 0)
                },
                null
            );
        }

        /// <summary>
        /// Ensures that RecurrencePattern.GetNextOccurrence() functions properly for a MONTHLY frequency.
        /// </summary>
        [Test, Category("Recurrence")]
        public void Monthly1()
        {
            var iCal = Calendar.LoadFromStream(new StringReader(IcsFiles.Monthly1))[0];
            EventOccurrenceTest(
                iCal,
                new CalDateTime(2007, 9, 10, 7, 0, 0),
                new CalDateTime(2008, 9, 10, 7, 0, 1), // Period end is exclusive, not inclusive
                new[]
                {
                    new CalDateTime(2007, 9, 10, 7, 0, 0),
                    new CalDateTime(2007, 10, 10, 7, 0, 0),
                    new CalDateTime(2007, 11, 10, 7, 0, 0),
                    new CalDateTime(2007, 12, 10, 7, 0, 0),
                    new CalDateTime(2008, 1, 10, 7, 0, 0),
                    new CalDateTime(2008, 2, 10, 7, 0, 0),
                    new CalDateTime(2008, 3, 10, 7, 0, 0),
                    new CalDateTime(2008, 4, 10, 7, 0, 0),
                    new CalDateTime(2008, 5, 10, 7, 0, 0),
                    new CalDateTime(2008, 6, 10, 7, 0, 0),
                    new CalDateTime(2008, 7, 10, 7, 0, 0),
                    new CalDateTime(2008, 8, 10, 7, 0, 0),
                    new CalDateTime(2008, 9, 10, 7, 0, 0)
                },
                null
            );
        }

        /// <summary>
        /// Ensures that RecurrencePattern.GetNextOccurrence() functions properly for a YEARLY frequency.
        /// </summary>
        [Test, Category("Recurrence")]
        public void Yearly1()
        {
            var iCal = Calendar.LoadFromStream(new StringReader(IcsFiles.Yearly1))[0];
            EventOccurrenceTest(
                iCal,
                new CalDateTime(2007, 9, 10, 7, 0, 0),
                new CalDateTime(2020, 9, 10, 7, 0, 1), // Period end is exclusive, not inclusive
                new[]
                {
                    new CalDateTime(2007, 9, 10, 7, 0, 0),
                    new CalDateTime(2008, 9, 10, 7, 0, 0),
                    new CalDateTime(2009, 9, 10, 7, 0, 0),
                    new CalDateTime(2010, 9, 10, 7, 0, 0),
                    new CalDateTime(2011, 9, 10, 7, 0, 0),
                    new CalDateTime(2012, 9, 10, 7, 0, 0),
                    new CalDateTime(2013, 9, 10, 7, 0, 0),
                    new CalDateTime(2014, 9, 10, 7, 0, 0),
                    new CalDateTime(2015, 9, 10, 7, 0, 0),
                    new CalDateTime(2016, 9, 10, 7, 0, 0),
                    new CalDateTime(2017, 9, 10, 7, 0, 0),
                    new CalDateTime(2018, 9, 10, 7, 0, 0),
                    new CalDateTime(2019, 9, 10, 7, 0, 0),
                    new CalDateTime(2020, 9, 10, 7, 0, 0)
                },
                null
            );
        }

        /// <summary>
        /// Tests a bug with WEEKLY recurrence values used with UNTIL.
        /// https://sourceforge.net/tracker/index.php?func=detail&aid=2912657&group_id=187422&atid=921236
        /// Sourceforge.net bug #2912657
        /// </summary>
        [Test, Category("Recurrence")]
        public void Bug2912657()
        {
            var iCal = Calendar.LoadFromStream(new StringReader(IcsFiles.Bug2912657))[0];
            var localTzid = iCal.TimeZones[0].TzId;

            // Daily recurrence
            EventOccurrenceTest(
                iCal,
                new CalDateTime(2009, 12, 4, 0, 0, 0, localTzid),
                new CalDateTime(2009, 12, 12, 0, 0, 0, localTzid),
                new[]
                {
                    new CalDateTime(2009, 12, 4, 2, 00, 00, localTzid),
                    new CalDateTime(2009, 12, 5, 2, 00, 00, localTzid),
                    new CalDateTime(2009, 12, 6, 2, 00, 00, localTzid),
                    new CalDateTime(2009, 12, 7, 2, 00, 00, localTzid),
                    new CalDateTime(2009, 12, 8, 2, 00, 00, localTzid),
                    new CalDateTime(2009, 12, 9, 2, 00, 00, localTzid),
                    new CalDateTime(2009, 12, 10, 2, 00, 00, localTzid)
                },
                null,
                0
            );

            // Weekly with UNTIL value
            EventOccurrenceTest(
                iCal,
                new CalDateTime(2009, 12, 4, localTzid),
                new CalDateTime(2009, 12, 10, localTzid),
                new[]
                {
                    new CalDateTime(2009, 12, 4, 2, 00, 00, localTzid)
                },
                null,
                1
            );

            // Weekly with COUNT=2
            EventOccurrenceTest(
                iCal,
                new CalDateTime(2009, 12, 4, localTzid),
                new CalDateTime(2009, 12, 12, localTzid),
                new[]
                {
                    new CalDateTime(2009, 12, 4, 2, 00, 00, localTzid),
                    new CalDateTime(2009, 12, 11, 2, 00, 00, localTzid)
                },
                null,
                2
            );
        }

        /// <summary>
        /// Tests a bug with WEEKLY recurrence values that cross year boundaries.
        /// https://sourceforge.net/tracker/?func=detail&aid=2916581&group_id=187422&atid=921236
        /// Sourceforge.net bug #2916581
        /// </summary>
        [Test, Category("Recurrence")]
        public void Bug2916581()
        {
            var iCal = Calendar.LoadFromStream(new StringReader(IcsFiles.Bug2916581))[0];
            var localTzid = iCal.TimeZones[0].TzId;

            // Weekly across year boundary
            EventOccurrenceTest(
                iCal,
                new CalDateTime(2009, 12, 25, 0, 0, 0, localTzid),
                new CalDateTime(2010, 1, 3, 0, 0, 0, localTzid),
                new[]
                {
                    new CalDateTime(2009, 12, 25, 11, 00, 00, localTzid),
                    new CalDateTime(2010, 1, 1, 11, 00, 00, localTzid)
                },
                null,
                0
            );

            // Weekly across year boundary
            EventOccurrenceTest(
                iCal,
                new CalDateTime(2009, 12, 25, 0, 0, 0, localTzid),
                new CalDateTime(2010, 1, 3, 0, 0, 0, localTzid),
                new[]
                {
                    new CalDateTime(2009, 12, 26, 11, 00, 00, localTzid),
                    new CalDateTime(2010, 1, 2, 11, 00, 00, localTzid)
                },
                null,
                1
            );
        }

        /// <summary>
        /// Tests a bug with WEEKLY recurrence values
        /// https://sourceforge.net/tracker/?func=detail&aid=2959692&group_id=187422&atid=921236
        /// Sourceforge.net bug #2959692
        /// </summary>
        [Test, Category("Recurrence")]
        public void Bug2959692()
        {
            var iCal = Calendar.LoadFromStream(new StringReader(IcsFiles.Bug2959692))[0];
            var localTzid = iCal.TimeZones[0].TzId;

            EventOccurrenceTest(
                iCal,
                new CalDateTime(2008, 1, 1, 0, 0, 0, localTzid),
                new CalDateTime(2008, 4, 1, 0, 0, 0, localTzid),
                new[]
                {
                    new CalDateTime(2008, 1, 3, 17, 00, 00, localTzid),
                    new CalDateTime(2008, 1, 17, 17, 00, 00, localTzid),
                    new CalDateTime(2008, 1, 31, 17, 00, 00, localTzid),
                    new CalDateTime(2008, 2, 14, 17, 00, 00, localTzid),
                    new CalDateTime(2008, 2, 28, 17, 00, 00, localTzid),
                    new CalDateTime(2008, 3, 13, 17, 00, 00, localTzid),
                    new CalDateTime(2008, 3, 27, 17, 00, 00, localTzid)
                },
                null,
                0
            );
        }

        /// <summary>
        /// Tests a bug with DAILY recurrence values
        /// https://sourceforge.net/tracker/?func=detail&aid=2966236&group_id=187422&atid=921236
        /// Sourceforge.net bug #2966236
        /// </summary>
        [Test, Category("Recurrence")]
        public void Bug2966236()
        {
            var iCal = Calendar.LoadFromStream(new StringReader(IcsFiles.Bug2966236))[0];
            var localTzid = iCal.TimeZones[0].TzId;

            EventOccurrenceTest(
                iCal,
                new CalDateTime(2010, 1, 1, 0, 0, 0, localTzid),
                new CalDateTime(2010, 3, 1, 0, 0, 0, localTzid),
                new[]
                {
                    new CalDateTime(2010, 1, 19, 8, 00, 00, localTzid),
                    new CalDateTime(2010, 1, 26, 8, 00, 00, localTzid),
                    new CalDateTime(2010, 2, 2, 8, 00, 00, localTzid),
                    new CalDateTime(2010, 2, 9, 8, 00, 00, localTzid),
                    new CalDateTime(2010, 2, 16, 8, 00, 00, localTzid),
                    new CalDateTime(2010, 2, 23, 8, 00, 00, localTzid)
                },
                null,
                0
            );

            EventOccurrenceTest(
                iCal,
                new CalDateTime(2010, 2, 1, 0, 0, 0, localTzid),
                new CalDateTime(2010, 3, 1, 0, 0, 0, localTzid),
                new[]
                {                    
                    new CalDateTime(2010, 2, 2, 8, 00, 00, localTzid),
                    new CalDateTime(2010, 2, 9, 8, 00, 00, localTzid),
                    new CalDateTime(2010, 2, 16, 8, 00, 00, localTzid),
                    new CalDateTime(2010, 2, 23, 8, 00, 00, localTzid)
                },
                null,
                0
            );
        }

        /// <summary>
        /// Tests a bug with events that span a very long period of time. (i.e. weeks, months, etc.)
        /// https://sourceforge.net/tracker/?func=detail&aid=3007244&group_id=187422&atid=921236
        /// Sourceforge.net bug #3007244
        /// </summary>
        [Test, Category("Recurrence")]
        public void Bug3007244()
        {
            var iCal = Calendar.LoadFromStream(new StringReader(IcsFiles.Bug3007244))[0];


            EventOccurrenceTest(
                iCal,
                new CalDateTime(2010, 7, 18, 0, 0, 0),
                new CalDateTime(2010, 7, 26, 0, 0, 0),
                new[]
                {
                    new CalDateTime(2010, 5, 23)
                },
                null,
                0
            );

            EventOccurrenceTest(
                iCal,
                new CalDateTime(2011, 7, 18, 0, 0, 0),
                new CalDateTime(2011, 7, 26, 0, 0, 0),
                new[]
                {
                    new CalDateTime(2011, 5, 23)
                },
                null,
                0
            );
        }

        /// <summary>
        /// Tests bug #3119920 - missing weekly occurences
        /// See https://sourceforge.net/tracker/?func=detail&aid=3119920&group_id=187422&atid=921236
        /// </summary>
        [Test, Category("Recurrence")]
        public void Bug3119920()
        {
            using (var sr = new StringReader("FREQ=WEEKLY;UNTIL=20251126T120000;INTERVAL=1;BYDAY=MO"))
            {
                var start = DateTime.Parse("2010-11-27 9:00:00");
                var serializer = new RecurrencePatternSerializer();
                var rp = (RecurrencePattern)serializer.Deserialize(sr);
                var rpe = new RecurrencePatternEvaluator(rp);
                var recurringPeriods = rpe.Evaluate(new CalDateTime(start), start, rp.Until, false);
                
                var period = recurringPeriods.ElementAt(recurringPeriods.Count() - 1);

                Assert.AreEqual(new CalDateTime(2025, 11, 24, 9, 0, 0), period.StartTime);
            }
        }

        /// <summary>
        /// Tests bug #3178652 - 29th day of February in recurrence problems
        /// See https://sourceforge.net/tracker/?func=detail&aid=3178652&group_id=187422&atid=921236
        /// </summary>
        [Test, Category("Recurrence")]
        public void Bug3178652()
        {
            var evt = new Event
            {
                Start = new CalDateTime(2011, 1, 29, 11, 0, 0),
                Duration = TimeSpan.FromHours(1.5),
                Summary = "29th February Test"
            };

            var pattern = new RecurrencePattern {
                Frequency = FrequencyType.Monthly,
                Until = new DateTime(2011, 12, 25, 0, 0, 0, DateTimeKind.Utc),
                FirstDayOfWeek = DayOfWeek.Sunday,
                ByMonthDay = new List<int>(new[] { 29 })
            };

            evt.RecurrenceRules.Add(pattern);

            var occurrences = evt.GetOccurrences(new DateTime(2011, 1, 1), new DateTime(2012, 1, 1));
            Assert.AreEqual(10, occurrences.Count, "There should be 10 occurrences of this event, one for each month except February and December.");
        }

        /// <summary>
        /// Tests bug #3292737 - Google Repeating Task Until Time  Bug
        /// See https://sourceforge.net/tracker/?func=detail&aid=3292737&group_id=187422&atid=921236
        /// </summary>
        [Test, Category("Recurrence")]
        public void Bug3292737()
        {
            using (var sr = new StringReader("FREQ=WEEKLY;UNTIL=20251126"))
            {
                var serializer = new RecurrencePatternSerializer();
                var rp = (RecurrencePattern)serializer.Deserialize(sr);

                Assert.IsNotNull(rp);
                Assert.AreEqual(new DateTime(2025, 11, 26), rp.Until);
            }
        }

        /// <summary>
        /// Tests the iCal holidays downloaded from apple.com
        /// </summary>
        [Test, Category("Recurrence")]
        public void UsHolidays()
        {
            var iCal = Calendar.LoadFromStream(new StringReader(IcsFiles.USHolidays))[0];
            Assert.IsNotNull(iCal, "iCalendar was not loaded.");
            var items = new Dictionary<string, CalDateTime>
            {
                { "Christmas", new CalDateTime(2006, 12, 25)},
                {"Thanksgiving", new CalDateTime(2006, 11, 23)},
                {"Veteran's Day", new CalDateTime(2006, 11, 11)},
                {"Halloween", new CalDateTime(2006, 10, 31)},
                {"Daylight Saving Time Ends", new CalDateTime(2006, 10, 29)},
                {"Columbus Day", new CalDateTime(2006, 10, 9)},
                {"Labor Day", new CalDateTime(2006, 9, 4)},
                {"Independence Day", new CalDateTime(2006, 7, 4)},
                {"Father's Day", new CalDateTime(2006, 6, 18)},
                {"Flag Day", new CalDateTime(2006, 6, 14)},
                {"John F. Kennedy's Birthday", new CalDateTime(2006, 5, 29)},
                {"Memorial Day", new CalDateTime(2006, 5, 29)},
                {"Mother's Day", new CalDateTime(2006, 5, 14)},
                {"Cinco de Mayo", new CalDateTime(2006, 5, 5)},
                {"Earth Day", new CalDateTime(2006, 4, 22)},
                {"Easter", new CalDateTime(2006, 4, 16)},
                {"Tax Day", new CalDateTime(2006, 4, 15)},
                {"Daylight Saving Time Begins", new CalDateTime(2006, 4, 2)},
                {"April Fool's Day", new CalDateTime(2006, 4, 1)},
                {"St. Patrick's Day", new CalDateTime(2006, 3, 17)},
                {"Washington's Birthday", new CalDateTime(2006, 2, 22)},
                {"President's Day", new CalDateTime(2006, 2, 20)},
                {"Valentine's Day", new CalDateTime(2006, 2, 14)},
                {"Lincoln's Birthday", new CalDateTime(2006, 2, 12)},
                {"Groundhog Day", new CalDateTime(2006, 2, 2)},
                {"Martin Luther King, Jr. Day", new CalDateTime(2006, 1, 16)},
                { "New Year's Day", new CalDateTime(2006, 1, 1)},
            };

            var occurrences = iCal.GetOccurrences(
                new CalDateTime(2006, 1, 1),
                new CalDateTime(2006, 12, 31));

            Assert.AreEqual(items.Count, occurrences.Count, "The number of holidays did not evaluate correctly.");
            foreach (var o in occurrences)
            {
                var evt = o.Source as IEvent;
                Assert.IsNotNull(evt);
                Assert.IsTrue(items.ContainsKey(evt.Summary), "Holiday text '" + evt.Summary + "' did not match known holidays.");
                Assert.AreEqual(items[evt.Summary], o.Period.StartTime, "Date/time of holiday '" + evt.Summary + "' did not match.");
            }
        }

        /// <summary>
        /// Ensures that the StartTime and EndTime of periods have
        /// HasTime set to true if the beginning time had HasTime set
        /// to false.
        /// </summary>
        [Test, Category("Recurrence")]
        public void Evaluate1()
        {
            ICalendar cal = new Calendar();
            IEvent evt = cal.Create<Event>();
            evt.Summary = "Event summary";

            // Start at midnight, UTC time
            evt.Start = new CalDateTime(DateTime.SpecifyKind(DateTime.Today, DateTimeKind.Utc));

            evt.RecurrenceRules.Add(new RecurrencePattern("FREQ=MINUTELY;INTERVAL=10;COUNT=5"));
            var occurrences = evt.GetOccurrences(CalDateTime.Today.AddDays(1), CalDateTime.Today.AddDays(2));

            foreach (var o in occurrences)
                Assert.IsTrue(o.Period.StartTime.HasTime, "All recurrences of this event should have a time set.");
        }

        [Test, Category("Recurrence")]
        public void RecurrencePattern1()
        {
            // NOTE: evaluators are not generally meant to be used directly like this.
            // However, this does make a good test to ensure they behave as they should.
            IRecurrencePattern pattern = new RecurrencePattern("FREQ=SECONDLY;INTERVAL=10");
            pattern.RestrictionType = RecurrenceRestrictionType.NoRestriction;

            var us = new CultureInfo("en-US");

            var startDate = new CalDateTime(DateTime.Parse("3/30/08 11:59:40 PM", us));
            var fromDate = new CalDateTime(DateTime.Parse("3/30/08 11:59:40 PM", us));
            var toDate = new CalDateTime(DateTime.Parse("3/31/08 12:00:11 AM", us));

            var evaluator = pattern.GetService(typeof(IEvaluator)) as IEvaluator;
            Assert.IsNotNull(evaluator);

            var occurrences = evaluator.Evaluate(
                startDate, 
                DateUtil.SimpleDateTimeToMatch(fromDate, startDate), 
                DateUtil.SimpleDateTimeToMatch(toDate, startDate),
                false)
                .OrderBy(o => o.StartTime)
                .ToList();
            Assert.AreEqual(4, occurrences.Count);
            Assert.AreEqual(new CalDateTime(DateTime.Parse("03/30/08 11:59:40 PM", us)), occurrences[0].StartTime);
            Assert.AreEqual(new CalDateTime(DateTime.Parse("03/30/08 11:59:50 PM", us)), occurrences[1].StartTime);
            Assert.AreEqual(new CalDateTime(DateTime.Parse("03/31/08 12:00:00 AM", us)), occurrences[2].StartTime);
            Assert.AreEqual(new CalDateTime(DateTime.Parse("03/31/08 12:00:10 AM", us)), occurrences[3].StartTime);
        }

        [Test, Category("Recurrence")]
        public void RecurrencePattern2()
        {
            // NOTE: evaluators are generally not meant to be used directly like this.
            // However, this does make a good test to ensure they behave as they should.
            var pattern = new RecurrencePattern("FREQ=MINUTELY;INTERVAL=1");

            var us = new CultureInfo("en-US");

            var startDate = new CalDateTime(DateTime.Parse("3/31/2008 12:00:10 AM", us));
            var fromDate = new CalDateTime(DateTime.Parse("4/1/2008 10:08:10 AM", us));
            var toDate = new CalDateTime(DateTime.Parse("4/1/2008 10:43:23 AM", us));

            var evaluator = pattern.GetService(typeof(IEvaluator)) as IEvaluator;
            Assert.IsNotNull(evaluator);

            var occurrences = evaluator.Evaluate(
                startDate, 
                DateUtil.SimpleDateTimeToMatch(fromDate, startDate), 
                DateUtil.SimpleDateTimeToMatch(toDate, startDate),
                false);
            Assert.AreNotEqual(0, occurrences.Count);
        }

        [Test, Category("Recurrence")]
        public void GetOccurrences1()
        {
            ICalendar cal = new Calendar();
            IEvent evt = cal.Create<Event>();
            evt.Start = new CalDateTime(2009, 11, 18, 5, 0, 0);
            evt.End = new CalDateTime(2009, 11, 18, 5, 10, 0);
            evt.RecurrenceRules.Add(new RecurrencePattern(FrequencyType.Daily));
            evt.Summary = "xxxxxxxxxxxxx";
 
            var previousDateAndTime = new CalDateTime(2009, 11, 17, 0, 15, 0);
            var previousDateOnly = new CalDateTime(2009, 11, 17, 23, 15, 0);
            var laterDateOnly = new CalDateTime(2009, 11, 19, 3, 15, 0);
            var laterDateAndTime = new CalDateTime(2009, 11, 19, 11, 0, 0);
            var end = new CalDateTime(2009, 11, 23, 0, 0, 0);

            var occurrences = evt.GetOccurrences(previousDateAndTime, end);
            Assert.AreEqual(5, occurrences.Count);

            occurrences = evt.GetOccurrences(previousDateOnly, end);
            Assert.AreEqual(5, occurrences.Count);

            occurrences = evt.GetOccurrences(laterDateOnly, end);
            Assert.AreEqual(4, occurrences.Count);

            occurrences = evt.GetOccurrences(laterDateAndTime, end);
            Assert.AreEqual(3, occurrences.Count);

            // Add ByHour "9" and "12"            
            evt.RecurrenceRules[0].ByHour.Add(9);
            evt.RecurrenceRules[0].ByHour.Add(12);

            // Clear the evaluation so we can calculate recurrences again.
            evt.ClearEvaluation();

            occurrences = evt.GetOccurrences(previousDateAndTime, end);
            Assert.AreEqual(11, occurrences.Count);

            occurrences = evt.GetOccurrences(previousDateOnly, end);
            Assert.AreEqual(11, occurrences.Count);

            occurrences = evt.GetOccurrences(laterDateOnly, end);
            Assert.AreEqual(8, occurrences.Count);

            occurrences = evt.GetOccurrences(laterDateAndTime, end);
            Assert.AreEqual(7, occurrences.Count);
        }

        [Test, Category("Recurrence")]
        public void Test1()
        {
            ICalendar cal = new Calendar();
            IEvent evt = cal.Create<Event>();
            evt.Summary = "Event summary";
            evt.Start = new CalDateTime(DateTime.SpecifyKind(DateTime.Today, DateTimeKind.Utc));

            IRecurrencePattern recur = new RecurrencePattern();
            evt.RecurrenceRules.Add(recur);

            try
            {
                var occurrences = evt.GetOccurrences(DateTime.Today.AddDays(1), DateTime.Today.AddDays(2));
                Assert.Fail("An exception should be thrown when evaluating a recurrence with no specified FREQUENCY");
            }
            catch { }
        }

        [Test, Category("Recurrence")]
        public void Test2()
        {
            ICalendar cal = new Calendar();
            IEvent evt = cal.Create<Event>();
            evt.Summary = "Event summary";
            evt.Start = new CalDateTime(DateTime.SpecifyKind(DateTime.Today, DateTimeKind.Utc));

            IRecurrencePattern recur = new RecurrencePattern();
            recur.Frequency = FrequencyType.Daily;
            recur.Count = 3;
            recur.ByDay.Add(new WeekDay(DayOfWeek.Monday));
            recur.ByDay.Add(new WeekDay(DayOfWeek.Wednesday));
            recur.ByDay.Add(new WeekDay(DayOfWeek.Friday));
            evt.RecurrenceRules.Add(recur);

            var serializer = new RecurrencePatternSerializer();
            Assert.IsTrue(string.Compare(serializer.SerializeToString(recur), "FREQ=DAILY;COUNT=3;BYDAY=MO,WE,FR", StringComparison.Ordinal) == 0,
                "Serialized recurrence string is incorrect");
        }

        [Test, Category("Recurrence")]
        public void Test4()
        {
            IRecurrencePattern rpattern = new RecurrencePattern();
            rpattern.ByDay.Add(new WeekDay(DayOfWeek.Saturday));
            rpattern.ByDay.Add(new WeekDay(DayOfWeek.Sunday));

            rpattern.Frequency = FrequencyType.Weekly;

            IDateTime evtStart = new CalDateTime(2006, 12, 1);
            IDateTime evtEnd = new CalDateTime(2007, 1, 1);

            var evaluator = rpattern.GetService(typeof(IEvaluator)) as IEvaluator;
            Assert.IsNotNull(evaluator);

            // Add the exception dates
            var periods = evaluator.Evaluate(
                evtStart,
                DateUtil.GetSimpleDateTimeData(evtStart), 
                DateUtil.SimpleDateTimeToMatch(evtEnd, evtStart),
                false)
                .OrderBy(p => p.StartTime)
                .ToList();
            Assert.AreEqual(10, periods.Count);
            Assert.AreEqual(2, periods[0].StartTime.Day);
            Assert.AreEqual(3, periods[1].StartTime.Day);
            Assert.AreEqual(9, periods[2].StartTime.Day);
            Assert.AreEqual(10, periods[3].StartTime.Day);
            Assert.AreEqual(16, periods[4].StartTime.Day);
            Assert.AreEqual(17, periods[5].StartTime.Day);
            Assert.AreEqual(23, periods[6].StartTime.Day);
            Assert.AreEqual(24, periods[7].StartTime.Day);
            Assert.AreEqual(30, periods[8].StartTime.Day);
            Assert.AreEqual(31, periods[9].StartTime.Day);
        }
    }
}
