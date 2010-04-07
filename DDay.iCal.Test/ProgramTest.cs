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
            IICalendar iCal = iCalendar.LoadFromFile(@"Calendars\General\USHolidays.ics")[0];
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
            string path = @"Calendars\General\Test1.ics";
            Assert.IsTrue(File.Exists(path), "File '" + path + "' does not exist.");

            IICalendar iCal = iCalendar.LoadFromFile(path)[0];
            ProgramTest.TestCal(iCal);
        }

        [Test]
        public void LoadFromUri()
        {
            string path = Directory.GetCurrentDirectory();
            path = Path.Combine(path, "Calendars/General/Test1.ics").Replace(@"\", "/");
            path = "file:///" + path;
            Uri uri = new Uri(path);
            IICalendar iCal = iCalendar.LoadFromUri(uri)[0];
            ProgramTest.TestCal(iCal);
        }

        // FIXME: re-imeplement
        //[Test]
        //public void CATEGORIES()
        //{
        //    IICalendar iCal = iCalendar.LoadFromFile(@"Calendars\General\CATEGORIES.ics");
        //    ProgramTest.TestCal(iCal);
        //    IEvent evt = iCal.Events[0];

        //    ArrayList items = new ArrayList();
        //    items.AddRange(new string[]
        //    {
        //        "One", "Two", "Three",
        //        "Four", "Five", "Six",
        //        "Seven", "A string of text with nothing less than a comma, semicolon; and a newline\n."
        //    });

        //    Hashtable found = new Hashtable();

        //    foreach (TextCollection tc in evt.Categories)
        //    {
        //        foreach (Text text in tc.Values)
        //        {
        //            if (items.Contains(text.Value))
        //                found[text.Value] = true;
        //        }
        //    }

        //    foreach (string item in items)
        //        Assert.IsTrue(found.ContainsKey(item), "Event should contain CATEGORY '" + item + "', but it was not found.");
        //}

        [Test]
        public void GeographicLocation1()
        {
            IICalendar iCal = iCalendar.LoadFromFile(@"Calendars\General\GeographicLocation1.ics")[0];
            ProgramTest.TestCal(iCal);
            IEvent evt = iCal.Events[0];

            Assert.AreEqual(37.386013, evt.GeographicLocation.Latitude, "Latitude should be 37.386013; it is not.");
            Assert.AreEqual(-122.082932, evt.GeographicLocation.Longitude, "Longitude should be -122.082932; it is not.");
        }

        

        

        

        /// <summary>
        /// The following test is an aggregate of MonthlyCountByMonthDay3() and MonthlyByDay1() in the
        /// <see cref="Recurrence"/> class.
        /// </summary>
        [Test]
        public void MERGE1()
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
                Assert.AreEqual(dt.Local, start.Local);
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
                Assert.AreEqual(dt.Local, start.Local);
                Assert.IsTrue(dt.TimeZoneName == TimeZones1[i], "Event " + dt + " should occur in the " + TimeZones1[i] + " timezone");
            }

            Assert.AreEqual(DateTimes1.Length, occurrences.Count, "There should be exactly " + DateTimes1.Length + " occurrences; there were " + occurrences.Count);
        }

        [Test]
        public void MERGE2()
        {
            iCalendar iCal = new iCalendar();
            IICalendar tmp_cal = iCalendar.LoadFromFile(@"Calendars\General\MERGE2.ics")[0];
            iCal.MergeWith(tmp_cal);

            tmp_cal = iCalendar.LoadFromFile(@"Calendars\General\MERGE2.ics")[0];

            // Compare the two calendars -- they should match exactly
            SerializationTest.CompareCalendars(iCal, tmp_cal);
        }

        /// <summary>
        /// The following tests the MergeWith() method of iCalendar to
        /// ensure that unique component merging happens as expected.
        /// </summary>
        [Test]
        public void MERGE3()
        {
            IICalendar iCal1 = iCalendar.LoadFromFile(@"Calendars\Recurrence\MonthlyCountByMonthDay3.ics")[0];
            IICalendar iCal2 = iCalendar.LoadFromFile(@"Calendars\Recurrence\YearlyByMonth1.ics")[0];

            iCal1.MergeWith(iCal2);

            Assert.AreEqual(1, iCal1.Events.Count);
        }

        

        // FIXME: re-implement
        //[Test]
        //public void AddEvent1()
        //{
        //    IICalendar iCal = iCalendar.LoadFromFile(@"Calendars\General\GEO1.ics");
        //    ProgramTest.TestCal(iCal);

        //    Event evt = iCal.Create<Event>();
        //    evt.Summary = "Test event";
        //    evt.Description = "This is an event to see if event creation works";
        //    evt.Start = new iCalDateTime(2006, 12, 15, "US-Eastern", iCal);
        //    evt.Duration = new TimeSpan(1, 0, 0);
        //    evt.Organizer = new Organizer("dougd@daywesthealthcare.com");

        //    if (!Directory.Exists(@"Calendars\General\Temp"))
        //        Directory.CreateDirectory(@"Calendars\General\Temp");

        //    iCalendarSerializer serializer = new iCalendarSerializer();
        //    serializer.Serialize(iCal, @"Calendars\General\Temp\GEO1_Serialized.ics");
        //}

        
        // FIXME: re-implement
        //[Test]
        //public void EVALUATION1()
        //{
        //    iCalendarCollection calendars = new iCalendarCollection();
        //    calendars.AddRange(iCalendar.LoadFromFile(@"Calendars\Recurrence\MonthlyCountByMonthDay3.ics"));
        //    calendars.AddRange(iCalendar.LoadFromFile(@"Calendars\Recurrence\MonthlyByDay1.ics"));

        //    iCalDateTime startDate = new iCalDateTime(1996, 1, 1, tzid, calendars[0]);
        //    iCalDateTime endDate = new iCalDateTime(1998, 4, 1, tzid, calendars[0]);

        //    List<IDateTime> DateTimes = new List<IDateTime>(new iCalDateTime[]
        //    {
        //        new iCalDateTime(1997, 9, 2, 9, 0, 0, tzid, calendars[0]),
        //        new iCalDateTime(1997, 9, 9, 9, 0, 0, tzid, calendars[0]),
        //        new iCalDateTime(1997, 9, 16, 9, 0, 0, tzid, calendars[0]),
        //        new iCalDateTime(1997, 9, 23, 9, 0, 0, tzid, calendars[0]),
        //        new iCalDateTime(1997, 9, 30, 9, 0, 0, tzid, calendars[0]),
        //        new iCalDateTime(1997, 11, 4, 9, 0, 0, tzid, calendars[0]),
        //        new iCalDateTime(1997, 11, 11, 9, 0, 0, tzid, calendars[0]),
        //        new iCalDateTime(1997, 11, 18, 9, 0, 0, tzid, calendars[0]),
        //        new iCalDateTime(1997, 11, 25, 9, 0, 0, tzid, calendars[0]),
        //        new iCalDateTime(1998, 1, 6, 9, 0, 0, tzid, calendars[0]),
        //        new iCalDateTime(1998, 1, 13, 9, 0, 0, tzid, calendars[0]),
        //        new iCalDateTime(1998, 1, 20, 9, 0, 0, tzid, calendars[0]),
        //        new iCalDateTime(1998, 1, 27, 9, 0, 0, tzid, calendars[0]),
        //        new iCalDateTime(1998, 3, 3, 9, 0, 0, tzid, calendars[0]),
        //        new iCalDateTime(1998, 3, 10, 9, 0, 0, tzid, calendars[0]),
        //        new iCalDateTime(1998, 3, 17, 9, 0, 0, tzid, calendars[0]),
        //        new iCalDateTime(1998, 3, 24, 9, 0, 0, tzid, calendars[0]),
        //        new iCalDateTime(1998, 3, 31, 9, 0, 0, tzid, calendars[0]),
        //        new iCalDateTime(1997, 9, 10, 9, 0, 0, tzid, calendars[0]),
        //        new iCalDateTime(1997, 9, 11, 9, 0, 0, tzid, calendars[0]),
        //        new iCalDateTime(1997, 9, 12, 9, 0, 0, tzid, calendars[0]),
        //        new iCalDateTime(1997, 9, 13, 9, 0, 0, tzid, calendars[0]),
        //        new iCalDateTime(1997, 9, 14, 9, 0, 0, tzid, calendars[0]),
        //        new iCalDateTime(1997, 9, 15, 9, 0, 0, tzid, calendars[0]),                
        //    });

        //    List<Event> occurrences = new List<Event>(calendars.GetRecurrencesForRange<Event>(startDate, endDate));
        //    foreach (Event evt in occurrences)
        //        Assert.IsTrue(DateTimes.Contains(evt.Start), "Event occurred on " + evt.Start + "; it should not have");
        //    foreach (iCalDateTime dt in DateTimes)
        //    {
        //        bool isFound = false;
        //        foreach (Event evt in occurrences)
        //        {
        //            if (evt.Start.Equals(dt))
        //            {
        //                isFound = true;
        //                break;
        //            }
        //        }
        //        Assert.IsTrue(isFound, "Event should occur on " + dt);
        //    }

        //    Assert.IsTrue(occurrences.Count == DateTimes.Count, "There should be exactly " + DateTimes.Count + " occurrences; there were " + occurrences.Count);
        //}

        [Test]
        public void PRODID1()
        {
            IICalendar iCal = iCalendar.LoadFromFile(@"Calendars/General/PRODID1.ics")[0];
        }

        [Test]
        public void PRODID2()
        {
            IICalendar iCal = iCalendar.LoadFromFile(@"Calendars/General/PRODID2.ics")[0];
        }

        [Test]
        public void Outlook2007_With_Folded_Lines_Using_Tabs_Contains_One_Event()
        {
            IICalendar iCal = iCalendar.LoadFromFile(@"Calendars/General/Outlook2007LineFolds.ics")[0];
            IList<Occurrence> events = iCal.GetOccurrences(new iCalDateTime(2009, 06, 20), new iCalDateTime(2009, 06, 22));
            Assert.AreEqual(1, events.Count);
        }

        [Test]
        public void Outlook2007_With_Folded_Lines_Using_Tabs_Is_Properly_Unwrapped()
        {
            string longName = "The Exceptionally Long Named Meeting Room Whose Name Wraps Over Several Lines When Exported From Leading Calendar and Office Software Application Microsoft Office 2007";
            IICalendar iCal = iCalendar.LoadFromFile(@"Calendars/General/Outlook2007LineFolds.ics")[0];
            IList<Occurrence> events = iCal.GetOccurrences<Event>(new iCalDateTime(2009, 06, 20), new iCalDateTime(2009, 06, 22));
            Assert.AreEqual(longName, ((IEvent)events[0].Source).Location);
        }

