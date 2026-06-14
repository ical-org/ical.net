//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.Collections.Generic;
using System.IO;
using Ical.Net.Serialization.DataTypes;

namespace Ical.Net.DataTypes;

/// <summary>
/// Represents an RFC 5545 "BYDAY" value.
/// </summary>
public class WeekDay : EncodableDataType
{
    public virtual int? Offset { get; set; }

    public virtual DayOfWeek DayOfWeek { get; set; }

    public WeekDay()
    { }

    public WeekDay(DayOfWeek day) : this()
    {
        DayOfWeek = day;
    }

    public WeekDay(DayOfWeek day, int num) : this(day)
    {
        Offset = num;
    }

    public WeekDay(DayOfWeek day, FrequencyOccurrence type) : this(day, (int) type) { }

    public WeekDay(string value)
    {
        var serializer = new WeekDaySerializer();
        if (serializer.Deserialize(new StringReader(value)) is ICopyable deserializedObject)
        {
            CopyFrom(deserializedObject);
        }
        else
        {
            throw new ArgumentException($"Cannot convert '{value}' to a {nameof(WeekDay)} object.", nameof(value));
        }
    }

    public override bool Equals(object? obj)
    {
        if (obj is not WeekDay weekday)
        {
            return false;
        }

        return weekday.Offset == Offset && weekday.DayOfWeek == DayOfWeek;
    }

    public override int GetHashCode() => HashCode.Combine(Offset, DayOfWeek);

    /// <inheritdoc/>
    public override void CopyFrom(ICopyable obj)
    {
        base.CopyFrom(obj);
        if (obj is not WeekDay weekday) return;

        Offset = weekday.Offset;
        DayOfWeek = weekday.DayOfWeek;
    }

    public int CompareTo(object? obj)
    {
        var weekday = obj switch
        {
            string str => new WeekDay(str),
            WeekDay day => day,
            _ => throw new ArgumentException($"Must be of type 'string' or 'Weekday'", nameof(obj))
        };

        var compare = DayOfWeek.CompareTo(weekday.DayOfWeek);
        if (compare == 0)
        {
            compare = Comparer<int?>.Default.Compare(Offset, weekday.Offset);
        }
        return compare;
    }
}
