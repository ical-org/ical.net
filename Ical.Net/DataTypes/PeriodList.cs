//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ical.Net.Evaluation;
using Ical.Net.Serialization.DataTypes;
using Ical.Net.Utility;

namespace Ical.Net.DataTypes;

/// <summary>
/// An iCalendar list used to represent a list of <see cref="Period"/> objects
/// for EXDATE and RDATE properties.
/// </summary>
public class PeriodList : EncodableDataType, IList<Period>
{
    /// <summary>
    /// Gets the timezone ID of the <see cref="PeriodList"/>.<br/>
    /// The timezone of the first item added determines the timezone for the list.
    /// </summary>
    internal string? TzId { get; private set; }

    /// <summary>
    /// Gets the kind that this <see cref="PeriodList"/> is representing.<br/>
    /// Only <see cref="Period"/>s with the same <see cref="PeriodKind"/> can be added to the list.
    /// </summary>
    internal PeriodKind PeriodListKind { get; private set; }

    /// <summary>
    /// Gets the number of <see cref="Period"/>s of the list.
    /// </summary>
    public int Count => Periods.Count;

    /// <summary>
    /// Gets the list of <see cref="Period"/>s of the list.
    /// </summary>
    protected IList<Period> Periods { get; } = new List<Period>();

    // Also needed for the serialization factory
    public PeriodList()
    {
        SetService(new PeriodListEvaluator(this));
        TzId = null;
    }

    /// <summary>
    /// Creates a new instance of the <see cref="PeriodList"/> class from the <see cref="StringReader"/>.
    /// </summary>
    /// <param name="value"></param>
    /// <exception cref="ArgumentException"></exception>
    private PeriodList(StringReader value)
    {
        var serializer = new PeriodListSerializer();
        if (serializer.Deserialize(value) is ICopyable deserialized)
        {
            CopyFrom(deserialized);
        }
        SetService(new PeriodListEvaluator(this));
    }

    /// <summary>
    /// Creates a new instance of the <see cref="PeriodList"/> class from the <see cref="StringReader"/> object.
    /// </summary>
    /// <param name="value"></param>
    /// <exception cref="ArgumentException"></exception>
    public static PeriodList FromStringReader(StringReader value) => new PeriodList(value);

    /// <summary>
    /// Creates a new instance of a <see cref="PeriodList"/> class from an <see cref="IDateTime"/> object,
    /// using the timezone from the <see cref="IDateTime"/>.
    /// </summary>
    /// <param name="value"></param>
    /// <returns>A new instance of the <see cref="PeriodList"/>.</returns>
    /// <exception cref="ArgumentException"></exception>
    public static PeriodList FromDateTime(IDateTime value)
    {
        var pl = new PeriodList().Add(value);
        return pl;
    }

    /// <inheritdoc/>
    public override void CopyFrom(ICopyable obj)
    {
        base.CopyFrom(obj);
        if (obj is not PeriodList list)
        {
            return;
        }

        foreach (var p in list)
        {
            Add(p.Copy<Period>());
        }
    }

    /// <summary>
    /// Gets the string representation of the list.
    /// </summary>
    /// <returns></returns>
    public override string? ToString() => new PeriodListSerializer().SerializeToString(this);
    
    /// <summary>
    /// Used for equality comparison of two lists of periods.
    /// </summary>
    public static Dictionary<string, IList<Period>> GetGroupedPeriods(IList<PeriodList> periodLists)
    {
        // In order to know if two events are equal, a semantic understanding of exdates, rdates, rrules, and exrules is required. This could be done by
        // computing the complete recurrence set (expensive) while being time-zone sensitive, or by comparing each List<Period> in each IPeriodList.

        // For example, events containing these rules generate the same recurrence set, including having the same time zone for each occurrence, so
        // they're the same:
        // Event A:
        // RDATE:20170302T060000Z,20170303T060000Z
        // Event B:
        // RDATE:20170302T060000Z
        // RDATE:20170303T060000Z

        var grouped = new Dictionary<string, HashSet<Period>>(StringComparer.OrdinalIgnoreCase);
        foreach (var periodList in periodLists)
        {
            // Dictionary key cannot be null, so an empty string is used for the default bucket
            var defaultBucket = string.IsNullOrWhiteSpace(periodList.TzId) ? string.Empty : periodList.TzId;
            foreach (var period in periodList)
            {
                var bucketTzId = period.StartTime.TzId ?? defaultBucket;

                if (!grouped.TryGetValue(bucketTzId, out var periods))
                {
                    periods = new HashSet<Period>();
                    grouped.Add(bucketTzId, periods);
                }

                periods.Add(period);
            }
        }

        return grouped.ToDictionary(k => k.Key, v => (IList<Period>) v.Value.OrderBy(d => d.StartTime).ToList());
    }

