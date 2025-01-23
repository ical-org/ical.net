//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

#nullable enable
using Ical.Net.DataTypes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

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

    public override string? SerializeToString(object obj)
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
        value.Append($"{dt.Year:0000}{dt.Month:00}{dt.Day:00}");
        if (dt.HasTime)
        {
            value.Append($"T{dt.Hour:00}{dt.Minute:00}{dt.Second:00}");
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

        var datePart = new DateOnly(); // Initialize. At this point, we know that the date part is present
        TimeOnly? timePart = null;

        if (match.Groups[1].Success)
        {
            datePart = new DateOnly(Convert.ToInt32(match.Groups[2].Value),
                Convert.ToInt32(match.Groups[3].Value),
                Convert.ToInt32(match.Groups[4].Value));
        }
        if (match.Groups.Count >= 6 && match.Groups[5].Success)
        {
            timePart = new TimeOnly(Convert.ToInt32(match.Groups[6].Value),
                Convert.ToInt32(match.Groups[7].Value),
                Convert.ToInt32(match.Groups[8].Value));
        }

        var isUtc = match.Groups[9].Success;
        if (isUtc) timeZoneId = "UTC";

        var res = timePart.HasValue
            ? new CalDateTime(datePart, timePart.Value, timeZoneId)
            : new CalDateTime(datePart);

        return res;
    }

    public IReadOnlyList<CalendarParameter> GetParameters(object value)
    {
        if (value is not CalDateTime dt)
            return [];

        var res = new List<CalendarParameter>(2);
        if (!dt.IsFloating && !dt.IsUtc)
            res.Add(new CalendarParameter("TZID", dt.TzId));

        if (!dt.HasTime)
            res.Add(new CalendarParameter("VALUE", "DATE"));

        return res;
    }
}
