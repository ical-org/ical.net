using System;
using System.Globalization;
using Ical.Net.DataTypes;
using Ical.Net.Evaluation;
using Ical.Net.ExtensionMethods;
using Ical.Net.General.Proxies;
using Ical.Net.Interfaces.Components;
using Ical.Net.Interfaces.DataTypes;
using Ical.Net.Interfaces.General;

namespace Ical.Net
{
    /// <summary>
    /// Represents an RFC 5545 VTIMEZONE component.
    /// </summary>
    [Serializable]
    public class VTimeZone : CalendarComponent, ITimeZone
    {
        #region Static Public Methods

        public static VTimeZone FromLocalTimeZone()
        {
            return FromSystemTimeZone(TimeZoneInfo.Local);
        }

        public static VTimeZone FromLocalTimeZone(DateTime earlistDateTimeToSupport, bool includeHistoricalData)
        {
            return FromSystemTimeZone(TimeZoneInfo.Local, earlistDateTimeToSupport, includeHistoricalData);
        }

        private static void PopulateiCalTimeZoneInfo(ITimeZoneInfo tzi, TimeZoneInfo.TransitionTime transition, int year)
        {
            var c = CultureInfo.CurrentCulture.Calendar;

            var recurrence = new RecurrencePattern();
            recurrence.Frequency = FrequencyType.Yearly;
            recurrence.ByMonth.Add(transition.Month);
            recurrence.ByHour.Add(transition.TimeOfDay.Hour);
            recurrence.ByMinute.Add(transition.TimeOfDay.Minute);

            if (transition.IsFixedDateRule)
            {
                recurrence.ByMonthDay.Add(transition.Day);
            }
            else
            {
                if (transition.Week != 5)
                {
                    recurrence.ByDay.Add(new WeekDay(transition.DayOfWeek, transition.Week));
                }
                else
                {
                    recurrence.ByDay.Add(new WeekDay(transition.DayOfWeek, -1));
                }
            }

            tzi.RecurrenceRules.Add(recurrence);
        }

        public static VTimeZone FromSystemTimeZone(TimeZoneInfo tzinfo)
        {
            // Support date/times for January 1st of the previous year by default.
            return FromSystemTimeZone(tzinfo, new DateTime(DateTime.Now.Year, 1, 1).AddYears(-1), false);
        }

        public static VTimeZone FromSystemTimeZone(TimeZoneInfo tzinfo, DateTime earlistDateTimeToSupport, bool includeHistoricalData)
        {
            var adjustmentRules = tzinfo.GetAdjustmentRules();
            var utcOffset = tzinfo.BaseUtcOffset;
            var ddayTz = new VTimeZone();
            ddayTz.TzId = tzinfo.Id;

            IDateTime earliest = new CalDateTime(earlistDateTimeToSupport);
            foreach (var adjustmentRule in adjustmentRules)
            {
                // Only include historical data if asked to do so.  Otherwise,
                // use only the most recent adjustment rule available.
                if (!includeHistoricalData && adjustmentRule.DateEnd < earlistDateTimeToSupport)
                {
                    continue;
                }

                var delta = adjustmentRule.DaylightDelta;
                var ddayTzinfoStandard = new CalTimeZoneInfo();
                ddayTzinfoStandard.Name = "STANDARD";
                ddayTzinfoStandard.Start =
                    new CalDateTime(
                        new DateTime(adjustmentRule.DateStart.Year, adjustmentRule.DaylightTransitionEnd.Month, adjustmentRule.DaylightTransitionEnd.Day,
                            adjustmentRule.DaylightTransitionEnd.TimeOfDay.Hour, adjustmentRule.DaylightTransitionEnd.TimeOfDay.Minute,
                            adjustmentRule.DaylightTransitionEnd.TimeOfDay.Second).AddDays(1));
                if (ddayTzinfoStandard.Start.LessThan(earliest))
                {
                    ddayTzinfoStandard.Start = ddayTzinfoStandard.Start.AddYears(earliest.Year - ddayTzinfoStandard.Start.Year);
                }
                ddayTzinfoStandard.OffsetFrom = new UtcOffset(utcOffset + delta);
                ddayTzinfoStandard.OffsetTo = new UtcOffset(utcOffset);
                PopulateiCalTimeZoneInfo(ddayTzinfoStandard, adjustmentRule.DaylightTransitionEnd, adjustmentRule.DateStart.Year);

                // Add the "standard" time rule to the time zone
                ddayTz.AddChild(ddayTzinfoStandard);

                if (tzinfo.SupportsDaylightSavingTime)
                {
                    var ddayTzinfoDaylight = new CalTimeZoneInfo();
                    ddayTzinfoDaylight.Name = "DAYLIGHT";
                    ddayTzinfoDaylight.Start =
                        new CalDateTime(new DateTime(adjustmentRule.DateStart.Year, adjustmentRule.DaylightTransitionStart.Month,
                            adjustmentRule.DaylightTransitionStart.Day, adjustmentRule.DaylightTransitionStart.TimeOfDay.Hour,
                            adjustmentRule.DaylightTransitionStart.TimeOfDay.Minute, adjustmentRule.DaylightTransitionStart.TimeOfDay.Second));
                    if (ddayTzinfoDaylight.Start.LessThan(earliest))
                    {
                        ddayTzinfoDaylight.Start = ddayTzinfoDaylight.Start.AddYears(earliest.Year - ddayTzinfoDaylight.Start.Year);
                    }
                    ddayTzinfoDaylight.OffsetFrom = new UtcOffset(utcOffset);
                    ddayTzinfoDaylight.OffsetTo = new UtcOffset(utcOffset + delta);
                    PopulateiCalTimeZoneInfo(ddayTzinfoDaylight, adjustmentRule.DaylightTransitionStart, adjustmentRule.DateStart.Year);

                    // Add the "daylight" time rule to the time zone
                    ddayTz.AddChild(ddayTzinfoDaylight);
                }
            }

            // If no time zone information was recorded, at least
            // add a STANDARD time zone element to indicate the
            // base time zone information.
            if (ddayTz.TimeZoneInfos.Count == 0)
            {
                var ddayTzinfoStandard = new CalTimeZoneInfo();
                ddayTzinfoStandard.Name = "STANDARD";
                ddayTzinfoStandard.Start = earliest;
                ddayTzinfoStandard.OffsetFrom = new UtcOffset(utcOffset);
                ddayTzinfoStandard.OffsetTo = new UtcOffset(utcOffset);

                // Add the "standard" time rule to the time zone
                ddayTz.AddChild(ddayTzinfoStandard);
            }

            return ddayTz;
        }

        #endregion

        public VTimeZone()
        {
            Name = Components.Timezone;

            _tzInfos = new CalendarObjectListProxy<ITimeZoneInfo>(Children);
            var evaluator = new TimeZoneEvaluator(this);
            SetService(evaluator);
        }

        public virtual string Id
        {
            get { return Properties.Get<string>("TZID"); }
            set { Properties.Set("TZID", value); }
        }

        public virtual string TzId
        {
            get { return Id; }
            set { Id = value; }
        }

        public virtual Uri Url
        {
            get { return Properties.Get<Uri>("TZURL"); }
            set { Properties.Set("TZURL", value); }
        }

        private ICalendarObjectList<ITimeZoneInfo> _tzInfos;
        public virtual ICalendarObjectList<ITimeZoneInfo> TimeZoneInfos
        {
            get { return _tzInfos; }
            set { _tzInfos = value; }
        }
    }
}