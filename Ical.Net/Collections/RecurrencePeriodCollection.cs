//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

#nullable enable
using System.Collections.Generic;
using Ical.Net.DataTypes;

namespace Ical.Net.Collections;

/// <summary>
/// Represents a collection of recurrence dates and periods for calendar events.
/// <para>
/// This class is used to manage dates and periods that should be included as recurring events.
/// </para>
/// <para>
/// The main feature of this class is the <see cref="ToRecurrenceDates"/> method, which aggregates
/// and converts the exception <see cref="CalDateTime"/> and <see cref="Period"/> objects into a
/// list of <see cref="PeriodList"/> objects. This method ensures that the periods are grouped
/// by their timezone IDs and period kinds, and that each <see cref="PeriodList"/> contains only
/// distinct periods.
/// </para>
/// </summary>
public class RecurrencePeriodCollection : PeriodCollectionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RecurrencePeriodCollection"/> class.
    /// </summary>
    public RecurrencePeriodCollection()
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="RecurrencePeriodCollection"/> class
    /// with a single <see cref="CalDateTime"/>.
    /// </summary>
    /// <param name="dt"></param>
    public RecurrencePeriodCollection(CalDateTime dt) : this()
    {
        Add(dt);
    }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="RecurrencePeriodCollection"/> class
    /// with a list of <see cref="Period"/> objects.
    /// </summary>
    /// <param name="periods"></param>
    public RecurrencePeriodCollection(IEnumerable<Period> periods) : this()
    {
        AddRange(periods);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RecurrencePeriodCollection"/> class
    /// with a list of <see cref="CalDateTime"/> objects.
    /// </summary>
    /// <param name="dtList"></param>
    public RecurrencePeriodCollection(IEnumerable<CalDateTime> dtList) : this()
    {
        AddRange(dtList);
    }

    /// <summary>
    /// Adds a <see cref="Period"/> to the collection.
    /// </summary>
    /// <param name="period"></param>
    /// <returns>This instance.</returns>
    public new RecurrencePeriodCollection Add(Period period)
    {
        base.Add(period);
        return this;
    }

    /// <summary>
    /// Adds a list of <see cref="Period"/>s to the collection.
    /// </summary>
    /// <param name="periods"></param>
    /// <returns></returns>
    public RecurrencePeriodCollection AddRange(IEnumerable<Period> periods)
    {
        Periods.AddRange(periods);
        return this;
    }

    /// <summary>
    /// Aggregates and converts the recurrence <see cref="CalDateTime"/>s and <see cref="Period"/>s to a list of <see cref="PeriodList"/> objects.
    /// </summary>
    /// <returns>A list of <see cref="PeriodList"/> objects.</returns>
    public List<PeriodList> ToRecurrenceDates() => ToListOfPeriodList();
}
