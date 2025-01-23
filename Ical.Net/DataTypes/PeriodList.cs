//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Ical.Net.Evaluation;
using Ical.Net.Serialization.DataTypes;
using Ical.Net.Utility;

namespace Ical.Net.DataTypes;

/// <summary>
/// An iCalendar list used to represent a list of <see cref="Period"/> objects
/// for EXDATE and RDATE properties.
/// </summary>
internal class PeriodList : EncodableDataType, IList<Period>
{
    internal PeriodKind PeriodKind => Count == 0 ? PeriodKind.Undefined : Periods[0].PeriodKind;

    internal string? TzId => Count == 0 ? null : Periods[0].TzId;

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

    protected bool Equals(PeriodList other) => CollectionHelpers.Equals(Periods, other.Periods);

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        return obj.GetType() == GetType() && Equals((PeriodList) obj);
    }

    /// <inheritdoc/>
    public override int GetHashCode() => CollectionHelpers.GetHashCode(Periods);

    /// <inheritdoc/>
    public Period this[int index]
    {
        get => Periods[index];
        set
        {
            EnsureConsistentTimezoneAndPeriodKind(value);
            Periods[index] = value;
        }
    }

    /// <inheritdoc/>
    public bool Remove(Period item) => Periods.Remove(item);

    /// <inheritdoc/>
    public bool IsReadOnly => Periods.IsReadOnly;

    /// <inheritdoc/>
    public int IndexOf(Period item) => Periods.IndexOf(item);

    /// <summary>
    /// Inserts a <see cref="Period"/> to the list if it does not already exist.<br/>
    /// The timezone period kind of the first value added determines the timezone for the whole list.
    /// </summary>
    /// <exception cref="ArgumentException"></exception>
    public void Insert(int index, Period item)
    {
        EnsureConsistentTimezoneAndPeriodKind(item);
        if (Periods.Contains(item)) return;
        Periods.Insert(index, item);
    }

    /// <inheritdoc/>
    public void RemoveAt(int index) => Periods.RemoveAt(index);

    /// <summary>
    /// Adds a <see cref="Period"/> to the list if it does not already exist.<br/>
    /// The timezone period kind of the first value added determines the timezone for the whole list.
    /// </summary>
    /// <param name="item">The <see cref="Period"/> for an 'RDATE'.</param>
    /// <exception cref="ArgumentException"></exception>
    public void Add(Period item)
    {
        EnsureConsistentTimezoneAndPeriodKind(item);
        if (Periods.Contains(item)) return;
        Periods.Add(item);
    }

    /// <summary>
    /// Adds a DATE or DATE-TIME value for an 'EXDATE' or 'RDATE' to the list if it does not already exist.<br/>
    /// The timezone period kind of the first value added determines the timezone for the whole list.
    /// </summary>
    /// <param name="dt"></param>
    /// <exception cref="ArgumentException"></exception>
    public void Add(CalDateTime dt)
    {
        var p = new Period(dt);
        Add(p);
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
        if (Count != 0 && p.PeriodKind != Periods[0].PeriodKind)
        {
            throw new ArgumentException(
                $"All Periods of a PeriodList must be of the same period kind. Current Kind: {Periods[0].PeriodKind}, Provided Kind: {p.PeriodKind}");
        }

        if (Count != 0 && p.TzId != Periods[0].TzId)
        {
            throw new ArgumentException(
                $"All Periods of a PeriodList must have the same timezone. Current TzId: {Periods[0].TzId}, Provided TzId: {p.TzId}");
        }
    }
}
