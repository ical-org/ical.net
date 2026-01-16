// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.

using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Ical.Net.Tests.Logging;

internal record Options
{
    /// <summary>
    /// Represents the default output template used for formatting log messages
    /// in Serilog format.
    /// </summary>
    public const string DefaultOutputTemplate =
        "{Timestamp:yyyy-MM-dd HH:mm:ss.ffffZ}|{Level:u3}|{SourceContext}|{Message:lj}{NewLine}{Exception}";

    /// <summary>
    /// Gets or sets the template used to format log output in Serilog format.
    /// </summary>
    public string OutputTemplate { get; set; } = DefaultOutputTemplate;

    /// <summary>
    /// The default maximum number of logs to keep in memory or return from file.
    /// </summary>
    public const int DefaultMaxLogsCount = 1000;

    /// <summary>
    /// Gets or sets the minimum log level for messages to be recorded.
    /// </summary>
    public LogLevel MinLogLevel { get; set; } = LogLevel.Information;

    /// <summary>
    /// Gets or sets the collection of filters applied to logging.
    /// It allows for filtering log messages by category corresponding and log level.
    /// </summary>
    public List<Filter> Filters { get; set; } = new();

    /// <summary>
    /// Limits the number of logs stored in memory or latest logs returned from file.
    /// When the limit is reached, <b>the oldest logs are discarded</b>.
    /// </summary>
    public int MaxLogsCount { get; set; } = DefaultMaxLogsCount;

    /// <summary>
    /// Gets or sets a value indicating whether logging is enabled only in debug mode.
    /// <see langword="true"/> is the default, so logging won't be active in release builds unless explicitly enabled.
    /// Be careful when asserting log entries in tests, as they may not be present in release builds.
    /// </summary>
    public bool DebugModeOnly { get; set; } = true;

    /// <summary>
    /// Gets or sets the file path where logs are written.
    /// If <see langword="null"/>, logs are not written to a file.
    /// </summary>
    public string? LogToFileLogPath { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether logging should be performed in memory.
    /// </summary>
    public bool LogToMemory { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether log messages should be written to the console.
    /// </summary>
    public bool LogToConsole { get; set; } = false;
}
