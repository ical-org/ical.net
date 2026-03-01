//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ical.Net.Serialization.DataTypes;
using Ical.Net.Utility;

namespace Ical.Net.DataTypes;

/// <summary>
/// An iCalendar representation of the <c>RRULE</c> property.
/// https://tools.ietf.org/html/rfc5545#section-3.3.10
/// </summary>
public class RecurrenceRule : EncodableDataType
{
    private int? _interval;
    private FrequencyType _frequency;
    private CalDateTime? _until;

    /// <summary>
    /// Specifies the frequency <i>FREQ</i> of the recurrence.
    /// The default value is <see cref="FrequencyType.Yearly"/>.
    /// </summary>
    public FrequencyType Frequency
    {
        get => _frequency;
        set
        {
            if (!Enum.IsDefined(typeof(FrequencyType), value))
            {
                throw new ArgumentOutOfRangeException(nameof(Frequency), $"Invalid FrequencyType '{value}'.");
            }
            _frequency = value;
        }
    }

    /// <summary>
    /// Specifies the end date of the recurrence (optional).
    /// This property <b>must be null</b> if the <see cref="Count"/> property is set.
    /// </summary>
    public CalDateTime? Until
    {
        get => _until;
        set
        {
            if (value != null && value.TzId != null && value.TzId != CalDateTime.UtcTzId)
                throw new ArgumentOutOfRangeException(nameof(Until),
                    $"{nameof(Until)} must be either NULL, or its TzId must be UTC or NULL");

            _until = value;
        }
    }

    /// <summary>
    /// Specifies the number of occurrences of the recurrence (optional).
    /// This property <b>must be null</b> if the <see cref="Until"/> property is set.
    /// </summary>
    public int? Count { get; set; }


    /// <summary>
    /// The INTERVAL rule part contains a positive integer representing at
    /// which intervals the recurrence rule repeats. The default value is
    /// 1, meaning every second for a SECONDLY rule, every minute for a
    /// MINUTELY rule, every hour for an HOURLY rule, every day for a
    /// DAILY rule, every week for a WEEKLY rule, every month for a
    /// MONTHLY rule, and every year for a YEARLY rule. For example,
    /// within a DAILY rule, a value of 8 means every eight days.
    /// </summary>
    public int Interval
    {
        get => _interval ?? 1;
        set => _interval = value;
    }

    public List<int> BySecond { get; set; } = new List<int>();

    /// <summary> The ordinal minutes of the hour associated with this recurrence pattern. Valid values are 0-59. </summary>
    public List<int> ByMinute { get; set; } = new List<int>();

    public List<int> ByHour { get; set; } = new List<int>();

    public List<WeekDay> ByDay { get; set; } = new List<WeekDay>();

    /// <summary> The ordinal days of the month associated with this recurrence pattern. Valid values are 1-31. </summary>
    public List<int> ByMonthDay { get; set; } = new List<int>();

    /// <summary>
    /// The ordinal days of the year associated with this recurrence pattern. Something recurring on the first day of the year would be a list containing
    /// 1, and would also be New Year's Day.
    /// </summary>
    public List<int> ByYearDay { get; set; } = new List<int>();

    /// <summary>
    /// The ordinal week of the year. Valid values are -53 to +53. Negative values count backwards from the end of the specified year.
    /// A week is defined by ISO.8601.2004
    /// </summary>
    public List<int> ByWeekNo { get; set; } = new List<int>();

    /// <summary>
    /// List of months in the year associated with this rule. Valid values are 1 through 12.
    /// </summary>
    public List<int> ByMonth { get; set; } = new List<int>();

    /// <summary>
    /// Specify the n-th occurrence within the set of occurrences specified by the RRULE.
    /// It is typically used in conjunction with other rule parts like BYDAY, BYMONTHDAY, etc.
    /// </summary>
    public List<int> BySetPosition { get; set; } = new List<int>();

    public DayOfWeek FirstDayOfWeek { get; set; } = DayOfWeek.Monday;

    /// <summary>
    /// Default constructor. Sets the <see cref="Frequency"/> to <see cref="FrequencyType.Yearly"/>
    /// and <see cref="Interval"/> to 1.
    /// </summary>
    public RecurrenceRule()
    {
        Frequency = FrequencyType.Yearly;
        Interval = 1;
    }

    public RecurrenceRule(FrequencyType frequency) : this(frequency, 1) { }

    public RecurrenceRule(FrequencyType frequency, int interval) : this()
    {
        Frequency = frequency; // for proper validation don't use the backing field
        Interval = interval;
    }

    public RecurrenceRule(string value) : this()
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }
        var serializer = new RecurrenceRuleSerializer();
        if (serializer.Deserialize(new StringReader(value)) is ICopyable deserialized)
            CopyFrom(deserialized);
    }

    public override string? ToString()
    {
        var serializer = new RecurrenceRuleSerializer();
        return serializer.SerializeToString(this);
    }

    /// <inheritdoc/>
    public override void CopyFrom(ICopyable obj)
    {
        base.CopyFrom(obj);
        if (obj is not RecurrenceRule r)
        {
            return;
        }

        Frequency = r.Frequency;
        Until = r.Until;
        Count = r.Count;
        Interval = r.Interval;
        BySecond = new List<int>(r.BySecond);
        ByMinute = new List<int>(r.ByMinute);
        ByHour = new List<int>(r.ByHour);
        ByDay = new List<WeekDay>(r.ByDay);
        ByMonthDay = new List<int>(r.ByMonthDay);
        ByYearDay = new List<int>(r.ByYearDay);
        ByWeekNo = new List<int>(r.ByWeekNo);
        ByMonth = new List<int>(r.ByMonth);
        BySetPosition = new List<int>(r.BySetPosition);
        FirstDayOfWeek = r.FirstDayOfWeek;
    }
}
