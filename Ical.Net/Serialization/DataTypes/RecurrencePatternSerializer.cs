//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ical.Net.DataTypes;

namespace Ical.Net.Serialization.DataTypes;

public class RecurrencePatternSerializer : EncodableDataTypeSerializer
{
    public RecurrencePatternSerializer() { }

    public RecurrencePatternSerializer(SerializationContext ctx) : base(ctx) { }

    public static DayOfWeek GetDayOfWeek(string value)
    {
        return value.ToUpper() switch
        {
            "SU" => DayOfWeek.Sunday,
            "MO" => DayOfWeek.Monday,
            "TU" => DayOfWeek.Tuesday,
            "WE" => DayOfWeek.Wednesday,
            "TH" => DayOfWeek.Thursday,
            "FR" => DayOfWeek.Friday,
            "SA" => DayOfWeek.Saturday,
            _ => throw new ArgumentOutOfRangeException(nameof(value),
                $"{value} is not a valid iCal day-of-week indicator.")
        };
    }

    protected static void AddInt32Values(IList<int> list, string value)
    {
        var values = value.Split(',');
        foreach (var v in values)
        {
            list.Add(Convert.ToInt32(v));
        }
    }

    public virtual void CheckRange(string name, IList<int> values, int min, int max)
    {
        var allowZero = (min == 0 || max == 0);
        foreach (var value in values)
        {
            CheckRange(name, value, min, max, allowZero);
        }
    }

    public virtual void CheckRange(string name, int value, int min, int max)
    {
        var allowZero = min == 0 || max == 0;
        CheckRange(name, value, min, max, allowZero);
    }

    public virtual void CheckRange(string name, int? value, int min, int max)
    {
        var allowZero = min == 0 || max == 0;
        CheckRange(name, value, min, max, allowZero);
    }

    public virtual void CheckRange(string name, int value, int min, int max, bool allowZero)
    {
        if ((value < min || value > max || (!allowZero && value == 0)))
        {
            throw new ArgumentOutOfRangeException(nameof(name),
                $"{name} value {value} is out of range. Valid values are between {min} and {max}{(allowZero ? "" : ", excluding zero (0)")}.");
        }
    }

    public virtual void CheckRange(string name, int? value, int min, int max, bool allowZero)
    {
        if (value != null && (value < min || value > max || (!allowZero && value == 0)))
        {
            throw new ArgumentOutOfRangeException(nameof(name),
                $"{name} value {value} is out of range. Valid values are between {min} and {max}{(allowZero ? "" : ", excluding zero (0)")}.");
        }
    }

    public virtual void CheckMutuallyExclusive(string name1, string name2, int? obj1, CalDateTime? obj2)
    {
        if ((obj1 == null) || (obj2 == null))
        {
            return;
        }

        throw new ArgumentOutOfRangeException(nameof(name1),
            $"Both {name1} and {name2} cannot be supplied together; they are mutually exclusive.");
    }

    private static void SerializeByValue(List<string> aggregate, IList<int> byValue, string name)
    {
        if (byValue.Any())
        {
            aggregate.Add($"{name}={string.Join(",", byValue.Select(i => i.ToString()))}");
        }
    }

    public override Type TargetType => typeof(RecurrencePattern);

    public override string? SerializeToString(object obj)
    {
        var factory = GetService<ISerializerFactory>();
        if (obj is not RecurrencePattern recur || factory == null)
        {
            return null;
        }

        // Push the recurrence pattern onto the serialization stack
        SerializationContext.Push(recur);
        var values = new List<string>()
        {
            $"FREQ={Enum.GetName(typeof(FrequencyType), recur.Frequency)?.ToUpper()}"
        };


        //-- FROM RFC2445 --
        //The INTERVAL rule part contains a positive integer representing how
        //often the recurrence rule repeats. The default value is "1", meaning
        //every second for a SECONDLY rule, or every minute for a MINUTELY
        //rule, every hour for an HOURLY rule, every day for a DAILY rule,
        //every week for a WEEKLY rule, every month for a MONTHLY rule and
        //every year for a YEARLY rule.
        var interval = recur.Interval;
        if (interval != 1)
        {
            values.Add($"INTERVAL={interval}");
        }

        if (recur.Until is not null
            && factory.Build(typeof(CalDateTime), SerializationContext) is IStringSerializer serializer1)
        {
            values.Add($"UNTIL={serializer1.SerializeToString(recur.Until)}");
        }

        if (recur.FirstDayOfWeek != DayOfWeek.Monday)
        {
            values.Add($"WKST={Enum.GetName(typeof(DayOfWeek), recur.FirstDayOfWeek)?.ToUpper().Substring(0, 2)}");
        }

        if (recur.Count.HasValue)
        {
            values.Add($"COUNT={recur.Count}");
        }

        if (recur.ByDay.Count > 0)
        {
            var bydayValues = new List<string>(recur.ByDay.Count);

            if (factory.Build(typeof(WeekDay), SerializationContext) is IStringSerializer serializer)
            {
                bydayValues.AddRange(recur.ByDay.Select(byday => serializer.SerializeToString(byday)));
            }

            values.Add($"BYDAY={string.Join(",", bydayValues)}");
        }

        SerializeByValue(values, recur.ByHour, "BYHOUR");
        SerializeByValue(values, recur.ByMinute, "BYMINUTE");
        SerializeByValue(values, recur.ByMonth, "BYMONTH");
        SerializeByValue(values, recur.ByMonthDay, "BYMONTHDAY");
        SerializeByValue(values, recur.BySecond, "BYSECOND");
        SerializeByValue(values, recur.BySetPosition, "BYSETPOS");
        SerializeByValue(values, recur.ByWeekNo, "BYWEEKNO");
        SerializeByValue(values, recur.ByYearDay, "BYYEARDAY");

        // Pop the recurrence pattern off the serialization stack
        SerializationContext.Pop();

        return Encode(recur, string.Join(";", values));
    }

