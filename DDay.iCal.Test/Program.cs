using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Resources;
using System.Web;
using System.Web.UI;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;

using DDay.iCal.Components;
using DDay.iCal.DataTypes;
using DDay.iCal.Test;
using DDay.iCal.Serialization;
using System.Net;

namespace DDay.iCal.Test
{
    [TestFixture]
    public class Program
    {
        [Test]
        public void LoadAndDisplayCalendar()
        {
            // The following code loads and displays an iCalendar
            // with US Holidays for 2006.
            //
            iCalendar iCal = iCalendar.LoadFromFile(@"Calendars\General\USHolidays.ics");
            Assert.IsNotNull(iCal, "iCalendar did not load.  Are you connected to the internet?");

            List<Occurrence> occurrences = iCal.GetOccurrences(
             new iCalDateTime(2006, 1, 1, "US-Eastern", iCal),
             new iCalDateTime(2006, 12, 31, "US-Eastern", iCal));

            foreach (Occurrence o in occurrences)
            {
                Event evt = o.Component as Event;
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
                        if (evt.Start.TimeZoneInfo != null)
                            Console.Write(" " + evt.Start.TimeZoneInfo.TimeZoneName);
                        Console.Write(")");
                    }

                    Console.Write(Environment.NewLine);
                }
            }
        }

        private DateTime Start;
        private DateTime End;
        private TimeSpan TotalTime;
        private TZID tzid;

        [TestFixtureSetUp]
        public void InitAll()
        {
            TotalTime = new TimeSpan(0);
            tzid = new TZID("US-Eastern");
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

        static public void TestCal(iCalendar iCal)
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
            
            iCalendar iCal = iCalendar.LoadFromFile(path);
            Program.TestCal(iCal);
        }

        [Test]
        public void LoadFromUri()
        {
            string path = Directory.GetCurrentDirectory();            
            path = Path.Combine(path, "Calendars/General/Test1.ics").Replace(@"\", "/");
            path = "file:///" + path;            
            Uri uri = new Uri(path);
            iCalendar iCal = iCalendar.LoadFromUri(uri);
            Program.TestCal(iCal);
        }

        [Test]
        public void CATEGORIES()
        {
            iCalendar iCal = iCalendar.LoadFromFile(@"Calendars\General\CATEGORIES.ics");
            Program.TestCal(iCal);
            Event evt = iCal.Events[0];

            ArrayList items = new ArrayList();
            items.AddRange(new string[]
            {
                "One", "Two", "Three",
                "Four", "Five", "Six",
                "Seven", "A string of text with nothing less than a comma, semicolon; and a newline\n."
            });

            Hashtable found = new Hashtable();

            foreach (TextCollection tc in evt.Categories)
            {
                foreach (Text text in tc.Values)
                {
                    if (items.Contains(text.Value))
                        found[text.Value] = true;
                }
            }

            foreach (string item in items)
                Assert.IsTrue(found.ContainsKey(item), "Event should contain CATEGORY '" + item + "', but it was not found.");
        }

        [Test]
        public void GEO1()
        {
            iCalendar iCal = iCalendar.LoadFromFile(@"Calendars\General\GEO1.ics");
            Program.TestCal(iCal);
            Event evt = iCal.Events[0];

            Assert.IsTrue(evt.Geo.Latitude.Value == 37.386013, "Latitude should be 37.386013; it is not.");
            Assert.IsTrue(evt.Geo.Longitude.Value == -122.082932, "Longitude should be -122.082932; it is not.");
        }

        [Test]
        public void BASE64()
        {
            iCalendar iCal = iCalendar.LoadFromFile(@"Calendars\General\BASE64.ics");
            Program.TestCal(iCal);
            Event evt = iCal.Events[0];

            Assert.IsTrue(evt.Attach[0].Value ==
"This is a test to try out base64 encoding without being too large.\r\n" +
"This is a test to try out base64 encoding without being too large.\r\n" +
"This is a test to try out base64 encoding without being too large.\r\n" +
"This is a test to try out base64 encoding without being too large.\r\n" +
"This is a test to try out base64 encoding without being too large.\r\n" +
"This is a test to try out base64 encoding without being too large.\r\n" +
"This is a test to try out base64 encoding without being too large.\r\n" +
"This is a test to try out base64 encoding without being too large.\r\n" +
"This is a test to try out base64 encoding without being too large.\r\n" +
"This is a test to try out base64 encoding without being too large.\r\n" +
"This is a test to try out base64 encoding without being too large.\r\n" +
"This is a test to try out base64 encoding without being too large.", "Attached value does not match.");
        }

        [Test]
        public void BASE64_1()
        {
            iCalendar iCal = iCalendar.LoadFromFile(@"Calendars\General\BASE64_1.ics");
            Program.TestCal(iCal);
            Event evt = iCal.Events[0];

            Assert.IsTrue(evt.UID.Value == "uuid1153170430406", "UID should be 'uuid1153170430406'; it is " + evt.UID.Value);
            Assert.IsTrue(evt.Sequence.Value == 1, "SEQUENCE should be 1; it is " + evt.Sequence.Value);
        }

        /// <summary>
        /// At times, this may throw a WebException if an internet connection is not present.
        /// This is safely ignored.
        /// </summary>
        [Test, ExpectedException(typeof(WebException))]
        public void BINARY()
        {
            iCalendar iCal = iCalendar.LoadFromFile(@"Calendars\General\BINARY.ics");
            Program.TestCal(iCal);
            Event evt = iCal.Events[0];

            Binary b = evt.Attach[0];
            if (b.Uri != null)
                b.LoadDataFromUri();

            MemoryStream ms = new MemoryStream();
            ms.SetLength(b.Data.Length);
            b.Data.CopyTo(ms.GetBuffer(), 0);

            iCalendar ical = iCalendar.LoadFromStream(ms);
            Assert.IsNotNull(ical, "Attached iCalendar did not load correctly");

            throw new WebException();
        }
        
        /// <summary>
        /// The following test is an aggregate of RRULE21() and RRULE22() in the
        /// <see cref="Recurrence"/> class.
        /// </summary>
        [Test]
        public void MERGE1()
        {
            iCalendar iCal1 = iCalendar.LoadFromFile(@"Calendars\Recurrence\RRULE21.ics");
            iCalendar iCal2 = iCalendar.LoadFromFile(@"Calendars\Recurrence\RRULE22.ics");

            // Change the UID of the 2nd event to make sure it's different
            iCal2.Events[iCal1.Events[0].UID].UID = "1234567890";
            iCal1.MergeWith(iCal2);
            
            Event evt1 = iCal1.Events[0];
            Event evt2 = iCal1.Events[1];

            // Get occurrences for the first event
            List<Occurrence> occurrences = evt1.GetOccurrences(
                new iCalDateTime(1996, 1, 1, tzid, iCal1),
                new iCalDateTime(2000, 1, 1, tzid, iCal1));            

            iCalDateTime[] DateTimes = new iCalDateTime[]
            {
                new iCalDateTime(1997, 9, 10, 9, 0, 0, tzid, iCal1),
                new iCalDateTime(1997, 9, 11, 9, 0, 0, tzid, iCal1),
                new iCalDateTime(1997, 9, 12, 9, 0, 0, tzid, iCal1),
                new iCalDateTime(1997, 9, 13, 9, 0, 0, tzid, iCal1),
                new iCalDateTime(1997, 9, 14, 9, 0, 0, tzid, iCal1),
                new iCalDateTime(1997, 9, 15, 9, 0, 0, tzid, iCal1),
                new iCalDateTime(1999, 3, 10, 9, 0, 0, tzid, iCal1),
                new iCalDateTime(1999, 3, 11, 9, 0, 0, tzid, iCal1),
                new iCalDateTime(1999, 3, 12, 9, 0, 0, tzid, iCal1),
                new iCalDateTime(1999, 3, 13, 9, 0, 0, tzid, iCal1),
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
                iCalDateTime start = occurrences[i].Period.StartTime;
                Assert.AreEqual(dt.Local, start.Local);
                Assert.IsTrue(dt.TimeZoneInfo.TimeZoneName == TimeZones[i], "Event " + dt + " should occur in the " + TimeZones[i] + " timezone");
            }

            Assert.IsTrue(occurrences.Count == DateTimes.Length, "There should be exactly " + DateTimes.Length + " occurrences; there were " + occurrences.Count);

            // Get occurrences for the 2nd event
            occurrences = evt2.GetOccurrences(
                new iCalDateTime(1996, 1, 1, tzid, iCal1),
                new iCalDateTime(1998, 4, 1, tzid, iCal1));

            iCalDateTime[] DateTimes1 = new iCalDateTime[]
            {
                new iCalDateTime(1997, 9, 2, 9, 0, 0, tzid, iCal1),
                new iCalDateTime(1997, 9, 9, 9, 0, 0, tzid, iCal1),
                new iCalDateTime(1997, 9, 16, 9, 0, 0, tzid, iCal1),
                new iCalDateTime(1997, 9, 23, 9, 0, 0, tzid, iCal1),
                new iCalDateTime(1997, 9, 30, 9, 0, 0, tzid, iCal1),
                new iCalDateTime(1997, 11, 4, 9, 0, 0, tzid, iCal1),
                new iCalDateTime(1997, 11, 11, 9, 0, 0, tzid, iCal1),
                new iCalDateTime(1997, 11, 18, 9, 0, 0, tzid, iCal1),
                new iCalDateTime(1997, 11, 25, 9, 0, 0, tzid, iCal1),
                new iCalDateTime(1998, 1, 6, 9, 0, 0, tzid, iCal1),
                new iCalDateTime(1998, 1, 13, 9, 0, 0, tzid, iCal1),
                new iCalDateTime(1998, 1, 20, 9, 0, 0, tzid, iCal1),
                new iCalDateTime(1998, 1, 27, 9, 0, 0, tzid, iCal1),
                new iCalDateTime(1998, 3, 3, 9, 0, 0, tzid, iCal1),
                new iCalDateTime(1998, 3, 10, 9, 0, 0, tzid, iCal1),
                new iCalDateTime(1998, 3, 17, 9, 0, 0, tzid, iCal1),
                new iCalDateTime(1998, 3, 24, 9, 0, 0, tzid, iCal1),
                new iCalDateTime(1998, 3, 31, 9, 0, 0, tzid, iCal1)
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
                iCalDateTime dt = (iCalDateTime)DateTimes1[i];
                iCalDateTime start = occurrences[i].Period.StartTime;
                Assert.AreEqual(dt.Local, start.Local);
                Assert.IsTrue(dt.TimeZoneInfo.TimeZoneName == TimeZones1[i], "Event " + dt + " should occur in the " + TimeZones1[i] + " timezone");
            }

            Assert.AreEqual(DateTimes1.Length, occurrences.Count, "There should be exactly " + DateTimes1.Length + " occurrences; there were " + occurrences.Count);
        }

        [Test]
        public void MERGE2()
        {
            iCalendar iCal = new iCalendar();
            iCalendar tmp_cal = iCalendar.LoadFromFile(@"Calendars\General\MERGE2.ics");
            iCal.MergeWith(tmp_cal);

            tmp_cal = iCalendar.LoadFromFile(@"Calendars\General\MERGE2.ics");

            // Compare the two calendars -- they should match exactly
            Serialization.CompareCalendars(iCal, tmp_cal);
        }

        [Test]
        public void UID1()
        {
            iCalendar iCal = iCalendar.LoadFromFile(@"Calendars\General\BINARY.ics");
            Program.TestCal(iCal);
            
            Event evt = iCal.Events["uuid1153170430406"];
            Assert.IsNotNull(evt, "Event could not be accessed by UID");
        }

        [Test]
        public void ADDEVENT1()
        {
            iCalendar iCal = iCalendar.LoadFromFile(@"Calendars\General\GEO1.ics");
            Program.TestCal(iCal);

            Event evt = iCal.Create<Event>();
            evt.Summary = "Test event";
            evt.Description = "This is an event to see if event creation works";
            evt.Start = new iCalDateTime(2006, 12, 15, "US-Eastern", iCal);
            evt.Duration = new TimeSpan(1, 0, 0);
            evt.Organizer = "dougd@daywesthealthcare.com";

            if (!Directory.Exists(@"Calendars\General\Temp"))
                Directory.CreateDirectory(@"Calendars\General\Temp");

            iCalendarSerializer serializer = new iCalendarSerializer(iCal);
            serializer.Serialize(@"Calendars\General\Temp\GEO1_Serialized.ics");
        }        
        
        [Test]
        public void LANGUAGE1()
        {
            iCalendar iCal = iCalendar.LoadFromFile(@"Calendars/General/Barça 2006 - 2007.ics");
        }

        [Test]
        public void GOOGLE1()
        {
            TZID tzid = "Europe/Berlin";
            iCalendar iCal = iCalendar.LoadFromFile(@"Calendars/General/GoogleCalendar.ics");
            Event evt = iCal.Events["594oeajmftl3r9qlkb476rpr3c@google.com"];
            Assert.IsNotNull(evt);

            iCalDateTime dtStart = new iCalDateTime(2006, 12, 18, tzid, iCal);
            iCalDateTime dtEnd = new iCalDateTime(2006, 12, 23, tzid, iCal);
            List<Occurrence> occurrences = iCal.GetOccurrences(dtStart, dtEnd);

            iCalDateTime[] DateTimes = new iCalDateTime[]
            {
                new iCalDateTime(2006, 12, 18, 7, 0, 0, tzid, iCal),
                new iCalDateTime(2006, 12, 19, 7, 0, 0, tzid, iCal),
                new iCalDateTime(2006, 12, 20, 7, 0, 0, tzid, iCal),
                new iCalDateTime(2006, 12, 21, 7, 0, 0, tzid, iCal),
                new iCalDateTime(2006, 12, 22, 7, 0, 0, tzid, iCal)
            };

            for (int i = 0; i < DateTimes.Length; i++)            
                Assert.AreEqual(DateTimes[i], occurrences[i].Period.StartTime, "Event should occur at " + DateTimes[i]);

            Assert.IsTrue(occurrences.Count == DateTimes.Length, "There should be exactly " + DateTimes.Length + " occurrences; there were " + occurrences.Count);
        }

        [Test]
        public void LOAD1()
        {
            StringReader sr = new StringReader(@"BEGIN:VCALENDAR
VERSION:2.0
PRODID:-//Apple Computer\, Inc//iCal 1.0//EN
CALSCALE:GREGORIAN
BEGIN:VEVENT
CREATED:20070404T211714Z
DTEND:20070407T010000Z
DTSTAMP:20070404T211714Z
DTSTART:20070406T230000Z
DURATION:PT2H
RRULE:FREQ=WEEKLY;UNTIL=20070801T070000Z;BYDAY=FR
SUMMARY:Friday Meetings
DTSTAMP:20040103T033800Z
SEQUENCE:1
UID:fd940618-45e2-4d19-b118-37fd7a8e3906
END:VEVENT
BEGIN:VEVENT
CREATED:20070404T204310Z
DTEND:20070416T030000Z
DTSTAMP:20070404T204310Z
DTSTART:20070414T200000Z
DURATION:P1DT7H
RRULE:FREQ=DAILY;COUNT=12;BYDAY=SA,SU
SUMMARY:Weekend Yea!
DTSTAMP:20040103T033800Z
SEQUENCE:1
UID:ebfbd3e3-cc1e-4a64-98eb-ced2598b3908
END:VEVENT
END:VCALENDAR
");
            iCalendar iCal = iCalendar.LoadFromStream(sr);
            Assert.IsTrue(iCal.Events.Count == 2, "There should be 2 events in the parsed calendar");
            Assert.IsNotNull(iCal.Events["fd940618-45e2-4d19-b118-37fd7a8e3906"], "Event fd940618-45e2-4d19-b118-37fd7a8e3906 should exist in the calendar");
            Assert.IsNotNull(iCal.Events["ebfbd3e3-cc1e-4a64-98eb-ced2598b3908"], "Event ebfbd3e3-cc1e-4a64-98eb-ced2598b3908 should exist in the calendar");
        }

        [Test]
        public void EVALUATION1()
        {
            //iCalendarCollection calendars = new iCalendarCollection();
            //calendars.Add(iCalendar.LoadFromFile(@"Calendars\Recurrence\RRULE21.ics"));
            //calendars.Add(iCalendar.LoadFromFile(@"Calendars\Recurrence\RRULE22.ics"));

            //iCalDateTime startDate = new iCalDateTime(1996, 1, 1, tzid, calendars[0]);
            //iCalDateTime endDate = new iCalDateTime(1998, 4, 1, tzid, calendars[0]);
            
            //List<iCalDateTime> DateTimes = new List<iCalDateTime>(new iCalDateTime[]
            //{
            //    new iCalDateTime(1997, 9, 2, 9, 0, 0, tzid, calendars[0]),
            //    new iCalDateTime(1997, 9, 9, 9, 0, 0, tzid, calendars[0]),
            //    new iCalDateTime(1997, 9, 16, 9, 0, 0, tzid, calendars[0]),
            //    new iCalDateTime(1997, 9, 23, 9, 0, 0, tzid, calendars[0]),
            //    new iCalDateTime(1997, 9, 30, 9, 0, 0, tzid, calendars[0]),
            //    new iCalDateTime(1997, 11, 4, 9, 0, 0, tzid, calendars[0]),
            //    new iCalDateTime(1997, 11, 11, 9, 0, 0, tzid, calendars[0]),
            //    new iCalDateTime(1997, 11, 18, 9, 0, 0, tzid, calendars[0]),
            //    new iCalDateTime(1997, 11, 25, 9, 0, 0, tzid, calendars[0]),
            //    new iCalDateTime(1998, 1, 6, 9, 0, 0, tzid, calendars[0]),
            //    new iCalDateTime(1998, 1, 13, 9, 0, 0, tzid, calendars[0]),
            //    new iCalDateTime(1998, 1, 20, 9, 0, 0, tzid, calendars[0]),
            //    new iCalDateTime(1998, 1, 27, 9, 0, 0, tzid, calendars[0]),
            //    new iCalDateTime(1998, 3, 3, 9, 0, 0, tzid, calendars[0]),
            //    new iCalDateTime(1998, 3, 10, 9, 0, 0, tzid, calendars[0]),
            //    new iCalDateTime(1998, 3, 17, 9, 0, 0, tzid, calendars[0]),
            //    new iCalDateTime(1998, 3, 24, 9, 0, 0, tzid, calendars[0]),
            //    new iCalDateTime(1998, 3, 31, 9, 0, 0, tzid, calendars[0]),
            //    new iCalDateTime(1997, 9, 10, 9, 0, 0, tzid, calendars[0]),
            //    new iCalDateTime(1997, 9, 11, 9, 0, 0, tzid, calendars[0]),
            //    new iCalDateTime(1997, 9, 12, 9, 0, 0, tzid, calendars[0]),
            //    new iCalDateTime(1997, 9, 13, 9, 0, 0, tzid, calendars[0]),
            //    new iCalDateTime(1997, 9, 14, 9, 0, 0, tzid, calendars[0]),
            //    new iCalDateTime(1997, 9, 15, 9, 0, 0, tzid, calendars[0]),                
            //});
            
            //List<Event> occurrences = new List<Event>(calendars.GetRecurrencesForRange<Event>(startDate, endDate));
            //foreach (Event evt in occurrences)
            //    Assert.IsTrue(DateTimes.Contains(evt.Start), "Event occurred on " + evt.Start + "; it should not have");
            //foreach(iCalDateTime dt in DateTimes)
            //{
            //    bool isFound = false;
            //    foreach (Event evt in occurrences)
            //    {
            //        if (evt.Start.Equals(dt))
            //        {
            //            isFound = true;
            //            break;
            //        }
            //    }
            //    Assert.IsTrue(isFound, "Event should occur on " + dt);
            //}
                    

            //Assert.IsTrue(occurrences.Count == DateTimes.Count, "There should be exactly " + DateTimes.Count + " occurrences; there were " + occurrences.Count);
        }
    }
}
