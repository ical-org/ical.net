//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.Collections.Generic;
using System.Linq;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Evaluation;
using NodaTime;

namespace Ical.Net;

public static class CollectionExtensions
{
    /// <summary>
    /// Returns the elements of the sequence of occurrences to only include those that start before the specified period end.
    /// Important: The input sequence <b>must be ordered</b> according to the type's default equality comparer.
    /// <para/>
    /// A perfect fit is the sequence returned by <see cref="RecurringComponent.GetOccurrences(CalDateTime, EvaluationOptions)"/>,
    /// like the frequently used <see cref="CalendarEvent.GetOccurrences(CalDateTime, EvaluationOptions)"/>
    /// or <see cref="Calendar.GetOccurrences"/>.
    /// <para/>
    /// This is a convenience method for <see cref="Enumerable.TakeWhile{TSource}(IEnumerable{TSource}, Func{TSource, bool})"/>.
    /// But even with this method, the input sequence must be ordered according to the type's default equality comparer.
    /// </summary>
    /// <param name="sequence">The ordered sequence to be filtered.</param>
    /// <param name="periodEnd">The exclusive end of the period to be used as a filter.</param>
    /// <returns>
    /// The elements of the sequence of occurrences to only include those that start before the specified period end.
    /// </returns>
    ///
    [Obsolete("Use NodaTime.Instant to specify period end.")]
    public static IEnumerable<Occurrence> TakeWhileBefore(this IEnumerable<Occurrence> sequence, CalDateTime periodEnd)
        => sequence.TakeWhile(p => p.Start.ToInstant() < periodEnd.ToInstant());

    public static IEnumerable<Occurrence> TakeWhileBefore(this IEnumerable<Occurrence> sequence, Instant periodEnd)
        => sequence.TakeWhile(p => p.Start.ToInstant() < periodEnd);

    /// <summary>
    /// Returns the elements of the sequence of periods to only include those that start before the specified period end.
    /// Important: The input sequence <b>must be ordered</b> according to the type's default equality comparer.
    /// <para/>
    /// A perfect fit is the sequence returned by the <see cref="Evaluator.Evaluate"/> implementations
    /// like <see cref="RecurrencePatternEvaluator.Evaluate"/>.
    /// <para/>
    /// This is a convenience method for <see cref="Enumerable.TakeWhile{TSource}(IEnumerable{TSource}, Func{TSource, bool})"/>
    /// <param name="sequence">The ordered sequence to be filtered.</param>
    /// <param name="periodEnd">The exclusive end of the period to be used as a filter.</param>
    /// <returns>
    /// The elements of the sequence of periods to only include those that start before the specified period end.
    /// But even with this method, the input sequence must be ordered according to the type's default equality comparer.
    /// </returns>
    /// </summary>
    /// <param name="sequence">The ordered sequence to be filtered.</param>
    /// <param name="periodEnd">The exclusive end of the period to be used as a filter.</param>
    /// <returns>
    /// The elements of the sequence of periods to only include those that start before the specified period end.
    /// </returns>
    public static IEnumerable<DataTypes.Period> TakeWhileBefore(this IEnumerable<DataTypes.Period> sequence, CalDateTime periodEnd)
        => sequence.TakeWhile(p => p.StartTime < periodEnd);

    public static IEnumerable<EvaluationPeriod> TakeWhileBefore(this IEnumerable<EvaluationPeriod> sequence, Instant periodEnd)
        => sequence.TakeWhile(p => p.Start.ToInstant() < periodEnd);
}
