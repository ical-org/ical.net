//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.Collections.Generic;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;

namespace Ical.Net;

public interface IGetOccurrences
{
    /// <summary>
    /// Returns all occurrences of this component that start on the date provided.
    /// All components starting between 12:00:00AM and 11:59:59 PM will be
    /// returned.
    /// <note>
    /// This will first Evaluate() the date range required in order to
    /// determine the occurrences for the date provided, and then return
    /// the occurrences.
    /// </note>
    /// </summary>
    /// <param name="dt">The date for which to return occurrences.</param>
    /// <returns>An IEnumerable that calculates and returns Periods representing the occurrences of this object in ascending order.</returns>
    IEnumerable<Occurrence> GetOccurrencesOfDay(IDateTime dt);

    IEnumerable<Occurrence> GetOccurrencesOfDay(DateTime dt);

    /// <summary>
    /// Returns all occurrences of this component that overlap with the date range provided.
    /// All components that overlap with the time range between <paramref name="startTime"/> and <paramref name="endTime"/> will be returned.
    /// </summary>
    /// <param name="startTime">The starting date range</param>
    /// <param name="endTime">The ending date range</param>
    /// <returns>An IEnumerable that calculates and returns Periods representing the occurrences of this object in ascending order.</returns>
    IEnumerable<Occurrence> GetOccurrences(IDateTime startTime = null, IDateTime endTime = null);

    IEnumerable<Occurrence> GetOccurrences(DateTime? startTime, DateTime? endTime);
}

public interface IGetOccurrencesTyped : IGetOccurrences
{
    /// <summary>
    /// Returns all occurrences of components of type T that start on the date provided.
    /// All components starting between 12:00:00AM and 11:59:59 PM will be
    /// returned.
    /// <note>
    /// This will first Evaluate() the date range required in order to
    /// determine the occurrences for the date provided, and then return
    /// the occurrences.
    /// </note>
    /// </summary>
    /// <param name="dt">The date for which to return occurrences.</param>
    /// <returns>An IEnumerable that calculates and returns Periods representing the occurrences of this object in ascending order.</returns>
    IEnumerable<Occurrence> GetOccurrences<T>(IDateTime dt) where T : IRecurringComponent;

    IEnumerable<Occurrence> GetOccurrences<T>(DateTime dt) where T : IRecurringComponent;

    /// <summary>
    /// Returns all occurrences of components of type T that start within the date range provided.
    /// All components occurring between <paramref name="startTime"/> and <paramref name="endTime"/>
    /// will be returned.
    /// </summary>
    /// <param name="startTime">The starting date range. If set to null, occurrences are returned from the beginning.</param>
    /// <param name="endTime">The ending date range. If set to null, occurrences are returned until the end.</param>
    /// <returns>An IEnumerable that calculates and returns Periods representing the occurrences of this object in ascending order.</returns>
    IEnumerable<Occurrence> GetOccurrences<T>(IDateTime startTime = null, IDateTime endTime = null) where T : IRecurringComponent;

    IEnumerable<Occurrence> GetOccurrences<T>(DateTime? startTime, DateTime? endTime) where T : IRecurringComponent;
}
