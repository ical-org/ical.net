using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Resources;
using System.Web;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Net;
using NUnit.Framework;
using DDay.iCal.Serialization.iCalendar;

namespace DDay.iCal.Test
{
    [TestFixture]
    public class ProgramTest
    {
        [Test]
        public void LoadAndDisplayCalendar()
        {
            // The following code loads and displays an iCalendar
            // with US Holidays for 2006.
            //
            IICalendar iCal = iCalendar.LoadFromFile(@"Calendars\Serialization\USHolidays.ics")[0];
            Assert.IsNotNull(iCal, "iCalendar did not load.  Are you connected to the internet?");

            IList<Occurrence> occurrences = iCal.GetOccurrences(
                new iCalDateTime(2006, 1, 1, "US-Eastern"),
                new iCalDateTime(2006, 12, 31, "US-Eastern"));

            foreach (Occurrence o in occurrences)
            {
                IEvent evt = o.Source as IEvent;
                if (evt != null)
                {
                    // Display the date of the event
                    Console.Write(o.Period.StartTime.Local.Date.ToString("MM/dd/yyyy") + " -\t");

                    // Display the event summary
                    Console.Write(evt.Summary);

                    // Display the time the event happens (unless it's an all-day event)
                    if (evt.Start.HasTime)
                    {
                        Console.Write(" (" + evt.Start.Local.ToShortTimeString() + " - " + evt.End.Local.ToShortTimeString());
                        if (evt.Start.TimeZoneObservance != null && evt.Start.TimeZoneObservance.HasValue)
                            Console.Write(" " + evt.Start.TimeZoneObservance.Value.TimeZoneInfo.TimeZoneName);
                        Console.Write(")");
                    }

                    Console.Write(Environment.NewLine);
                }
            }
        }

        private DateTime Start;
        private DateTime End;
        private TimeSpan TotalTime;
        private string tzid;

        [TestFixtureSetUp]
        public void InitAll()
        {
            TotalTime = new TimeSpan(0);
            tzid = "US-Eastern";
        }

        [TestFixtureTearDown]
        public void DisposeAll()
        {
            Console.WriteLine("Total Processing Time: " + Math.Round(TotalTime.TotalMilliseconds) + "ms");
        }

        [SetUp]
        public void Init()
        {
            Start = DateTime.Now;
        }

        [TearDown]
        public void Dispose()
        {
            End = DateTime.Now;
            TotalTime = TotalTime.Add(End - Start);
            Console.WriteLine("Time: " + Math.Round(End.Subtract(Start).TotalMilliseconds) + "ms");
        }

        static public void TestCal(IICalendar iCal)
        {
            Assert.IsNotNull(iCal, "The iCalendar was not loaded");
            if (iCal.Events.Count > 0)
                Assert.IsTrue(iCal.Events.Count == 1, "Calendar should contain 1 event; however, the iCalendar loaded " + iCal.Events.Count + " events");
            else if (iCal.Todos.Count > 0)
                Assert.IsTrue(iCal.Todos.Count == 1, "Calendar should contain 1 todo; however, the iCalendar loaded " + iCal.Todos.Count + " todos");
        }

        [Test]
        public void LoadFromFile()
        {
            string path = @"Calendars\Serialization\Calendar1.ics";
            Assert.IsTrue(File.Exists(path), "File '" + path + "' does not exist.");

            IICalendar iCal = iCalendar.LoadFromFile(path)[0];
            Assert.AreEqual(14, iCal.Events.Count);
        }

