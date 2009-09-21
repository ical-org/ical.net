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
using DDay.iCal.Serialization;
using DDay.iCal.Serialization.iCalendar.Components;

namespace DDay.iCal.Test
{
    [TestFixture]
    public class SerializationTest
    {
        private TZID tzid;

        [TestFixtureSetUp]
        public void InitAll()
        {            
            tzid = new TZID("US-Eastern");
        }

        private void SerializeTest(string filename, Type iCalSerializerType) { SerializeTest(filename, typeof(iCalendar), iCalSerializerType); }
        private void SerializeTest(string filename, Type iCalType, Type iCalSerializerType)
        {
            if (!Directory.Exists(@"Calendars\Serialization\Temp"))
                Directory.CreateDirectory(@"Calendars\Serialization\Temp");

            iCalendar iCal1 = iCalendar.LoadFromFile(iCalType, @"Calendars\Serialization\" + filename);
            
            ConstructorInfo ci = iCalSerializerType.GetConstructor(new Type[] { typeof(iCalendar) });
            ISerializable serializer = ci.Invoke(new object[] { iCal1 }) as ISerializable;
            
            Assert.IsTrue(iCal1.Properties.Count > 0, "iCalendar has no properties; did it load correctly?");
            Assert.IsTrue(iCal1.UniqueComponents.Count > 0, "iCalendar has no unique components; it must to be used in SerializeTest(). Did it load correctly?");

            FileStream fs = new FileStream(@"Calendars\Serialization\Temp\" + Path.GetFileNameWithoutExtension(filename) + "_Serialized" + Path.GetExtension(filename), FileMode.Create, FileAccess.Write);
            serializer.Serialize(fs, Encoding.UTF8);
            fs.Close();

            iCalendar iCal2 = iCalendar.LoadFromFile(iCalType, @"Calendars\Serialization\Temp\" + Path.GetFileNameWithoutExtension(filename) + "_Serialized" + Path.GetExtension(filename), Encoding.UTF8, serializer);

            CompareCalendars(iCal1, iCal2);
        }

        static public void CompareCalendars(iCalendar iCal1, iCalendar iCal2)
        {
            Assert.AreEqual(iCal1.Method, iCal2.Method, "Methods do not match");
            Assert.AreEqual(iCal1.ProductID, iCal2.ProductID, "ProductIDs do not match");
            Assert.AreEqual(iCal1.Scale, iCal2.Scale, "Scales do not match");
            Assert.AreEqual(iCal1.Version, iCal2.Version, "Versions do not match");
            
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

        static public void CompareComponents(ComponentBase cb1, ComponentBase cb2)
        {
            Type type = cb1.GetType();
            Assert.IsTrue(type == cb2.GetType(), "Types do not match");
            FieldInfo[] fields = type.GetFields();
            PropertyInfo[] properties = type.GetProperties();
            
            foreach (FieldInfo field in fields)
            {
                if (field.GetCustomAttributes(typeof(SerializedAttribute), true).Length > 0)
                {
                    object obj1 = field.GetValue(cb1);
                    object obj2 = field.GetValue(cb2);

                    if (field.FieldType.IsArray)
                        CompareEnumerables(obj1 as Array, obj2 as Array, field.Name);
                    else Assert.IsTrue(object.Equals(obj1, obj2), field.Name + " does not match");
                }                
            }

            foreach (PropertyInfo prop in properties)
            {
                if (prop.GetCustomAttributes(typeof(SerializedAttribute), true).Length > 0)
                {
                    object obj1 = prop.GetValue(cb1, null);
                    object obj2 = prop.GetValue(cb2, null);

                    if (typeof(IEnumerable).IsAssignableFrom(prop.PropertyType))
                        CompareEnumerables(obj1 as Array, obj2 as Array, prop.Name);
                    else
                        Assert.IsTrue(object.Equals(obj1, obj2), prop.Name + " does not match");
                }
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

        [Test, Category("Serialization")]
        public void SERIALIZE1()
        {
            SerializeTest("SERIALIZE1.ics", typeof(iCalendarSerializer));            
        }

        [Test, Category("Serialization")]
        public void SERIALIZE2()
        {
            SerializeTest("SERIALIZE2.ics", typeof(iCalendarSerializer));
        }

        [Test, Category("Serialization")]
        public void SERIALIZE3()
        {
            SerializeTest("SERIALIZE3.ics", typeof(iCalendarSerializer));
        }

        [Test, Category("Serialization")]
        public void SERIALIZE4()
        {
            SerializeTest("SERIALIZE4.ics", typeof(iCalendarSerializer));
        }

        [Test, Category("Serialization")]
        public void SERIALIZE5()
        {
            SerializeTest("SERIALIZE5.ics", typeof(iCalendarSerializer));
        }

        [Test, Category("Serialization")]
        public void SERIALIZE6()
        {
            SerializeTest("SERIALIZE6.ics", typeof(iCalendarSerializer));
        }

        [Test, Category("Serialization")]
        public void SERIALIZE7()
        {
            SerializeTest("SERIALIZE7.ics", typeof(iCalendarSerializer));
        }

        [Test, Category("Serialization")]
        public void SERIALIZE8()
        {
            SerializeTest("SERIALIZE8.ics", typeof(iCalendarSerializer));
        }

        [Test, Category("Serialization")]
        public void SERIALIZE9()
        {
            SerializeTest("SERIALIZE9.ics", typeof(iCalendarSerializer));
        }

        [Test, Category("Serialization")]
        public void SERIALIZE10()
        {
            SerializeTest("SERIALIZE10.ics", typeof(iCalendarSerializer));
        }

        [Test, Category("Serialization")]
        public void SERIALIZE11()
        {
            SerializeTest("SERIALIZE11.ics", typeof(iCalendarSerializer));
        }

        [Test, Category("Serialization")]
        public void SERIALIZE12()
        {
            SerializeTest("SERIALIZE12.ics", typeof(iCalendarSerializer));
        }

        [Test, Category("Serialization")]
        public void SERIALIZE13()
        {
            SerializeTest("SERIALIZE13.ics", typeof(iCalendarSerializer));
        }

        [Test, Category("Serialization")]
        public void SERIALIZE14()
        {
            SerializeTest("SERIALIZE14.ics", typeof(iCalendarSerializer));
        }

        [Test, Category("Serialization")]
        public void SERIALIZE15()
        {
            SerializeTest("SERIALIZE15.ics", typeof(iCalendarSerializer));
        }

        [Test, Category("Serialization")]
        public void SERIALIZE16()
        {
            CustomICal1 iCal = new CustomICal1();
            string nonstandardText = "Some nonstandard property we want to serialize";

            CustomEvent1 evt = iCal.Create<CustomEvent1>();
            evt.Summary = "Test event";
            evt.Start = new DateTime(2007, 02, 15);
            evt.NonstandardProperty = nonstandardText;
            evt.IsAllDay = true;

            iCalendarSerializer serializer = new iCalendarSerializer(iCal);
            serializer.Serialize(@"Calendars\Serialization\SERIALIZE16.ics");

            iCal = iCalendar.LoadFromFile<CustomICal1>(@"Calendars\Serialization\SERIALIZE16.ics");
            foreach (CustomEvent1 evt1 in iCal.Events)
                Assert.IsTrue(evt1.NonstandardProperty.Equals(nonstandardText));

            SerializeTest("SERIALIZE16.ics", typeof(CustomICal1), typeof(iCalendarSerializer));
        }

        [Test, Category("Serialization")]
        public void SERIALIZE17()
        {
            // Create a normal iCalendar, serialize it, and load it as a custom calendar
            iCalendar iCal = new iCalendar();            

            Event evt = iCal.Create<Event>();
            evt.Summary = "Test event";
            evt.Start = new DateTime(2007, 02, 15, 8, 0, 0);            

            iCalendarSerializer serializer = new iCalendarSerializer(iCal);
            serializer.Serialize(@"Calendars\Serialization\SERIALIZE17.ics");

            SerializeTest("SERIALIZE17.ics", typeof(CustomICal1), typeof(iCalendarSerializer));
        }

        [Test, Category("Serialization")]
        public void SERIALIZE18()
        {
            iCalendar iCal = new iCalendar();

            Event evt = iCal.Create<Event>();
            evt.Summary = "Test event title";
            evt.Start = new iCalDateTime(2007, 3, 19);
            evt.Start.IsUniversalTime = true;
            evt.Duration = new TimeSpan(24, 0, 0);
            evt.Created = evt.Start.Copy();
            evt.DTStamp = evt.Start.Copy();
            evt.UID = "123456789";
            evt.IsAllDay = true;

            RecurrencePattern rec = new RecurrencePattern("FREQ=WEEKLY;INTERVAL=3;BYDAY=TU,FR,SU;COUNT=4");
            evt.AddRecurrencePattern(rec);

            iCalendarSerializer serializer = new iCalendarSerializer(iCal);
            string icalString = serializer.SerializeToString();

            Assert.IsNotEmpty(icalString, "iCalendarSerializer.SerializeToString() must not be empty");

            ComponentBaseSerializer compSerializer = new ComponentBaseSerializer(evt);
            string evtString = compSerializer.SerializeToString();

            Assert.IsTrue(evtString.Equals("BEGIN:VEVENT\r\nCREATED:20070319T000000Z\r\nDTEND;VALUE=DATE:20070320\r\nDTSTAMP:20070319T000000Z\r\nDTSTART;VALUE=DATE:20070319\r\nRRULE:FREQ=WEEKLY;INTERVAL=3;COUNT=4;BYDAY=TU,FR,SU\r\nSEQUENCE:0\r\nSUMMARY:Test event title\r\nUID:123456789\r\nEND:VEVENT\r\n"), "ComponentBaseSerializer.SerializeToString() serialized incorrectly");

            serializer.Serialize(@"Calendars\Serialization\SERIALIZE18.ics");
            SerializeTest("SERIALIZE18.ics", typeof(iCalendarSerializer));            
        }

        [Test, Category("Serialization")]
        public void SERIALIZE19()
        {
            iCalendar iCal = new iCalendar();

            Event evt = iCal.Create<Event>();
            evt.Summary = "Test event title";
            evt.Start = new iCalDateTime(2007, 4, 29);
            evt.End = evt.Start.AddDays(1);
            evt.IsAllDay = true;

            RecurrencePattern rec = new RecurrencePattern("FREQ=WEEKLY;INTERVAL=3;BYDAY=TU,FR,SU;COUNT=4");
            evt.AddRecurrencePattern(rec);

            ComponentBaseSerializer compSerializer = new ComponentBaseSerializer(evt);

            FileStream fs = new FileStream(@"Calendars\Serialization\SERIALIZE19.ics", FileMode.Create, FileAccess.Write);
            compSerializer.Serialize(fs, Encoding.UTF8);
            fs.Close();

            iCalendar iCal1 = new iCalendar();
            
            fs = new FileStream(@"Calendars\Serialization\SERIALIZE19.ics", FileMode.Open, FileAccess.Read);
            Event evt1 = ComponentBase.LoadFromStream<Event>(fs, Encoding.UTF8);            
            fs.Close();

            CompareComponents(evt, evt1);
        }

        [Test, Category("Serialization")]
        public void SERIALIZE20()
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
            iCalendar calendar = iCalendar.LoadFromStream(sr);

            Assert.IsTrue(calendar.Events.Count == 2, "There should be 2 events in the loaded iCalendar.");
            Assert.IsNotNull(calendar.Events["fd940618-45e2-4d19-b118-37fd7a8e3906"], "There should be an event with UID: fd940618-45e2-4d19-b118-37fd7a8e3906");
            Assert.IsNotNull(calendar.Events["ebfbd3e3-cc1e-4a64-98eb-ced2598b3908"], "There should be an event with UID: ebfbd3e3-cc1e-4a64-98eb-ced2598b3908");

            iCalendarSerializer serializer = new iCalendarSerializer(calendar);
            serializer.Serialize(@"Calendars\Serialization\SERIALIZE20.ics");

            SerializeTest("SERIALIZE20.ics", typeof(iCalendarSerializer));
        }

        [Test, Category("Serialization")]
        public void SERIALIZE21()
        {
            SerializeTest("SERIALIZE21.ics", typeof(iCalendarSerializer));
        }

        [Test, Category("Serialization")]
        public void SERIALIZE22()
        {
            SerializeTest("SERIALIZE22.ics", typeof(iCalendarSerializer));
        }

        [Test, Category("Serialization")]
        public void SERIALIZE23()
        {
            SerializeTest("SERIALIZE23.ics", typeof(iCalendarSerializer));
        }

        [Test, Category("Serialization")]
        public void SERIALIZE24()
        {
            //
            // Ensures that custom iCalendars are loaded correctly
            //
            iCalendar calendar = iCalendar.LoadFromFile<iCalendar>(@"Calendars\Serialization\SERIALIZE1.ics");
            CustomICal1 customiCal = iCalendar.LoadFromFile<CustomICal1>(@"Calendars\Serialization\SERIALIZE1.ics");

            Assert.IsTrue(calendar.Events.Count == 1, "Calendar should have 1 event");
            Assert.IsTrue(customiCal.Events.Count == 1, "Custom calendar should have 1 event");
            Assert.IsTrue(calendar.Events[0].GetType() == typeof(Event), "Calendar event should be of type Event");
            Assert.IsTrue(customiCal.Events[0].GetType() == typeof(CustomEvent1), "Custom calendar event should be of type CustomEvent1");
        }

        [Test, Category("Serialization")]
        public void SERIALIZE25()
        {
            iCalendar iCal = new iCalendar();
            Event evt = iCal.Create<Event>();
            evt.Start = DateTime.Now;
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
            serializer.Serialize(@"Calendars\Serialization\SERIALIZE25.ics");

            SerializeTest("SERIALIZE25.ics", typeof(iCalendarSerializer));
        }

        [Test, Category("Serialization")]
        public void SERIALIZE26()
        {
            URI uri = new URI("addressbook://D263B4AF-823F-4D1C-BBFE-9F11491F1559:ABPerson");
            Assert.AreEqual("addressbook", uri.Scheme);
            Assert.AreEqual("D263B4AF-823F-4D1C-BBFE-9F11491F1559:ABPerson", uri.Authority);
        }

        [Test, Category("Serialization")]
        public void SERIALIZE27()
        {
            SerializeTest("SERIALIZE27.ics", typeof(iCalendarSerializer));
        }

        [Test, Category("Serialization")]
        public void SERIALIZE28()
        {
            iCalendar iCal = new iCalendar();
            Event evt = iCal.Create<Event>();
            evt.Summary = "Test event";
            evt.Start = DateTime.Now;
            evt.Duration = TimeSpan.FromMinutes(30);
            evt.Organizer = new Cal_Address("doug@ddaysoftware.com");
            evt.AddAttendee("someone@someurl.com");
            evt.AddAttendee("another@someurl.com");
            evt.AddAttendee("lastone@someurl.com");

            iCalendarSerializer serializer = new iCalendarSerializer(iCal);
            serializer.Serialize(@"Calendars\Serialization\SERIALIZE28.ics");

            SerializeTest("SERIALIZE28.ics", typeof(iCalendarSerializer));
        }

        [Test, Category("Serialization")]
        public void SERIALIZE29()
        {            
            SerializeTest("SERIALIZE29.ics", typeof(iCalendarSerializer));
        }

        /// <summary>
        /// Tests bug #2148092 - Percent compelete serialization error
        /// </summary>
        [Test, Category("Serialization")]
        public void SERIALIZE30()
        {
            SerializeTest("SERIALIZE30.ics", typeof(iCalendarSerializer));
        }

        /// <summary>
        /// Tests adding custom properties to a calendar.
        /// </summary>
        [Test, Category("Serialization")]
        public void SERIALIZE31()
        {
            iCalendar iCal = new iCalendar();
            Event evt = iCal.Create<Event>();

            StringBuilder htmlBuilder = new StringBuilder();
            htmlBuilder.Append("<HTML><HEAD><META HTTP-EQUIV=\"Content-Type\" CONTENT=\"text/html;charset=iso-8859-1\"></HEAD><BODY>");
            htmlBuilder.Append("<B>Test</B>");
            htmlBuilder.Append("</BODY></HTML>");

            // This adds the property to the event automatically
            Property p = new Property(evt, "X-ALT-DESC", htmlBuilder.ToString());
            p.AddParameter("FMTTYPE", "text/html");

            iCalendarSerializer serializer = new iCalendarSerializer(iCal);
            serializer.Serialize(@"Calendars\Serialization\SERIALIZE31.ics");

            SerializeTest("SERIALIZE31.ics", typeof(iCalendarSerializer));
        }

        [Test, Category("Serialization")]
        public void SERIALIZE32()
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
            evt.Url = new URI("http://www.ftb.pl/news/59941_0_1/tunnel-electrocity-2008-timetable.htm");
            evt.Summary = "This is a title";
            evt.Description = "This is a description";

            iCalendarSerializer serializer = new iCalendarSerializer(iCal);
            string output = serializer.SerializeToString();
            serializer.Serialize(@"Calendars\Serialization\SERIALIZE32.ics");

            Assert.IsFalse(Regex.IsMatch(output, @"\r\n[\r\n]"));

            SerializeTest("SERIALIZE32.ics", typeof(iCalendarSerializer));
        }

        [Test, Category("Serialization")]
        public void SERIALIZE33()
        {            
            iCalendar iCal = iCalendar.LoadFromFile(@"Calendars\Serialization\SERIALIZE33.ics");
            Event evt = iCal.Events["edb7a48a-d846-47f8-bad2-9ea3f29bcda5"];
            
            Assert.IsNotNull(evt);
            Assert.AreEqual(new Duration(TimeSpan.FromDays(12) + TimeSpan.FromHours(1)), evt.Duration, "Duration should be 12 days, 1 hour");

            SerializeTest("SERIALIZE33.ics", typeof(iCalendarSerializer));
        }

        //[Test, Category("Serialization")]
        public void XCAL1()
        {
            iCalendar iCal = new iCalendar();

            Event evt = iCal.Create<Event>();
            evt.Summary = "Test event title";
            evt.Start = new iCalDateTime(2007, 4, 29);
            evt.End = evt.Start.AddDays(1);
            evt.IsAllDay = true;

            RecurrencePattern rec = new RecurrencePattern("FREQ=WEEKLY;INTERVAL=3;BYDAY=TU,FR,SU;COUNT=4");
            evt.AddRecurrencePattern(rec);

            xCalSerializer serializer = new xCalSerializer(iCal);
            serializer.Serialize(@"Calendars\Serialization\XCAL1.xcal");

            SerializeTest("XCAL1.xcal", typeof(xCalSerializer));
        }
        
        [Test, Category("Serialization")]
        public void USHOLIDAYS()
        {
            SerializeTest("USHolidays.ics", typeof(iCalendarSerializer));
        }

        [Test, Category("Serialization")]
        public void LANGUAGE1()
        {
            SerializeTest("LANGUAGE1.ics", typeof(iCalendarSerializer));
        }

        [Test, Category("Serialization")]
        public void LANGUAGE2()
        {
            SerializeTest("LANGUAGE2.ics", typeof(iCalendarSerializer));
        }

        [Test, Category("Serialization")]
        public void LANGUAGE3()
        {
            SerializeTest("RussiaHolidays.ics", typeof(iCalendarSerializer));

            string calendarPath = Path.Combine(Environment.CurrentDirectory, "Calendars");
            calendarPath = Path.Combine(calendarPath, "Serialization");

            // Ensure that LoadFromUri() and LoadFromFile() produce identical results.
            // Thanks to Eugene, a student from Russia, who helped track down this bug.
            Assembly assembly = Assembly.GetExecutingAssembly();
            iCalendar russia1 = iCalendar.LoadFromUri(new Uri(Path.Combine(calendarPath, "RussiaHolidays.ics")));
            iCalendar russia2 = iCalendar.LoadFromFile(Path.Combine(calendarPath, "RussiaHolidays.ics"));

            CompareCalendars(russia1, russia2);
        }

        [Test, Category("Serialization")]
        public void LANGUAGE4()
        {
            string calendarPath = Path.Combine(Environment.CurrentDirectory, "Calendars");
            calendarPath = Path.Combine(calendarPath, "Serialization");

            iCalendar russia1 = iCalendar.LoadFromUri(new Uri("http://www.mozilla.org/projects/calendar/caldata/RussiaHolidays.ics"));
            iCalendar russia2 = iCalendar.LoadFromFile(Path.Combine(calendarPath, "RussiaHolidays.ics"));

            CompareCalendars(russia1, russia2);
        }

        [Test]
        public void REQUIREDPARAMETERS1()
        {
            iCalendar iCal = new iCalendar();
            iCalendarSerializer serializer = new iCalendarSerializer(iCal);
            serializer.Serialize(@"Calendars\Serialization\Temp\REQUIREDPARAMETERS1.ics");

            iCal = iCalendar.LoadFromFile(@"Calendars\Serialization\Temp\REQUIREDPARAMETERS1.ics");
            Assert.IsNotEmpty(iCal.Version);
            Assert.IsNotEmpty(iCal.ProductID);

            iCal.Version = string.Empty;
            iCal.ProductID = null;
            Assert.IsNotEmpty(iCal.Version, "VERSION is required");
            Assert.IsNotEmpty(iCal.ProductID, "PRODID is required");
        }

        [Test]
        public void TIMEZONE1()
        {
            iCalendar iCal = iCalendar.LoadFromFile(@"Calendars\Serialization\TIMEZONE1.ics");
            
            iCalTimeZone tz = iCal.TimeZones[0];
            tz.Last_Modified = new iCalDateTime(2007, 1, 1);

            iCalendarSerializer serializer = new iCalendarSerializer(iCal);
            serializer.Serialize(@"Calendars\Serialization\Temp\TIMEZONE1.ics");

            iCal = iCalendar.LoadFromFile(@"Calendars\Serialization\Temp\TIMEZONE1.ics");
            tz = iCal.TimeZones[0];

            ContentLine cl = tz.Last_Modified.ContentLine;
            Assert.IsFalse(cl.Parameters.ContainsKey("VALUE"), "The \"VALUE\" parameter is not allowed on \"LAST-MODIFIED\"");
        }

        [Test]
        public void TIMEZONE2()
        {
            //
            // First, check against the VALUE parameter; it must be absent in DTSTART
            //

            iCalendar iCal = iCalendar.LoadFromFile(@"Calendars\Serialization\TIMEZONE2.ics");

            iCalTimeZone tz = iCal.TimeZones[0];
            foreach (iCalTimeZoneInfo tzi in tz.TimeZoneInfos)
                tzi.Start = new iCalDateTime(2007, 1, 1);

            iCalendarSerializer serializer = new iCalendarSerializer(iCal);
            serializer.Serialize(@"Calendars\Serialization\Temp\TIMEZONE2.ics");

            iCal = iCalendar.LoadFromFile(@"Calendars\Serialization\Temp\TIMEZONE2.ics");
            tz = iCal.TimeZones[0];

            foreach (iCalTimeZoneInfo tzi in tz.TimeZoneInfos)
            {
                ContentLine cl = tzi.Start.ContentLine;
                Assert.IsFalse(cl.Parameters.ContainsKey("VALUE"), "\"DTSTART\" property MUST be represented in local time in timezones");
            }

            //
            // Next, check against UTC time; DTSTART must be presented in local time
            //
            iCal = iCalendar.LoadFromFile(@"Calendars\Serialization\TIMEZONE2.ics");

            tz = iCal.TimeZones[0];
            foreach (iCalTimeZoneInfo tzi in tz.TimeZoneInfos)
                tzi.Start = DateTime.Now.ToUniversalTime();

            serializer = new iCalendarSerializer(iCal);
            serializer.Serialize(@"Calendars\Serialization\Temp\TIMEZONE2.ics");

            iCal = iCalendar.LoadFromFile(@"Calendars\Serialization\Temp\TIMEZONE2.ics");
            tz = iCal.TimeZones[0];

            foreach (iCalTimeZoneInfo tzi in tz.TimeZoneInfos)
            {
                ContentLine cl = tzi.Start.ContentLine;
                Assert.IsFalse(cl.Parameters.ContainsKey("VALUE"), "\"DTSTART\" property MUST be represented in local time in timezones");
            }
        }

        [Test, Category("Serialization")]
        public void TIMEZONE3()
        {
            SerializeTest("TIMEZONE3.ics", typeof(iCalendarSerializer));

            iCalendar iCal = new iCalendar();
            iCalendar tmp_cal = iCalendar.LoadFromFile(@"Calendars\Serialization\TIMEZONE3.ics");
            iCal.MergeWith(tmp_cal);

            iCalendarSerializer serializer = new iCalendarSerializer(iCal);
            serializer.Serialize(@"Calendars\Serialization\testMeOut.ics");
        }

        [Test, Category("Serialization")]
        public void PARSE1()
        {
            iCalendar iCal = iCalendar.LoadFromFile(@"Calendars\Serialization\PARSE1.ics");
            Assert.IsTrue(iCal.Events.Count == 2, "iCalendar should have 2 events");
        }

        [Test, Category("Serialization")]
        public void PARSE2()
        {
            iCalendar iCal = iCalendar.LoadFromFile(@"Calendars\Serialization\PARSE2.ics");
            Assert.IsTrue(iCal.Events.Count == 4, "iCalendar should have 4 events");
        }

        [Test, Category("Serialization")]
        public void PARSE3()
        {
            iCalendar iCal = iCalendar.LoadFromFile(@"Calendars\Serialization\PARSE3.ics");            
        }

        /// <summary>
        /// Verifies that blank lines between components are allowed
        /// (as occurs with some applications/parsers - i.e. KOrganizer)
        /// </summary>
        [Test, Category("Serialization")]
        public void PARSE4()
        {
            iCalendar iCal = iCalendar.LoadFromFile(@"Calendars\Serialization\PARSE4.ics");
        }

        /// <summary>
        /// Verifies that a calendar will load without a VERSION or PRODID
        /// specification.
        /// </summary>
        [Test, Category("Serialization")]
        public void PARSE5()
        {
            iCalendar iCal = iCalendar.LoadFromFile(@"Calendars\Serialization\PARSE5.ics");
        }

        /// <summary>
        /// Tests a calendar that should fail to properly parse.
        /// </summary>
        [Test, Category("Serialization"), ExpectedException("antlr.MismatchedTokenException")]
        public void PARSE6()
        {
            iCalendar iCal = iCalendar.LoadFromFile(@"Calendars\Serialization\PARSE6.ics");
        }

        /// <summary>
        /// Similar to PARSE4 and PARSE5 tests.
        /// </summary>
        [Test, Category("Serialization")]
        public void PARSE7()
        {
            iCalendar iCal = iCalendar.LoadFromFile(@"Calendars\Serialization\PARSE7.ics");
        }

        /// <summary>
        /// Tests that a mixed-case VERSION property is loaded properly
        /// </summary>
        [Test, Category("Serialization")]
        public void PARSE8()
        {
            iCalendar iCal = iCalendar.LoadFromFile(@"Calendars\Serialization\PARSE8.ics");
            Assert.AreEqual("2.5", iCal.Version);
        }

        /// <summary>
        /// Tests that multiple properties are allowed in iCalObjects
        /// </summary>
        [Test, Category("Serialization")]
        public void PARSE9()
        {
            iCalendar iCal = iCalendar.LoadFromFile(@"Calendars\Serialization\PARSE9.ics");
            
            IList<Property> props = iCal.Properties.AllOf("VERSION");
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
            iCalendar iCal = iCalendar.LoadFromFile(@"Calendars\Serialization\PARSE10.ics");

            Event evt = iCal.Events[0];
            IList<Parameter> parms = evt.DTStart.Parameters.AllOf("VALUE");
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
            iCalendar iCal = iCalendar.LoadFromUri(new Uri("http://www.google.com/calendar/ical/tvhot064q4p48frqdalgo3fb2k%40group.calendar.google.com/public/basic.ics"));
            Assert.IsNotNull(iCal);
            Assert.AreEqual(1, iCal.Events.Count);
            Assert.AreEqual(1, iCal.TimeZones.Count);

            TZID tzid = iCal.TimeZones[0].TZID;
            IList<Occurrence> occurrences = iCal.GetOccurrences(new iCalDateTime(2009, 8, 24, tzid, iCal), new iCalDateTime(2009, 9, 28, tzid, iCal));
            Assert.AreEqual(5, occurrences.Count);
            Assert.AreEqual(new iCalDateTime(2009, 8, 26, 8, 0, 0, tzid, iCal), occurrences[0].Period.StartTime);
            Assert.AreEqual(new iCalDateTime(2009, 9, 2, 8, 0, 0, tzid, iCal), occurrences[1].Period.StartTime);
            Assert.AreEqual(new iCalDateTime(2009, 9, 9, 8, 0, 0, tzid, iCal), occurrences[2].Period.StartTime);
            Assert.AreEqual(new iCalDateTime(2009, 9, 16, 8, 0, 0, tzid, iCal), occurrences[3].Period.StartTime);
            Assert.AreEqual(new iCalDateTime(2009, 9, 23, 8, 0, 0, tzid, iCal), occurrences[4].Period.StartTime);
            Assert.AreEqual(new iCalDateTime(2009, 8, 26, 10, 0, 0, tzid, iCal), occurrences[0].Period.EndTime);
        }

        /// <summary>
        /// Tests that string escaping works with Text elements.
        /// </summary>
        [Test, Category("Serialization")]
        public void PARSE12()
        {
            string value = @"test\with\;characters";
            Text v1 = value;
            Text v2 = new Text(value, true);

            Assert.AreEqual(value, v1.Value, "String escaping was incorrect.");
            Assert.AreEqual(@"test\with;characters", v2.Value, "String escaping was incorrect.");

            value = @"C:\Path\To\My\New\Information";
            v1 = value;
            v2 = new Text(value, true);
            Assert.AreEqual(value, v1.Value, "String escaping was incorrect.");
            Assert.AreEqual("C:\\Path\\To\\My\new\\Information", v2.Value, "String escaping was incorrect.");

            value = @"\""This\r\nis\Na\, test\""\;\\;,";
            v1 = value;
            v2 = new Text(value, true);

            Assert.AreEqual(value, v1.Value, "String escaping was incorrect.");
            Assert.AreEqual("\"This\\r\nis\na, test\";\\;,", v2.Value, "String escaping was incorrect.");
        }

        /// <summary>
        /// Tests that empty parameters are allowed in iCalObjects
        /// </summary>
        [Test, Category("Serialization")]
        public void PARSE13()
        {
            iCalendar iCal = iCalendar.LoadFromFile(@"Calendars\Serialization\PARSE13.ics");

            Assert.AreEqual(2, iCal.Events.Count);            
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

        /// <summary>
        /// Ensures that a basic, binary attachment functions as it should.
        /// </summary>
        [Test, Category("Serialization")]
        public void BINARY1()
        {
            iCalendar iCal = new iCalendar();
            
            // Create a test event
            Event evt = iCal.Create<Event>();
            evt.Summary = "Test Event";
            evt.Start = new iCalDateTime(2007, 10, 15, 8, 0, 0);
            evt.Duration = TimeSpan.FromHours(1);

            // Add an attachment to this event
            Binary binary = new Binary();
            binary.AddParameter("X-FILENAME", "WordDocument.doc");
            binary.Data = ReadBinary(@"Data\Test.doc");
            evt.AddAttachment(binary);

            iCalendarSerializer serializer = new iCalendarSerializer(iCal);
            if (!Directory.Exists(@"Calendars\Serialization\Temp"))
                Directory.CreateDirectory(@"Calendars\Serialization\Temp");
            serializer.Serialize(@"Calendars\Serialization\Temp\BINARY1.ics");

            iCal = iCalendar.LoadFromFile(@"Calendars\Serialization\Temp\BINARY1.ics");
            evt = iCal.Events[0];
            binary = evt.Attach[0];

            Assert.IsTrue(CompareBinary(@"Data\Test.doc", binary.Data), "Serialized version of Test.doc did not match the deserialized version.");
        }

        /// <summary>
        /// Ensures that very large attachments function as they should.
        /// </summary>
        [Test, Category("Serialization")]
        public void BINARY2()
        {
            iCalendar iCal = new iCalendar();

            // Create a test event
            Event evt = iCal.Create<Event>();
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
            Binary binary = new Binary();            
            binary.Data = UnicodeEncoding.Default.GetBytes(sb.ToString());
            evt.AddAttachment(binary);

            iCalendarSerializer serializer = new iCalendarSerializer(iCal);
            if (!Directory.Exists(@"Calendars\Serialization\Temp"))
                Directory.CreateDirectory(@"Calendars\Serialization\Temp");
            serializer.Serialize(@"Calendars\Serialization\Temp\BINARY2.ics");

            iCal = iCalendar.LoadFromFile(@"Calendars\Serialization\Temp\BINARY2.ics");
            evt = iCal.Events[0];
            binary = evt.Attach[0];

            // Ensure the generated and serialized strings match
            Assert.AreEqual(sb.ToString(), UnicodeEncoding.Default.GetString(binary.Data));

            // Times to finish the test for attachment file sizes (on my computer): 
            //  0.92MB = 1.2 seconds
            //  2.76MB = 6 seconds
            //  4.6MB = 15.1 seconds
            //  9.2MB = 54 seconds
        }

        [Test, Category("Serialization")]
        public void RELATED_TO1()
        {
            iCalendar iCal = new iCalendar();

            // Create a test event
            Event evt1 = iCal.Create<Event>();
            evt1.Summary = "Work Party";
            evt1.Start = new iCalDateTime(2007, 10, 15, 8, 0, 0);
            evt1.Duration = TimeSpan.FromHours(1);

            // Create another event that relates to evt1
            Event evt2 = iCal.Create<Event>();
            evt2.Summary = "Water Polo";
            evt2.Start = new iCalDateTime(2007, 10, 15, 10, 0, 0);
            evt2.Duration = TimeSpan.FromHours(1);
            evt2.AddRelatedTo(evt1.UID, RelationshipTypes.Parent); // evt1 is the parent of evt2
            
            iCalendarSerializer serializer = new iCalendarSerializer(iCal);
            serializer.Serialize(@"Calendars\Serialization\Temp\RELATED_TO1.ics");

            iCal = iCalendar.LoadFromFile(@"Calendars\Serialization\Temp\RELATED_TO1.ics");
            evt2 = iCal.Events[evt2.UID];

            Assert.AreEqual(1, evt2.Related_To.Length);
            Assert.AreEqual(evt1.UID, evt2.Related_To[0].Value);
            Assert.AreEqual(((Parameter)evt2.Related_To[0].Parameters["RELTYPE"]).Values[0], RelationshipTypes.Parent);
        }
    }
}
