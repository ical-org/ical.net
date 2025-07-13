//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//
#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace Ical.Net.Tests.Logging;

/// <summary>
/// A simple logger implementation that may be helpful in unit tests.
/// </summary>
internal sealed class InMemoryLogger : ILogger
{
    private readonly List<LogEntry> _logEntries;
    private readonly string _categoryName;

    public InMemoryLogger(List<LogEntry> logEntries, string categoryName)
    {
        _logEntries = logEntries;
        _categoryName = categoryName;
    }

    /// <inheritdoc/>
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    /// <inheritdoc/>
    public bool IsEnabled(LogLevel logLevel) => true;

    /// <inheritdoc/>
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        var message = formatter(state, exception);
        var structuredMessage = $"[{_logEntries.Count}] [{logLevel}] [{_categoryName}] {message}";

        if (state is IReadOnlyCollection<KeyValuePair<string, object>> structuredData)
        {
            var originalFormat = structuredData.FirstOrDefault(kvp => kvp.Key == "{OriginalFormat}").Value as string;
            var data = structuredData.Where(kvp => kvp.Key != "{OriginalFormat}").ToList();

            if (data.Count > 0)
            {
                var structuredDataString = string.Join(", ", data.Select(kvp => $"{kvp.Key}: {kvp.Value}"));
                structuredMessage = $"[{_logEntries.Count}] [{logLevel}] [{_categoryName}] {originalFormat} | {structuredDataString}";
            }
        }

        if (exception != null)
        {
            structuredMessage += $" | Exception: {exception}";
        }

        _logEntries.Add(new LogEntry
        {
            LogLevel = logLevel,
            EventId = _logEntries.Count,
            Message = structuredMessage,
            Exception = exception
        });
    }
}

/// <summary>
/// Represents a log entry containing details about a specific logging event.
/// </summary>
/// <remarks>A <see cref="LogEntry"/> includes information such as the log level, event identifier, message, and
/// any associated exception. This class is typically used to encapsulate logging information for output to a logging
/// system or file.
/// </remarks>
public sealed class LogEntry
{
    public LogLevel LogLevel { get; set; }
    public EventId EventId { get; set; }
    public string? Message { get; set; }
    public Exception? Exception { get; set; }
}

/// <summary>
/// Provides an in-memory implementation of the <see cref="ILoggerProvider"/> interface.
/// </summary>
/// <remarks>This provider stores log entries in memory, allowing for easy access and manipulation of log data
/// during runtime. It is particularly useful for testing and scenarios where persistent storage of logs is not
/// required. The provider can be used to create loggers that write to a shared list of log entries.
/// </remarks>
public sealed class InMemoryLoggerProvider : ILoggerProvider
{
    private readonly List<LogEntry> _logEntries;

    /// <summary>
    /// Initializes a new instance of the <see cref="InMemoryLoggerProvider"/>
    /// class with a specified list of log entries.
    /// </summary>
    /// <remarks>The provided list is used to store log entries in memory. Ensure that the list is not null
    /// before passing it to the constructor.
    /// </remarks>
    /// <param name="logEntries">A list of <see cref="LogEntry"/> objects that the logger provider will use to store log messages.</param>
    public InMemoryLoggerProvider(List<LogEntry> logEntries)
    {
        _logEntries = logEntries;
    }

    /// <summary>
    /// Creates a new logger instance for the specified category.
    /// </summary>
    /// <param name="categoryName">The category name for the logger. This is used to group log messages.</param>
    /// <returns>An <see cref="ILogger"/> instance configured for the specified category.</returns>
    public ILogger CreateLogger(string categoryName) => new InMemoryLogger(_logEntries, categoryName);

    /// <summary>
    /// This is a no-op method that is called when the logger provider is disposed.
    /// </summary>
    public void Dispose() {  }

    /// <summary>
    /// Creates a new instance of <see cref="ILoggerFactory"/> configured with an in-memory log provider.
    /// </summary>
    /// <param name="logEntries">A list of <see cref="LogEntry"/> objects to be used by the in-memory log provider.</param>
    /// <returns>An <see cref="ILoggerFactory"/> instance configured to use the specified log entries.</returns>
    public static ILoggerFactory CreateLoggerFactory(List<LogEntry> logEntries)
    {
        return LoggerFactory.Create(builder =>
        {
            var provider = new InMemoryLoggerProvider(logEntries);
            builder.AddProvider(provider);
        });
    }
}
