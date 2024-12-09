﻿//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ical.Net.Evaluation;
using Ical.Net.Serialization.DataTypes;
using Ical.Net.Utility;

namespace Ical.Net.DataTypes;

/// <summary>
/// An iCalendar representation of the <c>RRULE</c> property.
/// https://tools.ietf.org/html/rfc5545#section-3.3.10
/// </summary>
public class RecurrencePattern : EncodableDataType
{
    private int? _interval;
#pragma warning disable 0618
    private RecurrenceRestrictionType? _restrictionType;
    private RecurrenceEvaluationModeType? _evaluationMode;
#pragma warning restore 0618
    public FrequencyType Frequency { get; set; }

    public DateTime? Until { get; set; }

    public int? Count { get; set; }

    /// <summary>
    /// Specifies how often the recurrence should repeat.
    /// - 1 = every
    /// - 2 = every second
    /// - 3 = every third
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

    public List<int> BySetPosition { get; set; } = new List<int>();

    public DayOfWeek FirstDayOfWeek { get; set; } = DayOfWeek.Monday;

#pragma warning disable 0618
    /// <summary>
    /// The type of restriction to apply to the evaluation of this recurrence pattern.
    /// Returns <see cref="RecurrenceRestrictionType.NoRestriction"/> if not set.
    /// </summary>
    [Obsolete("Usage may cause undesired results or exceptions. Will be removed.", false)]
    public RecurrenceRestrictionType RestrictionType
    {
        get
        {
            // NOTE: Fixes bug #1924358 - Cannot evaluate Secondly patterns
            if (_restrictionType != null)
            {
                return _restrictionType.Value;
            }
            return Calendar?.RecurrenceRestriction ?? RecurrenceRestrictionType.Default;
        }
        set => _restrictionType = value;
    }

    [Obsolete("Usage may cause undesired results or exceptions. Will be removed.", false)]
    public RecurrenceEvaluationModeType EvaluationMode
    {
        get
        {
            // NOTE: Fixes bug #1924358 - Cannot evaluate Secondly patterns
            if (_evaluationMode != null)
            {
                return _evaluationMode.Value;
            }
            return Calendar?.RecurrenceEvaluationMode ?? RecurrenceEvaluationModeType.Default;
        }
        set => _evaluationMode = value;
    }
#pragma warning restore 0618

    public RecurrencePattern()
    {
        SetService(new RecurrencePatternEvaluator(this));
    }

    public RecurrencePattern(FrequencyType frequency) : this(frequency, 1) { }

    public RecurrencePattern(FrequencyType frequency, int interval) : this()
    {
        Frequency = frequency;
        Interval = interval;
    }

    public RecurrencePattern(string value) : this()
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }
        var serializer = new RecurrencePatternSerializer();
        if (serializer.Deserialize(new StringReader(value)) is ICopyable deserialized)
            CopyFrom(deserialized);
    }

    public override string ToString()
    {
        var serializer = new RecurrencePatternSerializer();
        return serializer.SerializeToString(this);
    }

    protected bool Equals(RecurrencePattern other) => (Interval == other.Interval)
#pragma warning disable 0618
                                                      && RestrictionType == other.RestrictionType
                                                      && EvaluationMode == other.EvaluationMode
#pragma warning restore 0618
                                                      && Frequency == other.Frequency
                                                      && Until.Equals(other.Until)
                                                      && Count == other.Count
                                                      && (FirstDayOfWeek == other.FirstDayOfWeek)
                                                      && CollectionEquals(BySecond, other.BySecond)
                                                      && CollectionEquals(ByMinute, other.ByMinute)
                                                      && CollectionEquals(ByHour, other.ByHour)
                                                      && CollectionEquals(ByDay, other.ByDay)
                                                      && CollectionEquals(ByMonthDay, other.ByMonthDay)
                                                      && CollectionEquals(ByYearDay, other.ByYearDay)
                                                      && CollectionEquals(ByWeekNo, other.ByWeekNo)
                                                      && CollectionEquals(ByMonth, other.ByMonth)
                                                      && CollectionEquals(BySetPosition, other.BySetPosition);

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        return obj.GetType() == GetType() && Equals((RecurrencePattern) obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = Interval.GetHashCode();
#pragma warning disable 0618
            hashCode = (hashCode * 397) ^ RestrictionType.GetHashCode();
            hashCode = (hashCode * 397) ^ EvaluationMode.GetHashCode();
#pragma warning restore 0618
            hashCode = (hashCode * 397) ^ (int) Frequency;
            hashCode = (hashCode * 397) ^ Until.GetHashCode();
            hashCode = (hashCode * 397) ^ (Count ?? 0);
            hashCode = (hashCode * 397) ^ (int) FirstDayOfWeek;
            hashCode = (hashCode * 397) ^ CollectionHelpers.GetHashCode(BySecond);
            hashCode = (hashCode * 397) ^ CollectionHelpers.GetHashCode(ByMinute);
            hashCode = (hashCode * 397) ^ CollectionHelpers.GetHashCode(ByHour);
            hashCode = (hashCode * 397) ^ CollectionHelpers.GetHashCode(ByDay);
            hashCode = (hashCode * 397) ^ CollectionHelpers.GetHashCode(ByMonthDay);
            hashCode = (hashCode * 397) ^ CollectionHelpers.GetHashCode(ByYearDay);
            hashCode = (hashCode * 397) ^ CollectionHelpers.GetHashCode(ByWeekNo);
            hashCode = (hashCode * 397) ^ CollectionHelpers.GetHashCode(ByMonth);
            hashCode = (hashCode * 397) ^ CollectionHelpers.GetHashCode(BySetPosition);
            return hashCode;
        }
    }

    /// <inheritdoc/>
    public override void CopyFrom(ICopyable obj)
    {
        base.CopyFrom(obj);
        if (obj is not RecurrencePattern r)
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
#pragma warning disable 0618
        RestrictionType = r.RestrictionType;
        EvaluationMode = r.EvaluationMode;
#pragma warning restore 0618
    }

    private static bool CollectionEquals<T>(IEnumerable<T> c1, IEnumerable<T> c2) => c1.SequenceEqual(c2);
}
