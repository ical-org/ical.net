//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using Ical.Net.Serialization.DataTypes;
using Ical.Net.Utility;
using NodaTime;
using NodaTime.Extensions;
using NodaTime.Text;

namespace Ical.Net.DataTypes;

/// <summary>
/// Represents a DATE, DATE-TIME, or DATE-TIME with a time zone.
/// <para/>
/// <see cref="Time"/> values are rounded to the nearest second.
/// Time zone offset is <i>not</i> stored.
/// <para/>
/// See RFC 5545, Section 3.3.4 and 3.3.5.
/// </summary>
public sealed class CalDateTime : IFormattable, IEquatable<CalDateTime>
{
    private readonly LocalDate _localDate;
    private readonly LocalTime? _localTime;
    private readonly string? _tzId;


    /// <summary>
    /// The time zone ID for Universal Coordinated Time (UTC).
    /// </summary>
    public const string UtcTzId = "UTC";

    /// <summary>
    /// Creates a <see cref="CalDateTime"/>
    /// with the current local date/time and no time zone.
    /// </summary>
    public static CalDateTime Now => new(DateTime.Now);

    /// <summary>
    /// Creates a <see cref="CalDateTime"/>
    /// with the current local date and no time or time zone.
    /// </summary>
    public static CalDateTime Today => FromDateTimeDate(DateTime.Today);

    /// <summary>
    /// Creates a <see cref="CalDateTime"/>
    /// with the current date/time in the UTC time zone.
    /// </summary>
    public static CalDateTime UtcNow => new(DateTime.UtcNow);

    /// <summary>
    /// This constructor is required for the SerializerFactory to work.
    /// </summary>
    [ExcludeFromCodeCoverage]
    private CalDateTime()
    {
        // required for the SerializerFactory to work
    }

    /// <summary>
    /// Creates a <see cref="CalDateTime"/> representing a DATE-TIME value
    /// with an optional time zone.
    /// <para/>
    /// Time zone will be UTC if <paramref name="tzId"/> is <see langword="null"/> and
    /// <paramref name="value"/> kind is <see cref="DateTimeKind.Utc"/>.
    /// </summary>
    /// <param name="value">The value to copy the local date and time from.</param>
    /// <param name="tzId">The time zone ID.</param>
    public CalDateTime(DateTime value, string? tzId = null) : this(
        LocalDateTime.FromDateTime(value),
        tzId ?? value.Kind switch
        {
            DateTimeKind.Utc => UtcTzId,
            _ => null
        })
    { }

    /// <summary>
    /// Creates a <see cref="CalDateTime"/> representing a DATE-TIME value
    /// with an optional time zone.
    /// </summary>
    /// <param name="year"></param>
    /// <param name="month"></param>
    /// <param name="day"></param>
    /// <param name="hour"></param>
    /// <param name="minute"></param>
    /// <param name="second"></param>
    /// <param name="tzId">The time zone ID.</param>
    public CalDateTime(int year, int month, int day, int hour, int minute, int second, string? tzId = null)
        : this(new LocalDate(year, month, day), new LocalTime(hour, minute, second), tzId)
    { }

    /// <summary>
    /// Creates a <see cref="CalDateTime"/> representing a DATE value.
    /// </summary>
    /// <param name="year"></param>
    /// <param name="month"></param>
    /// <param name="day"></param>
    public CalDateTime(int year, int month, int day)
        : this(new LocalDate(year, month, day))
    { }

    /// <summary>
    /// Creates a <see cref="CalDateTime"/> representing a DATE-TIME value
    /// with an optional time zone.
    /// </summary>
    /// <param name="value">The local date and time.</param>
    /// <param name="tzId">The time zone ID.</param>
    public CalDateTime(LocalDateTime value, string? tzId = null)
        : this(value.Date, value.TimeOfDay, tzId)
    { }

