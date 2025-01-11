//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

#nullable enable
using System.Collections.Generic;
using System.Linq;
using Ical.Net.Utility;

namespace Ical.Net.DataTypes;

/// <summary>
/// This class is used to manage ICalendar <c>RDATE</c> properties, which can be date-time, date-only and period.
/// <remarks>
/// The class is a wrapper around a list of <c>PeriodList</c> objects.
/// Specifically, it is used to group periods by their <c>TzId</c>, <c>PeriodKind</c> and <c>date-time/date-only</c>
/// in way that serialization conforms to the RFC 5545 standard.
/// </remarks>
/// </summary>
public class RecurrenceDates : PeriodListWrapperBase
{
    internal RecurrenceDates(IList<PeriodList> listOfPeriodList) : base(listOfPeriodList)
    { }

    /// <summary>
    /// Adds a date to the list, if it doesn't already exist.
    /// </summary>
    public RecurrenceDates Add(IDateTime dt)
    {
        var periodList = GetOrCreatePeriodList(dt);
        var dtPeriod = new Period(dt);
        periodList.Add(dtPeriod);

        return this;
    }

    /// <summary>
    /// Adds a period to the list, if it doesn't already exist.
    /// </summary>
    public RecurrenceDates Add(Period period)
    {
        var periodList = GetOrCreatePeriodList(period);
        periodList.Add(period);

        return this;
    }

    /// <summary>
    /// Adds a range of dates to the list, if they don't already exist.
    /// </summary>
    public RecurrenceDates AddRange(IEnumerable<IDateTime> dates)
    {
        foreach (var dt in dates)
        {
            Add(dt);
        }

        return this;
    }

    /// <summary>
    /// Adds a range of periods to the list, if they don't already exist.
    /// </summary>
    public RecurrenceDates AddRange(IEnumerable<Period> periods)
    {
        foreach (var period in periods)
        {
            Add(period);
        }

        return this;
    }

    /// <summary>
    /// Determines whether the list contains the <paramref name="period"/>.
    /// </summary>
    public bool Contains(Period period)
    {
        var periodList = GetPeriodList(period);

        return periodList?
            .FirstOrDefault(p => Equals(p, period)) != null;
    }

    /// <inheritdoc cref="PeriodList.Remove"/>
    public bool Remove(Period period)
    {
        var periodList = GetPeriodList(period);

        if (periodList == null) return false;

        return periodList.Remove(period);
    }

    /// <summary>
    /// Gets a flattened and ordered list of all distinct periods in the list.
    /// </summary>
    public IEnumerable<Period> GetAllPeriods()
        => ListOfPeriodList.
            SelectMany(pl => pl.Where(p => p.PeriodKind is PeriodKind.Period)).OrderedDistinct();
}
