//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

namespace Ical.Net.Logging.Internal;

/// <inheritdoc/>
internal class Logger<T> : Logger
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Logger"/> class
    /// for the specified type T, using the specified logger factory.
    /// </summary>
    public Logger(ILoggerFactory loggerFactory)
        : base(loggerFactory, typeof(T).FullName ?? typeof(T).Name)
    {
        // This constructor is used to create a logger for a specific type T.
    }
}
