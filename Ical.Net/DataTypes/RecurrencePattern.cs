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
}
