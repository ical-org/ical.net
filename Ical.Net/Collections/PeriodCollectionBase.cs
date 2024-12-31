//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

#nullable enable
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Ical.Net.DataTypes;

namespace Ical.Net.Collections;

/// <summary>
/// Represents a base class for collections of periods.
/// </summary>
public abstract class PeriodCollectionBase : ICollection<Period>
{
    /// <summary>
    /// The list of periods in the collection.
    /// </summary>
    protected readonly List<Period> Periods = new List<Period>();

    /// <summary>
    /// Gets or sets the <see cref="Period"/> at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the <see cref="Period"/> to get or set.</param>
    /// <returns>The <see cref="Period"/> at the specified index.</returns>
    public Period this[int index]
    {
        get => Periods[index];
        set => Periods[index] = value;
    }

    /// <inheritdoc cref="ICollection{T}.Count"/>
    public int Count => Periods.Count;

    /// <inheritdoc cref="ICollection{T}.IsReadOnly"/>
    public bool IsReadOnly => false;

    /// <inheritdoc cref="ICollection{T}.Add"/>
    public virtual void Add(Period item) => Periods.Add(item);

    /// <summary>
    /// Adds a <see cref="CalDateTime"/> to the collection as a <see cref="Period"/>.
    /// </summary>
    /// <param name="dt">The <see cref="CalDateTime"/> to add.</param>
    /// <returns>The current instance of <see cref="PeriodCollectionBase"/>.</returns>
    public PeriodCollectionBase Add(CalDateTime dt)
    {
        Periods.Add(new Period(dt));
        return this;
    }

    /// <summary>
    /// Adds a range of <see cref="CalDateTime"/> objects to the collection as <see cref="Period"/>s.
    /// </summary>
    /// <param name="dtList">The collection of <see cref="CalDateTime"/> objects to add.</param>
    /// <returns>The current instance of <see cref="PeriodCollectionBase"/>.</returns>
    public PeriodCollectionBase AddRange(IEnumerable<CalDateTime> dtList)
    {
        foreach (var date in dtList)
        {
            Periods.Add(new Period(date));
        }
        return this;
    }

    /// <summary>
    /// Adds a range of <see cref="PeriodList"/> objects to the collection.
    /// </summary>
    /// <param name="pList">The list of <see cref="PeriodList"/> objects to add.</param>
    internal void AddRange(IList<PeriodList> pList)
    {
        foreach (var periodList in pList)
        {
            foreach (var period in periodList)
            {
                Periods.Add(period);
            }
        }
    }

    /// <summary>
    /// Clears all periods from the collection.
    /// </summary>
    public void Clear() => Periods.Clear();

    /// <inheritdoc cref="ICollection{T}.Contains"/>
    public bool Contains(Period item) => Periods.Contains(item);

    /// <summary>
    /// Removes the first occurrence of a specific period from the collection.
    /// </summary>
    /// <param name="item"></param>
    public bool Remove(Period item) => Periods.Remove(item);

    /// <inheritdoc cref="ICollection{T}.CopyTo"/>
    public void CopyTo(Period[] array, int arrayIndex) => Periods.CopyTo(array, arrayIndex);

    /// <summary>
    /// Aggregates and converts the periods to a list of <see cref="PeriodList"/> objects.
    /// </summary>
    /// <returns>The periods as a list of <see cref="PeriodList"/> objects.</returns>
    protected List<PeriodList> ToListOfPeriodList()
    {
        var periodList = new List<PeriodList>();
        foreach (var tzId in GetDistinctTzIds())
        {
            foreach (var pKind in GetDistinctPeriodKindsByTzId(tzId))
            {
                var distinctPeriodList = new PeriodList();
                foreach (var period in GetPeriodsByTzIdAndKind(tzId, pKind))
                {
                    if (!distinctPeriodList.Contains(period))
                        distinctPeriodList.Add(period);
                }
                periodList.Add(distinctPeriodList);
            }
        }
        return periodList;
    }

    /// <summary>
    /// Gets an <see cref="IEnumerable{T}"/> of all distinct timezone IDs in the list of periods.
    /// </summary>
    /// <returns>An <see cref="IEnumerable{T}"/> of distinct timezone IDs.</returns>
    internal IEnumerable<string?> GetDistinctTzIds() => Periods.Select(p => p.TzId).Distinct();

    /// <summary>
    /// Gets all unique <see cref="PeriodKind"/> values for a given timezone ID.
    /// </summary>
    /// <param name="tzId">The timezone ID to filter periods by.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> of unique <see cref="PeriodKind"/> values that match the specified timezone ID.</returns>
    internal IEnumerable<PeriodKind> GetDistinctPeriodKindsByTzId(string? tzId) =>
        Periods.Where(p => p.TzId == tzId).Select(p => p.GetPeriodKind()).Distinct();

    /// <summary>
    /// Gets an <see cref="IEnumerable{T}"/> of periods with the same <see cref="PeriodKind"/> for a given timezone ID.
    /// </summary>
    /// <param name="tzId">The timezone ID to filter periods by.</param>
    /// <param name="periodKind">The <see cref="PeriodKind"/> to filter periods by.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> of periods that match the specified timezone ID and <see cref="PeriodKind"/>.</returns>
    internal IEnumerable<Period> GetPeriodsByTzIdAndKind(string? tzId, PeriodKind periodKind) =>
        Periods.Where(p => p.TzId == tzId && p.GetPeriodKind() == periodKind);

    /// <inheritdoc/>
    [ExcludeFromCodeCoverage]
    public IEnumerator<Period> GetEnumerator() => Periods.GetEnumerator();

    /// <inheritdoc/>
    [ExcludeFromCodeCoverage]
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
