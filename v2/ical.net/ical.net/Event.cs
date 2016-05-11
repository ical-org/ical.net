using System;
using NodaTime;

namespace ical.net
{
    /// <summary>
    /// A model of the VEVENT
    /// </summary>
    public class Event
    {
        public string Uid { get; private set; }
        public ZonedDateTime DtStamp { get; private set; }
        /// <summary>
        /// Inclusive start of the event. In the event that this represents a date without a time, AND no DtEnd property, the event's non-
        /// inclusive end is the end of the calendar date specified by this DtStart property. In effect, the end is 1 tick before midnight.
        /// </summary>
        public ZonedDateTime DtStart { get; private set; }
        /// <summary>
        /// Non-inclusive end of the event
        /// </summary>
        public ZonedDateTime DtEnd { get; private set; }
        public Duration Duration => DtEnd.ToInstant() - DtStart.ToInstant();

        private Event(string uid, ZonedDateTime dtStamp, ZonedDateTime dtStart)
        {
            if (dtStart.TimeOfDay != LocalTime.Midnight)
            {
                throw new ArgumentException("An event without a DtEnd property must not use times");
            }

            Uid = uid;
            DtStamp = dtStamp;
            DtStart = dtStart;
            var oneTickBeforeMidnight = Duration.FromStandardDays(1).Minus(Duration.FromTicks(1));
            DtEnd = ZonedDateTime.Add(dtStart, oneTickBeforeMidnight);
        }

        public static Event GetSingleDayEvent(string uid, ZonedDateTime dtStamp, ZonedDateTime dtStart)
        {
            return new Event(uid, dtStamp, dtStart);
        }

        public static Event GetSingleDayEvent(ZonedDateTime dtStamp, ZonedDateTime dtStart)
        {
            return GetSingleDayEvent(Guid.NewGuid().ToString(), dtStamp, dtStart);
        }

        public static Event GetSingleDayEvent(ZonedDateTime dtStart)
        {
            return GetSingleDayEvent(NodaUtilities.NowTimeWithSystemTimeZone(), dtStart);
        }

        private Event(string uid, ZonedDateTime dtStamp, ZonedDateTime dtStart, ZonedDateTime dtEnd)
        {
            if (dtEnd <= dtStart)
            {
                throw new ArgumentException($"Event start ({dtStart}) must come before event end ({dtEnd})");
            }

            Uid = uid;
            DtStamp = dtStamp;
            DtStart = dtStart;
            DtEnd = dtEnd;
        }

        public static Event GetEvent(string uid, ZonedDateTime dtStamp, ZonedDateTime dtStart, ZonedDateTime dtEnd)
        {
            return new Event(uid, dtStamp, dtStart, dtEnd);
        }

        public static Event GetEvent(ZonedDateTime dtStamp, ZonedDateTime dtStart, ZonedDateTime dtEnd)
        {
            return GetEvent(Guid.NewGuid().ToString(), dtStamp, dtStart, dtEnd);
        }

        public static Event GetEvent(ZonedDateTime dtStart, ZonedDateTime dtEnd)
        {
            return GetEvent(NodaUtilities.NowTimeWithSystemTimeZone(), dtStart, dtEnd);
        }
    }
}
