//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

#nullable enable
using System.Collections.Generic;
using Ical.Net.DataTypes;
using Ical.Net.Evaluation;

namespace Ical.Net.CalendarComponents;

public interface IRecurrable : IGetOccurrences
{
    /// <summary>
    /// Gets/sets the start date/time of the component.
    /// </summary>
    CalDateTime? Start { get; set; }

    ExceptionDates ExceptionDates { get; }
    IList<RecurrencePattern> ExceptionRules { get; set; }
    RecurrenceDates RecurrenceDates { get; }
    IList<RecurrencePattern> RecurrenceRules { get; set; }
    CalDateTime? RecurrenceId { get; set; }
    IEvaluator? Evaluator { get; }
}