    /// <summary>
    /// Creates a <see cref="CalDateTime"/> representing a DATE value.
    /// </summary>
    /// <param name="date">The local date.</param>
    /// <exception cref="ArgumentOutOfRangeException">Year must be a positive number.</exception>
    public CalDateTime(LocalDate date)
    {
        // NodaTime supports year values <1 (BCE). Make sure these
        // years are considered invalid.
        if (date.Year < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(date), "Year must be a positive value");
        }

        _localDate = date;
    }

    /// <summary>
    /// Creates a <see cref="CalDateTime"/> representing a DATE-TIME value
    /// with an optional time zone.
    /// </summary>
    /// <param name="date">The local date.</param>
    /// <param name="time">The local time.</param>
    /// <param name="tzId">The time zone ID.</param>
    private CalDateTime(LocalDate date, LocalTime time, string? tzId = null)
        : this(date)
    {
        // RFC 5545, Section 3.3.5 does not allow for fractional seconds.
        _localTime = TimeAdjusters.TruncateToSecond(time);

        _tzId = tzId;
    }

    /// <summary>
    /// Creates a <see cref="CalDateTime"/> by parsing <paramref name="value"/>
    /// using the <see cref="DateTimeSerializer"/>.
    /// </summary>
    /// <param name="value">An iCalendar-compatible date or date-time string.
    /// <para/>
    /// If the parsed string represents an RFC 5545, Section 3.3.4, DATE value,
    /// it cannot have a time zone, and the <paramref name="tzId"/> will be ignored.
    /// <para/>
    /// If the parsed string represents an RFC 5545, DATE-TIME value, the <paramref name="tzId"/> will be used.
    /// </param>
    /// <param name="tzId">The time zone ID.</param>
    public CalDateTime(string value, string? tzId = null)
    {
        var serializer = new DateTimeSerializer();
        var dt = serializer.Deserialize(new StringReader(value)) as CalDateTime
                 ?? throw new InvalidOperationException($"Failure when deserializing value '{value}'");

        _localDate = dt._localDate;
        _localTime = dt._localTime;
        _tzId = dt.IsUtc ? UtcTzId : tzId;

        if (dt.IsUtc && tzId != null && !string.Equals(tzId, UtcTzId, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                $"The value '{value}' represents UTC date/time, but the specified timezone '{tzId}' is not '{UtcTzId}'.",
                nameof(tzId));
        }
    }

    public bool Equals(CalDateTime? other) => this == other;

    /// <inheritdoc/>
    public override bool Equals(object? obj)
        => obj is CalDateTime other && this == other;

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(_localDate, _localTime, _tzId);

    public static bool operator ==(CalDateTime? left, CalDateTime? right)
    {
        if (ReferenceEquals(left, right))
        {
            return true;
        }

        if (left is null || right is null)
        {
            return false;
        }

        return left._localDate == right._localDate
            && left._localTime == right._localTime
            && string.Equals(left.TzId, right.TzId, StringComparison.OrdinalIgnoreCase);
    }

    public static bool operator !=(CalDateTime? left, CalDateTime? right)
    {
        return !(left == right);
    }

    /// <summary>
    /// Returns <see langword="true"/> if the date/time value is floating.
    /// <para/>
    /// A floating date/time value does not include a time zone ID or UTC offset,
    /// so it is interpreted as local time in the context where it is used.
    /// <para/>
    /// A floating date/time value is useful when the exact time zone is not
    /// known or when the event should be interpreted in the local time zone of
    /// the user or system processing the calendar data.
    /// </summary>
    public bool IsFloating => _tzId is null;

    /// <summary>
    /// Returns <see langword="true"/> if the time zone is UTC.
    /// </summary>
    public bool IsUtc => string.Equals(_tzId, UtcTzId, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Returns <see langword="true"/> if <see cref="Time"/> is not null.
    /// </summary>
    public bool HasTime => _localTime.HasValue;

    /// <summary>
    /// Gets the time zone ID.
    /// A <see langword="null"/> value indicates a floating date/time.
    /// </summary>
    public string? TzId => _tzId;

    /// <summary>
    /// Gets the time zone ID.
    /// A <see langword="null"/> value indicates a floating date/time.
    /// </summary>
    /// <remarks>This is an alias for <see cref="TzId"/></remarks>
    public string? TimeZoneName => TzId;

    /// <summary>
    /// Gets the year.
    /// </summary>
    public int Year => _localDate.Year;

    /// <summary>
    /// Gets the month.
    /// </summary>
    public int Month => _localDate.Month;

    /// <summary>
    /// Gets the day.
    /// </summary>
    public int Day => _localDate.Day;

    /// <summary>
    /// Gets the hour. Defaults to 0 for DATE values.
    /// </summary>
    public int Hour => _localTime?.Hour ?? 0;

    /// <summary>
    /// Gets the minute. Defaults to 0 for DATE values.
    /// </summary>
    public int Minute => _localTime?.Minute ?? 0;

    /// <summary>
    /// Gets the second. Defaults to 0 for DATE values.
    /// </summary>
    public int Second => _localTime?.Second ?? 0;

    /// <summary>
    /// Gets the DayOfWeek.
    /// </summary>
    public DayOfWeek DayOfWeek => _localDate.DayOfWeek.ToDayOfWeek();

    /// <summary>
    /// Gets the DayOfYear.
    /// </summary>
    public int DayOfYear => _localDate.DayOfYear;

    /// <summary>
    /// Gets the date.
    /// </summary>
    public LocalDate Date => _localDate;

    /// <summary>
    /// Gets the time, or <see langword="null"/> if there is no time.
    /// </summary>
    public LocalTime? Time => _localTime;

#if NET6_0_OR_GREATER

    /// <summary>
    /// Creates a <see cref="CalDateTime"/> representing a DATE value.
    /// </summary>
    /// <param name="date">The local date.</param>
    public static CalDateTime FromDateOnly(DateOnly date) => new(date.ToLocalDate());

    /// <summary>
    /// Gets the date.
    /// </summary>
    public DateOnly ToDateOnly() => _localDate.ToDateOnly();

    /// <summary>
    /// Gets the time, or <see langword="null"/> if there is no time.
    /// </summary>
    public TimeOnly? ToTimeOnly() => _localTime?.ToTimeOnly();
#endif

    /// <summary>
    /// Creates a <see cref="CalDateTime"/> representing a DATE value.
    /// </summary>
    /// <param name="value">The value to copy the local date from.</param>
    /// <returns>A new <see cref="CalDateTime"/> with same date as the specified <see cref="DateTime"/>.</returns>
    public static CalDateTime FromDateTimeDate(DateTime value) => new(LocalDate.FromDateTime(value));

    /// <summary>
    /// Converts this value to <see cref="DateTime"/> with <see cref="DateTimeKind.Utc"/>.
    /// <para/>
    /// DATE values will default to <see cref="LocalTime.Midnight"/>.
    /// <para/>
    /// Values with a time zone will be converted to the UTC time zone.
    /// Values without a time zone (<see cref="IsFloating"/>) will have the same local time.
    /// </summary>
    /// <returns>A <see cref="DateTime"/> value representing this value converted to the UTC time zone.</returns>
    public DateTime ToDateTimeUtc() => ToZonedOrDefault(DateTimeZone.Utc).ToDateTimeUtc();

    /// <summary>
    /// Returns the local date and time as a <see cref="DateTime"/> with <see cref="DateTimeKind.Unspecified"/>.
    /// <para/>
    /// DATE values will default to <see cref="LocalTime.Midnight"/>.
    /// </summary>
    /// <returns>A <see cref="DateTime"/> value with the same date and time (or midnight) as this value.</returns>
    public DateTime ToDateTimeUnspecified() => ToLocalDateTime().ToDateTimeUnspecified();

    /// <summary>
    /// Constructs a <see cref="LocalDateTime"/> from this value's date and time.
    /// <para/>
    /// DATE values will default to <see cref="LocalTime.Midnight"/>.
    /// </summary>
    /// <returns>A local date time with the same date and time (or midnight) as this value.</returns>
    public LocalDateTime ToLocalDateTime()
        => _localDate.At(_localTime ?? LocalTime.Midnight);

    /// <summary>
    /// Creates a <see cref="CalDateTime"/> representing a DATE-TIME value
    /// with a time zone.
    /// <para/>
    /// The time zone offset from the <see cref="ZonedDateTime"/> is ignored,
    /// so converting back to <see cref="ZonedDateTime"/> may produce a
    /// different value.
    /// </summary>
    /// <param name="value">The value to copy the date, time, and time zone ID from.</param>
    public static CalDateTime FromZonedDateTime(ZonedDateTime value) => new(value.LocalDateTime, value.Zone.Id);

    /// <summary>
    /// Converts this value to <see cref="ZonedDateTime"/>.
    /// <para/>
    /// Values without a time zone will throw an <see cref="InvalidOperationException"/>.
    /// Use <see cref="ToZonedOrDefault(DateTimeZone)"/> to handle floating values.
    /// <para/>
    /// If the local date and time is ambiguous due to the time zone, it will be resolved using <see cref="NodaTime.TimeZones.Resolvers.LenientResolver"/>.
    /// </summary>
    /// <returns>A zoned date time representing this value as close as possible.</returns>
    /// <exception cref="InvalidOperationException">Time zone is null</exception>
    public ZonedDateTime ToZonedDateTime()
    {
        if (_tzId is null)
        {
            throw new InvalidOperationException("CalDateTime must have a time zone to convert to ZonedDateTime");
        }

        return DateUtil.GetZone(_tzId).AtLeniently(ToLocalDateTime());
    }

    /// <summary>
    /// Converts this value to <see cref="ZonedDateTime"/>.
    /// <para/>
    /// DATE values will default to <see cref="LocalTime.Midnight"/>.
    /// <para/>
    /// Values without a time zone will be treated as being in the specified time zone.
    /// If the local date and time is ambiguous due to the time zone, it will be resolved using <see cref="NodaTime.TimeZones.Resolvers.LenientResolver"/>.
    /// </summary>
    /// <param name="defaultZone">The time zone to use if this value has no time zone.</param>
    /// <returns>A zoned date time representing this value in its own time zone or the specified time zone.</returns>
    public ZonedDateTime ToZonedOrDefault(DateTimeZone defaultZone)
    {
        if (_tzId is null)
        {
            return ToLocalDateTime().InZoneLeniently(defaultZone);
        }

        return DateUtil.GetZone(_tzId).AtLeniently(ToLocalDateTime());
    }

    /// <inheritdoc />
    public override string ToString() => ToString(null, null);

    /// <inheritdoc cref="ToString()"/>
    public string ToString(string? format) => ToString(format, null);

    /// <inheritdoc cref="ToString()"/>
    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        // Use the .NET format options to format
        formatProvider ??= CultureInfo.InvariantCulture;

        // Print only the time zone ID, not the time zone offset.
        // The spec does not allow including a time zone offset to
        // specify a DATE-TIME value, so this should not include it.
        var tzIdString = _tzId is not null ? $" {_tzId}" : string.Empty;

        if (HasTime)
        {
            return ToLocalDateTime().ToString(format, formatProvider) + tzIdString;
        }

        // Handle special case for "O" format that NodaTime.LocalDate
        // does not support. This is the ISO 8601 date format, so it
        // always uses the invariant culture.
        if (format == "O")
        {
            return LocalDatePattern.Iso.Format(_localDate) + tzIdString;
        }

        return _localDate.ToString(format ?? "d", formatProvider) + tzIdString;
    }
}
