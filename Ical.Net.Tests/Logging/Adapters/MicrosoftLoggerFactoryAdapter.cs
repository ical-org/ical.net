//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

namespace Ical.Net.Tests.Logging.Adapters;

/// <summary>
/// Adapts an <see cref="Microsoft.Extensions.Logging.ILoggerFactory"/>
/// to the <see cref="Ical.Net.Logging.ILoggerFactory"/> interface.
/// </summary>
/// <remarks>This class provides a bridge between the <see cref="Microsoft.Extensions.Logging"/> framework and the
/// <see cref="Ical.Net.Logging.ILoggerFactory"/> abstraction. It allows the creation of
/// <see cref="Ical.Net.Logging.ILoggerFactory"/> instances using the underlying
/// <see cref="Microsoft.Extensions.Logging.ILoggerFactory"/>.
/// </remarks>
internal sealed class MicrosoftLoggerFactoryAdapter : Ical.Net.Logging.ILoggerFactory
{
    private readonly Microsoft.Extensions.Logging.ILoggerFactory _msLoggerFactory;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="MicrosoftLoggerFactoryAdapter"/> class,  using the specified
    /// Microsoft.Extensions.Logging <see cref="Microsoft.Extensions.Logging.ILoggerFactory"/>.
    /// </summary>
    /// <param name="msLoggerFactory">The <see cref="Microsoft.Extensions.Logging.ILoggerFactory"/> instance used to create loggers.
    /// This parameter cannot be <see langword="null"/>.
    /// </param>
    public MicrosoftLoggerFactoryAdapter(Microsoft.Extensions.Logging.ILoggerFactory msLoggerFactory)
    {
        _msLoggerFactory = msLoggerFactory;
    }

    /// <summary>
    /// Creates a logger instance for the specified category name.
    /// </summary>
    /// <param name="categoryName">The name of the category for the logger.</param>
    /// <returns>A <see cref="Ical.Net.Logging.ILogger"/> instance configured to log messages for the specified category.</returns>
    public Ical.Net.Logging.ILogger CreateLogger(string categoryName)
    {
        var msLogger = _msLoggerFactory.CreateLogger(categoryName);
        return new MicrosoftLoggerAdapter(msLogger);
    }

    private void Dispose(bool disposing)
    {
        if (_disposed) return;

        if (disposing)
            _msLoggerFactory.Dispose();
        _disposed = true;
    }

    /// <inheritdoc/>
    public void Dispose() => Dispose(true);
}
