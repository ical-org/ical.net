using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Resources;
using System.Web;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;
using DDay.iCal.Serialization.iCalendar;
using DDay.iCal.Serialization;

namespace DDay.iCal.Test
{
    [TestFixture]
    public class SerializationTest
    {
        private string tzid;

        [TestFixtureSetUp]
        public void InitAll()
        {
            tzid = "US-Eastern";
        }

        private void SerializeTest(string filename, Type iCalSerializerType) { SerializeTest(filename, typeof(iCalendar), iCalSerializerType); }
        private void SerializeTest(string filename, Type iCalType, Type iCalSerializerType)
        {
            if (!Directory.Exists(@"Calendars\Serialization\Temp"))
                Directory.CreateDirectory(@"Calendars\Serialization\Temp");

            ISerializer serializer = Activator.CreateInstance(iCalSerializerType) as ISerializer;
            Assert.IsNotNull(serializer);

            // Set the iCalendar type for deserialization
            serializer.GetService<ISerializationSettings>().iCalendarType = iCalType;

            // Load the calendar from file
            IICalendar iCal1 = iCalendar.LoadFromFile(@"Calendars\Serialization\" + filename, Encoding.UTF8, serializer)[0];

            Assert.IsTrue(iCal1.Properties.Count > 0, "iCalendar has no properties; did it load correctly?");
            Assert.IsTrue(iCal1.UniqueComponents.Count > 0, "iCalendar has no unique components; it must to be used in SerializeTest(). Did it load correctly?");

            FileStream fs = new FileStream(@"Calendars\Serialization\Temp\" + Path.GetFileNameWithoutExtension(filename) + "_Serialized" + Path.GetExtension(filename), FileMode.Create, FileAccess.Write);
            serializer.Serialize(iCal1, fs, Encoding.UTF8);
            fs.Close();

            IICalendar iCal2 = iCalendar.LoadFromFile(@"Calendars\Serialization\Temp\" + Path.GetFileNameWithoutExtension(filename) + "_Serialized" + Path.GetExtension(filename), Encoding.UTF8, serializer)[0];

            CompareCalendars(iCal1, iCal2);
        }

        static public void CompareCalendars(IICalendar iCal1, IICalendar iCal2)
        {
            CompareComponents(iCal1, iCal2);

            Assert.AreEqual(iCal1.Children.Count, iCal2.Children.Count, "Children count is different between calendars.");

            for (int i = 0; i < iCal1.Events.Count; i++)
                CompareComponents(iCal1.Events[i], iCal2.Events[i]);
            for (int i = 0; i < iCal1.FreeBusy.Count; i++)
                CompareComponents(iCal1.FreeBusy[i], iCal2.FreeBusy[i]);
            for (int i = 0; i < iCal1.Journals.Count; i++)
                CompareComponents(iCal1.Journals[i], iCal2.Journals[i]);
            for (int i = 0; i < iCal1.Todos.Count; i++)
                CompareComponents(iCal1.Todos[i], iCal2.Todos[i]);
            for (int i = 0; i < iCal1.TimeZones.Count; i++)
                CompareComponents(iCal1.TimeZones[i], iCal2.TimeZones[i]);
        }

        static public void CompareComponents(ICalendarComponent cb1, ICalendarComponent cb2)
        {
            List<ICalendarProperty> c1Props = new List<ICalendarProperty>(cb1.Properties);
            List<ICalendarProperty> c2Props = new List<ICalendarProperty>(cb2.Properties);

            Assert.AreEqual(c1Props.Count, c2Props.Count, "The number of '" + cb1.Name + "' properties is not equal.");

            c1Props.Sort(new ComponentSerializer.PropertyAlphabetizer());
            c2Props.Sort(new ComponentSerializer.PropertyAlphabetizer());            

            for (int i = 0; i < c1Props.Count; i++)
            {
                ICalendarProperty p1 = c1Props[i];
                ICalendarProperty p2 = c2Props[i];
                Assert.AreEqual(p1, p2, "The properties '" + p1.Name + "' are not equal.");

                if (p1.Value is IComparable)
                    Assert.AreEqual(0, ((IComparable)p1.Value).CompareTo(p2.Value), "The '" + p1.Name + "' property values do not match.");
                else if (p1.Value is IEnumerable)
                    CompareEnumerables((IEnumerable)p1.Value, (IEnumerable)p2.Value, p1.Name);
                else
                    Assert.AreEqual(p1.Value, p2.Value, "The '" + p1.Name + "' property values are not equal.");
            }

            Assert.AreEqual(cb1.Children.Count, cb2.Children.Count, "The number of children are not equal.");
            for (int i = 0; i < cb1.Children.Count; i++)
            {
                ICalendarComponent child1 = cb1.Children[i] as ICalendarComponent;
                ICalendarComponent child2 = cb2.Children[i] as ICalendarComponent;
                if (child1 != null && child2 != null)
                    CompareComponents(child1, child2);
                else
                    Assert.AreEqual(child1, child2, "The child objects are not equal.");
            }
        }

        static public void CompareEnumerables(IEnumerable a1, IEnumerable a2, string value)
        {
            if (a1 == null && a2 == null)
                return;

            Assert.IsFalse((a1 == null && a2 != null) || (a1 != null && a2 == null), value + " do not match - one item is null");

            IEnumerator enum1 = a1.GetEnumerator();
            IEnumerator enum2 = a2.GetEnumerator();

            while (enum1.MoveNext() && enum2.MoveNext())
                Assert.AreEqual(enum1.Current, enum2.Current, value + " do not match");
        }

        /// <summary>
        /// Ensures that a basic, binary attachment functions as it should.
        /// </summary>
        [Test, Category("Serialization")]
        public void Attachment1()
        {
            IICalendar iCal = new iCalendar();

            // Create a test event
            IEvent evt = iCal.Create<Event>();
            evt.Summary = "Test Event";
            evt.Start = new iCalDateTime(2007, 10, 15, 8, 0, 0);
            evt.Duration = TimeSpan.FromHours(1);

            // Add an attachment to this event
            IAttachment attachment = new Attachment();
            attachment.Data = ReadBinary(@"Data\Test.doc");
            attachment.Parameters.Add("X-FILENAME", "WordDocument.doc");
            evt.Attachments.Add(attachment);

            iCalendarSerializer serializer = new iCalendarSerializer();
            if (!Directory.Exists(@"Calendars\Serialization\Temp"))
                Directory.CreateDirectory(@"Calendars\Serialization\Temp");
            serializer.Serialize(iCal, @"Calendars\Serialization\Temp\Attachment1.ics");

            iCal = iCalendar.LoadFromFile(@"Calendars\Serialization\Temp\Attachment1.ics")[0];
            evt = iCal.Events[0];
            attachment = evt.Attachments[0];

            Assert.IsTrue(CompareBinary(@"Data\Test.doc", attachment.Data), "Serialized version of Test.doc did not match the deserialized version.");
        }

        /// <summary>
        /// Ensures that very large attachments function as they should.
        /// </summary>
        [Test, Category("Serialization")]
        public void Attachment2()
        {
            IICalendar iCal = new iCalendar();

            // Create a test event
            IEvent evt = iCal.Create<Event>();
            evt.Summary = "Test Event";
            evt.Start = new iCalDateTime(2007, 10, 15, 8, 0, 0);
            evt.Duration = TimeSpan.FromHours(1);

            // Get a data file
            string loremIpsum = UnicodeEncoding.Default.GetString(ReadBinary(@"Data\LoremIpsum.txt"));
            StringBuilder sb = new StringBuilder();
            // If we copy it 300 times, we should end up with a file over 2.5MB in size.
            for (int i = 0; i < 300; i++)
                sb.AppendLine(loremIpsum);

            // Add an attachment to this event
            IAttachment attachment = new Attachment();
            attachment.Data = UnicodeEncoding.Default.GetBytes(sb.ToString());
            evt.Attachments.Add(attachment);

            iCalendarSerializer serializer = new iCalendarSerializer();
            if (!Directory.Exists(@"Calendars\Serialization\Temp"))
                Directory.CreateDirectory(@"Calendars\Serialization\Temp");
            serializer.Serialize(iCal, @"Calendars\Serialization\Temp\Attachment2.ics");

            iCal = iCalendar.LoadFromFile(@"Calendars\Serialization\Temp\Attachment2.ics")[0];
            evt = iCal.Events[0];
            attachment = evt.Attachments[0];

            // Ensure the generated and serialized strings match
            Assert.AreEqual(sb.ToString(), UnicodeEncoding.Default.GetString(attachment.Data));

            // Times to finish the test for attachment file sizes (on my computer, version 0.80): 
            //  0.92MB = 1.2 seconds
            //  2.76MB = 6 seconds
            //  4.6MB = 15.1 seconds
            //  9.2MB = 54 seconds
        }

        [Test, Category("Serialization")]
        public void Attachment3()
        {
            SerializeTest("Attachment3.ics", typeof(iCalendarSerializer));
        }

        /// <summary>
        /// Tests bug #2148092 - Percent compelete serialization error
        /// </summary>
        [Test, Category("Serialization")]
        public void Bug2148092()
        {
            SerializeTest("Language5.ics", typeof(iCalendarSerializer));
        }

        [Test]
        public void CalendarParameters1()
        {
            IICalendar iCal = new iCalendar();
            iCalendarSerializer serializer = new iCalendarSerializer();
            serializer.Serialize(iCal, @"Calendars\Serialization\Temp\CalendarParameters1.ics");

            iCal = iCalendar.LoadFromFile(@"Calendars\Serialization\Temp\CalendarParameters1.ics")[0];
            Assert.IsNotEmpty(iCal.Version);
            Assert.IsNotEmpty(iCal.ProductID);

            iCal.Version = string.Empty;
            iCal.ProductID = null;
            Assert.IsNotEmpty(iCal.Version, "VERSION is required");
            Assert.IsNotEmpty(iCal.ProductID, "PRODID is required");
        }

        [Test, Category("Serialization")]
        public void CaseInsensitive1()
        {
            SerializeTest("CaseInsensitive1.ics", typeof(iCalendarSerializer));
        }

        [Test, Category("Serialization")]
        public void CaseInsensitive2()
        {
            SerializeTest("CaseInsensitive2.ics", typeof(iCalendarSerializer));
        }

        [Test, Category("Serialization")]
        public void CaseInsensitive3()
        {
            SerializeTest("CaseInsensitive3.ics", typeof(iCalendarSerializer));
        }

        [Test, Category("Serialization")]
        public void Categories1()
        {
            SerializeTest("Categories1.ics", typeof(iCalendarSerializer));
        }

        [Test, Category("Serialization")]
        public void Encoding1()
        {
            SerializeTest("Encoding1.ics", typeof(iCalendarSerializer));
        }

        [Test, Category("Serialization")]
        public void Event1()
        {
            SerializeTest("Event1.ics", typeof(iCalendarSerializer));
        }

        [Test, Category("Serialization")]
        public void Event2()
        {
            SerializeTest("Event2.ics", typeof(iCalendarSerializer));
        }

        [Test, Category("Serialization")]
        public void Event3()
        {
            SerializeTest("Event3.ics", typeof(iCalendarSerializer));
        }

        [Test, Category("Serialization")]
        public void Event4()
        {
            SerializeTest("Event4.ics", typeof(iCalendarSerializer));
        }

        [Test, Category("Serialization")]
        public void Event5()
        {
            iCalendar iCal = new iCalendar();

            Event evt = iCal.Create<Event>();
            evt.Summary = "Test event title";
            evt.Start = new iCalDateTime(2007, 3, 19);
            evt.Start.IsUniversalTime = true;
            evt.Duration = new TimeSpan(24, 0, 0);
            evt.Created = evt.Start.Copy<IDateTime>();
            evt.DTStamp = evt.Start.Copy<IDateTime>();
            evt.UID = "123456789";
            evt.IsAllDay = true;

            RecurrencePattern rec = new RecurrencePattern("FREQ=WEEKLY;INTERVAL=3;BYDAY=TU,FR,SU;COUNT=4");
            evt.RecurrenceRules.Add(rec);

            iCalendarSerializer serializer = new iCalendarSerializer();
            string icalString = serializer.SerializeToString(iCal);

            Assert.IsNotEmpty(icalString, "iCalendarSerializer.SerializeToString() must not be empty");

            ComponentSerializer compSerializer = new ComponentSerializer();
            string evtString = compSerializer.SerializeToString(evt);

            Assert.IsTrue(evtString.Equals("BEGIN:VEVENT\r\nCREATED:20070319T000000Z\r\nDTEND;VALUE=DATE:20070320\r\nDTSTAMP:20070319T000000Z\r\nDTSTART;VALUE=DATE:20070319\r\nRRULE:FREQ=WEEKLY;INTERVAL=3;COUNT=4;BYDAY=TU,FR,SU\r\nSEQUENCE:0\r\nSUMMARY:Test event title\r\nUID:123456789\r\nEND:VEVENT\r\n"), "ComponentBaseSerializer.SerializeToString() serialized incorrectly");

            serializer.Serialize(iCal, @"Calendars\Serialization\Event5.ics");
            SerializeTest("Event5.ics", typeof(iCalendarSerializer));
        }

        [Test, Category("Serialization")]
        public void Event6()
        {
            iCalendar iCal = new iCalendar();

            Event evt = iCal.Create<Event>();
            evt.Summary = "Test event title";
            evt.Start = new iCalDateTime(2007, 4, 29);
            evt.End = evt.Start.AddDays(1);
            evt.IsAllDay = true;

            RecurrencePattern rec = new RecurrencePattern("FREQ=WEEKLY;INTERVAL=3;BYDAY=TU,FR,SU;COUNT=4");
            evt.RecurrenceRules.Add(rec);

            ComponentSerializer compSerializer = new ComponentSerializer();

            FileStream fs = new FileStream(@"Calendars\Serialization\Event6.ics", FileMode.Create, FileAccess.Write);
            compSerializer.Serialize(evt, fs, Encoding.UTF8);
            fs.Close();

            iCalendar iCal1 = new iCalendar();

            fs = new FileStream(@"Calendars\Serialization\Event6.ics", FileMode.Open, FileAccess.Read);
            Event evt1 = CalendarComponent.LoadFromStream<Event>(fs, Encoding.UTF8);
            fs.Close();

            CompareComponents(evt, evt1);
        }

        [Test, Category("Serialization")]
        public void Event7()
        {
            string iCalString = @"BEGIN:VCALENDAR
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
            StringReader sr = new StringReader(iCalString);
            IICalendar calendar = iCalendar.LoadFromStream(sr)[0];

            Assert.IsTrue(calendar.Events.Count == 2, "There should be 2 events in the loaded iCalendar.");
            Assert.IsNotNull(calendar.Events["fd940618-45e2-4d19-b118-37fd7a8e3906"], "There should be an event with UID: fd940618-45e2-4d19-b118-37fd7a8e3906");
            Assert.IsNotNull(calendar.Events["ebfbd3e3-cc1e-4a64-98eb-ced2598b3908"], "There should be an event with UID: ebfbd3e3-cc1e-4a64-98eb-ced2598b3908");

            iCalendarSerializer serializer = new iCalendarSerializer();
            serializer.Serialize(calendar, @"Calendars\Serialization\Event7.ics");

            SerializeTest("Event7.ics", typeof(iCalendarSerializer));
        }

        [Test, Category("Serialization")]
        public void Geo1()
        {
            SerializeTest("Geo1.ics", typeof(iCalendarSerializer));
        }

        [Test, Category("Serialization")]
        public void String1()
        {
            IICalendar iCal = new iCalendar();
            Event evt = iCal.Create<Event>();
            evt.Start = iCalDateTime.Now;
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

            iCalendarSerializer serializer = new iCalendarSerializer(iCal);
            serializer.Serialize(@"Calendars\Serialization\String1.ics");

            SerializeTest("String1.ics", typeof(iCalendarSerializer));
        }

        [Test]
        public void TimeZone1()
        {
            IICalendar iCal = iCalendar.LoadFromFile(@"Calendars\Serialization\TimeZone1.ics")[0];

            ITimeZone tz = iCal.TimeZones[0];
            tz.LastModified = new iCalDateTime(2007, 1, 1);

            iCalendarSerializer serializer = new iCalendarSerializer();
            serializer.Serialize(iCal, @"Calendars\Serialization\Temp\TimeZone1.ics");

            iCal = iCalendar.LoadFromFile(@"Calendars\Serialization\Temp\TimeZone1.ics")[0];
            tz = iCal.TimeZones[0];

            Assert.AreEqual(0, tz.Properties["LAST-MODIFIED"].Parameters.CountOf("VALUE"), "The \"VALUE\" parameter is not allowed on \"LAST-MODIFIED\"");
        }

        [Test]
        public void TimeZone2()
        {
            //
            // First, check against the VALUE parameter; it must be absent in DTSTART
            //

            IICalendar iCal = iCalendar.LoadFromFile(@"Calendars\Serialization\TimeZone2.ics")[0];

            ITimeZone tz = iCal.TimeZones[0];
            foreach (iCalTimeZoneInfo tzi in tz.TimeZoneInfos)
                tzi.Start = new iCalDateTime(2007, 1, 1);

            iCalendarSerializer serializer = new iCalendarSerializer();
            serializer.Serialize(iCal, @"Calendars\Serialization\Temp\TimeZone2.ics");

            iCal = iCalendar.LoadFromFile(@"Calendars\Serialization\Temp\TimeZone2.ics")[0];
            tz = iCal.TimeZones[0];

            foreach (iCalTimeZoneInfo tzi in tz.TimeZoneInfos)
            {
                Assert.AreEqual(
                    0,
                    tzi.Properties["DTSTART"].Parameters.CountOf("VALUE"),
                    "\"DTSTART\" property MUST be represented in local time in timezones");
            }

            //
            // Next, check against UTC time; DTSTART must be presented in local time
            //
            iCal = iCalendar.LoadFromFile(@"Calendars\Serialization\TimeZone2.ics")[0];

            tz = iCal.TimeZones[0];
            foreach (iCalTimeZoneInfo tzi in tz.TimeZoneInfos)
            {
                tzi.Start = iCalDateTime.Now;
                tzi.Start.IsUniversalTime = true;
            }

            serializer = new iCalendarSerializer();
            serializer.Serialize(iCal, @"Calendars\Serialization\Temp\TimeZone2.ics");

            iCal = iCalendar.LoadFromFile(@"Calendars\Serialization\Temp\TimeZone2.ics")[0];
            tz = iCal.TimeZones[0];

            foreach (iCalTimeZoneInfo tzi in tz.TimeZoneInfos)
            {
                Assert.AreEqual(0, tzi.Properties["DTSTART"].Parameters.CountOf("VALUE"),
                    "\"DTSTART\" property MUST be represented in local time in timezones");
            }
        }

        [Test, Category("Serialization")]
        public void TimeZone3()
        {
            SerializeTest("TimeZone3.ics", typeof(iCalendarSerializer));

            iCalendar iCal = new iCalendar();
            IICalendar tmp_cal = iCalendar.LoadFromFile(@"Calendars\Serialization\TimeZone3.ics")[0];
            iCal.MergeWith(tmp_cal);

            iCalendarSerializer serializer = new iCalendarSerializer();
            serializer.Serialize(iCal, @"Calendars\Serialization\Temp\TimeZone3.ics");
        }

        [Test, Category("Serialization")]
        public void Todo1()
        {
            SerializeTest("Todo1.ics", typeof(iCalendarSerializer));
        }

        [Test, Category("Serialization")]
        public void Todo2()
        {
            SerializeTest("Todo2.ics", typeof(iCalendarSerializer));
        }

        [Test, Category("Serialization")]
        public void Todo3()
        {
            SerializeTest("Todo3.ics", typeof(iCalendarSerializer));
        }

        [Test, Category("Serialization")]
        public void Todo4()
        {
            SerializeTest("Todo4.ics", typeof(iCalendarSerializer));
        }

        [Test, Category("Serialization")]
        public void Todo5()
        {
            SerializeTest("Todo5.ics", typeof(iCalendarSerializer));
        }

        [Test, Category("Serialization")]
        public void Todo6()
        {
            SerializeTest("Todo6.ics", typeof(iCalendarSerializer));
        }

        [Test, Category("Serialization")]
        public void Todo7()
        {
            SerializeTest("Todo7.ics", typeof(iCalendarSerializer));
        }

        [Test, Category("Serialization")]
        public void XProperty1()
        {
            SerializeTest("XProperty1.ics", typeof(iCalendarSerializer));
        }

        [Test, Category("Serialization")]
        public void XProperty2()
        {
            SerializeTest("XProperty2.ics", typeof(iCalendarSerializer));
        }

        /// <summary>
        /// Tests adding custom properties to a calendar.
        /// </summary>
        [Test, Category("Serialization")]
        public void XProperty3()
        {
            iCalendar iCal = new iCalendar();
            Event evt = iCal.Create<Event>();

            StringBuilder htmlBuilder = new StringBuilder();
            htmlBuilder.Append("<HTML><HEAD><META HTTP-EQUIV=\"Content-Type\" CONTENT=\"text/html;charset=iso-8859-1\"></HEAD><BODY>");
            htmlBuilder.Append("<B>Test</B>");
            htmlBuilder.Append("</BODY></HTML>");

            // This adds the property to the event automatically
            ICalendarProperty p = new CalendarProperty("X-ALT-DESC", htmlBuilder.ToString());
            p.Parameters.Add(new CalendarParameter("FMTTYPE", "text/html"));

            iCalendarSerializer serializer = new iCalendarSerializer();
            serializer.Serialize(iCal, @"Calendars\Serialization\XProperty3.ics");

            SerializeTest("XProperty3.ics", typeof(iCalendarSerializer));
        }

        [Test, Category("Serialization")]
        public void XProperty4()
        {
            iCalendar iCal = new iCalendar();
            iCal.AddProperty("X-WR-CALNAME", "DDay Test");
            iCal.AddProperty("X-WR-CALDESC", "Events for a DDay Test");
            iCal.AddProperty("X-PUBLISHED-TTL", "PT30M");
            iCal.ProductID = "-//DDAYTEST//NONSGML www.test.com//EN";

            // Create an event in the iCalendar
            Event evt = iCal.Create<Event>();

            //Populate the properties
            evt.Start = new iCalDateTime(2009, 6, 28, 8, 0, 0);
            evt.Duration = TimeSpan.FromHours(1);
            evt.Url = new Uri("http://www.ftb.pl/news/59941_0_1/tunnel-electrocity-2008-timetable.htm");
            evt.Summary = "This is a title";
            evt.Description = "This is a description";

            iCalendarSerializer serializer = new iCalendarSerializer(iCal);
            string output = serializer.SerializeToString();
            serializer.Serialize(@"Calendars\Serialization\SERIALIZE32.ics");

            Assert.IsFalse(Regex.IsMatch(output, @"\r\n[\r\n]"));

            SerializeTest("SERIALIZE32.ics", typeof(iCalendarSerializer));
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
        //    Assert.IsTrue(calendar.Events[0].GetType() == typeof(Event), "Calendar event should be of type Event");
        //    Assert.IsTrue(customiCal.Events[0].GetType() == typeof(CustomEvent1), "Custom calendar event should be of type CustomEvent1");
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

        

        

        [Test, Category("Serialization")]
        public void Duration1()
        {
            IICalendar iCal = iCalendar.LoadFromFile(@"Calendars\Serialization\Duration1.ics")[0];
            IEvent evt = iCal.Events["edb7a48a-d846-47f8-bad2-9ea3f29bcda5"];

            Assert.IsNotNull(evt);
            Assert.AreEqual(TimeSpan.FromDays(12) + TimeSpan.FromHours(1), evt.Duration, "Duration should be 12 days, 1 hour");

            SerializeTest("Duration1.ics", typeof(iCalendarSerializer));
        }

        [Test, Category("Serialization")]
        public void USHOLIDAYS()
        {
            SerializeTest("USHolidays.ics", typeof(iCalendarSerializer));
        }

        [Test, Category("Serialization")]
        public void Language1()
        {
            SerializeTest("Language1.ics", typeof(iCalendarSerializer));
        }

        [Test, Category("Serialization")]
        public void Language2()
        {
            SerializeTest("Language2.ics", typeof(iCalendarSerializer));
        }

        [Test, Category("Serialization")]
        public void Language3()
        {
            SerializeTest("Language3.ics", typeof(iCalendarSerializer));

            string calendarPath = Path.Combine(Environment.CurrentDirectory, "Calendars");
            calendarPath = Path.Combine(calendarPath, "Serialization");

            // Ensure that LoadFromUri() and LoadFromFile() produce identical results.
            // Thanks to Eugene, a student from Russia, who helped track down this bug.
            Assembly assembly = Assembly.GetExecutingAssembly();
            IICalendar russia1 = iCalendar.LoadFromUri(new Uri(Path.Combine(calendarPath, "Language3.ics")))[0];
            IICalendar russia2 = iCalendar.LoadFromFile(Path.Combine(calendarPath, "Language3.ics"))[0];

            CompareCalendars(russia1, russia2);
        }

        [Test, Category("Serialization")]
        public void Language3_1()
        {
            string calendarPath = Path.Combine(Environment.CurrentDirectory, "Calendars");
            calendarPath = Path.Combine(calendarPath, "Serialization");

            IICalendar russia1 = iCalendar.LoadFromUri(new Uri("http://www.mozilla.org/projects/calendar/caldata/RussiaHolidays.ics"))[0];
            IICalendar russia2 = iCalendar.LoadFromFile(Path.Combine(calendarPath, "Language3.ics"))[0];

            CompareCalendars(russia1, russia2);
        }

        [Test, Category("Serialization")]
        public void PARSE1()
        {
            IICalendar iCal = iCalendar.LoadFromFile(@"Calendars\Serialization\PARSE1.ics")[0];
            Assert.IsTrue(iCal.Events.Count == 2, "iCalendar should have 2 events");
        }

        [Test, Category("Serialization")]
        public void PARSE2()
        {
            IICalendar iCal = iCalendar.LoadFromFile(@"Calendars\Serialization\PARSE2.ics")[0];
            Assert.IsTrue(iCal.Events.Count == 4, "iCalendar should have 4 events");
        }

        [Test, Category("Serialization")]
        public void PARSE3()
        {
            IICalendar iCal = iCalendar.LoadFromFile(@"Calendars\Serialization\PARSE3.ics")[0];
        }

        /// <summary>
        /// Verifies that blank lines between components are allowed
        /// (as occurs with some applications/parsers - i.e. KOrganizer)
        /// </summary>
        [Test, Category("Serialization")]
        public void PARSE4()
        {
            IICalendar iCal = iCalendar.LoadFromFile(@"Calendars\Serialization\PARSE4.ics")[0];
        }

        /// <summary>
        /// Verifies that a calendar will load without a VERSION or PRODID
        /// specification.
        /// </summary>
        [Test, Category("Serialization")]
        public void PARSE5()
        {
            IICalendar iCal = iCalendar.LoadFromFile(@"Calendars\Serialization\PARSE5.ics")[0];
        }

        /// <summary>
        /// Tests a calendar that should fail to properly parse.
        /// </summary>
        [Test, Category("Serialization"), ExpectedException("antlr.MismatchedTokenException")]
        public void PARSE6()
        {
            IICalendar iCal = iCalendar.LoadFromFile(@"Calendars\Serialization\PARSE6.ics")[0];
        }

        /// <summary>
        /// Similar to PARSE4 and PARSE5 tests.
        /// </summary>
        [Test, Category("Serialization")]
        public void PARSE7()
        {
            IICalendar iCal = iCalendar.LoadFromFile(@"Calendars\Serialization\PARSE7.ics")[0];
        }

        /// <summary>
        /// Tests that a mixed-case VERSION property is loaded properly
        /// </summary>
        [Test, Category("Serialization")]
        public void PARSE8()
        {
            IICalendar iCal = iCalendar.LoadFromFile(@"Calendars\Serialization\PARSE8.ics")[0];
            Assert.AreEqual("2.5", iCal.Version);
        }

        /// <summary>
        /// Tests that multiple properties are allowed in iCalObjects
        /// </summary>
        [Test, Category("Serialization")]
        public void PARSE9()
        {
            IICalendar iCal = iCalendar.LoadFromFile(@"Calendars\Serialization\PARSE9.ics")[0];

            IList<ICalendarProperty> props = iCal.Properties.AllOf("VERSION");
            Assert.AreEqual(2, props.Count);

            for (int i = 0; i < props.Count; i++)
                Assert.AreEqual("2." + i, props[i].Value);
        }

        /// <summary>
        /// Tests that multiple parameters are allowed in iCalObjects
        /// </summary>
        [Test, Category("Serialization")]
        public void PARSE10()
        {
            IICalendar iCal = iCalendar.LoadFromFile(@"Calendars\Serialization\PARSE10.ics")[0];

            IEvent evt = iCal.Events[0];
            IList<ICalendarParameter> parms = evt.Properties["DTSTART"].Parameters.AllOf("VALUE");
            Assert.AreEqual(2, parms.Count);
            Assert.AreEqual("DATE", parms[0].Values[0]);
            Assert.AreEqual("OTHER", parms[1].Values[0]);
        }

        /// <summary>
        /// Tests that a Google calendar is correctly loaded and parsed.
        /// </summary>
        [Test, Category("Serialization")]
        public void PARSE11()
        {
            IICalendar iCal = iCalendar.LoadFromUri(new Uri("http://www.google.com/calendar/ical/tvhot064q4p48frqdalgo3fb2k%40group.calendar.google.com/public/basic.ics"))[0];
            Assert.IsNotNull(iCal);
            Assert.AreEqual(1, iCal.Events.Count);
            Assert.AreEqual(1, iCal.TimeZones.Count);

            string tzid = iCal.TimeZones[0].TZID;
            IList<Occurrence> occurrences = iCal.GetOccurrences(new iCalDateTime(2009, 8, 24, tzid), new iCalDateTime(2009, 9, 28, tzid));
            Assert.AreEqual(5, occurrences.Count);
            Assert.AreEqual(new iCalDateTime(2009, 8, 26, 8, 0, 0, tzid), occurrences[0].Period.StartTime);
            Assert.AreEqual(new iCalDateTime(2009, 9, 2, 8, 0, 0, tzid), occurrences[1].Period.StartTime);
            Assert.AreEqual(new iCalDateTime(2009, 9, 9, 8, 0, 0, tzid), occurrences[2].Period.StartTime);
            Assert.AreEqual(new iCalDateTime(2009, 9, 16, 8, 0, 0, tzid), occurrences[3].Period.StartTime);
            Assert.AreEqual(new iCalDateTime(2009, 9, 23, 8, 0, 0, tzid), occurrences[4].Period.StartTime);
            Assert.AreEqual(new iCalDateTime(2009, 8, 26, 10, 0, 0, tzid), occurrences[0].Period.EndTime);
        }

        // FIXME: re-implement
        ///// <summary>
        ///// Tests that string escaping works with Text elements.
        ///// </summary>
        //[Test, Category("Serialization")]
        //public void PARSE12()
        //{
        //    string value = @"test\with\;characters";
        //    Text v1 = value;
        //    Text v2 = new Text(value, true);

        //    Assert.AreEqual(value, v1.Value, "String escaping was incorrect.");
        //    Assert.AreEqual(@"test\with;characters", v2.Value, "String escaping was incorrect.");

        //    value = @"C:\Path\To\My\New\Information";
        //    v1 = value;
        //    v2 = new Text(value, true);
        //    Assert.AreEqual(value, v1.Value, "String escaping was incorrect.");
        //    Assert.AreEqual("C:\\Path\\To\\My\new\\Information", v2.Value, "String escaping was incorrect.");

        //    value = @"\""This\r\nis\Na\, test\""\;\\;,";
        //    v1 = value;
        //    v2 = new Text(value, true);

        //    Assert.AreEqual(value, v1.Value, "String escaping was incorrect.");
        //    Assert.AreEqual("\"This\\r\nis\na, test\";\\;,", v2.Value, "String escaping was incorrect.");
        //}

        /// <summary>
        /// Tests that empty parameters are allowed in iCalObjects
        /// </summary>
        [Test, Category("Serialization")]
        public void PARSE13()
        {
            IICalendar iCal = iCalendar.LoadFromFile(@"Calendars\Serialization\PARSE13.ics")[0];

            Assert.AreEqual(2, iCal.Events.Count);
        }

        /// <summary>
        /// Tests that valid REQUEST-STATUS properties are parsed correctly.
        /// </summary>
        [Test, Category("Serialization")]
        public void PARSE14()
        {
            IICalendar iCal = iCalendar.LoadFromFile(@"Calendars\Serialization\PARSE14.ics")[0];
            Assert.AreEqual(1, iCal.Events.Count);
            Assert.AreEqual(4, iCal.Events[0].RequestStatuses.Count);

            IRequestStatus rs = iCal.Events[0].RequestStatuses[0];
            Assert.AreEqual(2, rs.StatusCode.Primary);
            Assert.AreEqual(0, rs.StatusCode.Secondary);
            Assert.AreEqual("Success", rs.Description);
            Assert.IsNull(rs.ExtraData);

            rs = iCal.Events[0].RequestStatuses[1];
            Assert.AreEqual(3, rs.StatusCode.Primary);
            Assert.AreEqual(1, rs.StatusCode.Secondary);
            Assert.AreEqual("Invalid property value", rs.Description);
            Assert.AreEqual("DTSTART:96-Apr-01", rs.ExtraData);

            rs = iCal.Events[0].RequestStatuses[2];
            Assert.AreEqual(2, rs.StatusCode.Primary);
            Assert.AreEqual(8, rs.StatusCode.Secondary);
            Assert.AreEqual(" Success, repeating event ignored. Scheduled as a single event.", rs.Description);
            Assert.AreEqual("RRULE:FREQ=WEEKLY;INTERVAL=2", rs.ExtraData);

            rs = iCal.Events[0].RequestStatuses[3];
            Assert.AreEqual(4, rs.StatusCode.Primary);
            Assert.AreEqual(1, rs.StatusCode.Secondary);
            Assert.AreEqual("Event conflict. Date/time is busy.", rs.Description);
            Assert.IsNull(rs.ExtraData);
        }

        /// <summary>
        /// Tests that valid RDATE properties are parsed correctly.
        /// </summary>
        [Test, Category("Serialization")]
        public void PARSE15()
        {
            IICalendar iCal = iCalendar.LoadFromFile(@"Calendars\Serialization\PARSE15.ics")[0];
            Assert.AreEqual(1, iCal.Events.Count);
            Assert.AreEqual(3, iCal.Events[0].RecurrenceDates.Count);
            Assert.AreEqual((iCalDateTime)new DateTime(1997, 7, 14, 12, 30, 0, DateTimeKind.Utc), iCal.Events[0].RecurrenceDates[0][0].StartTime);
            Assert.AreEqual((iCalDateTime)new DateTime(1996, 4, 3, 2, 0, 0, DateTimeKind.Utc), iCal.Events[0].RecurrenceDates[1][0].StartTime);
            Assert.AreEqual((iCalDateTime)new DateTime(1996, 4, 3, 4, 0, 0, DateTimeKind.Utc), iCal.Events[0].RecurrenceDates[1][0].EndTime);
            Assert.AreEqual(new iCalDateTime(1997, 1, 1), iCal.Events[0].RecurrenceDates[2][0].StartTime);
            Assert.AreEqual(new iCalDateTime(1997, 1, 20), iCal.Events[0].RecurrenceDates[2][1].StartTime);
            Assert.AreEqual(new iCalDateTime(1997, 2, 17), iCal.Events[0].RecurrenceDates[2][2].StartTime);
            Assert.AreEqual(new iCalDateTime(1997, 4, 21), iCal.Events[0].RecurrenceDates[2][3].StartTime);
            Assert.AreEqual(new iCalDateTime(1997, 5, 26), iCal.Events[0].RecurrenceDates[2][4].StartTime);
            Assert.AreEqual(new iCalDateTime(1997, 7, 4), iCal.Events[0].RecurrenceDates[2][5].StartTime);
            Assert.AreEqual(new iCalDateTime(1997, 9, 1), iCal.Events[0].RecurrenceDates[2][6].StartTime);
            Assert.AreEqual(new iCalDateTime(1997, 10, 14), iCal.Events[0].RecurrenceDates[2][7].StartTime);
            Assert.AreEqual(new iCalDateTime(1997, 11, 28), iCal.Events[0].RecurrenceDates[2][8].StartTime);
            Assert.AreEqual(new iCalDateTime(1997, 11, 29), iCal.Events[0].RecurrenceDates[2][9].StartTime);
            Assert.AreEqual(new iCalDateTime(1997, 12, 25), iCal.Events[0].RecurrenceDates[2][10].StartTime);
        }

        /// <summary>
        /// Tests that DateTime values that are out-of-range are still parsed correctly
        /// and set to the closest representable date/time in .NET.
        /// </summary>
        [Test, Category("Serialization")]
        public void PARSE16()
        {
            IICalendar iCal = iCalendar.LoadFromFile(@"Calendars\Serialization\PARSE16.ics")[0];
            Assert.AreEqual(6, iCal.Events.Count);

            IEvent evt = iCal.Events["nc2o66s0u36iesitl2l0b8inn8@google.com"];
            Assert.IsNotNull(evt);

            // The "Created" date is out-of-bounds.  It should be coerced to the
            // closest representable date/time.
            Assert.AreEqual(DateTime.MinValue, evt.Created.Value);
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
        //    Assert.AreEqual(iCal.Events[0].Properties["DTSTART"].Value, "1234");
        //    Assert.AreEqual(iCal.Events[0].Properties["DTEND"].Value, "5678");
        //}

        /// <summary>
        /// Tests that Lotus Notes-style properties are properly handled.
        /// https://sourceforge.net/tracker/?func=detail&aid=2033495&group_id=187422&atid=921236
        /// Sourceforge bug #2033495
        /// </summary>
        [Test, Category("Serialization")]
        public void PARSE18()
        {
            IICalendar iCal = iCalendar.LoadFromFile(typeof(iCalendar), @"Calendars\Serialization\PARSE18.ics", Encoding.UTF8)[0];
            Assert.AreEqual(1, iCal.Events.Count);
            Assert.AreEqual(iCal.Properties["X-LOTUS-CHILD_UID"].Value, "XXX");
        }
                       
        /// <summary>
        /// Tests that line/column numbers are correctly tracked for
        /// parsed (deserialized) calendars.
        /// </summary>
        [Test, Category("Serialization")]
        public void LineColumns1()
        {
            SerializationContext ctx = new SerializationContext();

            ISerializationSettings settings = ctx.GetService(typeof(ISerializationSettings)) as ISerializationSettings;
            settings.EnsureAccurateLineNumbers = true;

            iCalendarSerializer serializer = new iCalendarSerializer();
            serializer.SerializationContext = ctx;
            IICalendar iCal = iCalendar.LoadFromFile(@"Calendars\Serialization\PARSE1.ics", Encoding.UTF8, serializer)[0];

            Assert.AreEqual(2, iCal.Events.Count);
            Assert.AreEqual(4, iCal.Events[0].Line);
            Assert.AreEqual(18, iCal.Events[1].Line);
            Assert.AreEqual(5, iCal.Events[0].Properties["CREATED"].Line);
            Assert.AreEqual(6, iCal.Events[0].Properties["LAST-MODIFIED"].Line);
            Assert.AreEqual(7, iCal.Events[0].Properties["DTSTAMP"].Line);
            Assert.AreEqual(8, iCal.Events[0].Properties["UID"].Line);
            Assert.AreEqual(9, iCal.Events[0].Properties["SUMMARY"].Line);
            Assert.AreEqual(10, iCal.Events[0].Properties["CLASS"].Line);
            Assert.AreEqual(11, iCal.Events[0].Properties["DTSTART"].Line);
            Assert.AreEqual(12, iCal.Events[0].Properties["DTEND"].Line);
            Assert.AreEqual(13, iCal.Events[0].Properties["CATEGORIES"].Line);
            Assert.AreEqual(14, iCal.Events[0].Properties["X-MOZILLA-ALARM-DEFAULT-LENGTH"].Line);
            Assert.AreEqual(15, iCal.Events[0].Properties["LOCATION"].Line);
        }

        /// <summary>
        /// Tests that line/column numbers are correctly tracked for
        /// parsed (deserialized) calendars.
        /// </summary>
        [Test, Category("Serialization")]
        public void LineColumns2()
        {
            SerializationContext ctx = new SerializationContext();

            ISerializationSettings settings = ctx.GetService(typeof(ISerializationSettings)) as ISerializationSettings;
            settings.EnsureAccurateLineNumbers = true;

            iCalendarSerializer serializer = new iCalendarSerializer();
            serializer.SerializationContext = ctx;
            IICalendar iCal = iCalendar.LoadFromFile(@"Calendars\Serialization\PARSE3.ics", Encoding.UTF8, serializer)[0];

            Assert.IsNotNull(iCal.Todos["2df60496-1e73-11db-ba96-e3cfe6793b5f"]);
            Assert.IsNotNull(iCal.Todos["4836c236-1e75-11db-835f-a024e2a6131f"]);
            Assert.AreEqual(110, iCal.Todos["4836c236-1e75-11db-835f-a024e2a6131f"].Properties["LOCATION"].Line);
            Assert.AreEqual(123, iCal.Todos["2df60496-1e73-11db-ba96-e3cfe6793b5f"].Properties["UID"].Line);
        }

        private static byte[] ReadBinary(string fileName)
        {
            byte[] binaryData = null;
            using (FileStream reader = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                binaryData = new byte[reader.Length];
                reader.Read(binaryData, 0, (int)reader.Length);
            }

            return binaryData;
        }

        private static bool CompareBinary(string fileName, byte[] data)
        {
            byte[] binaryData = null;
            using (FileStream reader = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                binaryData = new byte[reader.Length];
                reader.Read(binaryData, 0, (int)reader.Length);
            }

            if (binaryData == null && data == null)
                return true;
            else if (binaryData == null || data == null)
                return false;

            for (int i = 0; i < data.Length; i++)
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
        //    evt2.AddRelatedTo(evt1.UID, RelationshipTypes.Parent); // evt1 is the parent of evt2

        //    iCalendarSerializer serializer = new iCalendarSerializer(iCal);
        //    serializer.Serialize(@"Calendars\Serialization\Temp\RELATED_TO1.ics");

        //    iCal = iCalendar.LoadFromFile(@"Calendars\Serialization\Temp\RELATED_TO1.ics");
        //    evt2 = iCal.Events[evt2.UID];

        //    Assert.AreEqual(1, evt2.Related_To.Length);
        //    Assert.AreEqual(evt1.UID, evt2.RelatedTo[0].Value);
        //    Assert.AreEqual(((Parameter)evt2.RelatedTo[0].Parameters["RELTYPE"]).Values[0], RelationshipTypes.Parent);
        //}
    }
}
