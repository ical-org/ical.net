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
    public class Serialization
    {
        private TZID tzid;

        [TestFixtureSetUp]
        public void InitAll()
        {            
            tzid = new TZID("US-Eastern");
        }

        static public void DoTests()
        {
            // FIXME: add test case for merging calendars and then serializing/deserializing, vs. simply merging

            Serialization s = new Serialization();
            s.InitAll();
            s.SERIALIZE1();
            s.SERIALIZE2();
            s.SERIALIZE3();
            s.SERIALIZE4();
            s.SERIALIZE5();
            s.SERIALIZE6();
            s.SERIALIZE7();
            s.SERIALIZE8();
            s.SERIALIZE9();
            s.SERIALIZE10();
            s.SERIALIZE11();
            s.SERIALIZE12();
            s.SERIALIZE13();
            s.SERIALIZE14();
            s.SERIALIZE15();
            s.SERIALIZE16();
            s.SERIALIZE17();
            s.SERIALIZE18();
            s.SERIALIZE19();
            s.LANGUAGE1();
            s.LANGUAGE2();
            s.REQUIREDPARAMETERS1();
            s.TIMEZONE1();
            s.TIMEZONE2();

            s.USHOLIDAYS();
        }

        private void SerializeTest(string filename) { SerializeTest(filename, typeof(iCalendar)); }
        private void SerializeTest(string filename, Type iCalType)
        {
            iCalendar iCal1 = iCalendar.LoadFromFile(iCalType, @"Calendars\Serialization\" + filename);
            iCalendarSerializer serializer = new iCalendarSerializer(iCal1);

            if (!Directory.Exists(@"Calendars\Serialization\Temp"))
                Directory.CreateDirectory(@"Calendars\Serialization\Temp");

            serializer.Serialize(@"Calendars\Serialization\Temp\" + Path.GetFileNameWithoutExtension(filename) + "_Serialized.ics");
            iCalendar iCal2 = iCalendar.LoadFromFile(iCalType, @"Calendars\Serialization\Temp\" + Path.GetFileNameWithoutExtension(filename) + "_Serialized.ics");

            CompareCalendars(iCal1, iCal2);
        }

        static public void CompareCalendars(iCalendar iCal1, iCalendar iCal2)
        {
            Assert.IsTrue(object.Equals(iCal1.Method, iCal2.Method), "Methods do not match");
            Assert.IsTrue(object.Equals(iCal1.ProductID, iCal2.ProductID), "ProductIDs do not match");
            Assert.IsTrue(object.Equals(iCal1.Scale, iCal2.Scale), "Scales do not match");
            Assert.IsTrue(object.Equals(iCal1.Version, iCal2.Version), "Versions do not match");
            
            for (int i = 0; i < iCal1.Events.Count; i++)
                CompareComponents(iCal1.Events[i], iCal2.Events[i]);
            for (int i = 0; i < iCal1.FreeBusy.Count; i++)
                CompareComponents(iCal1.FreeBusy[i], iCal2.FreeBusy[i]);
            for (int i = 0; i < iCal1.Journals.Count; i++)
                CompareComponents(iCal1.Journals[i], iCal2.Journals[i]);
            for (int i = 0; i < iCal1.Todos.Count; i++)
                CompareComponents(iCal1.Todos[i], iCal2.Todos[i]);
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
                        CompareArrays(obj1 as Array, obj2 as Array, field.Name);
                    else Assert.IsTrue(object.Equals(obj1, obj2), field.Name + " does not match");
                }                
            }

            foreach (PropertyInfo prop in properties)
            {
                if (prop.GetCustomAttributes(typeof(SerializedAttribute), true).Length > 0)
                {
                    object obj1 = prop.GetValue(cb1, null);
                    object obj2 = prop.GetValue(cb2, null);

                    if (prop.PropertyType.IsArray)
                        CompareArrays(obj1 as Array, obj2 as Array, prop.Name);
                    else
                        Assert.IsTrue(object.Equals(obj1, obj2), prop.Name + " does not match");
                }
            }
        }

        static public void CompareArrays(Array a1, Array a2, string value)
        {
            if (a1 == null &&
                a2 == null)
                return;
            Assert.IsFalse((a1 == null && a2 != null) || (a1 != null && a2 == null), value + " do not match - one array is null");
            Assert.IsTrue(a1.Length == a2.Length, value + " do not match - array lengths do not match");


            IEnumerator enum1 = a1.GetEnumerator();
            IEnumerator enum2 = a2.GetEnumerator();
            while (enum1.MoveNext() && enum2.MoveNext())
                Assert.IsTrue(enum1.Current.Equals(enum2.Current), value + " do not match");                
        }

        [Test, Category("Serialization")]
        public void SERIALIZE1()
        {
            SerializeTest("SERIALIZE1.ics");            
        }

        [Test, Category("Serialization")]
        public void SERIALIZE2()
        {
            SerializeTest("SERIALIZE2.ics");
        }

        [Test, Category("Serialization")]
        public void SERIALIZE3()
        {
            SerializeTest("SERIALIZE3.ics");
        }

        [Test, Category("Serialization")]
        public void SERIALIZE4()
        {
            SerializeTest("SERIALIZE4.ics");
        }

        [Test, Category("Serialization")]
        public void SERIALIZE5()
        {
            SerializeTest("SERIALIZE5.ics");
        }

        [Test, Category("Serialization")]
        public void SERIALIZE6()
        {
            SerializeTest("SERIALIZE6.ics");
        }

        [Test, Category("Serialization")]
        public void SERIALIZE7()
        {
            SerializeTest("SERIALIZE7.ics");
        }

        [Test, Category("Serialization")]
        public void SERIALIZE8()
        {
            SerializeTest("SERIALIZE8.ics");
        }

        [Test, Category("Serialization")]
        public void SERIALIZE9()
        {
            SerializeTest("SERIALIZE9.ics");
        }

        [Test, Category("Serialization")]
        public void SERIALIZE10()
        {
            SerializeTest("SERIALIZE10.ics");
        }

        [Test, Category("Serialization")]
        public void SERIALIZE11()
        {
            SerializeTest("SERIALIZE11.ics");
        }

        [Test, Category("Serialization")]
        public void SERIALIZE12()
        {
            SerializeTest("SERIALIZE12.ics");
        }

        [Test, Category("Serialization")]
        public void SERIALIZE13()
        {
            SerializeTest("SERIALIZE13.ics");
        }

        [Test, Category("Serialization")]
        public void SERIALIZE14()
        {
            SerializeTest("SERIALIZE14.ics");
        }

        [Test, Category("Serialization")]
        public void SERIALIZE15()
        {
            SerializeTest("SERIALIZE15.ics");
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

            SerializeTest("SERIALIZE16.ics", typeof(CustomICal1));
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

            SerializeTest("SERIALIZE17.ics", typeof(CustomICal1));
        }

        [Test, Category("Serialization")]
        public void SERIALIZE18()
        {
            iCalendar iCal = new iCalendar();

            Event evt = iCal.Create<Event>();
            evt.Summary = "Test event title";
            evt.Start = new Date_Time(2007, 3, 19);
            evt.Start.Kind = DateTimeKind.Utc;
            evt.Duration = new TimeSpan(24, 0, 0);
            evt.Created = evt.Start.Copy();
            evt.DTStamp = evt.Start.Copy();
            evt.UID = "123456789";
            evt.IsAllDay = true;            

            Recur rec = new Recur("FREQ=WEEKLY;INTERVAL=3;BYDAY=TU,FR,SU;COUNT=4");
            evt.AddRecurrence(rec);

            iCalendarSerializer serializer = new iCalendarSerializer(iCal);
            string icalString = serializer.SerializeToString();

            Assert.IsNotEmpty(icalString, "iCalendarSerializer.SerializeToString() must not be empty");

            ComponentBaseSerializer compSerializer = new ComponentBaseSerializer(evt);
            string evtString = compSerializer.SerializeToString();

            Assert.IsTrue(evtString.Equals("BEGIN:VEVENT\r\nCREATED:20070319T000000Z\r\nDTEND:20070320T000000Z\r\nDTSTAMP:20070319T000000Z\r\nDTSTART;VALUE=DATE:20070319\r\nDURATION:P1D\r\nRRULE:FREQ=WEEKLY;INTERVAL=3;COUNT=4;BYDAY=TU,FR,SU\r\nSUMMARY:Test event title\r\nUID:123456789\r\nEND:VEVENT\r\n"), "ComponentBaseSerializer.SerializeToString() serialized incorrectly");
        }

        [Test, Category("Serialization")]
        public void SERIALIZE19()
        {
            iCalendar iCal = new iCalendar();

            Event evt = iCal.Create<Event>();
            evt.Summary = "Test event title";
            evt.Start = new Date_Time(2007, 4, 29);
            evt.End = evt.Start.AddDays(1);
            evt.IsAllDay = true;

            Recur rec = new Recur("FREQ=WEEKLY;INTERVAL=3;BYDAY=TU,FR,SU;COUNT=4");
            evt.AddRecurrence(rec);

            ComponentBaseSerializer compSerializer = new ComponentBaseSerializer(evt);

            FileStream fs = new FileStream(@"Calendars\Serialization\SERIALIZE19.ics", FileMode.Create, FileAccess.Write);
            compSerializer.Serialize(fs, Encoding.UTF8);
            fs.Close();

            iCalendar iCal1 = new iCalendar();
            
            fs = new FileStream(@"Calendars\Serialization\SERIALIZE19.ics", FileMode.Open, FileAccess.Read);
            Event evt1 = ComponentBase.LoadFromStream<Event>(fs);
            fs.Close();

            CompareComponents(evt, evt1);
        }
        
        [Test, Category("Serialization")]
        public void USHOLIDAYS()
        {
            SerializeTest("USHolidays.ics");
        }

        [Test, Category("Serialization")]
        public void LANGUAGE1()
        {
            SerializeTest("LANGUAGE1.ics");
        }

        [Test, Category("Serialization")]
        public void LANGUAGE2()
        {
            SerializeTest("LANGUAGE2.ics");
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
            
            DDay.iCal.Components.TimeZone tz = iCal.TimeZones[0];
            tz.Last_Modified = new Date_Time(2007, 1, 1);

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

            DDay.iCal.Components.TimeZone tz = iCal.TimeZones[0];
            foreach (DDay.iCal.Components.TimeZone.TimeZoneInfo tzi in tz.TimeZoneInfos)
                tzi.Start = new Date_Time(2007, 1, 1);

            iCalendarSerializer serializer = new iCalendarSerializer(iCal);
            serializer.Serialize(@"Calendars\Serialization\Temp\TIMEZONE2.ics");

            iCal = iCalendar.LoadFromFile(@"Calendars\Serialization\Temp\TIMEZONE2.ics");
            tz = iCal.TimeZones[0];

            foreach (DDay.iCal.Components.TimeZone.TimeZoneInfo tzi in tz.TimeZoneInfos)
            {
                ContentLine cl = tzi.Start.ContentLine;
                Assert.IsFalse(cl.Parameters.ContainsKey("VALUE"), "\"DTSTART\" property MUST be represented in local time in timezones");
            }

            //
            // Next, check against UTC time; DTSTART must be presented in local time
            //
            iCal = iCalendar.LoadFromFile(@"Calendars\Serialization\TIMEZONE2.ics");

            tz = iCal.TimeZones[0];
            foreach (DDay.iCal.Components.TimeZone.TimeZoneInfo tzi in tz.TimeZoneInfos)
                tzi.Start = DateTime.Now.ToUniversalTime();

            serializer = new iCalendarSerializer(iCal);
            serializer.Serialize(@"Calendars\Serialization\Temp\TIMEZONE2.ics");

            iCal = iCalendar.LoadFromFile(@"Calendars\Serialization\Temp\TIMEZONE2.ics");
            tz = iCal.TimeZones[0];

            foreach (DDay.iCal.Components.TimeZone.TimeZoneInfo tzi in tz.TimeZoneInfos)
            {
                ContentLine cl = tzi.Start.ContentLine;
                Assert.IsFalse(cl.Parameters.ContainsKey("VALUE"), "\"DTSTART\" property MUST be represented in local time in timezones");
            }
        }
    }
}
