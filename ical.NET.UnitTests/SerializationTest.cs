using System;
using System.Collections;
using Ical.Net;
using Ical.Net.DataTypes;
using Ical.Net.Interfaces;
using Ical.Net.Interfaces.Components;
using Ical.Net.Serialization;
using Ical.Net.Serialization.iCalendar.Serializers;
using NUnit.Framework;
using Ical.Net.Interfaces.DataTypes;
using System.IO;
using System.Collections.Generic;

namespace ical.NET.UnitTests
{
    [TestFixture]
    public class SerializationTest
    {
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

                Assert.IsTrue(isMatch, "Could not find a matching property - " + p1.Name + ":" + (p1.Value?.ToString() ?? string.Empty));
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

        const string nl = "\r\n";
        public static string InspectSerializedSection(string serialized, string sectionName, IEnumerable<string> elements)
        {
            const string notFound = "expected '{0}' not found";
            string searchFor = "BEGIN:" + sectionName;
            int begin = serialized.IndexOf(searchFor);
            Assert.AreNotEqual(-1, begin, string.Format(notFound, searchFor));
            searchFor = "END:" + sectionName;
            int end = serialized.IndexOf(searchFor, begin);
            Assert.AreNotEqual(-1, end, string.Format(notFound, searchFor));

            string searchRegion = serialized.Substring(begin, end - begin + 1);

            foreach (var e in elements)
            {
                Assert.IsTrue(searchRegion.Contains(nl + e +nl), string.Format(notFound, e));
            }

            return searchRegion;
        }
        
        [Test, Category("Serialization")]
        public void TimeZoneSerialize()
        {

            using (var cal = new Calendar() { Method = "PUBLISH", Version = "2.0" })
            {
                const string exampleTZ = "New Zealand Standard Time"; // can change this but should SupportDaylightTime
                var tzi = TimeZoneInfo.FindSystemTimeZoneById(exampleTZ);
                var timezone = VTimeZone.FromSystemTimeZone(tzi);
                cal.AddTimeZone(timezone);
                var evt = new Event
                {
                    Summary = "Testing",
                    Start = new CalDateTime(2016, 7, 14, timezone.Id),
                    End = new CalDateTime(2016, 7, 15, timezone.Id)
                };
                cal.Events.Add(evt);

                var serializer = new CalendarSerializer(new SerializationContext());
                var serializedCalendar = serializer.SerializeToString(cal);

                Console.Write(serializedCalendar);

                string vTimezone = InspectSerializedSection(serializedCalendar, "VTIMEZONE", new[] {
                        "TZID:" + timezone.Id
                    });

                string o = tzi.BaseUtcOffset.ToString("hhmm", System.Globalization.CultureInfo.InvariantCulture);
                InspectSerializedSection(vTimezone, "STANDARD", new[] {
                        "TZNAME:" + tzi.StandardName,
                        "TZOFFSETTO:" + o
                        //todo - standard time, for NZ standard time (current example)
                        //"DTSTART:20150402T030000",
                        //"RRULE:FREQ=YEARLY;BYDAY=1SU;BYHOUR=3;BYMINUTE=0;BYMONTH=4",
                        //"TZOFFSETFROM:+1300"
                });


                InspectSerializedSection(vTimezone, "DAYLIGHT", new[] {
                        "TZNAME:" + tzi.DaylightName,
                        "TZOFFSETFROM:" + o
                });
            }
        }
        [Test, Category("Serialization")]
        public void SerializeDeserialize()
        {
            using (var cal1 = new Calendar() { Method = "PUBLISH", Version = "2.0" })
            {
                var evt = new Event
                {
                    Class = "PRIVATE",
                    Created = new CalDateTime(2010, 3, 25, 12, 53, 35),
                    DtStamp = new CalDateTime(2010, 3, 25, 12, 53, 35),
                    LastModified = new CalDateTime(2010, 3, 27, 13, 53, 35),
                    Sequence = 0,
                    Uid = "42f58d4f-847e-46f8-9f4a-ce52697682cf",
                    Priority = 5,
                    Location = "here",
                    Summary = "test",
                    DtStart = new CalDateTime(2012, 3, 25, 12, 50, 00),
                    DtEnd = new CalDateTime(2012, 3, 25, 13, 10, 00)
                };
                cal1.Events.Add(evt);

                var serializer = new CalendarSerializer(new SerializationContext());
                var serializedCalendar = serializer.SerializeToString(cal1);
                using (var sr = new StringReader(serializedCalendar))
                {
                    var cal2 = Calendar.LoadFromStream(sr)[0];
                    CompareCalendars(cal1, cal2);
                }
                
            }
        }
        //3 formats - UTC, local time as defined in vTimeZone, and unspecified,
        //this is just an early iteration to get things underway
        static string CalDateString(IDateTime cdt)
        {
            return cdt.ToString("yyyyMMddhhmm", System.Globalization.CultureInfo.InvariantCulture);
        }
        [Test, Category("Serialization")]
        public void EventPropertiesSerialized()
        {
            using (var cal = new Calendar() { Method = "PUBLISH", Version= "2.0"})
            {
                var evt = new Event
                {
                    Class = "PRIVATE",
                    Created = new CalDateTime(2010, 3, 25, 12, 53, 35),
                    DtStamp = new CalDateTime(2010, 3, 25, 12, 53, 35),
                    LastModified = new CalDateTime(2010, 3, 27, 13, 53, 35),
                    Sequence = 0,
                    Uid = "42f58d4f-847e-46f8-9f4a-ce52697682cf",
                    Priority = 5,
                    Location = "here",
                    Summary = "test",
                    DtStart = new CalDateTime(2012, 3, 25, 12, 50, 00),
                    DtEnd = new CalDateTime(2012, 3, 25, 13, 10, 00)
                };
                cal.Events.Add(evt);

                var serializer = new CalendarSerializer(new SerializationContext());
                var serializedCalendar = serializer.SerializeToString(cal);

                Console.Write(serializedCalendar);
                Assert.IsTrue(serializedCalendar.StartsWith("BEGIN:VCALENDAR"));
                Assert.IsTrue(serializedCalendar.EndsWith("END:VCALENDAR\r\n"));

                var expectProperties = new[]
                {
                    "METHOD:PUBLISH",
                    "VERSION:2.0",
                    "CLASS:PRIVATE"
                };

                foreach (var p in expectProperties)
                {
                    Assert.IsTrue(serializedCalendar.Contains(nl+p+nl), "expected '"+p+"' not found");
                }

                InspectSerializedSection(serializedCalendar,"VEVENT", new[]
                {
                    "CLASS:" + evt.Class,
                    "CREATED:" + CalDateString(evt.Created),
                    "DTSTAMP:" + CalDateString(evt.DtStamp),
                    "LASTMODIFIED:" + CalDateString(evt.LastModified),
                    "SEQUENCE:" + evt.Sequence,
                    "UID:" + evt.Uid,
                    "PRIORITY:" + evt.Priority,
                    "LOCATION:" + evt.Location,
                    "SUMMARY:" + evt.Summary,
                    "DTSTART:" + CalDateString(evt.DtStart),
                    "DTEND:" + CalDateString(evt.DtEnd)
                });
            }
        }
    }
}
