//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using Ical.Net.DataTypes;

namespace Ical.Net.Serialization.DataTypes;

public class WeekDaySerializer : EncodableDataTypeSerializer
{
    public WeekDaySerializer() { }

    public WeekDaySerializer(SerializationContext ctx) : base(ctx) { }

    public override Type TargetType => typeof(WeekDay);

    public override string? SerializeToString(object? obj)
    {
        if (obj is not WeekDay ds)
        {
            return null;
        }

        var value = string.Empty;
        if (ds.Offset.HasValue)
        {
            value += ds.Offset;
        }

        try
        {
            var name = Enum.GetName(typeof(DayOfWeek), ds.DayOfWeek);
            if (name == null) return null;

            value += name.ToUpper().Substring(0, 2);
        }
        catch
        {
            return null;
        }

        return Encode(ds, value);
    }

    private static readonly Regex _dayOfWeek = new Regex(@"(\+|-)?(\d{1,2})?(\w{2})", RegexOptions.Compiled | RegexOptions.IgnoreCase, RegexDefaults.Timeout);

    public override object? Deserialize(TextReader tr)
    {
        var value = tr.ReadToEnd();

        // Create the day specifier and associate it with a calendar object
        if (CreateAndAssociate() is not WeekDay ds)
            return null;

        // Decode the value, if necessary
        value = Decode(ds, value);
        if (value == null) return null;

        var match = _dayOfWeek.Match(value);
        if (!match.Success)
        {
            return null;
        }

        if (match.Groups[2].Success)
        {
            ds.Offset = Convert.ToInt32(match.Groups[2].Value, CultureInfo.InvariantCulture);
            if (match.Groups[1].Success && match.Groups[1].Value.Contains("-"))
            {
                ds.Offset *= -1;
            }
        }
        ds.DayOfWeek = RecurrenceRuleSerializer.GetDayOfWeek(match.Groups[3].Value);
        return ds;
    }
}
