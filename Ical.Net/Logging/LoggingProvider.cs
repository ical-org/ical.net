//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Ical.Net.Logging;

/// <summary>
/// Provides a static entry point for obtaining <see cref="ILogger"/> instances in Ical.Net.
/// Consumers should use <see cref="LoggingProvider.SetLoggerFactory"/> at application startup.
/// Otherwise, a no-op logger factory will be used.
/// </summary>
internal static class LoggingProvider // Make public when logging is used in library classes
{
    /// <summary>
    /// Sets the global <see cref="ILoggerFactory"/> used by Ical.Net for creating <see cref="ILogger"/> instances.
    /// This should be called once at application startup by the consuming application.
    /// If <see langword="null"/>, <see cref="NullLoggerFactory.Instance"/> will be used, resulting in no-op logging.
    /// </summary>
    /// <param name="loggerFactory">The <see cref="ILoggerFactory"/> instance from the consuming application's DI container or direct creation.</param>
    public static void SetLoggerFactory(ILoggerFactory? loggerFactory)
        => LoggerFactory = loggerFactory ?? NullLoggerFactory.Instance;

    /// <summary>
    /// Gets an <see cref="ILogger"/> instance for the specified category name.
    /// </summary>
    /// <param name="categoryName">The category name for the logger (typically the full type name).</param>
    /// <returns>An <see cref="ILogger"/> instance.</returns>
    internal static ILogger CreateLogger(string categoryName)
        => LoggerFactory.CreateLogger(categoryName);

    /// <summary>
    /// Gets an <see cref="ILogger{T}"/> instance for the specified type.
    /// The category name will be the full name of the type T.
    /// </summary>
    /// <typeparam name="T">The type for which to create the logger.</typeparam>
    /// <returns>An <see cref="ILogger"/> instance.</returns>
    internal static ILogger<T> CreateLogger<T>()
        => new Logger<T>(LoggerFactory);

    /// <summary>
    /// Gets the current instance of the logger factory for Ical.Net loggers.
    /// </summary>
    internal static ILoggerFactory LoggerFactory { get; private set; } = NullLoggerFactory.Instance;
}
