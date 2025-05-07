//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

#nullable enable
namespace Ical.Net.Evaluation;

/// <summary>
/// Represents an exception that will be raised during calendar evaluation if the
/// maximum supported date is exceeded.
/// </summary>
public class EvaluationOutOfRangeException(string message) : EvaluationException(message)
{ }
