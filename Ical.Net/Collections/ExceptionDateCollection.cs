//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

#nullable enable
using System.Collections.Generic;
using Ical.Net.DataTypes;

namespace Ical.Net.Collections;

/// <summary>
/// Represents a collection of exception dates for calendar events.
/// <para>
/// This class is used to manage dates that should be excluded from recurring events.
/// </para>
/// <para>
/// The main feature of this class is the <see cref="ToExceptionDates"/> method, which aggregates
/// and converts the exception <see cref="CalDateTime"/> objects into a list of <see cref="PeriodList"/> objects.
/// This method ensures that the periods are grouped by their timezone IDs and period kinds, and that
/// each <see cref="PeriodList"/> contains only distinct periods.
/// </para>
/// </summary>
public class ExceptionDateCollection : PeriodCollectionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ExceptionDateCollection"/> class.
    /// </summary>
    public ExceptionDateCollection()
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="ExceptionDateCollection"/> class with a single <see cref="CalDateTime"/>.
    /// </summary>
    /// <param name="dt">The <see cref="CalDateTime"/> to add to the collection.</param>
    public ExceptionDateCollection(CalDateTime dt) : this()
    {
        Add(dt);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ExceptionDateCollection"/> class with a collection of <see cref="CalDateTime"/> objects.
    /// </summary>
    /// <param name="dtList">The collection of <see cref="CalDateTime"/> objects to add to the collection.</param>
    public ExceptionDateCollection(IEnumerable<CalDateTime> dtList) : this()
    {
        AddRange(dtList);
    }

    /// <summary>
    /// Aggregates and converts the exception <see cref="CalDateTime"/>s to a list of <see cref="PeriodList"/> objects.
    /// </summary>
    /// <returns>A list of <see cref="PeriodList"/> objects.</returns>
    public List<PeriodList> ToExceptionDates() => ToListOfPeriodList();

    /// <summary>
    /// Adds the <see cref="Period.StartTime"/> to the collection as a <see cref="Period"/>.
    /// Other properties of the <see cref="Period"/> are neglected.
    /// </summary>
    /// <param name="item"></param>
    public new ExceptionDateCollection Add(Period item)
    {
        Add(new CalDateTime(item.StartTime));
        return this;
    }
}
