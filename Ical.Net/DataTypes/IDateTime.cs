//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

#nullable enable
using System;

namespace Ical.Net.DataTypes;

public interface IDateTime : IEncodableDataType, IComparable<IDateTime>, IFormattable, ICalendarDataType
{
    /// <summary>
    /// Converts the date/time to UTC (Coordinated Universal Time)
    /// If <see cref="IsFloating"/>==<see langword="true"/>
    /// it means that the <see cref="Value"/> is considered as local time for every timezone:
    /// The returned <see cref="Value"/> is unchanged, but with <see cref="DateTimeKind.Utc"/>.
    /// </summary>
    DateTime AsUtc { get; }

    /// <summary>
    /// Gets/sets whether the Value of this date/time represents
    /// a universal time.
    /// </summary>
    bool IsUtc { get; }

    /// <summary>
    /// Gets the timezone name this time is in, if it references a timezone.
    /// </summary>
    string? TimeZoneName { get; }

    /// <summary>
    /// Gets the date and time value in the ISO calendar as a <see cref="DateTime"/> type with <see cref="DateTimeKind.Unspecified"/>.
    /// The value has no associated timezone.<br/>
    /// The precision of the time part is up to seconds.
    /// <para/>
    /// Use <see cref="IsUtc"/> along with <see cref="TzId"/> and <see cref="IsFloating"/>
    /// to control how this date/time is handled.
    /// </summary>
    DateTime Value { get; }

    /// <summary>
    /// Returns <see langword="true"/>, if the date/time value contains a 'time' part.
    /// </summary>
    bool HasTime { get; }

    /// <summary>
    /// Returns <see langword="true"/>, if the date/time value is floating.
    /// <para/>
    /// A floating date/time value does not include a timezone identifier or UTC offset,
    /// so it is interpreted as local time in the context where it is used.
    /// <para/>
    /// A floating date/time value is useful when the exact timezone is not
    /// known or when the event should be interpreted in the local timezone of
    /// the user or system processing the calendar data.
    /// </summary>
    bool IsFloating { get; }

    /// <summary>
    /// Gets the timezone ID that applies to the <see cref="Value"/>.
    /// </summary>
    string? TzId { get; }

    /// <summary>
    /// Gets the year that applies to the <see cref="Value"/>.
    /// </summary>
    int Year { get; }

    /// <summary>
    /// Gets the month that applies to the <see cref="Value"/>.
    /// </summary>
    int Month { get; }

    /// <summary>
    /// Gets the day that applies to the <see cref="Value"/>.
    /// </summary>
    int Day { get; }

    /// <summary>
    /// Gets the hour that applies to the <see cref="Value"/>.
    /// </summary>
    int Hour { get; }

    /// <summary>
    /// Gets the minute that applies to the <see cref="Value"/>.
    /// </summary>
    int Minute { get; }

    /// <summary>
    /// Gets the second that applies to the <see cref="Value"/>.
    /// </summary>
    int Second { get; }

    /// <summary>
    /// Gets the millisecond that applies to the <see cref="Value"/>.
    /// </summary>
    int Millisecond { get; }

    /// <summary>
    /// Gets the ticks that applies to the <see cref="Value"/>.
    /// </summary>
    long Ticks { get; }

    /// <summary>
    /// Gets the DayOfWeek that applies to the <see cref="Value"/>.
    /// </summary>
    DayOfWeek DayOfWeek { get; }

    /// <summary>
    /// Gets the date portion of the <see cref="Value"/>.
    /// </summary>
    DateOnly Date { get; }

    /// <summary>
    /// Gets the time portion of the <see cref="Value"/>, or <see langword="null"/> if the <see cref="Value"/> is a pure date.
    /// </summary>
    TimeOnly? Time { get; }

    /// <summary>
    /// Converts the <see cref="Value"/> to a date/time
    /// within the specified <see paramref="otherTzId"/> timezone.
    /// <para/>
    /// If <see cref="IsFloating"/>==<see langword="true"/>
    /// it means that the <see cref="Value"/> is considered as local time for every timezone:
    /// The returned <see cref="Value"/> is unchanged and the <see paramref="otherTzId"/> is set as <see cref="TzId"/>.
    /// </summary>
    IDateTime ToTimeZone(string otherTzId);
    IDateTime Add(TimeSpan ts);
    IDateTime Subtract(TimeSpan ts);
    TimeSpan Subtract(IDateTime dt);
    IDateTime AddYears(int years);
    IDateTime AddMonths(int months);
    IDateTime AddDays(int days);
    IDateTime AddHours(int hours);
    IDateTime AddMinutes(int minutes);
    IDateTime AddSeconds(int seconds);
    IDateTime AddMilliseconds(double milliseconds);
    IDateTime AddTicks(long ticks);
    bool LessThan(IDateTime dt);
    bool GreaterThan(IDateTime dt);
    bool LessThanOrEqual(IDateTime dt);
    bool GreaterThanOrEqual(IDateTime dt);
    void AssociateWith(IDateTime dt);
}
