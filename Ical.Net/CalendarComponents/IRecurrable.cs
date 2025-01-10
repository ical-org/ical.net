//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System.Collections.Generic;
using Ical.Net.DataTypes;

namespace Ical.Net.CalendarComponents;

public interface IRecurrable : IGetOccurrences, IServiceProvider
{
    /// <summary>
    /// Gets/sets the start date/time of the component.
    /// </summary>
    IDateTime Start { get; set; }

    ExceptionDates ExceptionDates { get; }
    IList<RecurrencePattern> ExceptionRules { get; set; }
    RecurrenceDates RecurrenceDates { get; }
    IList<RecurrencePattern> RecurrenceRules { get; set; }
    IDateTime RecurrenceId { get; set; }
}
