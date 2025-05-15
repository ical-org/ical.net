//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System.Collections.Generic;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Evaluation;

namespace Ical.Net;

public interface IGetOccurrences
{
    /// <summary>  
    /// Returns an IEnumerable that generates and returns all occurrences at or after <paramref name="startTime"/>.
    /// If <paramref name="startTime"/> isn't provided, all occurrences will be generated.
    /// </summary>  
    /// <param name="startTime">The starting date range</param>  
    /// <param name="options">Evaluation options to be applied. If null, default options will be applied.</param>  
    /// <returns>An IEnumerable that calculates and returns Periods representing the occurrences of this object in ascending order.</returns>
    /// <remarks>  
    /// The returned enumerable will evaluate as it is enumerated and can therefore also represent an indefinite sequence.
    /// The sequence is ordered.
    /// If enumeration hits year 10,000, an <see cref="EvaluationOutOfRangeException"/> is raised.
    /// This particularly can happen with recurrence rules (rrules) that neither have a count nor an until date specified.
    /// <para/>
    /// It is advisable to limit indefinite sequences by applying the <see cref="CollectionExtensions.TakeBefore(IEnumerable{Occurrence},Ical.Net.DataTypes.CalDateTime?)"/> extension method,
    /// or LINQ methods like <c>.TakeWhile()</c>
    /// </remarks>  
    IEnumerable<Occurrence> GetOccurrences(CalDateTime? startTime = null, EvaluationOptions? options = null);
}

public interface IGetOccurrencesTyped : IGetOccurrences
{
    /// <summary>  
    /// Returns an IEnumerable that generates and returns all occurrences at or after <paramref name="startTime"/>.
    /// If <paramref name="startTime"/> isn't provided, all occurrences will be generated.
    /// </summary>  
    /// <param name="startTime">The starting date range</param>  
    /// <param name="options">Evaluation options to be applied. If null, default options will be applied.</param>  
    /// <returns>An IEnumerable that calculates and returns Periods representing the occurrences of this object in ascending order.</returns>
    /// <remarks>  
    /// The returned enumerable will evaluate as it is enumerated and can therefore also represent an indefinite sequence.
    /// The sequence is ordered.
    /// If enumeration hits year 10,000, an <see cref="EvaluationOutOfRangeException"/> is raised.
    /// This particularly can happen with recurrence rules (rrules) that neither have a count nor an until date specified.
    /// <para/>
    /// It is advisable to limit indefinite sequences by applying the <see cref="CollectionExtensions.TakeBefore(IEnumerable{Occurrence},Ical.Net.DataTypes.CalDateTime?)"/> extension method,
    /// or LINQ methods like <c>.TakeWhile()</c>
    /// </remarks>  
    IEnumerable<Occurrence> GetOccurrences<T>(CalDateTime? startTime = null, EvaluationOptions? options = null) where T : IRecurringComponent;
}
