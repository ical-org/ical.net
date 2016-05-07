using System;
using Ical.Net.Interfaces.DataTypes;
using NodaTime;

namespace Ical.Net
{
    public interface INodaDateTime : IComparable<INodaDateTime>, IFormattable, IEncodableDataType
    {
        /// <summary>
        /// Converts the date/time to this computer's local date/time.
        /// </summary>
        ZonedDateTime AsSystemLocal { get; }

        /// <summary>
        /// Converts the date/time to UTC (Coordinated Universal Time)
        /// </summary>
        ZonedDateTime AsUtc { get; }

        /// <summary>
        /// Retrieves the <see cref="Components.iCalTimeZoneInfo"/> object for the time
        /// zone set by <see cref="TzId"/>.
        /// </summary>
        //TimeZoneObservance? TimeZoneObservance { get; set; }
        /// <summary>
        /// Gets/sets whether the Value of this date/time represents
        /// a universal time.
        /// </summary>
        bool IsUniversalTime { get; }

        /// <summary>
        /// Gets the time zone name this time is in, if it references a time zone.
        /// </summary>
        string TimeZoneName { get; }

        /// <summary>
        /// Gets/sets the underlying DateTime value stored.  This should always
        /// use DateTimeKind.Utc, regardless of its actual representation.
        /// Use IsUniversalTime along with the TZID to control how this
        /// date/time is handled.
        /// </summary>
        ZonedDateTime Value { get; set; }

        /// <summary>
        /// Gets/sets whether or not this date/time value contains a 'date' part.
        /// </summary>
        bool HasDate { get; }

        /// <summary>
        /// Gets/sets whether or not this date/time value contains a 'time' part.
        /// </summary>
        bool HasTime { get; }

        /// <summary>
        /// Gets/sets the time zone ID for this date/time value.
        /// </summary>
        string TzId { get; }

        /// <summary>
        /// Gets the year for this date/time value.
        /// </summary>
        int Year { get; }

        /// <summary>
        /// Gets the month for this date/time value.
        /// </summary>
        int Month { get; }

        /// <summary>
        /// Gets the day for this date/time value.
        /// </summary>
        int Day { get; }

        /// <summary>
        /// Gets the hour for this date/time value.
        /// </summary>
        int Hour { get; }

        /// <summary>
        /// Gets the minute for this date/time value.
        /// </summary>
        int Minute { get; }

        /// <summary>
        /// Gets the second for this date/time value.
        /// </summary>
        int Second { get; }

        /// <summary>
        /// Gets the millisecond for this date/time value.
        /// </summary>
        int Millisecond { get; }

        /// <summary>
        /// Gets the ticks for this date/time value.
        /// </summary>
        long Ticks { get; }

        /// <summary>
        /// Gets the DayOfWeek for this date/time value.
        /// </summary>
        DayOfWeek DayOfWeek { get; }

        /// <summary>
        /// Gets the date portion of the date/time value.
        /// </summary>
        LocalDate Date { get; }

        /// <summary>
        /// Converts the date/time value to a local time
        /// within the specified time zone.
        /// </summary>
        //INodaDateTime ToTimeZone(TimeZoneObservance tzo);
        /// <summary>
        /// Converts the date/time value to a local time
        /// within the specified time zone.
        /// </summary>
        INodaDateTime ToTimeZone(string tzId);

        //INodaDateTime ToTimeZone(ITimeZone tz);

        INodaDateTime Add(Duration duration);
        INodaDateTime Subtract(Duration duration);
        Duration Subtract(INodaDateTime dt);

        INodaDateTime AddYears(int years);
        INodaDateTime AddMonths(int months);
        INodaDateTime AddDays(int days);
        INodaDateTime AddHours(int hours);
        INodaDateTime AddMinutes(int minutes);
        INodaDateTime AddSeconds(int seconds);
        INodaDateTime AddMilliseconds(int milliseconds);
        INodaDateTime AddTicks(long ticks);

        bool LessThan(INodaDateTime dt);
        bool GreaterThan(INodaDateTime dt);
        bool LessThanOrEqual(INodaDateTime dt);
        bool GreaterThanOrEqual(INodaDateTime dt);

        void AssociateWith(INodaDateTime dt);
    }
}