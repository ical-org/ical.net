//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;

namespace Ical.Net.Evaluation;

/// <summary>
/// Represents an exception that may occur during calendar evaluation.
/// </summary>
public class EvaluationException : Exception
{
    public EvaluationException() { }

    public EvaluationException(string message) : base(message)
    { }
}
