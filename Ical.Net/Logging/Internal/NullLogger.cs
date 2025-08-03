//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.Diagnostics.CodeAnalysis;

namespace Ical.Net.Logging.Internal;

/// <summary>
/// Represents a no-operation implementation of the <see cref="ILogger"/> interface.
/// </summary>
internal class NullLogger : ILogger
{
    public static readonly NullLogger Instance = new();

    private NullLogger() { }

    /// <inheritdoc/>
    [ExcludeFromCodeCoverage]
    public void Log(LogLevel level, string messageTemplate, params object[] args)
    {
        // will never get called because all LogLevels are disabled
    }

    /// <inheritdoc/>
    [ExcludeFromCodeCoverage]
    public void Log(LogLevel level, Exception exception, string messageTemplate, params object[] args)
    {
        // will never get called because all LogLevels are disabled
    }

    /// <inheritdoc/>
    public bool IsEnabled(LogLevel level) => false;
}
