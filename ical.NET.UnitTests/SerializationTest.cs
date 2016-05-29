using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Ical.Net;
using Ical.Net.DataTypes;
using Ical.Net.ExtensionMethods;
using Ical.Net.General;
using Ical.Net.Interfaces;
using Ical.Net.Interfaces.Components;
using Ical.Net.Interfaces.DataTypes;
using Ical.Net.Interfaces.General;
using Ical.Net.Interfaces.Serialization;
using Ical.Net.Serialization;
using Ical.Net.Serialization.iCalendar.Serializers;
using Ical.Net.Serialization.iCalendar.Serializers.Components;
using Ical.Net.Serialization.iCalendar.Serializers.Other;
using NUnit.Framework;

namespace ical.NET.UnitTests
{
    [TestFixture]
    public class SerializationTest
    {
        private string _tzid;

        [TestFixtureSetUp]
        public void InitAll()
        {
            _tzid = "US-Eastern";
        }

        private void SerializeTest(string filename, Type iCalSerializerType) { SerializeTest(filename, typeof(Calendar), iCalSerializerType); }
        private void SerializeTest(string filename, Type iCalType, Type iCalSerializerType)
        {
            if (!Directory.Exists(@"Calendars\Serialization\Temp"))
                Directory.CreateDirectory(@"Calendars\Serialization\Temp");

            var serializer = Activator.CreateInstance(iCalSerializerType) as ISerializer;
            Assert.IsNotNull(serializer);

            // Set the iCalendar type for deserialization
            serializer.GetService<ISerializationSettings>().CalendarType = iCalType;

            // Load the calendar from file
            var iCal1 = Calendar.LoadFromFile(@"Calendars\Serialization\" + filename, Encoding.UTF8, serializer)[0];

            Assert.IsTrue(iCal1.UniqueComponents.Count > 0, "iCalendar has no unique components; it must to be used in SerializeTest(). Did it load correctly?");

            var fs = new FileStream(@"Calendars\Serialization\Temp\" + Path.GetFileNameWithoutExtension(filename) + "_Serialized" + Path.GetExtension(filename), FileMode.Create, FileAccess.Write);
            serializer.Serialize(iCal1, fs, Encoding.UTF8);
            fs.Close();

            var iCal2 = Calendar.LoadFromFile(@"Calendars\Serialization\Temp\" + Path.GetFileNameWithoutExtension(filename) + "_Serialized" + Path.GetExtension(filename), Encoding.UTF8, serializer)[0];

            CompareCalendars(iCal1, iCal2);
        }

        public static void CompareCalendars(ICalendar cal1, ICalendar cal2)
        {
            CompareComponents(cal1, cal2);

            Assert.AreEqual(cal1.Children.Count, cal2.Children.Count, "Children count is different between calendars.");

            for (var i = 0; i < cal1.Children.Count; i++)
            {
                var component1 = cal1.Children[i] as ICalendarComponent;
                var component2 = cal2.Children[i] as ICalendarComponent;
                if (component1 != null && component2 != null)
                {
                    CompareComponents(component1, component2);
                }
            }
        }

        public static void CompareComponents(ICalendarComponent cb1, ICalendarComponent cb2)
        {
            foreach (var p1 in cb1.Properties)
            {
                var isMatch = false;
                foreach (var p2 in cb2.Properties.AllOf(p1.Name))
                {
                    try
                    {
                        Assert.AreEqual(p1, p2, "The properties '" + p1.Name + "' are not equal.");
                        if (p1.Value is IComparable)
                            Assert.AreEqual(0, ((IComparable)p1.Value).CompareTo(p2.Value), "The '" + p1.Name + "' property values do not match.");
                        else if (p1.Value is IEnumerable)
                            CompareEnumerables((IEnumerable)p1.Value, (IEnumerable)p2.Value, p1.Name);
                        else
                            Assert.AreEqual(p1.Value, p2.Value, "The '" + p1.Name + "' property values are not equal.");

                        isMatch = true;
                        break;
                    }
                    catch { }
                }

                Assert.IsTrue(isMatch, "Could not find a matching property - " + p1.Name + ":" + (p1.Value != null ? p1.Value.ToString() : string.Empty));                    
            }

            Assert.AreEqual(cb1.Children.Count, cb2.Children.Count, "The number of children are not equal.");
            for (var i = 0; i < cb1.Children.Count; i++)
            {
                var child1 = cb1.Children[i] as ICalendarComponent;
                var child2 = cb2.Children[i] as ICalendarComponent;
                if (child1 != null && child2 != null)
                    CompareComponents(child1, child2);
                else
                    Assert.AreEqual(child1, child2, "The child objects are not equal.");
            }
        }

        public static void CompareEnumerables(IEnumerable a1, IEnumerable a2, string value)
        {
            if (a1 == null && a2 == null)
                return;

            Assert.IsFalse((a1 == null && a2 != null) || (a1 != null && a2 == null), value + " do not match - one item is null");

            var enum1 = a1.GetEnumerator();
            var enum2 = a2.GetEnumerator();

            while (enum1.MoveNext() && enum2.MoveNext())
                Assert.AreEqual(enum1.Current, enum2.Current, value + " do not match");
        }

        /// <summary>
        /// Ensures that a basic, binary attachment functions as it should.
        /// </summary>
        //[Test, Category("Serialization")]     //Broken in dday
        public void Attachment1()
        {
            ICalendar cal = new Calendar();

            // Create a test event
            IEvent evt = cal.Create<Event>();
            evt.Summary = "Test Event";
            evt.Start = new CalDateTime(2007, 10, 15, 8, 0, 0);
            evt.Duration = TimeSpan.FromHours(1);

            // Add an attachment to this event
            IAttachment attachment = new Attachment();
            attachment.Data = ReadBinary(@"Data\Test.doc");
            attachment.Parameters.Add("X-FILENAME", "WordDocument.doc");
            evt.Attachments.Add(attachment);

            var serializer = new CalendarSerializer();
            if (!Directory.Exists(@"Calendars\Serialization\Temp"))
                Directory.CreateDirectory(@"Calendars\Serialization\Temp");
            serializer.Serialize(cal, @"Calendars\Serialization\Temp\Attachment1.ics");

            cal = Calendar.LoadFromFile(@"Calendars\Serialization\Temp\Attachment1.ics")[0];
            evt = cal.Events.First();
            attachment = evt.Attachments[0];

            Assert.IsTrue(CompareBinary(@"Data\Test.doc", attachment.Data), "Serialized version of Test.doc did not match the deserialized version.");
        }

        /// <summary>
        /// Ensures that very large attachments function as they should.
        /// </summary>
        //[Test, Category("Serialization")]     //Broken in dday
        public void Attachment2()
        {
            ICalendar cal = new Calendar();

            // Create a test event
            IEvent evt = cal.Create<Event>();
            evt.Summary = "Test Event";
            evt.Start = new CalDateTime(2007, 10, 15, 8, 0, 0);
            evt.Duration = TimeSpan.FromHours(1);

            // Get a data file
            var loremIpsum = UnicodeEncoding.Default.GetString(ReadBinary(@"Data\LoremIpsum.txt"));
            var sb = new StringBuilder();
            // If we copy it 300 times, we should end up with a file over 2.5MB in size.
            for (var i = 0; i < 300; i++)
                sb.AppendLine(loremIpsum);

            // Add an attachment to this event
            IAttachment attachment = new Attachment();
            attachment.Data = UnicodeEncoding.Default.GetBytes(sb.ToString());
            evt.Attachments.Add(attachment);

            var serializer = new CalendarSerializer();
            if (!Directory.Exists(@"Calendars\Serialization\Temp"))
                Directory.CreateDirectory(@"Calendars\Serialization\Temp");
            serializer.Serialize(cal, @"Calendars\Serialization\Temp\Attachment2.ics");

            cal = Calendar.LoadFromFile(@"Calendars\Serialization\Temp\Attachment2.ics")[0];
            evt = cal.Events.First();
            attachment = evt.Attachments[0];

            // Ensure the generated and serialized strings match
            Assert.AreEqual(sb.ToString(), UnicodeEncoding.Default.GetString(attachment.Data));

            // Times to finish the test for attachment file sizes (on my computer, version 0.80): 
            //  0.92MB = 1.2 seconds
            //  2.76MB = 6 seconds
            //  4.6MB = 15.1 seconds
            //  9.2MB = 54 seconds
        }

        //[Test, Category("Serialization")]     //Broken in dday
        public void Attachment3()
        {
            SerializeTest("Attachment3.ics", typeof(CalendarSerializer));
        }

        /// <summary>
        /// At times, this may throw a WebException if an internet connection is not present.
        /// This is safely ignored.
        /// </summary>
        [Test, ExpectedException(typeof(WebException))]
        public void Attachment4()
        {
            var iCal = Calendar.LoadFromFile(@"Calendars\Serialization\Attachment4.ics")[0];
            ProgramTest.TestCal(iCal);

            var evt = iCal.Events["uuid1153170430406"];
            Assert.IsNotNull(evt, "Event could not be accessed by UID");

            var a = evt.Attachments[0];
            a.LoadDataFromUri();
            Assert.IsNotNull(a.Data);
            Assert.AreNotEqual(0, a.Data.Length);

            var ms = new MemoryStream();
            ms.SetLength(a.Data.Length);
            a.Data.CopyTo(ms.GetBuffer(), 0);

            var iCal1 = Calendar.LoadFromStream(ms)[0];
            Assert.IsNotNull(iCal1, "Attached iCalendar did not load correctly");

            throw new WebException();
        }

        [Test, Category("Serialization")]
        public void Attendee1()
        {
            var iCal = Calendar.LoadFromFile(typeof(Calendar), @"Calendars\Serialization\Attendee1.ics", Encoding.UTF8)[0];
            Assert.AreEqual(1, iCal.Events.Count);
            
            var evt = iCal.Events.First();
            // Ensure there are 2 attendees
            Assert.AreEqual(2, evt.Attendees.Count);            

            var attendee1 = evt.Attendees[0];
            var attendee2 = evt.Attendees[1];

            // Values
            Assert.AreEqual(new Uri("mailto:joecool@example.com"), attendee1.Value);
            Assert.AreEqual(new Uri("mailto:ildoit@example.com"), attendee2.Value);

            // MEMBERS
            Assert.AreEqual(1, attendee1.Members.Count);
            Assert.AreEqual(0, attendee2.Members.Count);
            Assert.AreEqual(new Uri("mailto:DEV-GROUP@example.com"), attendee1.Members[0]);

            // DELEGATED-FROM
            Assert.AreEqual(0, attendee1.DelegatedFrom.Count);
            Assert.AreEqual(1, attendee2.DelegatedFrom.Count);
            Assert.AreEqual(new Uri("mailto:immud@example.com"), attendee2.DelegatedFrom[0]);

            // DELEGATED-TO
            Assert.AreEqual(0, attendee1.DelegatedTo.Count);
            Assert.AreEqual(0, attendee2.DelegatedTo.Count);
        }

        /// <summary>
        /// Tests that multiple parameters of the
        /// same name are correctly aggregated into
        /// a single list.
        /// </summary>
        [Test, Category("Serialization")]
        public void Attendee2()
        {
            var iCal = Calendar.LoadFromFile(typeof(Calendar), @"Calendars\Serialization\Attendee2.ics", Encoding.UTF8)[0];
            Assert.AreEqual(1, iCal.Events.Count);

            var evt = iCal.Events.First();
            // Ensure there is 1 attendee
            Assert.AreEqual(1, evt.Attendees.Count);

            var attendee1 = evt.Attendees[0];

            // Values
            Assert.AreEqual(new Uri("mailto:joecool@example.com"), attendee1.Value);

            // MEMBERS
            Assert.AreEqual(3, attendee1.Members.Count);
            Assert.AreEqual(new Uri("mailto:DEV-GROUP@example.com"), attendee1.Members[0]);
            Assert.AreEqual(new Uri("mailto:ANOTHER-GROUP@example.com"), attendee1.Members[1]);
            Assert.AreEqual(new Uri("mailto:THIRD-GROUP@example.com"), attendee1.Members[2]);
        }

        /// <summary>
        /// Tests that Lotus Notes-style properties are properly handled.
        /// https://sourceforge.net/tracker/?func=detail&aid=2033495&group_id=187422&atid=921236
        /// Sourceforge bug #2033495
        /// </summary>
        [Test, Category("Serialization")]
        public void Bug2033495()
        {
            var iCal = Calendar.LoadFromFile(typeof(Calendar), @"Calendars\Serialization\Bug2033495.ics", Encoding.UTF8)[0];
            Assert.AreEqual(1, iCal.Events.Count);
            Assert.AreEqual(iCal.Properties["X-LOTUS-CHILD_UID"].Value, "XXX");
        }

        /// <summary>
        /// Tests bug #2148092 - Percent compelete serialization error
        /// </summary>
        //[Test, Category("Serialization")]     //Broken in dday
        public void Bug2148092()
        {
            SerializeTest("Bug2148092.ics", typeof(CalendarSerializer));
        }

        /// <summary>
        /// Tests bug #2938007 - involving the HasTime property in IDateTime values.
        /// See https://sourceforge.net/tracker/?func=detail&aid=2938007&group_id=187422&atid=921236
        /// </summary>
        [Test, Category("Serialization")]
        public void Bug2938007()
        {
            var iCal = Calendar.LoadFromFile(@"Calendars\Serialization\Bug2938007.ics")[0];
            Assert.AreEqual(1, iCal.Events.Count);

            var evt = iCal.Events.First();
            Assert.AreEqual(true, evt.Start.HasTime);
            Assert.AreEqual(true, evt.End.HasTime);

            foreach (var o in evt.GetOccurrences(new CalDateTime(2010, 1, 17, 0, 0, 0), new CalDateTime(2010, 2, 1, 0, 0, 0)))
            {
                Assert.AreEqual(true, o.Period.StartTime.HasTime);
                Assert.AreEqual(true, o.Period.EndTime.HasTime);
            }
        }

        /// <summary>
        /// Tests bug #3177278 - Serialize closes stream
        /// See https://sourceforge.net/tracker/?func=detail&aid=3177278&group_id=187422&atid=921236
        /// </summary>
        [Test, Category("Serialization")]
        public void Bug3177278()
        {
            var calendar = new Calendar();
            var serializer = new CalendarSerializer();

            var ms = new MemoryStream();
            serializer.Serialize(calendar, ms, Encoding.UTF8);

            Assert.IsTrue(ms.CanWrite);
        }

        /// <summary>
        /// Tests bug #3211934 - Bug in iCalendar.cs - UnauthorizedAccessException
        /// See https://sourceforge.net/tracker/?func=detail&aid=3211934&group_id=187422&atid=921236
        /// </summary>
        [Test, Category("Serialization")]
        public void Bug3211934()
        {
            var calendar = new Calendar();
            var serializer = new CalendarSerializer();

            var filename = "Bug3211934.ics";

            if (File.Exists(filename))
            {
                // Reset the file attributes and delete
                File.SetAttributes(filename, FileAttributes.Normal);
                File.Delete(filename);
            }

            serializer.Serialize(calendar, filename);

            // Set the file as read-only
            File.SetAttributes(filename, FileAttributes.ReadOnly);

            // Load the calendar from file, and ensure the read-only attribute doesn't affect the load
            var calendars = Calendar.LoadFromFile(filename, Encoding.UTF8, serializer);
            Assert.IsNotNull(calendars);

            // Reset the file attributes and delete
            File.SetAttributes(filename, FileAttributes.Normal);
            File.Delete(filename);
        }

        //[Test, Category("Serialization")]     //Broken in dday
        public void Calendar1()
        {
            SerializeTest("Calendar1.ics", typeof(CalendarSerializer));
        }

        /// <summary>
        /// Verifies that a calendar will load without a VERSION or PRODID
        /// specification.
        /// </summary>
        //[Test, Category("Serialization")]     //Broken in dday
        public void CalendarParameters2()
        {
            SerializeTest("CalendarParameters2.ics", typeof(CalendarSerializer));
        }

        //[Test, Category("Serialization")]     //Broken in dday
        public void CaseInsensitive1()
        {
            SerializeTest("CaseInsensitive1.ics", typeof(CalendarSerializer));
        }

        //[Test, Category("Serialization")]     //Broken in dday
        public void CaseInsensitive2()
        {
            SerializeTest("CaseInsensitive2.ics", typeof(CalendarSerializer));
        }

        //[Test, Category("Serialization")]     //Broken in dday
        public void CaseInsensitive3()
        {
            SerializeTest("CaseInsensitive3.ics", typeof(CalendarSerializer));
        }

        /// <summary>
        /// Tests that a mixed-case VERSION property is loaded properly
        /// </summary>
        [Test, Category("Serialization")]
        public void CaseInsensitive4()
        {
            var iCal = Calendar.LoadFromFile(@"Calendars\Serialization\CaseInsensitive4.ics")[0];
            Assert.AreEqual("2.5", iCal.Version);
        }

        //[Test, Category("Serialization")]     //Broken in dday
        public void Categories1_1()
        {
            SerializeTest("Categories1.ics", typeof(CalendarSerializer));
        }

        [Test, Category("Serialization")]
        public void Categories1_2()
        {
            var iCal = Calendar.LoadFromFile(@"Calendars\Serialization\Categories1.ics")[0];
            ProgramTest.TestCal(iCal);
            var evt = iCal.Events.First();

            var items = new ArrayList();
            items.AddRange(new[]
            {
                "One", "Two", "Three",
                "Four", "Five", "Six",
                "Seven", "A string of text with nothing less than a comma, semicolon; and a newline\n."
            });

            var found = new Hashtable();

            foreach (var s in evt.Categories)
            {
                if (items.Contains(s))
                    found[s] = true;
            }

            foreach (string item in items)
                Assert.IsTrue(found.ContainsKey(item), "Event should contain CATEGORY '" + item + "', but it was not found.");
        }

        [Test, Category("Serialization")]
        public void EmptyLines1()
        {
            var iCal = Calendar.LoadFromFile(@"Calendars\Serialization\EmptyLines1.ics")[0];
            Assert.AreEqual(2, iCal.Events.Count, "iCalendar should have 2 events");
        }

        [Test, Category("Serialization")]
        public void EmptyLines2()
        {
            var calendars = Calendar.LoadFromFile(@"Calendars\Serialization\EmptyLines2.ics");
            Assert.AreEqual(2, calendars.Count);
            Assert.AreEqual(2, calendars[0].Events.Count, "iCalendar should have 2 events");
            Assert.AreEqual(2, calendars[1].Events.Count, "iCalendar should have 2 events");
        }

        /// <summary>
        /// Verifies that blank lines between components are allowed
        /// (as occurs with some applications/parsers - i.e. KOrganizer)
        /// </summary>
        [Test, Category("Serialization")]
        public void EmptyLines3()
        {
            var iCal = Calendar.LoadFromFile(@"Calendars\Serialization\EmptyLines3.ics")[0];
            Assert.AreEqual(1, iCal.Todos.Count, "iCalendar should have 1 todo");
        }

        /// <summary>
        /// Similar to PARSE4 and PARSE5 tests.
        /// </summary>
        [Test, Category("Serialization")]
        public void EmptyLines4()
        {
            var iCal = Calendar.LoadFromFile(@"Calendars\Serialization\EmptyLines4.ics")[0];
            Assert.AreEqual(28, iCal.Events.Count);
        }

        [Test]
        public void Encoding2()
        {
            var iCal = Calendar.LoadFromFile(@"Calendars\Serialization\Encoding2.ics")[0];
            ProgramTest.TestCal(iCal);
            var evt = iCal.Events.First();

            Assert.AreEqual(
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
"This is a test to try out base64 encoding without being too large.",
                evt.Attachments[0].Value,
                "Attached value does not match.");
        }

        [Test]
        public void Encoding3()
        {
            var iCal = Calendar.LoadFromFile(@"Calendars\Serialization\Encoding3.ics")[0];
            ProgramTest.TestCal(iCal);
            var evt = iCal.Events.First();

            Assert.AreEqual("uuid1153170430406", evt.Uid, "UID should be 'uuid1153170430406'; it is " + evt.Uid);
            Assert.AreEqual(1, evt.Sequence, "SEQUENCE should be 1; it is " + evt.Sequence);
        }

        //[Test, Category("Serialization")]     //Broken in dday
        public void Event1()
        {
            SerializeTest("Event1.ics", typeof(CalendarSerializer));
        }

        //[Test, Category("Serialization")]     //Broken in dday
        public void Event2()
        {
            SerializeTest("Event2.ics", typeof(CalendarSerializer));
        }

        //[Test, Category("Serialization")]     //Broken in dday
        public void Event3()
        {
            SerializeTest("Event3.ics", typeof(CalendarSerializer));
        }

        //[Test, Category("Serialization")]     //Broken in dday
        public void Event4()
        {
            SerializeTest("Event4.ics", typeof(CalendarSerializer));
        }

        //[Test, Category("Serialization")]     //Broken in dday
        public void Event5()
        {
            var iCal = new Calendar();

            var evt = iCal.Create<Event>();
            evt.Summary = "Test event title";
            evt.Start = new CalDateTime(2007, 3, 19);
            evt.Start.IsUniversalTime = true;
            evt.Duration = new TimeSpan(24, 0, 0);
            evt.Created = evt.Start.Copy<IDateTime>();
            evt.DtStamp = evt.Start.Copy<IDateTime>();
            evt.Uid = "123456789";
            evt.IsAllDay = true;

            var rec = new RecurrencePattern("FREQ=WEEKLY;INTERVAL=3;BYDAY=TU,FR,SU;COUNT=4");
            evt.RecurrenceRules.Add(rec);

            var serializer = new CalendarSerializer();
            var icalString = serializer.SerializeToString(iCal);

            Assert.IsNotEmpty(icalString, "iCalendarSerializer.SerializeToString() must not be empty");

            var eventSerializer = new EventSerializer();
            var evtString = eventSerializer.SerializeToString(evt);

            Assert.IsTrue(evtString.Equals("BEGIN:VEVENT\r\nCREATED:20070319T000000Z\r\nDTEND;VALUE=DATE:20070320\r\nDTSTAMP:20070319T000000Z\r\nDTSTART;VALUE=DATE:20070319\r\nRRULE:FREQ=WEEKLY;INTERVAL=3;COUNT=4;BYDAY=TU,FR,SU\r\nSEQUENCE:0\r\nSUMMARY:Test event title\r\nUID:123456789\r\nEND:VEVENT\r\n"), "ComponentBaseSerializer.SerializeToString() serialized incorrectly");

            serializer.Serialize(iCal, @"Calendars\Serialization\Event5.ics");
            SerializeTest("Event5.ics", typeof(CalendarSerializer));
        }

        //[Test, Category("Serialization")]     //Broken in dday
        public void Event6()
        {
            var iCal = new Calendar();

            var evt = iCal.Create<Event>();
            evt.Summary = "Test event title";
            evt.Start = new CalDateTime(2007, 4, 29);
            evt.End = evt.Start.AddDays(1);
            evt.IsAllDay = true;

            var rec = new RecurrencePattern("FREQ=WEEKLY;INTERVAL=3;BYDAY=TU,FR,SU;COUNT=4");
            evt.RecurrenceRules.Add(rec);

            var eventSerializer = new EventSerializer();

            var fs = new FileStream(@"Calendars\Serialization\Event6.ics", FileMode.Create, FileAccess.Write);
            eventSerializer.Serialize(evt, fs, Encoding.UTF8);
            fs.Close();

            var iCal1 = new Calendar();

            fs = new FileStream(@"Calendars\Serialization\Event6.ics", FileMode.Open, FileAccess.Read);
            var evt1 = CalendarComponent.LoadFromStream<Event>(fs, Encoding.UTF8);
            fs.Close();

            CompareComponents(evt, evt1);
        }

        //[Test, Category("Serialization")]     //Broken in dday
        public void Event7()
        {
            var iCalString = @"BEGIN:VCALENDAR
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
";
            var sr = new StringReader(iCalString);
            var calendar = Calendar.LoadFromStream(sr)[0];

            Assert.IsTrue(calendar.Events.Count == 2, "There should be 2 events in the loaded iCalendar.");
            Assert.IsNotNull(calendar.Events["fd940618-45e2-4d19-b118-37fd7a8e3906"], "There should be an event with UID: fd940618-45e2-4d19-b118-37fd7a8e3906");
            Assert.IsNotNull(calendar.Events["ebfbd3e3-cc1e-4a64-98eb-ced2598b3908"], "There should be an event with UID: ebfbd3e3-cc1e-4a64-98eb-ced2598b3908");

            var serializer = new CalendarSerializer();
            serializer.Serialize(calendar, @"Calendars\Serialization\Event7.ics");

            SerializeTest("Event7.ics", typeof(CalendarSerializer));
        }

        [Test, Category("Serialization")]
        public void Event8()
        {
            var sr = new StringReader(@"BEGIN:VCALENDAR
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
            var iCal = Calendar.LoadFromStream(sr)[0];
            Assert.IsTrue(iCal.Events.Count == 2, "There should be 2 events in the parsed calendar");
            Assert.IsNotNull(iCal.Events["fd940618-45e2-4d19-b118-37fd7a8e3906"], "Event fd940618-45e2-4d19-b118-37fd7a8e3906 should exist in the calendar");
            Assert.IsNotNull(iCal.Events["ebfbd3e3-cc1e-4a64-98eb-ced2598b3908"], "Event ebfbd3e3-cc1e-4a64-98eb-ced2598b3908 should exist in the calendar");
        }

        //[Test, Category("Serialization")]     //Broken in dday
        public void FreeBusy1()
        {
            ICalendar cal = new Calendar();

            IEvent evt = cal.Create<Event>();
            evt.Summary = "Test event";
            evt.Start = new CalDateTime(2010, 10, 1, 8, 0, 0);
            evt.End = new CalDateTime(2010, 10, 1, 9, 0, 0);

            ICalendar freeBusyCalendar = new Calendar();
            var freeBusy = cal.GetFreeBusy(new CalDateTime(2010, 10, 1, 0, 0, 0), new CalDateTime(2010, 10, 7, 11, 59, 59));
            freeBusyCalendar.AddChild(freeBusy);

            var serializer = new CalendarSerializer();
            serializer.Serialize(freeBusyCalendar, @"Calendars\Serialization\FreeBusy1.ics");

            SerializeTest("FreeBusy1.ics", typeof(CalendarSerializer));
        }

        //[Test, Category("Serialization")]     //Broken in dday
        public void FreeBusy2()
        {
            ICalendar cal = new Calendar();

            IEvent evt = cal.Create<Event>();
            evt.Summary = "Test event";
            evt.Start = new CalDateTime(2010, 10, 1, 8, 0, 0);
            evt.End = new CalDateTime(2010, 10, 1, 9, 0, 0);

            IAttendee attendee = new Attendee("mailto:test@test.com");
            attendee.ParticipationStatus = ParticipationStatus.Tentative;
            evt.Attendees.Add(attendee);

            ICalendar freeBusyCalendar = new Calendar();
            var freeBusy = cal.GetFreeBusy(
                null, 
                new IAttendee[] { new Attendee("mailto:test@test.com") }, 
                new CalDateTime(2010, 10, 1, 0, 0, 0), 
                new CalDateTime(2010, 10, 7, 11, 59, 59));

            freeBusyCalendar.AddChild(freeBusy);

            var serializer = new CalendarSerializer();
            serializer.Serialize(freeBusyCalendar, @"Calendars\Serialization\FreeBusy2.ics");

            SerializeTest("FreeBusy2.ics", typeof(CalendarSerializer));
        }

        //[Test, Category("Serialization")]     //Broken in dday
        public void GeographicLocation1_1()
        {
            SerializeTest("GeographicLocation1.ics", typeof(CalendarSerializer));
        }

        [Test]
        public void GeographicLocation1_2()
        {
            var iCal = Calendar.LoadFromFile(@"Calendars\Serialization\GeographicLocation1.ics")[0];
            ProgramTest.TestCal(iCal);
            var evt = iCal.Events.First();

            Assert.AreEqual(37.386013, evt.GeographicLocation.Latitude, "Latitude should be 37.386013; it is not.");
            Assert.AreEqual(-122.082932, evt.GeographicLocation.Longitude, "Longitude should be -122.082932; it is not.");
        }

        [Test, Category("Serialization")]
        public void Google1()
        {
            var tzId = "Europe/Berlin";
            var iCal = Calendar.LoadFromFile(@"Calendars/Serialization/Google1.ics")[0];
            var evt = iCal.Events["594oeajmftl3r9qlkb476rpr3c@google.com"];
            Assert.IsNotNull(evt);

            IDateTime dtStart = new CalDateTime(2006, 12, 18, tzId);
            IDateTime dtEnd = new CalDateTime(2006, 12, 23, tzId);
            var occurrences = iCal.GetOccurrences(dtStart, dtEnd).OrderBy(o => o.Period.StartTime).ToList();

            var dateTimes = new[]
            {
                new CalDateTime(2006, 12, 18, 7, 0, 0, tzId),
                new CalDateTime(2006, 12, 19, 7, 0, 0, tzId),
                new CalDateTime(2006, 12, 20, 7, 0, 0, tzId),
                new CalDateTime(2006, 12, 21, 7, 0, 0, tzId),
                new CalDateTime(2006, 12, 22, 7, 0, 0, tzId)
            };

            for (var i = 0; i < dateTimes.Length; i++)
                Assert.AreEqual(dateTimes[i], occurrences[i].Period.StartTime, "Event should occur at " + dateTimes[i]);

            Assert.AreEqual(dateTimes.Length, occurrences.Count, "There should be exactly " + dateTimes.Length + " occurrences; there were " + occurrences.Count);
        }

        /// <summary>
        /// Tests that valid RDATE properties are parsed correctly.
        /// </summary>
        [Test, Category("Serialization")]
        public void RecurrenceDates1()
        {
            var iCal = Calendar.LoadFromFile(@"Calendars\Serialization\RecurrenceDates1.ics")[0];
            Assert.AreEqual(1, iCal.Events.Count);
            Assert.AreEqual(3, iCal.Events.First().RecurrenceDates.Count);
            
            Assert.AreEqual((CalDateTime)new DateTime(1997, 7, 14, 12, 30, 0, DateTimeKind.Utc), iCal.Events.First().RecurrenceDates[0][0].StartTime);
            Assert.AreEqual((CalDateTime)new DateTime(1996, 4, 3, 2, 0, 0, DateTimeKind.Utc), iCal.Events.First().RecurrenceDates[1][0].StartTime);
            Assert.AreEqual((CalDateTime)new DateTime(1996, 4, 3, 4, 0, 0, DateTimeKind.Utc), iCal.Events.First().RecurrenceDates[1][0].EndTime);
            Assert.AreEqual(new CalDateTime(1997, 1, 1), iCal.Events.First().RecurrenceDates[2][0].StartTime);
            Assert.AreEqual(new CalDateTime(1997, 1, 20), iCal.Events.First().RecurrenceDates[2][1].StartTime);
            Assert.AreEqual(new CalDateTime(1997, 2, 17), iCal.Events.First().RecurrenceDates[2][2].StartTime);
            Assert.AreEqual(new CalDateTime(1997, 4, 21), iCal.Events.First().RecurrenceDates[2][3].StartTime);
            Assert.AreEqual(new CalDateTime(1997, 5, 26), iCal.Events.First().RecurrenceDates[2][4].StartTime);
            Assert.AreEqual(new CalDateTime(1997, 7, 4), iCal.Events.First().RecurrenceDates[2][5].StartTime);
            Assert.AreEqual(new CalDateTime(1997, 9, 1), iCal.Events.First().RecurrenceDates[2][6].StartTime);
            Assert.AreEqual(new CalDateTime(1997, 10, 14), iCal.Events.First().RecurrenceDates[2][7].StartTime);
            Assert.AreEqual(new CalDateTime(1997, 11, 28), iCal.Events.First().RecurrenceDates[2][8].StartTime);
            Assert.AreEqual(new CalDateTime(1997, 11, 29), iCal.Events.First().RecurrenceDates[2][9].StartTime);
            Assert.AreEqual(new CalDateTime(1997, 12, 25), iCal.Events.First().RecurrenceDates[2][10].StartTime);
        }


        /// <summary>
        /// Tests that valid REQUEST-STATUS properties are parsed correctly.
        /// </summary>
        [Test, Category("Serialization")]
        public void RequestStatus1()
        {
            var iCal = Calendar.LoadFromFile(@"Calendars\Serialization\RequestStatus1.ics")[0];
            Assert.AreEqual(1, iCal.Events.Count);
            Assert.AreEqual(4, iCal.Events.First().RequestStatuses.Count);

            var rs = iCal.Events.First().RequestStatuses[0];
            Assert.AreEqual(2, rs.StatusCode.Primary);
            Assert.AreEqual(0, rs.StatusCode.Secondary);
            Assert.AreEqual("Success", rs.Description);
            Assert.IsNull(rs.ExtraData);

            rs = iCal.Events.First().RequestStatuses[1];
            Assert.AreEqual(3, rs.StatusCode.Primary);
            Assert.AreEqual(1, rs.StatusCode.Secondary);
            Assert.AreEqual("Invalid property value", rs.Description);
            Assert.AreEqual("DTSTART:96-Apr-01", rs.ExtraData);

            rs = iCal.Events.First().RequestStatuses[2];
            Assert.AreEqual(2, rs.StatusCode.Primary);
            Assert.AreEqual(8, rs.StatusCode.Secondary);
            Assert.AreEqual(" Success, repeating event ignored. Scheduled as a single event.", rs.Description);
            Assert.AreEqual("RRULE:FREQ=WEEKLY;INTERVAL=2", rs.ExtraData);

            rs = iCal.Events.First().RequestStatuses[3];
            Assert.AreEqual(4, rs.StatusCode.Primary);
            Assert.AreEqual(1, rs.StatusCode.Secondary);
            Assert.AreEqual("Event conflict. Date/time is busy.", rs.Description);
            Assert.IsNull(rs.ExtraData);
        }

        //[Test, Category("Serialization")]     //Broken in dday
        public void String1()
        {
            ICalendar cal = new Calendar();
            var evt = cal.Create<Event>();
            evt.Start = CalDateTime.Now;
            evt.Duration = TimeSpan.FromHours(1);
            evt.Summary = @"
Thank you for purchasing tickets on Ticketmaster.
Your order number for this purchase is 19-36919/UK1.

Tickets will be despatched as soon as possible, but may not be received until 7-10 days before the event. Please do not contact us unless you have not received your tickets within 7 days of the event.


You purchased 2 tickets to: 
_____________________________________________________________________________________________ 
Prince
The O2, London, UK
Fri 31 Aug 2007, 18:00 

Seat location: section BK 419, row M, seats 912-913
Total Charge: £69.42

http://ads.as4x.tmcs.ticketmaster.com/click.ng/site=tm&pagepos=531&adsize=336x102&lang=en-uk&majorcatid=10001&minorcatid=1&event_id=12003EA8AD65189AD&venueid=148826&artistid=135895&promoter=161&TransactionID=0902229695751936911UKA
Thanks again for using Ticketmaster.
Show complete  HYPERLINK ""http://ntr.ticketmaster.com:80/ssp/?&C=%39%33%30%30%35%5F%33%30%33&R=%6F%6C%5F%31%33%31&U=%31%39%2D%33%36%41%31%39%2F%55%4B%31&M=%35&B=%32%2E%30&S=%68%80%74%70%73%3A%2F%3F%77%77%77%2E%74%80%63%6B%65%71%6D%61%73%74%65%72%2E%63%6F%2E"" \t ""_blank"" order detail.
You can always check your order and manage your preferences in  HYPERLINK ""http://ntr.ticketmaster.com:80/ssp/?&C=%39%33%30%30%30%5F%33%30%33&R=%6F%6C%5F%6D%65%6D%62%65%72&U=%31%39%2D%33%36%39%31%39%2F%55%4B%31&M=%31&B=%32%2E%30&S=%68%74%74%70%73%3A%2F%2F%77%"" \t ""_blank"" My Ticketmaster. 

_____________________________________________________________________________________________

C  U  S  T  O  M  E  R      S  E  R  V  I  C  E 
_____________________________________________________________________________________________

If you have any questions regarding your booking you can search for answers using our online helpdesk at http://ticketmaster.custhelp.com

You can search our extensive range of answers and in the unlikely event that you cannot find an answer to your query, you can use 'Ask a Question' to contact us directly.



_____________________________________________________________________________________________
This email confirms your ticket order, so print/save it for future reference. All purchases are subject to credit card approval and billing address verification. We make every effort to be accurate, but we cannot be responsible for changes, cancellations, or postponements announced after this email is sent. 
Please do not reply to this email. Replies to this email will not be responded to or read. If you have any questions or comments,  HYPERLINK ""http://ntr.ticketmaster.com:80/ssp/?&C=%39%33%30%30%30%5F%33%30%33&R=%32&U=%31%39%2D%33%36%39%31%39%2F%55%4B%31&M=%31&B=%32%2E%30&S=%68%74%74%70%3A%2F%2F%77%77%77%2E%74%69%63%6B%65%74%6D%61%73%74%65%72%2E%63%6F%2E%75%6B%2F%68%2F%63%75%73%74%6F%6D%65%72%5F%73%65%72%76%65%2E%68%74%6D%6C"" \t ""_blank"" contact us.

Ticketmaster UK Limited Registration in England No 2662632, Registered Office, 48 Leicester Square, London WC2H 7LR ";

            var serializer = new CalendarSerializer(cal);
            serializer.Serialize(@"Calendars\Serialization\String1.ics");

            SerializeTest("String1.ics", typeof(CalendarSerializer));
        }

        /// <summary>
        /// Tests that string escaping works with Text elements.
        /// </summary>
        [Test, Category("Serialization")]
        public void String2()
        {
            var serializer = new StringSerializer();
            var value = @"test\with\;characters";
            var unescaped = (string)serializer.Deserialize(new StringReader(value));

            Assert.AreEqual(@"test\with;characters", unescaped, "String unescaping was incorrect.");

            value = @"C:\Path\To\My\New\Information";
            unescaped = (string)serializer.Deserialize(new StringReader(value));
            Assert.AreEqual("C:\\Path\\To\\My\new\\Information", unescaped, "String unescaping was incorrect.");

            value = @"\""This\r\nis\Na\, test\""\;\\;,";
            unescaped = (string)serializer.Deserialize(new StringReader(value));

            Assert.AreEqual("\"This\\r\nis\na, test\";\\;,", unescaped, "String unescaping was incorrect.");
        }

        //[Test]     //Broken in dday
        public void TimeZone1()
        {
            var iCal = Calendar.LoadFromFile(@"Calendars\Serialization\TimeZone1.ics")[0];

            var tz = iCal.TimeZones[0];

            var serializer = new CalendarSerializer();
            serializer.Serialize(iCal, @"Calendars\Serialization\Temp\TimeZone1.ics");

            iCal = Calendar.LoadFromFile(@"Calendars\Serialization\Temp\TimeZone1.ics")[0];
            tz = iCal.TimeZones[0];

            Assert.AreEqual(0, tz.Properties["LAST-MODIFIED"].Parameters.CountOf("VALUE"), "The \"VALUE\" parameter is not allowed on \"LAST-MODIFIED\"");
        }

        //[Test, Category("Serialization")]     //Broken in dday
        public void TimeZone3()
        {
            SerializeTest("TimeZone3.ics", typeof(CalendarSerializer));

            var iCal = new Calendar();
            var tmpCal = Calendar.LoadFromFile(@"Calendars\Serialization\TimeZone3.ics")[0];
            iCal.MergeWith(tmpCal);

            var serializer = new CalendarSerializer();
            serializer.Serialize(iCal, @"Calendars\Serialization\Temp\TimeZone3.ics");
        }

        //[Test, Category("Serialization")]     //Broken in dday
        public void Todo1()
        {
            SerializeTest("Todo1.ics", typeof(CalendarSerializer));
        }

        //[Test, Category("Serialization")]     //Broken in dday
        public void Todo2()
        {
            SerializeTest("Todo2.ics", typeof(CalendarSerializer));
        }

        //[Test, Category("Serialization")]     //Broken in dday
        public void Todo3()
        {
            SerializeTest("Todo3.ics", typeof(CalendarSerializer));
        }

        //[Test, Category("Serialization")]     //Broken in dday
        public void Todo4()
        {
            SerializeTest("Todo4.ics", typeof(CalendarSerializer));
        }

        //[Test, Category("Serialization")]     //Broken in dday
        public void Todo5()
        {
            SerializeTest("Todo5.ics", typeof(CalendarSerializer));
        }

        //[Test, Category("Serialization")]     //Broken in dday
        public void Todo6()
        {
            SerializeTest("Todo6.ics", typeof(CalendarSerializer));
        }

        //[Test, Category("Serialization")]     //Broken in dday
        public void Todo7()
        {
            SerializeTest("Todo7.ics", typeof(CalendarSerializer));
        }

        //[Test, Category("Serialization")]     //Broken in dday
        public void Transparency1()
        {
            var iCal = Calendar.LoadFromFile(@"Calendars\Serialization\Transparency1.ics")[0];
            
            Assert.AreEqual(1, iCal.Events.Count);
            var evt = iCal.Events.First();
            
            Assert.AreEqual(TransparencyType.Opaque, evt.Transparency);
        }

        [Test, Category("Serialization")]
        public void Transparency2()
        {
            var iCal = Calendar.LoadFromFile(@"Calendars\Serialization\Transparency2.ics")[0];

            Assert.AreEqual(1, iCal.Events.Count);
            var evt = iCal.Events.First();

            Assert.AreEqual(TransparencyType.Transparent, evt.Transparency);
        }

        //[Test, Category("Serialization")]     //Broken in dday
        public void Trigger1()
        {
            SerializeTest("Trigger1.ics", typeof(CalendarSerializer));            
        }

        //[Test, Category("Serialization")]     //Broken in dday
        public void XProperty1()
        {
            SerializeTest("XProperty1.ics", typeof(CalendarSerializer));
        }

        //[Test, Category("Serialization")]     //Broken in dday
        public void XProperty2()
        {
            SerializeTest("XProperty2.ics", typeof(CalendarSerializer));
        }

        /// <summary>
        /// Tests adding custom properties to a calendar.
        /// </summary>
        //[Test, Category("Serialization")]     //Broken in dday
        public void XProperty3()
        {
            var iCal = new Calendar();
            var evt = iCal.Create<Event>();

            var htmlBuilder = new StringBuilder();
            htmlBuilder.Append("<HTML><HEAD><META HTTP-EQUIV=\"Content-Type\" CONTENT=\"text/html;charset=iso-8859-1\"></HEAD><BODY>");
            htmlBuilder.Append("<B>Test</B>");
            htmlBuilder.Append("</BODY></HTML>");
                        
            ICalendarProperty p = new CalendarProperty("X-ALT-DESC", htmlBuilder.ToString());
            p.Parameters.Add(new CalendarParameter("FMTTYPE", "text/html"));
            evt.Properties.Add(p);

            var serializer = new CalendarSerializer();
            serializer.Serialize(iCal, @"Calendars\Serialization\XProperty3.ics");

            SerializeTest("XProperty3.ics", typeof(CalendarSerializer));
        }

        //[Test, Category("Serialization")]     //Broken in dday
        public void XProperty4()
        {
            var iCal = new Calendar();
            iCal.AddProperty("X-WR-CALNAME", "DDay Test");
            iCal.AddProperty("X-WR-CALDESC", "Events for a DDay Test");
            iCal.AddProperty("X-PUBLISHED-TTL", "PT30M");
            iCal.ProductId = "-//DDAYTEST//NONSGML www.test.com//EN";

            // Create an event in the iCalendar
            var evt = iCal.Create<Event>();

            // Populate the properties
            evt.Start = new CalDateTime(2009, 6, 28, 8, 0, 0);
            evt.Duration = TimeSpan.FromHours(1);
            evt.Url = new Uri("http://www.ftb.pl/news/59941_0_1/tunnel-electrocity-2008-timetable.htm");
            evt.Summary = "This is a title";
            evt.Description = "This is a description";

            var serializer = new CalendarSerializer();
            var output = serializer.SerializeToString(iCal);
            serializer.Serialize(iCal, @"Calendars\Serialization\XProperty4.ics");

            Assert.IsFalse(Regex.IsMatch(output, @"\r\n[\r\n]"));

            SerializeTest("XProperty4.ics", typeof(CalendarSerializer));
        }

        // FIXME: re-implement
        //[Test, Category("Serialization")]
        //public void SERIALIZE16()
        //{
        //    CustomICal1 iCal = new CustomICal1();
        //    string nonstandardText = "Some nonstandard property we want to serialize";

        //    CustomEvent1 evt = iCal.Create<CustomEvent1>();
        //    evt.Summary = "Test event";
        //    evt.Start = new DateTime(2007, 02, 15);
        //    evt.NonstandardProperty = nonstandardText;
        //    evt.IsAllDay = true;

        //    iCalendarSerializer serializer = new iCalendarSerializer(iCal);
        //    serializer.Serialize(@"Calendars\Serialization\SERIALIZE16.ics");

        //    iCal = iCalendar.LoadFromFile<CustomICal1>(@"Calendars\Serialization\SERIALIZE16.ics");
        //    foreach (CustomEvent1 evt1 in iCal.Events)
        //        Assert.IsTrue(evt1.NonstandardProperty.Equals(nonstandardText));

        //    SerializeTest("SERIALIZE16.ics", typeof(CustomICal1), typeof(iCalendarSerializer));
        //}

        // FIXME: re-implement
        //[Test, Category("Serialization")]
        //public void SERIALIZE17()
        //{
        //    // Create a normal iCalendar, serialize it, and load it as a custom calendar
        //    iCalendar iCal = new iCalendar();

        //    Event evt = iCal.Create<Event>();
        //    evt.Summary = "Test event";
        //    evt.Start = new DateTime(2007, 02, 15, 8, 0, 0);

        //    iCalendarSerializer serializer = new iCalendarSerializer(iCal);
        //    serializer.Serialize(@"Calendars\Serialization\SERIALIZE17.ics");

        //    SerializeTest("SERIALIZE17.ics", typeof(CustomICal1), typeof(iCalendarSerializer));
        //}

        

        
             

        

        

        

        // FIXME: re-implement
        //[Test, Category("Serialization")]
        //public void SERIALIZE24()
        //{
        //    //
        //    // Ensures that custom iCalendars are loaded correctly
        //    //
        //    IICalendar calendar = iCalendar.LoadFromFile<iCalendar>(@"Calendars\Serialization\SERIALIZE1.ics");
        //    CustomICal1 customiCal = iCalendar.LoadFromFile<CustomICal1>(@"Calendars\Serialization\SERIALIZE1.ics");

        //    Assert.IsTrue(calendar.Events.Count == 1, "Calendar should have 1 event");
        //    Assert.IsTrue(customiCal.Events.Count == 1, "Custom calendar should have 1 event");
        //    Assert.IsTrue(calendar.Events.First().GetType() == typeof(Event), "Calendar event should be of type Event");
        //    Assert.IsTrue(customiCal.Events.First().GetType() == typeof(CustomEvent1), "Custom calendar event should be of type CustomEvent1");
        //}

        

        // FIXME: remove?
        //[Test, Category("Serialization")]
        //public void SERIALIZE26()
        //{
        //    URI uri = new URI("addressbook://D263B4AF-823F-4D1C-BBFE-9F11491F1559:ABPerson");
        //    Assert.AreEqual("addressbook", uri.Scheme);
        //    Assert.AreEqual("D263B4AF-823F-4D1C-BBFE-9F11491F1559:ABPerson", uri.Authority);
        //}        

        // FIXME: re-implement
        //[Test, Category("Serialization")]
        //public void SERIALIZE28()
        //{
        //    iCalendar iCal = new iCalendar();
        //    Event evt = iCal.Create<Event>();
        //    evt.Summary = "Test event";
        //    evt.Start = iCalDateTime.Now;
        //    evt.Duration = TimeSpan.FromMinutes(30);
        //    evt.Organizer = new Organizer("doug@ddaysoftware.com");
        //    evt.Attendees.Add("someone@someurl.com");
        //    evt.Attendees.Add("another@someurl.com");
        //    evt.Attendees.Add("lastone@someurl.com");

        //    iCalendarSerializer serializer = new iCalendarSerializer();
        //    serializer.Serialize(iCal, @"Calendars\Serialization\SERIALIZE28.ics");

        //    SerializeTest("SERIALIZE28.ics", typeof(iCalendarSerializer));
        //}


        /// <summary>
        /// Tests that DateTime values that are out-of-range are still parsed correctly
        /// and set to the closest representable date/time in .NET.
        /// </summary>
        [Test, Category("Serialization")]
        public void DateTime1()
        {
            var iCal = Calendar.LoadFromFile(@"Calendars\Serialization\DateTime1.ics")[0];
            Assert.AreEqual(6, iCal.Events.Count);

            var evt = iCal.Events["nc2o66s0u36iesitl2l0b8inn8@google.com"];
            Assert.IsNotNull(evt);

            // The "Created" date is out-of-bounds.  It should be coerced to the
            // closest representable date/time.
            Assert.AreEqual(DateTime.MinValue, evt.Created.Value);
        }

        //[Test, Category("Serialization")]     //Broken in dday
        public void DateTime2()
        {
            var iCal = Calendar.LoadFromFile(@"Calendars\Serialization\DateTime2.ics")[0];
            Assert.AreEqual(1, iCal.Events.Count);

            Assert.IsNull(iCal.Events.First().Start);
            Assert.AreEqual("19970412", iCal.Events.First().Properties["DTSTART"].Value);
        }

        //[Test, Category("Serialization")]     //Broken in dday
        public void Duration1()
        {
            var iCal = Calendar.LoadFromFile(@"Calendars\Serialization\Duration1.ics")[0];
            var evt = iCal.Events["edb7a48a-d846-47f8-bad2-9ea3f29bcda5"];

            Assert.IsNotNull(evt);
            Assert.AreEqual(TimeSpan.FromDays(12) + TimeSpan.FromHours(1), evt.Duration, "Duration should be 12 days, 1 hour");

            SerializeTest("Duration1.ics", typeof(CalendarSerializer));
        }

        //[Test, Category("Serialization")]     //Broken in dday
        public void UsHolidays()
        {
            SerializeTest("USHolidays.ics", typeof(CalendarSerializer));
        }

        //[Test, Category("Serialization")]     //Broken in dday
        public void Language1()
        {
            SerializeTest("Language1.ics", typeof(CalendarSerializer));
        }

        //[Test, Category("Serialization")]     //Broken in dday
        public void Language2()
        {
            SerializeTest("Language2.ics", typeof(CalendarSerializer));
        }

        //[Test, Category("Serialization")]     //Broken in dday
        public void Language3()
        {
            SerializeTest("Language3.ics", typeof(CalendarSerializer));

            var calendarPath = Path.Combine(Environment.CurrentDirectory, "Calendars");
            calendarPath = Path.Combine(calendarPath, "Serialization");

            // Ensure that LoadFromUri() and LoadFromFile() produce identical results.
            // Thanks to Eugene, a student from Russia, who helped track down this bug.
            var assembly = Assembly.GetExecutingAssembly();
            var russia1 = Calendar.LoadFromUri(new Uri(Path.Combine(calendarPath, "Language3.ics")))[0];
            var russia2 = Calendar.LoadFromFile(Path.Combine(calendarPath, "Language3.ics"))[0];

            CompareCalendars(russia1, russia2);
        }

        [Test, Category("Serialization")]
        public void Language3_1()
        {
            var calendarPath = Path.Combine(Environment.CurrentDirectory, "Calendars");
            calendarPath = Path.Combine(calendarPath, "Serialization");

            var russia1 = Calendar.LoadFromUri(new Uri("http://www.mozilla.org/projects/calendar/caldata/RussiaHolidays.ics"))[0];
            var russia2 = Calendar.LoadFromFile(Path.Combine(calendarPath, "Language3.ics"))[0];

            CompareCalendars(russia1, russia2);
        }

        [Test, Category("Serialization")]
        public void Language4()
        {
            var iCal = Calendar.LoadFromFile(@"Calendars/Serialization/Language4.ics")[0];
        }

        [Test, Category("Serialization")]
        public void Outlook2007_LineFolds1()
        {
            var iCal = Calendar.LoadFromFile(@"Calendars/Serialization/Outlook2007LineFolds.ics")[0];
            var events = iCal.GetOccurrences(new CalDateTime(2009, 06, 20), new CalDateTime(2009, 06, 22));
            Assert.AreEqual(1, events.Count);
        }

        [Test, Category("Serialization")]
        public void Outlook2007_LineFolds2()
        {
            var longName = "The Exceptionally Long Named Meeting Room Whose Name Wraps Over Several Lines When Exported From Leading Calendar and Office Software Application Microsoft Office 2007";
            var iCal = Calendar.LoadFromFile(@"Calendars/Serialization/Outlook2007LineFolds.ics")[0];
            var events = iCal.GetOccurrences<Event>(new CalDateTime(2009, 06, 20), new CalDateTime(2009, 06, 22)).OrderBy(o => o.Period.StartTime).ToList();
            Assert.AreEqual(longName, ((IEvent)events[0].Source).Location);
        }

        /// <summary>
        /// Tests that multiple parameters are allowed in iCalObjects
        /// </summary>
        [Test, Category("Serialization")]
        public void Parameter1()
        {
            var iCal = Calendar.LoadFromFile(@"Calendars\Serialization\Parameter1.ics")[0];

            var evt = iCal.Events.First();
            IList<ICalendarParameter> parms = evt.Properties["DTSTART"].Parameters.AllOf("VALUE").ToList();
            Assert.AreEqual(2, parms.Count);
            Assert.AreEqual("DATE", parms[0].Values.First());
            Assert.AreEqual("OTHER", parms[1].Values.First());
        }

        /// <summary>
        /// Tests that empty parameters are allowed in iCalObjects
        /// </summary>
        [Test, Category("Serialization")]
        public void Parameter2()
        {
            var iCal = Calendar.LoadFromFile(@"Calendars\Serialization\Parameter2.ics")[0];
            Assert.AreEqual(2, iCal.Events.Count);
        }

        /// <summary>
        /// Tests a calendar that should fail to properly parse.
        /// </summary>
        [Test, Category("Serialization"), ExpectedException("antlr.MismatchedTokenException")]
        public void Parse1()
        {
            var iCal = Calendar.LoadFromFile(@"Calendars\Serialization\Parse1.ics")[0];
        }

        //[Test]     //Broken in dday
        public void ProdId1()
        {
            SerializeTest("ProdID1.ics", typeof(CalendarSerializer));
        }

        //[Test]     //Broken in dday
        public void ProdId2()
        {
            SerializeTest("ProdID2.ics", typeof(CalendarSerializer));
        }

        /// <summary>
        /// Tests that multiple properties are allowed in iCalObjects
        /// </summary>
        [Test, Category("Serialization")]
        public void Property1()
        {
            var iCal = Calendar.LoadFromFile(@"Calendars\Serialization\Property1.ics")[0];

            IList<ICalendarProperty> props = iCal.Properties.AllOf("VERSION").ToList();
            Assert.AreEqual(2, props.Count);

            for (var i = 0; i < props.Count; i++)
                Assert.AreEqual("2." + i, props[i].Value);
        }

        

        // FIXME: re-implement
        ///// <summary>
        ///// Tests that DateTime values that are not valid do not cause the
        ///// calendar to fail to parse when "Loose" parsing is enabled.
        ///// </summary>
        //[Test, Category("Serialization")]
        //public void PARSE17()
        //{
        //    SerializationContext ctx = new SerializationContext();
        //    ctx.ParsingMode = ParsingModeType.Loose;

        //    iCalendarSerializer serializer = new iCalendarSerializer();
        //    serializer.SerializationContext = ctx;
        //    IICalendar iCal = iCalendar.LoadFromFile(typeof(iCalendar), @"Calendars\Serialization\PARSE17.ics", Encoding.UTF8, serializer);

        //    Assert.AreEqual(1, iCal.Events.Count);
        //    Assert.AreEqual(iCal.Events.First().Properties["DTSTART"].Value, "1234");
        //    Assert.AreEqual(iCal.Events.First().Properties["DTEND"].Value, "5678");
        //}

        
                       
        /// <summary>
        /// Tests that line/column numbers are correctly tracked for
        /// parsed (deserialized) calendars.
        /// </summary>
        [Test, Category("Serialization")]
        public void LineColumns1()
        {
            var ctx = new SerializationContext();

            var settings = ctx.GetService(typeof(ISerializationSettings)) as ISerializationSettings;
            settings.EnsureAccurateLineNumbers = true;

            var serializer = new CalendarSerializer();
            serializer.SerializationContext = ctx;
            var iCal = Calendar.LoadFromFile(@"Calendars\Serialization\EmptyLines1.ics", Encoding.UTF8, serializer)[0];

            Assert.AreEqual(2, iCal.Events.Count);
            Assert.AreEqual(4, iCal.Events.First().Line);
            Assert.AreEqual(18, iCal.Events[1].Line);
            Assert.AreEqual(5, iCal.Events.First().Properties["CREATED"].Line);
            Assert.AreEqual(6, iCal.Events.First().Properties["LAST-MODIFIED"].Line);
            Assert.AreEqual(7, iCal.Events.First().Properties["DTSTAMP"].Line);
            Assert.AreEqual(8, iCal.Events.First().Properties["UID"].Line);
            Assert.AreEqual(9, iCal.Events.First().Properties["SUMMARY"].Line);
            Assert.AreEqual(10, iCal.Events.First().Properties["CLASS"].Line);
            Assert.AreEqual(11, iCal.Events.First().Properties["DTSTART"].Line);
            Assert.AreEqual(12, iCal.Events.First().Properties["DTEND"].Line);
            Assert.AreEqual(13, iCal.Events.First().Properties["CATEGORIES"].Line);
            Assert.AreEqual(14, iCal.Events.First().Properties["X-MOZILLA-ALARM-DEFAULT-LENGTH"].Line);
            Assert.AreEqual(15, iCal.Events.First().Properties["LOCATION"].Line);
        }

        /// <summary>
        /// Tests that line/column numbers are correctly tracked for
        /// parsed (deserialized) calendars.
        /// </summary>
        [Test, Category("Serialization")]
        public void LineColumns2()
        {
            var ctx = new SerializationContext();

            var settings = ctx.GetService(typeof(ISerializationSettings)) as ISerializationSettings;
            settings.EnsureAccurateLineNumbers = true;

            var serializer = new CalendarSerializer();
            serializer.SerializationContext = ctx;
            var iCal = Calendar.LoadFromFile(@"Calendars\Serialization\Calendar1.ics", Encoding.UTF8, serializer)[0];

            Assert.IsNotNull(iCal.Todos["2df60496-1e73-11db-ba96-e3cfe6793b5f"]);
            Assert.IsNotNull(iCal.Todos["4836c236-1e75-11db-835f-a024e2a6131f"]);
            Assert.AreEqual(110, iCal.Todos["4836c236-1e75-11db-835f-a024e2a6131f"].Properties["LOCATION"].Line);
            Assert.AreEqual(123, iCal.Todos["2df60496-1e73-11db-ba96-e3cfe6793b5f"].Properties["UID"].Line);
        }

        private static byte[] ReadBinary(string fileName)
        {
            byte[] binaryData = null;
            using (var reader = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                binaryData = new byte[reader.Length];
                reader.Read(binaryData, 0, (int)reader.Length);
            }

            return binaryData;
        }

        private static bool CompareBinary(string fileName, byte[] data)
        {
            byte[] binaryData = null;
            using (var reader = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                binaryData = new byte[reader.Length];
                reader.Read(binaryData, 0, (int)reader.Length);
            }

            if (binaryData == null && data == null)
                return true;
            if (binaryData == null || data == null)
                return false;

            for (var i = 0; i < data.Length; i++)
            {
                if (binaryData[i] != data[i])
                    return false;
            }

            return true;
        }

        

        // FIXME: re-implement
        //[Test, Category("Serialization")]
        //public void RELATED_TO1()
        //{
        //    iCalendar iCal = new iCalendar();

        //    // Create a test event
        //    Event evt1 = iCal.Create<Event>();
        //    evt1.Summary = "Work Party";
        //    evt1.Start = new iCalDateTime(2007, 10, 15, 8, 0, 0);
        //    evt1.Duration = TimeSpan.FromHours(1);

        //    // Create another event that relates to evt1
        //    Event evt2 = iCal.Create<Event>();
        //    evt2.Summary = "Water Polo";
        //    evt2.Start = new iCalDateTime(2007, 10, 15, 10, 0, 0);
        //    evt2.Duration = TimeSpan.FromHours(1);
        //    evt2.AddRelatedTo(evt1.Uid, RelationshipTypes.Parent); // evt1 is the parent of evt2

        //    iCalendarSerializer serializer = new iCalendarSerializer(iCal);
        //    serializer.Serialize(@"Calendars\Serialization\Temp\RELATED_TO1.ics");

        //    iCal = iCalendar.LoadFromFile(@"Calendars\Serialization\Temp\RELATED_TO1.ics");
        //    evt2 = iCal.Events[evt2.Uid];

        //    Assert.AreEqual(1, evt2.Related_To.Length);
        //    Assert.AreEqual(evt1.Uid, evt2.RelatedTo[0].Value);
        //    Assert.AreEqual(((Parameter)evt2.RelatedTo[0].Parameters["RELTYPE"]).Values[0], RelationshipTypes.Parent);
        //}
    }
}
