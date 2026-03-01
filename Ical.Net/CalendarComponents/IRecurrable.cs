//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
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

    [Obsolete("EXRULE is marked as deprecated in RFC 5545 and will be removed in a future version")]
    IList<RecurrencePattern> ExceptionRules { get; set; }

    RecurrenceDates RecurrenceDates { get; }

    [Obsolete("Use RecurrenceRule instead. Support for multiple recurrence rules will be removed in a future version.")]
    IList<RecurrencePattern> RecurrenceRules { get; set; }

    RecurrenceRule? RecurrenceRule { get; set; }

    CalDateTime? RecurrenceId { get; set; }
    IEvaluator? Evaluator { get; }
}
