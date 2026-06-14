// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.

using Microsoft.Extensions.Logging;

namespace Ical.Net.Tests.Logging;

/// <summary>
/// Represents a filter that determines which log messages are
/// captured based on a minimum log level and a logger name pattern.
/// </summary>
/// <remarks>The filter is applied to log messages by matching the
/// logger name against the specified pattern and ensuring the log level
/// meets or exceeds the given minimum log level.</remarks>
internal class Filter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Filter"/> class
    /// with the specified minimum log level and logger name pattern.
    /// </summary>
    /// <param name="logLevel">
    /// The minimum <see cref="LogLevel"/> required for a log message to pass the filter.
    /// </param>
    /// <param name="loggerNamePattern">
    /// A pattern used to match logger names.
    /// Do not use a wildcard pattern &quot;*&quot; here.
    /// </param>
    public Filter(LogLevel logLevel, string loggerNamePattern)
    {
        MinLogLevel = logLevel;
        LoggerNamePattern = loggerNamePattern;
    }

    /// <summary>
    /// Gets or sets the minimum log level for which log messages are captured
    /// for the <see cref="LoggerNamePattern"/>.
    /// </summary>
    public LogLevel MinLogLevel { get; set; }

    /// <summary>
    /// Gets or sets the pattern where the <see cref="MinLogLevel"/> should be applied.
    /// Do not use a wildcard pattern &quot;*&quot; here.
    /// </summary>
    public string LoggerNamePattern { get; set; }
}
