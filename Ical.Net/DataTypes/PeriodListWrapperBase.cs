//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

#nullable enable
using System.Collections.Generic;
using System.Linq;
using Ical.Net.Utility;

namespace Ical.Net.DataTypes;

/// This base class is used to manage ICalendar <c>EXDATE</c> and <c>RATE</c> properties.
/// <remarks>
/// The class is a wrapper around a list of <c>PeriodList</c> objects.
/// Specifically, it is used to group periods by their <c>TzId</c> and <c>PeriodKind</c>
/// is way that serialization conforms to the RFC 5545 standard.
/// </remarks>
public abstract class PeriodListWrapperBase
{
    private protected IList<PeriodList> ListOfPeriodList;

    private protected PeriodListWrapperBase(IList<PeriodList> periodList) => ListOfPeriodList = periodList;

    /// <summary>
    /// Gets a flattened and ordered list of all distinct dates in the list
    /// </summary>
    public IEnumerable<IDateTime> GetAllDates()
        => ListOfPeriodList.SelectMany(pl =>
                pl.Where(p => p.PeriodKind is PeriodKind.DateOnly or PeriodKind.DateTime)
                    .Select(p => p.StartTime))
            .OrderedDistinct();

    /// <summary>
    /// Clears all elements from the list.
    /// </summary>
    public void Clear() => ListOfPeriodList.Clear();

    /// <summary>
    /// Determines whether the list contains the <paramref name="dt"/>.
    /// </summary>
    public bool Contains(IDateTime dt)
    {
        var periodList = GetPeriodList(dt);

        return periodList?
            .FirstOrDefault(period => Equals(period.StartTime, dt)) != null;
    }

    /// <inheritdoc cref="PeriodList.Remove"/>
    public bool Remove(IDateTime dt)
    {
        var periodList = GetPeriodList(dt);

        if (periodList == null) return false;

        var dtPeriod = new Period(dt);

        return periodList.Remove(dtPeriod);
    }

    private protected PeriodList GetOrCreatePeriodList(IDateTime dt)
    {
        var periodList = GetPeriodList(dt);

        if (periodList != null) return periodList;

        periodList = new PeriodList();
        ListOfPeriodList.Add(periodList);
        return periodList;
    }

    private protected PeriodList GetOrCreatePeriodList(Period period)
    {
        var periodList = GetPeriodList(period);

        if (periodList != null) return periodList;

        periodList = new PeriodList();
        ListOfPeriodList.Add(periodList);
        return periodList;
    }

    private protected PeriodList? GetPeriodList(IDateTime dt)
    {
        // The number of PeriodLists is expected to be small, so a linear search is acceptable.
        return ListOfPeriodList
            .FirstOrDefault(p =>
                p.TzId == dt.TzId
                && p.PeriodKind == (dt.HasTime ? PeriodKind.DateTime : PeriodKind.DateOnly));
    }

    private protected PeriodList? GetPeriodList(Period period)
    {
        // The number of PeriodLists is expected to be small, so a linear search is acceptable.
        return ListOfPeriodList
            .FirstOrDefault(p =>
                p.TzId == period.TzId
                && p.PeriodKind == period.PeriodKind
                && p[0].StartTime.HasTime == period.StartTime.HasTime);
    }

    /// <summary>
    /// Gets a flattened and ordered list of all distinct periods with
    /// <see cref="PeriodKind.Period"/>, <see cref="PeriodKind.DateOnly"/> and <see cref="PeriodKind.DateTime"/>.
    /// </summary>
    internal IEnumerable<Period> GetAllPeriodsByKind(params PeriodKind[] periodKinds)
        => ListOfPeriodList.SelectMany(pl => pl.Where(p => periodKinds.Contains(p.PeriodKind))).OrderedDistinct();
}
