//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

#nullable enable
using System;
using System.Collections.Generic;
using Ical.Net.DataTypes;

namespace Ical.Net.Evaluation;

public interface IEvaluator
{
    /// <summary>
    /// The system calendar that governs the evaluation rules.
    /// </summary>
    System.Globalization.Calendar Calendar { get; }

    /// <summary>
    /// Evaluates this item to determine the dates and times for which it occurs/recurs.
    /// This method only evaluates items which occur/recur between <paramref name="periodStart"/>
    /// and <paramref name="periodEnd"/>; therefore, if you require a list of items which
    /// occur outside of this range, you must specify a <paramref name="periodStart"/> and
    /// <paramref name="periodEnd"/> which encapsulate the date(s) of interest.
    /// This method evaluates using the <paramref name="periodStart" /> as the beginning
    /// point.  For example, for a WEEKLY occurrence, the <paramref name="periodStart"/>
    /// determines the day of week that this item will recur on.
    ///
    /// Items are returned in ascending order.
    /// <note type="caution">
    ///     For events with very complex recurrence rules, this method may be a bottleneck
    ///     during processing time, especially when this method is called for a large number
    ///     of items, in sequence, or for a very large time span.
    /// </note>
    /// </summary>
    /// <param name="referenceDate"></param>
    /// <param name="periodStart"></param>
    /// <param name="periodEnd"></param>
    /// <param name="includeReferenceDateInResults"></param>
    /// <returns>
    ///     A sequence of <see cref="Ical.Net.DataTypes.Period"/> objects for
    ///     each date/time when this item occurs/recurs.
    /// </returns>
    IEnumerable<Period> Evaluate(IDateTime referenceDate, IDateTime? periodStart, IDateTime? periodEnd, bool includeReferenceDateInResults);
}
