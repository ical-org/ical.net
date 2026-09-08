//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Ical.Net.DataTypes;

public partial class RecurrenceRule
{
    public override string ToString() => ToRecurString();

    private string ToRecurString()
    {
        var values = new List<string>
        {
            $"FREQ={Frequency.ToString().ToUpperInvariant()}"
        };

        //-- FROM RFC2445 --
        //The INTERVAL rule part contains a positive integer representing how
        //often the recurrence rule repeats. The default value is "1", meaning
        //every second for a SECONDLY rule, or every minute for a MINUTELY
        //rule, every hour for an HOURLY rule, every day for a DAILY rule,
        //every week for a WEEKLY rule, every month for a MONTHLY rule and
        //every year for a YEARLY rule.
        var interval = Interval;
        if (interval != 1)
        {
            values.Add($"INTERVAL={interval}");
        }

        if (Until is not null)
        {
            values.Add($"UNTIL={Until.ToBasicIso()}");
        }

        if (FirstDayOfWeek != DayOfWeek.Monday)
        {
            if (!WeekDay.TryFormat(FirstDayOfWeek, out var dayOfWeek))
            {
                throw new FormatException("FirstDayOfWeek is an invalid value");
            }

            values.Add($"WKST={dayOfWeek}");
        }

        if (Count.HasValue)
        {
            values.Add($"COUNT={Count}");
        }

        if (ByDay.Count > 0)
        {
            var bydayValues = new List<string>(ByDay.Count);

            bydayValues.AddRange(ByDay.Select(static x => x.ToString()));

            values.Add($"BYDAY={string.Join(",", bydayValues)}");
        }

        SerializeByValue(values, ByHour, "BYHOUR");
        SerializeByValue(values, ByMinute, "BYMINUTE");
        SerializeByValue(values, ByMonth, "BYMONTH");
        SerializeByValue(values, ByMonthDay, "BYMONTHDAY");
        SerializeByValue(values, BySecond, "BYSECOND");
        SerializeByValue(values, BySetPosition, "BYSETPOS");
        SerializeByValue(values, ByWeekNo, "BYWEEKNO");
        SerializeByValue(values, ByYearDay, "BYYEARDAY");

        return string.Join(";", values);
    }

    private static void SerializeByValue(List<string> aggregate, IList<int> byValue, string name)
    {
        if (byValue.Any())
        {
            aggregate.Add($"{name}={string.Join(",", byValue.Select(i => i.ToString(CultureInfo.InvariantCulture)))}");
        }
    }

    /// <summary>
    /// Parses an RRULE formatted string (e.g. "FREQ=WEEKLY;COUNT=10").
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
    /// <param name="value">The value to parse</param>
    /// <exception cref="FormatException">The RRULE format is invalid.</exception>
    /// <exception cref="ArgumentOutOfRangeException">A rule part value is outside of the valid range.</exception>
    public static RecurrenceRule Parse(string value)
    {
        var r = new RecurrenceRule();

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
                throw new FormatException($"The recurrence rule part '{keywordPair}' is invalid.");
            }

            if (keyValues[0].Equals("FREQ", StringComparison.OrdinalIgnoreCase))
            {
                freqPartExists = true;
            }

            ProcessKeyValuePair(keyValues[0].ToLowerInvariant(), keyValues[1], r);
        }

        if (!freqPartExists)
        {
            throw new FormatException("The recurrence rule must specify a valid FREQ part.");
        }

        CheckMutuallyExclusive("COUNT", "UNTIL", r.Count, r.Until);
        CheckRanges(r);

        return r;
    }


    private static void AddInt32Values(IList<int> list, string value)
    {
        var values = value.Split(',');
        foreach (var v in values)
        {
            list.Add(Convert.ToInt32(v, CultureInfo.InvariantCulture));
        }
    }

    private static void CheckRange(string name, IList<int> values, int min, int max)
    {
        var allowZero = (min == 0 || max == 0);
        foreach (var value in values)
        {
            CheckRange(name, value, min, max, allowZero);
        }
    }

    private static void CheckRange(string name, int value, int min, int max)
    {
        var allowZero = min == 0 || max == 0;
        CheckRange(name, value, min, max, allowZero);
    }

    private static void CheckRange(string name, int? value, int min, int max)
    {
        var allowZero = min == 0 || max == 0;
        CheckRange(name, value, min, max, allowZero);
    }

    private static void CheckRange(string name, int value, int min, int max, bool allowZero)
    {
        if ((value < min || value > max || (!allowZero && value == 0)))
        {
            throw new ArgumentOutOfRangeException(nameof(name),
                $"{name} value {value} is out of range. Valid values are between {min} and {max}{(allowZero ? "" : ", excluding zero (0)")}.");
        }
    }

    private static void CheckRange(string name, int? value, int min, int max, bool allowZero)
    {
        if (value != null && (value < min || value > max || (!allowZero && value == 0)))
        {
            throw new ArgumentOutOfRangeException(nameof(name),
                $"{name} value {value} is out of range. Valid values are between {min} and {max}{(allowZero ? "" : ", excluding zero (0)")}.");
        }
    }

    private static void CheckMutuallyExclusive(string name1, string name2, int? obj1, CalDateTime? obj2)
    {
        if ((obj1 == null) || (obj2 == null))
        {
            return;
        }

        throw new ArgumentOutOfRangeException(nameof(name1),
            $"Both {name1} and {name2} cannot be supplied together; they are mutually exclusive.");
    }

    private static void AddWeekDays(IList<WeekDay> byDay, string keyValue)
    {
        var days = keyValue.Split(',');
        foreach (var day in days)
        {
            if (!WeekDay.TryParse(day, out var weekDay))
            {
                throw new FormatException($"BYDAY value \"{day}\" is invalid");
            }

            byDay.Add(weekDay);
        }
    }

    private static void CheckRanges(RecurrenceRule r)
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

    private static void ProcessKeyValuePair(string key, string value, RecurrenceRule r)
    {
        switch (key)
        {
            case "freq" when Enum.TryParse(value, true, out FrequencyType freq):
                r.Frequency = freq;
                break;

            case "until":
                r.Until = CalDateTime.Parse(value, tzId: null);
                break;

            case "count":
                r.Count = Convert.ToInt32(value, CultureInfo.InvariantCulture);
                break;

            case "interval":
                r.Interval = Convert.ToInt32(value, CultureInfo.InvariantCulture);
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
                if (!WeekDay.TryGetDayOfWeek(value, out var dayOfWeek))
                {
                    throw new ArgumentOutOfRangeException($"WKST value \"{value}\" is not valid");
                }

                r.FirstDayOfWeek = dayOfWeek;
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(key),
                    $"The recurrence rule part '{key}' or its value {value} is not supported.");
        }
    }
}
