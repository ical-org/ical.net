using Ical.Net.DataTypes;
using Ical.Net.Interfaces.Components;
using Ical.Net.Interfaces.DataTypes;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Ical.Net.UnitTests
{
    [TestFixture]
    public class TimeZoneTest
    {
        internal static readonly System.TimeZoneInfo _customTimeZone;
        static TimeZoneTest()
        {
            //sunday on the second week of March
            var transitionRuleStart = TimeZoneInfo.TransitionTime.CreateFloatingDateRule(new DateTime(1, 1, 1, 2, 0, 0), 3, 2, DayOfWeek.Sunday);
            //sunday on the first week of November
            var transitionRuleEnd = TimeZoneInfo.TransitionTime.CreateFloatingDateRule(new DateTime(1, 1, 1, 2, 0, 0), 11, 1, DayOfWeek.Sunday);
            //move clock 1 hour forward from standard time
            //rule applies to any date after 2007
            var adjustment = TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule(new DateTime(2007, 1, 1), DateTime.MaxValue.Date,
                                                                       new TimeSpan(1, 0, 0), transitionRuleStart, transitionRuleEnd);
            const string custName = "example timezone";
            //standard time is 4 hours before UTC
            _customTimeZone = TimeZoneInfo.CreateCustomTimeZone(custName, new TimeSpan(-4,0,0), custName, "standard " + custName, "daylight saving " + custName,
                new[] { adjustment });

        }
        #region HelperMethods
        public static void AssertTimezoneProperties(ICalendarComponent comp,IEnumerable<KeyValuePair<string, object>> expected)
        { 
            foreach (var kv in expected)
            {
                var prop = comp.Properties[kv.Key];
                Assert.IsNotNull(prop, comp.Name + ' ' + kv.Key);
                var val = prop.Value;
                if (val is IDateTime)
                {
                    var dt = (IDateTime)val;
                    Assert.AreEqual(kv.Value, dt.Value);
                }
                else if (val is IUtcOffset)
                {
                    var ts = (IUtcOffset)val;
                    Assert.AreEqual(kv.Value, ts.Offset);
                }
                else if (val is IEnumerable)
                {
                    CollectionAssert.AreEqual(kv.Value as IEnumerable, (IEnumerable)val);
                }
                else
                {
                    Assert.AreEqual(kv.Value, val, kv.Key);
                }
            }
        }
        #endregion //HelperMethods
        [Test, Category("TimeZone")]
        public void FromSystemTimeZone()
        {
            var timezone = VTimeZone.FromSystemTimeZone(_customTimeZone);

            Assert.AreEqual(_customTimeZone.Id, timezone.TzId);

            var adjustments = timezone.Children.Where(c => c.Name == "STANDARD" || c.Name == "DAYLIGHT").Cast<ICalendarComponent>().ToDictionary(c=>c.Name);
            CollectionAssert.IsNotEmpty(adjustments, "Could not find STANDARD or DAYLIGHT child on VTIMEZONE");
            var adjust = _customTimeZone.GetAdjustmentRules()[0];
            //{ "DTSTART", "TZOFFSETFROM", "TZOFFSETTO" } are mandated in the spec
            ICalendarComponent std;
            if (adjustments.TryGetValue("STANDARD", out std))
            {
                Assert.IsInstanceOf<IDateTime>(std.Properties["DTSTART"].Value);
                AssertTimezoneProperties(std, new Dictionary<string, object>
                {
                    ["TZOFFSETFROM"] = _customTimeZone.BaseUtcOffset + adjust.DaylightDelta,
                    ["TZOFFSETTO"] = _customTimeZone.BaseUtcOffset,
                });
                //rrule is not mandated by the spec, but given the timezoneinfo we would expect:
                //(comment out as necessary)

                var rrule = std.Properties["RRULE"].Value as RecurrencePattern;
                Assert.IsNotNull(rrule);
                var compareTo = new RecurrencePattern(FrequencyType.Yearly, 1)
                {
                    ByDay = new List<IWeekDay> { new WeekDay(DayOfWeek.Sunday, FrequencyOccurrence.First) },
                    ByMonth = new List<int> { 11 },
                    ByHour = new List<int> { 2 },
                    ByMinute = new List<int> { 0 }
                };
                Assert.AreEqual(compareTo,rrule,"standard RRULE");
            }

            ICalendarComponent daylight;
            if (adjustments.TryGetValue("DAYLIGHT", out daylight))
            {
                AssertTimezoneProperties(daylight, new Dictionary<string, object>
                {
                    ["TZOFFSETFROM"] = _customTimeZone.BaseUtcOffset,
                    ["TZOFFSETTO"] = _customTimeZone.BaseUtcOffset + adjust.DaylightDelta,
                });

                var rrule = daylight.Properties["RRULE"].Value as RecurrencePattern;
                Assert.IsNotNull(rrule);
                var compareTo = new RecurrencePattern(FrequencyType.Yearly, 1)
                {
                    ByDay = new List<IWeekDay> { new WeekDay(DayOfWeek.Sunday, FrequencyOccurrence.Second) },
                    ByMonth = new List<int> { 3 },
                    ByHour = new List<int> { 2 },
                    ByMinute = new List<int> { 0 }
                };
                Assert.AreEqual(compareTo, rrule, "standard RRULE");
            }

        }
       
    }
}
