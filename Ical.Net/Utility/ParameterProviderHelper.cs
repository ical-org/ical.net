//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System.Collections.Generic;
using Ical.Net.DataTypes;

namespace Ical.Net.Utility;

/// <summary>
/// Helpers for classes that implement <see cref="Ical.Net.Serialization.IParameterProvider"/>.
/// The <see cref="Ical.Net.Serialization.IParameterProvider.GetParameters"/> method
/// should use the helper in this class to get parameters for specific types.
/// </summary>
internal static class ParameterProviderHelper
{
    /// <summary>
    /// Retrieves a collection of calendar parameters based on the properties
    /// of the specified <see cref="CalDateTime"/> value.
    /// </summary>
    /// <returns>
    /// A list of <see cref="CalendarParameter"/> objects representing
    /// the parameters derived from the <see cref="CalDateTime"/> value.
    /// If <paramref name="value"/> is not a <see cref="CalDateTime"/>,
    /// an empty list is returned.
    /// </returns>
    public static List<CalendarParameter> GetCalDateTimeParameters(object? value)
    {
        if (value is not CalDateTime dt)
            return [];

        var param = new List<CalendarParameter>(2);
        if (dt is { IsFloating: false, IsUtc: false })
            param.Add(new CalendarParameter("TZID", dt.TzId));

        if (!dt.HasTime)
            param.Add(new CalendarParameter("VALUE", "DATE"));

        return param;
    }

    /// <summary>
    /// Generates a list of calendar parameters based on the provided recurrence identifier.
    /// </summary>
    /// <returns>
    /// A list of <see cref="CalendarParameter"/> objects representing the recurrence identifier parameters.  The list
    /// includes parameters for the start time and, if applicable, a "RANGE" parameter with the value "THISANDFUTURE".
    /// Returns an empty list if <paramref name="value"/> is not a <see cref="RecurrenceIdentifier"/>.
    /// </returns>
    public static List<CalendarParameter> GetRecurrenceIdentifierParameters(object? value)
    {
        if (value is not RecurrenceIdentifier rid)
            return [];

        var param = new List<CalendarParameter>(3);
        param.AddRange(GetCalDateTimeParameters(rid.StartTime));
        if (rid.Range == RecurrenceRange.ThisAndFuture)
        {
            param.Add(new CalendarParameter("RANGE", "THISANDFUTURE"));
        }

        return param;
    }
}
