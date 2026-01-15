//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

namespace Ical.Net.DataTypes;

/// <summary>
/// An iCalendar representation of the <c>RRULE</c> property.
/// https://tools.ietf.org/html/rfc5545#section-3.3.10
/// </summary>
public class RecurrencePattern : RecurrenceRule
{
    public RecurrencePattern()
    {
    }

    public RecurrencePattern(FrequencyType frequency) : base(frequency)
    {
    }

    public RecurrencePattern(string value) : base(value)
    {
    }

    public RecurrencePattern(FrequencyType frequency, int interval) : base(frequency, interval)
    {
    }

    /// <summary>
    /// Shallow copy properties to wrap RecurrenceRule.
    /// </summary>
    internal RecurrencePattern(RecurrenceRule r)
    {
        CopyDataType(r);
        Frequency = r.Frequency;
        Until = r.Until;
        Count = r.Count;
        Interval = r.Interval;
        BySecond = r.BySecond;
        ByMinute = r.ByMinute;
        ByHour = r.ByHour;
        ByDay = r.ByDay;
        ByMonthDay = r.ByMonthDay;
        ByYearDay = r.ByYearDay;
        ByWeekNo = r.ByWeekNo;
        ByMonth = r.ByMonth;
        BySetPosition = r.BySetPosition;
        FirstDayOfWeek = r.FirstDayOfWeek;
    }

    internal bool HasByRules()
    {
        return ByDay.Count > 0
            || ByMonth.Count > 0
            || ByMonthDay.Count > 0
            || ByWeekNo.Count > 0
            || ByYearDay.Count > 0
            || ByHour.Count > 0
            || ByMinute.Count > 0
            || BySecond.Count > 0
            || BySetPosition.Count > 0;
    }
}