    /// <summary>
    /// Deserializes an RRULE value string into an <see cref="RecurrencePattern"/> object.
    /// <para/>
    /// RFC5545, section 3.3.10:
    /// The RRULE value type is a structured value consisting of a
    /// list of one or more recurrence grammar parts. Each rule part is
    /// defined by a NAME=VALUE pair. The rule parts are separated from
    /// each other by the SEMICOLON character. The rule parts are not
    /// ordered in any particular sequence. Individual rule parts MUST
    /// only be specified once. Compliant applications MUST accept rule
    /// parts ordered in any sequence.
    /// </summary>
    /// <param name="tr"></param>
    /// <returns>An <see cref="RecurrencePattern"/> object or <see langword="null"/> for invalid input.</returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public override object? Deserialize(TextReader tr)
    {
        var value = tr.ReadToEnd();

        // Instantiate the data type
        var r = CreateAndAssociate() as RecurrencePattern;
        var factory = GetService<ISerializerFactory>();

        System.Diagnostics.Debug.Assert(r != null);
        System.Diagnostics.Debug.Assert(factory != null);

        // Decode the value, if necessary
        value = Decode(r, value);

        DeserializePattern(value, r, factory);
        return r;
    }

    /// <summary>
    /// Deserializes the recurrence rule.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Always throws on failure.</exception>
    private void DeserializePattern(string value, RecurrencePattern r, ISerializerFactory factory)
    {
        var freqPartExists = false;
        var keywordPairs = value.Split(';');
        foreach (var keywordPair in keywordPairs)
        {
            if (keywordPair.Length == 0)
            {
                // ignore subsequent semi-colons
                continue;
            }

            var keyValues = keywordPair.Split('=');
            if (keyValues.Length != 2)
            {
                throw new ArgumentOutOfRangeException(nameof(value),
                    $"The recurrence rule part '{keywordPair}' is invalid.");
            }

            if (keyValues[0].Equals("FREQ", StringComparison.OrdinalIgnoreCase))
            {
                freqPartExists = true;
            }

            ProcessKeyValuePair(keyValues[0].ToLower(), keyValues[1], r, factory);
        }

        if (!freqPartExists || r.Frequency == FrequencyType.None)
        {
            throw new ArgumentOutOfRangeException(nameof(value),
                "The recurrence rule must specify a FREQ part that is not NONE.");
        }
        CheckMutuallyExclusive("COUNT", "UNTIL", r.Count, r.Until);
        CheckRanges(r);
    }

    private void ProcessKeyValuePair(string key, string value, RecurrencePattern r, ISerializerFactory factory)
    {
        switch (key)
        {
            case "freq":
                r.Frequency = (FrequencyType) Enum.Parse(typeof(FrequencyType), value, true);
                break;

            case "until":
                var serializer = factory.Build(typeof(CalDateTime), SerializationContext) as IStringSerializer;
                r.Until = serializer?.Deserialize(new StringReader(value)) as CalDateTime;
                break;

            case "count":
                r.Count = Convert.ToInt32(value);
                break;

            case "interval":
                r.Interval = Convert.ToInt32(value);
                break;

            case "bysecond":
                AddInt32Values(r.BySecond, value);
                break;

            case "byminute":
                AddInt32Values(r.ByMinute, value);
                break;

            case "byhour":
                AddInt32Values(r.ByHour, value);
                break;

            case "byday":
                AddWeekDays(r.ByDay, value);
                break;

            case "bymonthday":
                AddInt32Values(r.ByMonthDay, value);
                break;

            case "byyearday":
                AddInt32Values(r.ByYearDay, value);
                break;

            case "byweekno":
                AddInt32Values(r.ByWeekNo, value);
                break;

            case "bymonth":
                AddInt32Values(r.ByMonth, value);
                break;

            case "bysetpos":
                AddInt32Values(r.BySetPosition, value);
                break;

            case "wkst":
                r.FirstDayOfWeek = GetDayOfWeek(value);
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(key),
                    $"The recurrence rule part '{key}' is not supported.");
        }
    }

    private static void AddWeekDays(IList<WeekDay> byDay, string keyValue)
    {
        var days = keyValue.Split(',');
        foreach (var day in days)
        {
            byDay.Add(new WeekDay(day));
        }
    }

    private void CheckRanges(RecurrencePattern r)
    {
        CheckRange("INTERVAL", r.Interval, 1, int.MaxValue);
        CheckRange("COUNT", r.Count, 1, int.MaxValue);
        CheckRange("BYSECOND", r.BySecond, 0, 59);
        CheckRange("BYMINUTE", r.ByMinute, 0, 59);
        CheckRange("BYHOUR", r.ByHour, 0, 23);
        CheckRange("BYMONTHDAY", r.ByMonthDay, -31, 31);
        CheckRange("BYYEARDAY", r.ByYearDay, -366, 366);
        CheckRange("BYWEEKNO", r.ByWeekNo, -53, 53);
        CheckRange("BYMONTH", r.ByMonth, 1, 12);
        CheckRange("BYSETPOS", r.BySetPosition, -366, 366);
    }
}