        [Test]
        public void LoadFromUri()
        {
            string path = Directory.GetCurrentDirectory();
            path = Path.Combine(path, "Calendars/Serialization/Calendar1.ics").Replace(@"\", "/");
            path = "file:///" + path;
            Uri uri = new Uri(path);
            IICalendar iCal = iCalendar.LoadFromUri(uri)[0];
            Assert.AreEqual(14, iCal.Events.Count);
        }        

        /// <summary>
        /// The following test is an aggregate of MonthlyCountByMonthDay3() and MonthlyByDay1() in the
        /// <see cref="Recurrence"/> class.
        /// </summary>
        [Test]
        public void Merge1()
        {
            IICalendar iCal1 = iCalendar.LoadFromFile(@"Calendars\Recurrence\MonthlyCountByMonthDay3.ics")[0];
            IICalendar iCal2 = iCalendar.LoadFromFile(@"Calendars\Recurrence\MonthlyByDay1.ics")[0];

            // Change the UID of the 2nd event to make sure it's different
            iCal2.Events[iCal1.Events[0].UID].UID = "1234567890";
            iCal1.MergeWith(iCal2);

            IEvent evt1 = iCal1.Events[0];
            IEvent evt2 = iCal1.Events[1];

            // Get occurrences for the first event
            IList<Occurrence> occurrences = evt1.GetOccurrences(
                new iCalDateTime(1996, 1, 1, tzid),
                new iCalDateTime(2000, 1, 1, tzid));

            iCalDateTime[] DateTimes = new iCalDateTime[]
            {
                new iCalDateTime(1997, 9, 10, 9, 0, 0, tzid),
                new iCalDateTime(1997, 9, 11, 9, 0, 0, tzid),
                new iCalDateTime(1997, 9, 12, 9, 0, 0, tzid),
                new iCalDateTime(1997, 9, 13, 9, 0, 0, tzid),
                new iCalDateTime(1997, 9, 14, 9, 0, 0, tzid),
                new iCalDateTime(1997, 9, 15, 9, 0, 0, tzid),
                new iCalDateTime(1999, 3, 10, 9, 0, 0, tzid),
                new iCalDateTime(1999, 3, 11, 9, 0, 0, tzid),
                new iCalDateTime(1999, 3, 12, 9, 0, 0, tzid),
                new iCalDateTime(1999, 3, 13, 9, 0, 0, tzid),
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
                IDateTime dt = DateTimes[i];
                IDateTime start = occurrences[i].Period.StartTime;
                Assert.AreEqual(dt, start);
                Assert.IsTrue(dt.TimeZoneName == TimeZones[i], "Event " + dt + " should occur in the " + TimeZones[i] + " timezone");
            }

            Assert.IsTrue(occurrences.Count == DateTimes.Length, "There should be exactly " + DateTimes.Length + " occurrences; there were " + occurrences.Count);

            // Get occurrences for the 2nd event
            occurrences = evt2.GetOccurrences(
                new iCalDateTime(1996, 1, 1, tzid),
                new iCalDateTime(1998, 4, 1, tzid));

            iCalDateTime[] DateTimes1 = new iCalDateTime[]
            {
                new iCalDateTime(1997, 9, 2, 9, 0, 0, tzid),
                new iCalDateTime(1997, 9, 9, 9, 0, 0, tzid),
                new iCalDateTime(1997, 9, 16, 9, 0, 0, tzid),
                new iCalDateTime(1997, 9, 23, 9, 0, 0, tzid),
                new iCalDateTime(1997, 9, 30, 9, 0, 0, tzid),
                new iCalDateTime(1997, 11, 4, 9, 0, 0, tzid),
                new iCalDateTime(1997, 11, 11, 9, 0, 0, tzid),
                new iCalDateTime(1997, 11, 18, 9, 0, 0, tzid),
                new iCalDateTime(1997, 11, 25, 9, 0, 0, tzid),
                new iCalDateTime(1998, 1, 6, 9, 0, 0, tzid),
                new iCalDateTime(1998, 1, 13, 9, 0, 0, tzid),
                new iCalDateTime(1998, 1, 20, 9, 0, 0, tzid),
                new iCalDateTime(1998, 1, 27, 9, 0, 0, tzid),
                new iCalDateTime(1998, 3, 3, 9, 0, 0, tzid),
                new iCalDateTime(1998, 3, 10, 9, 0, 0, tzid),
                new iCalDateTime(1998, 3, 17, 9, 0, 0, tzid),
                new iCalDateTime(1998, 3, 24, 9, 0, 0, tzid),
                new iCalDateTime(1998, 3, 31, 9, 0, 0, tzid)
            };

            string[] TimeZones1 = new string[]
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

            for (int i = 0; i < DateTimes1.Length; i++)
            {
                IDateTime dt = DateTimes1[i];
                IDateTime start = occurrences[i].Period.StartTime;
                Assert.AreEqual(dt, start);
                Assert.IsTrue(dt.TimeZoneName == TimeZones1[i], "Event " + dt + " should occur in the " + TimeZones1[i] + " timezone");
            }

            Assert.AreEqual(DateTimes1.Length, occurrences.Count, "There should be exactly " + DateTimes1.Length + " occurrences; there were " + occurrences.Count);
        }

        [Test]
        public void Merge2()
        {
            iCalendar iCal = new iCalendar();
            IICalendar tmp_cal = iCalendar.LoadFromFile(@"Calendars\Serialization\TimeZone3.ics")[0];
            iCal.MergeWith(tmp_cal);

            tmp_cal = iCalendar.LoadFromFile(@"Calendars\Serialization\TimeZone3.ics")[0];

            // Compare the two calendars -- they should match exactly
            SerializationTest.CompareCalendars(iCal, tmp_cal);
        }

        /// <summary>
        /// The following tests the MergeWith() method of iCalendar to
        /// ensure that unique component merging happens as expected.
        /// </summary>
        [Test]
        public void Merge3()
        {
            IICalendar iCal1 = iCalendar.LoadFromFile(@"Calendars\Recurrence\MonthlyCountByMonthDay3.ics")[0];
            IICalendar iCal2 = iCalendar.LoadFromFile(@"Calendars\Recurrence\YearlyByMonth1.ics")[0];

            iCal1.MergeWith(iCal2);

            Assert.AreEqual(1, iCal1.Events.Count);
        }        

#if !SILVERLIGHT
        /// <summary>
        /// Tests conversion of the system time zone to one compatible with DDay.iCal.
        /// Also tests the gaining/loss of an hour over time zone boundaries.
        /// </summary>
        [Test]
        public void SystemTimeZone1()
        {
            System.TimeZoneInfo tzi = System.TimeZoneInfo.FindSystemTimeZoneById("Mountain Standard Time");
            Assert.IsNotNull(tzi);

            iCalendar iCal = new iCalendar();
            iCalTimeZone tz = iCalTimeZone.FromSystemTimeZone(tzi, new DateTime(2000, 1, 1), false);
            Assert.IsNotNull(tz);
            iCal.AddChild(tz);

            iCalendarSerializer serializer = new iCalendarSerializer();
            serializer.Serialize(iCal, @"Calendars\Serialization\SystemTimeZone1.ics");

            // Ensure the time zone transition works as expected
            // (i.e. it takes 1 hour and 1 second to transition from
            // 2003-10-26 12:59:59 AM to
            // 2003-10-26 01:00:00 AM)
            iCalDateTime dt1 = new iCalDateTime(2003, 10, 26, 0, 59, 59, tz.TZID, iCal);
            iCalDateTime dt2 = new iCalDateTime(2003, 10, 26, 1, 0, 0, tz.TZID, iCal);

            TimeSpan result = dt2 - dt1;
            Assert.AreEqual(TimeSpan.FromHours(1) + TimeSpan.FromSeconds(1), result);

            // Ensure another time zone transition works as expected
            // (i.e. it takes negative 59 minutes and 59 seconds to transition from
            // 2004-04-04 01:59:59 AM to
            // 2004-04-04 02:00:00 AM)
            dt1 = new iCalDateTime(2004, 4, 4, 1, 59, 59, tz.TZID, iCal);
            dt2 = new iCalDateTime(2004, 4, 4, 2, 0, 0, tz.TZID, iCal);
            result = dt2 - dt1;
            Assert.AreEqual(TimeSpan.FromHours(-1) + TimeSpan.FromSeconds(1), result);            
        }

        /// <summary>
        /// Ensures the AddTimeZone() method works as expected.
        /// </summary>
        [Test]
        public void SystemTimeZone2()
        {
            System.TimeZoneInfo tzi = System.TimeZoneInfo.FindSystemTimeZoneById("Mountain Standard Time");
            Assert.IsNotNull(tzi);

            iCalendar iCal = new iCalendar();
            ITimeZone tz = iCal.AddTimeZone(tzi, new DateTime(2000, 1, 1), false);
            Assert.IsNotNull(tz);

            iCalendarSerializer serializer = new iCalendarSerializer();
            serializer.Serialize(iCal, @"Calendars\Serialization\SystemTimeZone2.ics");

            // Ensure the time zone transition works as expected
            // (i.e. it takes 1 hour and 1 second to transition from
            // 2003-10-26 12:59:59 AM to
            // 2003-10-26 01:00:00 AM)
            iCalDateTime dt1 = new iCalDateTime(2003, 10, 26, 0, 59, 59, tz.TZID, iCal);
            iCalDateTime dt2 = new iCalDateTime(2003, 10, 26, 1, 0, 0, tz.TZID, iCal);
            TimeSpan result = dt2 - dt1;
            Assert.AreEqual(TimeSpan.FromHours(1) + TimeSpan.FromSeconds(1), result);

            // Ensure another time zone transition works as expected
            // (i.e. it takes negative 59 minutes and 59 seconds to transition from
            // 2004-04-04 01:59:59 AM to
            // 2004-04-04 02:00:00 AM)
            dt1 = new iCalDateTime(2004, 4, 4, 1, 59, 59, tz.TZID, iCal);
            dt2 = new iCalDateTime(2004, 4, 4, 2, 0, 0, tz.TZID, iCal);
            result = dt2 - dt1;
            Assert.AreEqual(TimeSpan.FromHours(-1) + TimeSpan.FromSeconds(1), result);
        }

        [Test]
        public void SystemTimeZone3()
        {
            // Per Jon Udell's test, we should be able to get all 
            // system time zones on the machine and ensure they
            // are properly translated.
            var zones = System.TimeZoneInfo.GetSystemTimeZones();
            TimeZoneInfo tzinfo;
            foreach (var zone in zones)
            {
                tzinfo = null;
                try
                {
                    tzinfo = System.TimeZoneInfo.FindSystemTimeZoneById(zone.Id);                    
                }
                catch (Exception e)
                {
                    Assert.Fail("Not found: " + zone.StandardName);                    
                }

                if (tzinfo != null)
                {
                    var ical_tz = DDay.iCal.iCalTimeZone.FromSystemTimeZone(tzinfo);
                    Assert.AreNotEqual(0, ical_tz.TimeZoneInfos.Count, zone.StandardName + ": no time zone information was extracted.");
                }
            }            
        }
#endif
    }
}
