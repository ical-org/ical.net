//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;

namespace Ical.Net.Logging;

/// <summary>
/// Represents a factory for creating <see cref="ILogger"/> instances.
/// </summary>
internal interface ILoggerFactory : IDisposable // Make public when logging is used in library classes
{
    /// <summary>
    /// Creates a new logger instance for the specified category name.
    /// </summary>
    /// <param name="categoryName">
    /// The name of the category for the logger.
    /// </param>
    /// <returns>
    /// An <see cref="ILogger"/> instance configured for the specified category name.
    /// </returns>
    ILogger CreateLogger(string categoryName);
}
