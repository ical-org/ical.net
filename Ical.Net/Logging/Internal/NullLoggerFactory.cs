//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

namespace Ical.Net.Logging.Internal;

/// <summary>
/// Provides a no-operation implementation of <see cref="ILoggerFactory"/> that produces <see cref="NullLogger"/>
/// instances. This factory is intended for scenarios where logging is not required or should be disabled.
/// </summary>
internal class NullLoggerFactory : ILoggerFactory
{
    /// <summary>
    /// Provides a singleton instance of a <see cref="NullLoggerFactory"/> that produces no-op loggers.
    /// </summary>
    public static readonly NullLoggerFactory Instance = new();

    private NullLoggerFactory() { }

    /// <summary>
    /// Creates a <see cref="NullLogger.Instance"/>.
    /// </summary>
    /// <param name="categoryName">The parameter is not used.</param>
    /// <returns>A <see cref="NullLogger.Instance"/></returns>
    public ILogger CreateLogger(string categoryName) => NullLogger.Instance;

    /// <inheritdoc/>
    public void Dispose()
    {
        // no-op
    }
}
