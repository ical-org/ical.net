//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;

namespace Ical.Net.Logging;

/// <summary>
/// Represents a logging interface for writing log entries with varying severity levels.
/// </summary>
internal interface ILogger // Make public when logging is used in library classes
{
    /// <summary>
    /// Writes a log entry.
    /// </summary>
    /// <param name="level">The severity level of the log entry.</param>
    /// <param name="messageTemplate">The message template. Ex: "Processed {OrderId} in {ElapsedTime}ms".</param>
    /// <param name="args">The objects to format into the message template.</param>
    void Log(LogLevel level, string messageTemplate, params object[] args);

    /// <summary>
    /// Writes a log entry that includes an exception.
    /// </summary>
    /// <param name="level">The severity level of the log entry.</param>
    /// <param name="exception">The exception to log.</param>
    /// <param name="messageTemplate">The message template.</param>
    /// <param name="args">The objects to format into the message template.</param>
    void Log(LogLevel level, Exception exception, string messageTemplate, params object[] args);

    /// <summary>
    /// Determines whether logging is enabled for the specified log level.
    /// </summary>
    /// <remarks>This method can be used to check if a particular log level is enabled before performing logging
    /// operations. It is useful for optimizing performance by avoiding unnecessary log message generation when logging is
    /// disabled for the given level.
    /// </remarks>
    /// <param name="level">
    /// The log level to check. Must be a valid <see cref="LogLevel"/> value.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if logging is enabled for the specified
    /// level; otherwise, <see langword="false"/>.
    /// </returns>
    bool IsEnabled(LogLevel level);
}
