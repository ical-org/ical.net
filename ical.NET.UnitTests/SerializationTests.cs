using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Ical.Net;
using Ical.Net.DataTypes;
using Ical.Net.ExtensionMethods;
using Ical.Net.Interfaces;
using Ical.Net.Interfaces.Components;
using Ical.Net.Serialization;
using Ical.Net.Serialization.iCalendar.Serializers;
using NUnit.Framework;
using Calendar = Ical.Net.Calendar;
using Ical.Net.Serialization.iCalendar.Serializers.DataTypes;
using Ical.Net.Interfaces.DataTypes;
using System.Linq;

namespace ical.NET.UnitTests
{
    [TestFixture]
    public class SerializationTests
    {
        #region helperMethods
        public static void CompareCalendars(ICalendar cal1, ICalendar cal2)
        {
            CompareComponents(cal1, cal2);
        }

        public static void CompareComponents(ICalendarComponent cb1, ICalendarComponent cb2)
        {
            Assert.AreEqual(cb1.Name, cb2.Name);
            var names1 = cb1.Properties.Select(c => c.Name).ToList();
            var names2 = cb2.Properties.Select(c => c.Name).ToList();
            //sometimes it may be appropriate to add extra properties (duration for instance if a DATESTART and DATEEND property are specified)
            //therefore check we have not lost properties, and if extra properties have been added we will include these in the test output
            CollectionAssert.IsSubsetOf(names1, names2, cb1.Name + " properties");

            var extra = names2.Except(names1).ToList();
            if (extra.Any())
            {
                Console.WriteLine($"The following properties of {cb1.Name} are only present in the 2nd calendar:{Environment.NewLine}" + string.Join(Environment.NewLine, extra));
            }

            foreach (var p1 in cb1.Properties)
            {
                var isMatch = false;
                foreach (var p2 in cb2.Properties.AllOf(p1.Name))
                {
                    try
                    {
                        Assert.AreEqual(p1?.Name, p2?.Name, "The properties '" + p1.Name + "' are not equal.");
                        //quite reasonably, IDateTimes are equivalent if the UTC value is equal
                        //for this test (copying and serializing->deserializing, it is important both the UTC values and the date form are the same)
                        if (p1.Value is IDateTime)
                        {
                            var d1 = (IDateTime)p1.Value;
                            var d2 = (IDateTime)p2.Value;
                            Assert.AreEqual(d1.TzId, d2.TzId, "TimeZoneId " + p1.Name);
                            Assert.AreEqual(d1.IsUniversalTime, d2.IsUniversalTime, "UTC " + p1.Name);
                            Assert.AreEqual(d1.Value, d2.Value, "The '" + p1.Name + "' property values do not match.");
                        }
                        else if (p1.Value is IComparable)
                        {
                            Assert.AreEqual(0, ((IComparable) p1.Value).CompareTo(p2.Value), "The '" + p1.Name + "' property values do not match.");
                        }
                        else if (p1.Value is IEnumerable)
                        {
                            CollectionAssert.AreEquivalent((IEnumerable)p1.Value, (IEnumerable)p2.Value, p1.Name);
                        }
                        else
                        {
                            Assert.AreEqual(p1.Value, p2.Value, "The '" + p1.Name + "' property values are not equal.");
                        }

                        isMatch = true;
                        break;
                    }
                    catch {
                    }
                }

                Assert.IsTrue(isMatch, "Could not find a matching property - " + p1.Name + ":" + (p1.Value?.ToString() ?? string.Empty));
            }

            CollectionAssert.AreEquivalent(cb1.Children.Select(c => c.Name), cb2.Children.Select(c => c.Name), cb1.Name + " children");
            for (var i = 0; i < cb1.Children.Count; i++)
            {
                var child1 = cb1.Children[i] as ICalendarComponent;
                var child2 = cb2.Children[i] as ICalendarComponent;
                if (child1 != null && child2 != null)
                {
                    CompareComponents(child1, child2);
                }
                else
                {
                    Assert.AreEqual(child1, child2, "The child objects are not equal.");
                }
            }
        }

