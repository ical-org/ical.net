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
    /// Evaluates this item to determine the dates and times for which it occurs/recurs.
    /// This method only evaluates items which occur/recur at or after <paramref name="periodStart"/>.
    /// To apply an upper bound, consider using <see cref="System.Linq.Enumerable.TakeWhile{TSource}(IEnumerable{TSource}, Func{TSource, bool})"/>.
    /// This method evaluates using the <paramref name="periodStart" /> as the beginning
    /// point. For example, for a WEEKLY occurrence, the <paramref name="periodStart"/>
    /// determines the day of week that this item will recur on. If <paramref name="periodStart"/>
    /// is set to null, all recurrences will be returned.
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
    /// <param name="options"></param>
    /// <returns>
    ///     A sequence of <see cref="Ical.Net.DataTypes.Period"/> objects for
    ///     each date/time when this item occurs/recurs.
    /// </returns>
    IEnumerable<Period> Evaluate(CalDateTime referenceDate, CalDateTime? periodStart, EvaluationOptions? options);
}
