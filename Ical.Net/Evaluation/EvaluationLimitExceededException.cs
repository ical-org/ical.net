//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

namespace Ical.Net.Evaluation;

/// <summary>
/// Represents an exception will be raised during calendar evaluation when the maximum number of
/// increments is exceeded.
/// </summary>
/// <seealso cref="EvaluationOptions.MaxUnmatchedIncrements"/>
public class EvaluationLimitExceededException : EvaluationException
{ }
