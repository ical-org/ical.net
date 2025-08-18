//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Ical.Net.DataTypes;
using Ical.Net.Utility;
using NodaTime;

namespace Ical.Net.Serialization.DataTypes;

/// <summary>
/// A serializer for the <see cref="CalDateTime"/> data type.
/// </summary>
public class DateTimeSerializer : SerializerBase, IParameterProvider
{
    /// <summary>
    /// This constructor is required for the SerializerFactory to work.
    /// </summary>
    public DateTimeSerializer() { }

    /// <summary>
    /// Creates a new instance of the <see cref="DateTimeSerializer"/> class.
    /// </summary>
    /// <param name="ctx"></param>
    public DateTimeSerializer(SerializationContext ctx) : base(ctx) { }

    public override Type TargetType => typeof(CalDateTime);

    public override string? SerializeToString(object? obj)
    {
        if (obj is not CalDateTime dt)
        {
            return null;
        }

        // RFC 5545 3.3.5:
        // The date with UTC time, or absolute time, is identified by a LATIN
        // CAPITAL LETTER Z suffix character, the UTC designator, appended to
        // the time value. The "TZID" property parameter MUST NOT be applied to DATE-TIME
        // properties whose time values are specified in UTC.

        var value = new StringBuilder(512);
        // NOSONAR: netstandard2.x does not support string.Create(CultureInfo.InvariantCulture, $"{...}");
        value.Append(FormattableString.Invariant($"{dt.Year:0000}{dt.Month:00}{dt.Day:00}")); // NOSONAR
        if (dt.HasTime)
        {
            value.Append(FormattableString.Invariant($"T{dt.Hour:00}{dt.Minute:00}{dt.Second:00}")); // NOSONAR
            if (dt.IsUtc)
            {
                value.Append("Z");
            }
        }

        // Encode the value as necessary
        return value.ToString();
    }

    private const RegexOptions Options = RegexOptions.Compiled | RegexOptions.IgnoreCase;
    internal static readonly Regex DateOnlyMatch = new Regex(@"^((\d{4})(\d{2})(\d{2}))?$", Options, RegexDefaults.Timeout);
    internal static readonly Regex FullDateTimePatternMatch = new Regex(@"^((\d{4})(\d{2})(\d{2}))T((\d{2})(\d{2})(\d{2})(Z)?)$", Options, RegexDefaults.Timeout);

    public override object? Deserialize(TextReader tr)
    {
        var value = tr.ReadToEnd();

        // CalDateTime is defined as the Target type
        var parent = SerializationContext.Peek();

        // The associated object is an ICalendarObject of type CalendarProperty
        // that contains any timezone ("TZID" property) deserialized in a prior step
        var timeZoneId = (parent as ICalendarParameterCollectionContainer)?.Parameters.Get("TZID");

        var match = FullDateTimePatternMatch.Match(value);
        if (!match.Success)
        {
            match = DateOnlyMatch.Match(value);
        }

        if (!match.Success)
        {
            return null;
        }

        var datePart = new LocalDate(); // Initialize. At this point, we know that the date part is present
        LocalTime? timePart = null;

        if (match.Groups[1].Success)
        {
            datePart = new LocalDate(Convert.ToInt32(match.Groups[2].Value, CultureInfo.InvariantCulture),
                Convert.ToInt32(match.Groups[3].Value, CultureInfo.InvariantCulture),
                Convert.ToInt32(match.Groups[4].Value, CultureInfo.InvariantCulture));
        }
        if (match.Groups.Count >= 6 && match.Groups[5].Success)
        {
            timePart = new LocalTime(Convert.ToInt32(match.Groups[6].Value, CultureInfo.InvariantCulture),
                Convert.ToInt32(match.Groups[7].Value, CultureInfo.InvariantCulture),
                Convert.ToInt32(match.Groups[8].Value, CultureInfo.InvariantCulture));
        }

        var isUtc = match.Groups[9].Success;
        if (isUtc) timeZoneId = "UTC";

        var res = timePart.HasValue
            ? new CalDateTime(datePart, timePart.Value, timeZoneId)
            : new CalDateTime(datePart);

        return res;
    }

    public IReadOnlyList<CalendarParameter> GetParameters(object? value)
        => ParameterProviderHelper.GetCalDateTimeParameters(value);
}
