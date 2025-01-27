//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

#nullable enable
using System.Collections.Generic;

namespace Ical.Net.DataTypes;

/// <summary>
/// This class is used to manage ICalendar <c>EXDATE</c> properties, which can be date-time and date-only.
/// <remarks>
/// The class is a wrapper around a list of <c>PeriodList</c> objects.
/// Specifically, it is used to group periods by their <c>TzId</c>, <c>PeriodKind</c> and <c>date-time/date-only</c>
/// in way that serialization conforms to the RFC 5545 standard.
/// </remarks>
/// </summary>
public class ExceptionDates : PeriodListWrapperBase
{
    internal ExceptionDates(IList<PeriodList> listOfPeriodList) : base(listOfPeriodList)
    { }

    /// <summary>
    /// Adds a date to the list, if it doesn't already exist.
    /// </summary>
    public ExceptionDates Add(CalDateTime dt)
    {
        var periodList = GetOrCreatePeriodList(dt);

        var dtPeriod = new Period(dt);
        periodList.Add(dtPeriod);

        return this;
    }

    /// <summary>
    /// Adds a range of dates to the list, if they don't already exist.
    /// </summary>
    public ExceptionDates AddRange(IEnumerable<CalDateTime> dates)
    {
        foreach (var dt in dates)
        {
            Add(dt);
        }

        return this;
    }
}