        static ICalendar SerializeAndCompare(ICalendar cal1)
        {
            var serializer = new CalendarSerializer(new SerializationContext());
            var serializedCalendar = serializer.SerializeToString(cal1);

            Console.WriteLine(serializedCalendar);

            using (var sr = new StringReader(serializedCalendar))
            {
                var cal2 = Calendar.LoadFromStream(sr)[0];
                CompareCalendars(cal1, cal2);
                return cal2;
            }
        }
        #endregion //HelperMethods
        #region tests
        //http://icalendar.org/iCalendar-RFC-5545/3-3-5-date-time.html
        [Test, Category("Serialization")]
        public void DateTimeSerialization()
        {
            var serializer = new DateTimeSerializer();
            
            var cdt = new CalDateTime(2010, 3, 25, 12, 53, 35);
            Assert.AreEqual("20100325T125335", serializer.SerializeToString(cdt), "date with local time");
            cdt = new CalDateTime(new DateTime(2010, 3, 25, 12, 53, 35));
            Assert.AreEqual("20100325T125335", serializer.SerializeToString(cdt), "date with local time");
            cdt = new CalDateTime(new DateTime(2010, 3, 25, 12, 53, 35, DateTimeKind.Utc));
            Assert.AreEqual("20100325T125335Z", serializer.SerializeToString(cdt), "date with utc time");

            //below is not working - TODO improve testability by allowing objects which are not calendars to be serialized, then uncomment BM
            //const string timezone = "America/New_York";
            //cdt = new CalDateTime(2010, 3, 25, 12, 53, 35, timezone);
            //Assert.AreEqual($"TZID={timezone}:20100325T125335", serializer.SerializeToString(cdt), "date with local time & time zone ref.");
            //cdt = new CalDateTime(2010, 3, 25);
            //Assert.AreEqual("20100325", serializer.SerializeToString(cdt), "date only");

        }

        [Test, Category("SerializeThenDeserialize")]
        public void SerializeDeserializeEmptyCal()
        {
            var cal1 = new Calendar
            {
                Method = CalendarMethods.Publish,
                Version = "2.0"
            };

            SerializeAndCompare(cal1);
        }

        [Test, Category("SerializeThenDeserialize")]
        public void SerializeDeserializeEventProperties()
        {
            var cal1 = new Calendar
            {
                Method = CalendarMethods.Publish,
                Version = "2.0"
            };

            var evt = new Event
            {
                Class = "PRIVATE",
                Created = new CalDateTime(new DateTime(2010, 3, 25, 12, 53, 35, DateTimeKind.Utc)),
                DtStamp = new CalDateTime(new DateTime(2010, 3, 25, 12, 53, 35, DateTimeKind.Utc)),
                LastModified = new CalDateTime(2010, 3, 27, 13, 53, 35),
                Sequence = 0,
                Uid = "42f58d4f-847e-46f8-9f4a-ce52697682cf",
                Priority = 5,
                Location = "here",
                Summary = "test",
                DtStart = new CalDateTime(2012, 3, 25, 12, 50, 00),
                DtEnd = new CalDateTime(2012, 3, 25, 13, 10, 00),
                Transparency = TransparencyType.Opaque,
                Status = EventStatus.Confirmed
            };
            cal1.Events.Add(evt);

            SerializeAndCompare(cal1);
        }

        [Test, Category("SerializeThenDeserialize")]
        public void SerializeDeserializeEventDateTimes()
        {
            var cal1 = new Calendar
            {
                Method = "PUBLISH",
                Version = "2.0"
            };

            var evt = new Event
            {
                Class = "PRIVATE",
                Created = new CalDateTime(new DateTime(2010, 3, 25, 12, 53, 35,DateTimeKind.Utc)),
                DtStart = new CalDateTime(2012, 3, 25, 12, 50, 00, "New Zealand Standard Time"),
                DtEnd = new CalDateTime(2012, 3, 25, 13, 10, 00)
            };
            cal1.Events.Add(evt);

            SerializeAndCompare(cal1);
        }

        private const string _requiredParticipant = "REQ-PARTICIPANT"; //this string may be added to the api in the future
        private static readonly IList<Attendee> _attendees = new List<Attendee>
        {
            new Attendee("MAILTO:james@example.com")
            {
                CommonName = "James James",
                Role = _requiredParticipant,
                Rsvp = true,
                ParticipationStatus = ParticipationStatus.Tentative
            },
            new Attendee("MAILTO:mary@example.com")
            {
                CommonName = "Mary Mary",
                Role = _requiredParticipant,
                Rsvp = true,
                ParticipationStatus = ParticipationStatus.Accepted
            }
        }.AsReadOnly();

        [Test, Category("SerializeThenDeserialize")]
        public void SerializeDeserializeEvtAttendees()
        {
            var cal = new Calendar
            {
                Method = CalendarMethods.Request,
                Version = "2.0"
            };

            var evt = AttendeeTest.VEventFactory();
            cal.Events.Add(evt);
            const string org = "MAILTO:james@example.com";
            evt.Organizer = new Organizer(org);

            evt.Attendees.AddRange(_attendees);

            SerializeAndCompare(cal);
        }