    protected bool Equals(PeriodList other) => string.Equals(TzId, other.TzId, StringComparison.OrdinalIgnoreCase)
                                               && CollectionHelpers.Equals(Periods, other.Periods);

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        return obj.GetType() == GetType() && Equals((PeriodList) obj);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = TzId?.GetHashCode() ?? 0;
            hashCode = (hashCode * 397) ^ CollectionHelpers.GetHashCode(Periods);
            return hashCode;
        }
    }

    /// <inheritdoc/>
    public Period this[int index]
    {
        get => Periods[index];
        set => Periods[index] = value;
    }

    /// <inheritdoc/>
    public bool Remove(Period item) => Periods.Remove(item);

    /// <inheritdoc/>
    public bool IsReadOnly => Periods.IsReadOnly;

    /// <inheritdoc/>
    public int IndexOf(Period item) => Periods.IndexOf(item);

    /// <inheritdoc/>
    public void Insert(int index, Period item) => Periods.Insert(index, item);

    /// <inheritdoc/>
    public void RemoveAt(int index) => Periods.RemoveAt(index);

    /// <summary>
    /// Adds a <see cref="Period"/> for an 'RDATE' to the list.<br/>
    /// The timezone of the first value added determines the timezone for the list.
    /// <para/>
    /// To add an 'EXDATE', use the <see cref="Add(IDateTime)"/> method instead,
    /// because <see cref="Period"/>s are not permitted for 'EXDATE' according to
    /// RFC 5545 section 3.8.5.1.
    /// </summary>
    /// <param name="item">The <see cref="Period"/> for an 'RDATE'.</param>
    /// <exception cref="ArgumentException"></exception>
    public void Add(Period item)
    {
        EnsureConsistentTimezoneAndPeriodKind(item);
        Periods.Add(item);
    }

    /// <summary>
    /// Adds a DATE or DATE-TIME value for an 'EXDATE' or 'RDATE' to the list.<br/>
    /// The timezone of the first value added determines the timezone for the list.
    /// <para/>
    /// To add an 'RDATE' <see cref="Period"/>, use the <see cref="Add(Period)"/> method instead.
    /// </summary>
    /// <param name="dt"></param>
    /// <returns>This instance of the <see cref="PeriodList"/>.</returns>
    /// <exception cref="ArgumentException"></exception>
    public PeriodList Add(IDateTime dt)
    {
        var p = new Period(dt);
        EnsureConsistentTimezoneAndPeriodKind(p);
        Periods.Add(p);
        return this;
    }

    /// <summary>
    /// Adds a <see cref="Period"/> for an 'RDATE' to the list.<br/>
    /// The timezone of the first value added determines the timezone for the list.
    /// </summary>
    /// <param name="item"></param>
    /// <returns>This instance of the <see cref="PeriodList"/>.</returns>
    /// <exception cref="ArgumentException"></exception>
    public PeriodList AddPeriod(Period item)
    {
        EnsureConsistentTimezoneAndPeriodKind(item);
        Add(item);
        return this;
    }

    /// <inheritdoc/>
    public void Clear() => Periods.Clear();

    /// <inheritdoc/>
    public bool Contains(Period item) => Periods.Contains(item);

    /// <inheritdoc/>
    public void CopyTo(Period[] array, int arrayIndex) => Periods.CopyTo(array, arrayIndex);

    /// <inheritdoc/>
    public IEnumerator<Period> GetEnumerator() => Periods.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => Periods.GetEnumerator();
    
    private void EnsureConsistentTimezoneAndPeriodKind(Period p)
    {
        if (Count != 0 && p.GetPeriodKind() != PeriodListKind)
        {
            throw new ArgumentException($"All Periods of a PeriodList must have the same value type. Current ValueType: {PeriodListKind}, Provided ValueType: {p.GetPeriodKind()}");
        }

        if (Count != 0 && p.TzId != TzId)
        {
            throw new ArgumentException($"All Periods of a PeriodList must have the same timezone. Current TzId: {TzId}, Provided TzId: {p.TzId}");
        }

        if (Count == 0)
        {
            TzId = p.StartTime.TzId;
            PeriodListKind = p.GetPeriodKind();
        }
    }
}
