//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;

namespace Ical.Net.Logging.Internal;

/// <summary>
/// Provides logging functionality by delegating to an
/// underlying <see cref="ILogger"/> instance.
/// </summary>
internal class Logger : ILogger
{
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="Logger"/> class
    /// using the specified logger factory and category name.
    /// </summary>
    /// <param name="loggerFactory">
    /// The factory used to create the logger instance.
    /// </param>
    /// <param name="categoryName">The category name used to identify the logger.
    /// </param>
    public Logger(ILoggerFactory loggerFactory, string categoryName)
    {
        _logger = loggerFactory.CreateLogger(categoryName);
    }

    /// <summary>
    /// Determines whether logging is enabled for the specified <see cref="LogLevel"/>.
    /// </summary>
    /// <param name="level">The log level to check.</param>
    /// <returns>
    /// <see langword="true"/> if logging is enabled for the specified <see cref="LogLevel"/>;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool IsEnabled(LogLevel level) => _logger.IsEnabled(level);

    /// <summary>
    /// Logs a message at the specified log level using the provided message template and arguments.
    /// </summary>
    /// <param name="level">The severity level of the log message. Must be a valid <see cref="LogLevel"/>.</param>
    /// <param name="messageTemplate">The message template containing placeholders for arguments.</param>
    /// <param name="args">An array of objects to format into the placeholders in the message template.</param>
    public void Log(LogLevel level, string messageTemplate, params object[] args)
    {
        if (IsEnabled(level))
        {
            _logger.Log(level, messageTemplate, args);
        }
    }

    /// <summary>
    /// Logs a message at the specified log level using the provided message template and arguments.
    /// </summary>
    /// <param name="level">The severity level of the log message. Must be a valid <see cref="LogLevel"/>.</param>
    /// <param name="exception">The <see cref="Exception"/> to add to the log.</param>
    /// <param name="messageTemplate">The message template containing placeholders for arguments.</param>
    /// <param name="args">An array of objects to format into the placeholders in the message template.</param>
    public void Log(LogLevel level, Exception exception, string messageTemplate, params object[] args)
    {
        if (IsEnabled(level))
        {
            _logger.Log(level, exception, messageTemplate, args);
        }
    }
}
