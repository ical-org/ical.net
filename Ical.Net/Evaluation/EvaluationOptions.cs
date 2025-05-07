//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

namespace Ical.Net.Evaluation;

public class EvaluationOptions
{
    /// <summary>
    /// The maximum number of increments to evaluate without finding a recurrence before
    /// evaluation is stopped exceptionally. If null, the evaluation will continue indefinitely.
    /// </summary>
    /// <remarks>
    /// This option only applies to the evaluation of RecurrencePatterns.
    /// <para/>
    /// If the specified number of increments is exceeded without finding a recurrence, an
    /// exception of type <see cref="Ical.Net.Evaluation.EvaluationLimitExceededException"/> will be thrown.
    /// </remarks>
    public int? MaxUnmatchedIncrementsLimit { get; set; }
}