#if DATACONTRACT && !SILVERLIGHT
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
            iCalTimeZone tz = iCalTimeZone.FromSystemTimeZone(tzi);
            Assert.IsNotNull(tz);

            iCal.AddChild(tz);

            iCalDateTime dt1 = new iCalDateTime(2003, 10, 26, 0, 59, 59, tz.TZID, iCal);
            iCalDateTime dt2 = new iCalDateTime(2003, 10, 26, 1, 0, 0, tz.TZID, iCal);
            TimeSpan result = dt2 - dt1;
            Assert.AreEqual(TimeSpan.FromHours(1) + TimeSpan.FromSeconds(1), result);

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
            iCalTimeZone tz = iCal.AddTimeZone(tzi);
            Assert.IsNotNull(tz);
            
            iCalDateTime dt1 = new iCalDateTime(2003, 10, 26, 0, 59, 59, tz.TZID, iCal);
            iCalDateTime dt2 = new iCalDateTime(2003, 10, 26, 1, 0, 0, tz.TZID, iCal);
            TimeSpan result = dt2 - dt1;
            Assert.AreEqual(TimeSpan.FromHours(1) + TimeSpan.FromSeconds(1), result);

            dt1 = new iCalDateTime(2004, 4, 4, 1, 59, 59, tz.TZID, iCal);
            dt2 = new iCalDateTime(2004, 4, 4, 2, 0, 0, tz.TZID, iCal);
            result = dt2 - dt1;
            Assert.AreEqual(TimeSpan.FromHours(-1) + TimeSpan.FromSeconds(1), result);
        }
#endif
    }
}
