//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using Ical.Net.Logging.Internal;

namespace Ical.Net.Logging;

/// <summary>
/// Provides a static entry point for obtaining <see cref="ILogger"/> instances in Ical.Net.
/// Consumers should use <see cref="LoggingProvider.SetLoggerFactory"/> at application startup.
/// Otherwise, a no-op logger factory will be used.
/// </summary>
internal static class LoggingProvider // Make public when logging is used in library classes
{
    private static ILoggerFactory? _loggerFactory;

    /// <summary>
    /// Returns <see langword="true"/> if a <see cref="ILoggerFactory"/> implementation
    /// has been set (using <see cref="LoggingProvider.SetLoggerFactory"/>).
    /// </summary>
    public static bool FactoryIsSet => _loggerFactory is not null;

    /// <summary>
    /// Sets the global <see cref="ILoggerFactory"/> used by Ical.Net for creating <see cref="ILogger"/> instances.
    /// This should be called once at application startup by the consuming application or thread that initializes logging.
    /// <para/>
    /// Before calling this method, or setting <see langword="null"/> a no-op logger factory will be used.
    /// <para/>
    /// Check <see cref="FactoryIsSet"/> to determine if a logger factory has already been set.
    /// </summary>
    /// <param name="loggerFactory">The <see cref="ILoggerFactory"/> instance from the consuming application's DI container or direct creation.</param>
    /// <param name="force">If <see langword="true"/>, an existing <see cref="ILoggerFactory"/> will be overwritten; else the method will throw.</param>
    public static void SetLoggerFactory(ILoggerFactory? loggerFactory, bool force = false)
    {
        if (!force && _loggerFactory is not null)
            throw new InvalidOperationException(
                "LoggerFactory has already been set. To override, set force to true.");

        _loggerFactory = loggerFactory;
    }

    /// <summary>
    /// Gets an <see cref="ILogger"/> instance for the specified category name.
    /// </summary>
    /// <param name="categoryName">The category name for the logger (typically the full type name).</param>
    /// <returns>An <see cref="ILogger"/> instance.</returns>
    internal static ILogger CreateLogger(string categoryName)
        => (_loggerFactory ?? NullLoggerFactory.Instance).CreateLogger(categoryName);

    /// <summary>
    /// Gets an <see cref="ILogger"/> instance for the specified type.
    /// The category name will be the full name of the type T.
    /// </summary>
    /// <typeparam name="T">The type for which to create the logger.</typeparam>
    /// <returns>An <see cref="ILogger"/> instance.</returns>
    internal static ILogger CreateLogger<T>()
        => new Logger<T>(_loggerFactory ?? NullLoggerFactory.Instance);

    /// <summary>
    /// Gets the current instance of the logger factory for Ical.Net loggers.
    /// </summary>
    internal static ILoggerFactory LoggerFactory
        => _loggerFactory ?? NullLoggerFactory.Instance;
}