        [Test, Category("SerializeThenDeserialize")]
        public void SerializeDeserializeEvtCategories()
        {
            var cal = new Calendar
            {
                Method = CalendarMethods.Request,
                Version = "2.0"
            };

            var evt = AttendeeTest.VEventFactory();
            cal.Events.Add(evt);
            const string org = "MAILTO:james@example.com";
            evt.Organizer = new Organizer(org);

            evt.Categories.AddRange(new[] { "1","3","8", "text with chars -;:\n" });

            SerializeAndCompare(cal);
        }

        [Test, Category("SerializeThenDeserialize")]
        public void SerializeDeserializeRRules()
        {
            //code similar to the example on the readme on the github repo - we definately want this to work
            var rrule = new RecurrencePattern(FrequencyType.Yearly, 1)
            {
                Count = 5,
                ByDay = new List<IWeekDay> { new WeekDay(DayOfWeek.Monday, FrequencyOccurrence.Last) },
                ByMonth = new List<int> { 7 }
            };
            var e = new Event
            {
                DtStart = new CalDateTime(2020, 3, 25, 13, 10, 00),
                DtEnd = new CalDateTime(2020, 3, 25, 13, 50, 00),
                RecurrenceRules = new List<IRecurrencePattern> { rrule },
            };

            var cal = new Calendar();
            cal.Events.Add(e);

            SerializeAndCompare(cal);

        }

        [Test, Category("SerializeThenDeserialize")]
        public void SerializeDeserializeGeographicLocation()
        {
            var cal = new Calendar
            {
                Method = CalendarMethods.Publish,
                Version = "2.0"
            };

            var evt = AttendeeTest.VEventFactory();
            cal.Events.Add(evt);
            const string org = "MAILTO:james@example.com";
            evt.Organizer = new Organizer(org);
            //Madison Square Gardens Lat/Long!
            evt.GeographicLocation = new GeographicLocation(40.750505, -73.993439);

            SerializeAndCompare(cal);
        }

        [Test, Category("SerializeThenDeserialize")]
        public void SerializeDeserializeEvtAlarm()
        {
            var cal = new Calendar
            {
                Method = CalendarMethods.Request,
                Version = "2.0"
            };

            var evt = AttendeeTest.VEventFactory();
            cal.Events.Add(evt);
            evt.Alarms.Add(new Alarm
            {
                Action = AlarmAction.Display,
                Summary = "test",
                Trigger = new Trigger(TimeSpan.FromHours(-1)),
                Description = "descr."
            });
            cal.Events.Add(evt);

            SerializeAndCompare(cal);
        }
        [Test, Category("SerializeThenDeserialize")]
        public void SerializeDeserializeFreeBusyResponse()
        {
            var cal = new Calendar
            {
                Method = CalendarMethods.Reply,
                Version = "2.0"
            };
            var fb = new FreeBusy
            {
                Organizer = new Organizer("mailto:maryadams@example.com"),
                DtStart = new CalDateTime(new DateTime(2020, 1, 1, 2, 0, 0, DateTimeKind.Utc)),
                DtEnd = new CalDateTime(new DateTime(2020, 12, 31, 4, 0, 0, DateTimeKind.Utc))
            };
            fb.Comments.Add("testing availability");
            fb.Attendees.Add(new Attendee("mailto:johnsmith@example.com"));
            fb.Entries.Add(new FreeBusyEntry(new Period(new CalDateTime(2020,11,3,9,0,0), new CalDateTime(2020,11,3,12,0,0)),FreeBusyStatus.Free));
            fb.Entries.Add(new FreeBusyEntry(new Period(new CalDateTime(2020, 11, 4, 9, 0, 0), new CalDateTime(2020, 11, 4, 12, 0, 0)), FreeBusyStatus.BusyTentative));
            cal.FreeBusy.Add(fb);

            SerializeAndCompare(cal);
        }
        [Test, Category("SerializeThenDeserialize")]
        public void SerializeDeserializeUtcMidnight()
        {
            var cal = new Calendar
            {
            };
            var fb = new FreeBusy
            {
                Organizer = new Organizer("mailto:maryadams@example.com"),
                DtStart = new CalDateTime(new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc)),
                DtEnd = new CalDateTime(new DateTime(2020, 12, 31, 0, 0, 0, DateTimeKind.Utc))
            };
            cal.FreeBusy.Add(fb);

            SerializeAndCompare(cal);
        }
        [Test, Category("Serialization")]
        public void EnumHyphenation()
        {
            Assert.AreEqual("REQ-PARTICIPANT", Ical.Net.Serialization.iCalendar.Serializers.Other.EnumSerializer.CamelCaseToHyphenatedUpper("ReqParticipant"));
        }
        #endregion
    }
}
