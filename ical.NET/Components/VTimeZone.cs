using System;
using Ical.Net.Interfaces.Components;
using Ical.Net.DataTypes;
using System.Linq;
using Ical.Net.ExtensionMethods;

namespace Ical.Net
{
    /// <summary>
    /// Represents an RFC 5545 VTIMEZONE component.
    /// </summary>
    // This class has slightly different objectives to most other classes, as aside from the TZId component, it is only necessary for serialization
    public class VTimeZone : CalendarComponent, ITimeZone
    {
        public static VTimeZone FromLocalTimeZone()
        {
            return FromSystemTimeZone(TimeZoneInfo.Local);
        }

        public static VTimeZone FromLocalTimeZone(DateTime earlistDateTimeToSupport)
        {
            return FromSystemTimeZone(TimeZoneInfo.Local, earlistDateTimeToSupport);
        }

        /// <summary>
        /// This will create a new VTIMEZONE component including STANDARD +/- DAYLIGHT components.
        /// It will be deprecated as soon as a more up to date spec is released
        /// If you do not wish to include this information (required under the spec but archaic)
        /// simply create a new VTimeZone instance and set TZID = timeZoneInfoInstance.StandardName
        /// </summary>
        public static VTimeZone FromSystemTimeZone(TimeZoneInfo tzinfo)
        {
            // Support date/times for January 1st of the previous year by default.
            return FromSystemTimeZone(tzinfo, new DateTime(DateTime.UtcNow.Year, 1, 1).AddYears(-1));
        }

        public static VTimeZone FromSystemTimeZone(TimeZoneInfo tzinfo, DateTime earlistDateTimeToSupport)
        {
            var returnVar = new VTimeZone { TzId = tzinfo.Id };
            returnVar.AddTimeZone(tzinfo, earlistDateTimeToSupport);
            return returnVar;
        }

        //because element is invalid without an id
        public VTimeZone()
        {
            Name = Components.Timezone;

            //TimeZoneInfos = new List<ITimeZoneInfo>();
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

        //This component is designed for serialization (in order to meet the spec)
        //but NOT deserialisation (as information within the components [DAYLIGHT/STANDARD] are not used for date calculations within this library)
        //Deserialisation of these elements may be required if the deloper wishes to:
        //- use this library as the basis for an iCalendar spec validation engine
        //- interpret dates within an iCalendar containing a non standard/unrecognizable TZID
        //If this is the case, tree https://github.com/rianjs/ical.net/tree/8792b136800273aef33d59896fb36bfbf33236dd is the last version ofthe 
        //library to support the VTimeZoneInfo [DAYLIGHT/STANDARD] components
        //
        //The plan would be to remove this method as soon as the iCalendar spec has been updated to account for more modern techniques of
        //communicating timezone information
        //Serialisation is included for now as (while this format is unambiguously mandated in the spec):
        //- an email client interpreting iCalendar information might rely on the information within the calendar component
        //- validation will fail if using a TimeZoneId on any times within the calendar
        //BM
        public void AddTimeZone(TimeZoneInfo tzi, DateTime earliest)
        {
            foreach (var adjustmentRule in tzi.GetAdjustmentRules().Where(r=>r.DateEnd > earliest))
            {
                var delta = adjustmentRule.DaylightDelta;
                var stdOffset = new UtcOffset(tzi.BaseUtcOffset);
                var daylightOffset = new UtcOffset(tzi.BaseUtcOffset + delta);
                var recurence = GetRecurrencePattern(adjustmentRule.DaylightTransitionEnd, adjustmentRule.DateStart.Year);
                var vTzinfoStandard = new CalendarComponent
                {
                    Name = "STANDARD"
                };
                //magic strings
                vTzinfoStandard.Properties.Set("TZNAME", tzi.StandardName);
                vTzinfoStandard.Properties.Set("TZOFFSETFROM", daylightOffset);
                vTzinfoStandard.Properties.Set("TZOFFSETTO", stdOffset);
                vTzinfoStandard.Properties.Set("RRULE", recurence);
                vTzinfoStandard.Properties.Set("DTSTART",new CalDateTime(
                            new DateTime(adjustmentRule.DateStart.Year, adjustmentRule.DaylightTransitionEnd.Month, adjustmentRule.DaylightTransitionEnd.Day,
                                adjustmentRule.DaylightTransitionEnd.TimeOfDay.Hour, adjustmentRule.DaylightTransitionEnd.TimeOfDay.Minute,
                                adjustmentRule.DaylightTransitionEnd.TimeOfDay.Second).AddDays(1)));

                // Add the "standard" time rule to the time zone
                this.AddChild(vTzinfoStandard);

                if (tzi.SupportsDaylightSavingTime)
                {
                    recurence = GetRecurrencePattern(adjustmentRule.DaylightTransitionStart, adjustmentRule.DateStart.Year);
                    var vTzinfoDaylight = new CalendarComponent
                    {
                        Name = "DAYLIGHT",
                    };
                    vTzinfoStandard.Properties.Set("TZNAME", tzi.DaylightName);
                    vTzinfoDaylight.Properties.Set("TZOFFSETFROM", new UtcOffset(tzi.BaseUtcOffset));
                    vTzinfoDaylight.Properties.Set("TZOFFSETTO", new UtcOffset(tzi.BaseUtcOffset + delta));
                    vTzinfoDaylight.Properties.Set("RRULE", recurence);
                    vTzinfoDaylight.Properties.Set("DTSTART", new CalDateTime(new DateTime(adjustmentRule.DateStart.Year, adjustmentRule.DaylightTransitionStart.Month,
                            adjustmentRule.DaylightTransitionStart.Day, adjustmentRule.DaylightTransitionStart.TimeOfDay.Hour,
                            adjustmentRule.DaylightTransitionStart.TimeOfDay.Minute, adjustmentRule.DaylightTransitionStart.TimeOfDay.Second))
                    );

                    this.AddChild(vTzinfoDaylight);
                }
            }

        }
        private static RecurrencePattern GetRecurrencePattern(TimeZoneInfo.TransitionTime transition, int year)
        {

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

            return recurrence;
        }
    }
}