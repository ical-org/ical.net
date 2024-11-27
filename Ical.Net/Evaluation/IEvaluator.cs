//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

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
    /// Gets the object associated with this evaluator.
    /// </summary>
    ICalendarObject AssociatedObject { get; }

    /// <summary>
    /// Evaluates this item to determine the dates and times for which it occurs/recurs.
    /// This method only evaluates items which occur/recur between <paramref name="periodStart"/>
    /// and <paramref name="periodEnd"/>; therefore, if you require a list of items which
    /// occur outside of this range, you must specify a <paramref name="periodStart"/> and
    /// <paramref name="periodEnd"/> which encapsulate the date(s) of interest.
    /// This method evaluates using the <paramref name="periodStart" /> as the beginning
    /// point.  For example, for a WEEKLY occurrence, the <paramref name="periodStart"/>
    /// determines the day of week that this item will recur on.
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
    ///     A list of <see cref="System.DateTime"/> objects for
    ///     each date/time when this item occurs/recurs.
    /// </returns>
    HashSet<Period> Evaluate(IDateTime referenceDate, DateTime periodStart, DateTime periodEnd, bool includeReferenceDateInResults);
}
