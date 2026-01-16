//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using Microsoft.Extensions.Logging;

namespace Ical.Net.Tests.Logging.Abstractions;

/// <summary>
/// Provides an <see cref="ILoggerFactory"/> implementation for testing scenarios that supports only <see
/// cref="Serilog.Extensions.Logging.SerilogLoggerProvider"/>.
/// </summary>
/// <remarks><para> <b>TestLoggerFactory</b> is intended for use in unit tests 
/// where logging is required</para>
/// <para> Only <see cref="Serilog.Extensions.Logging.SerilogLoggerProvider"/> instances can be added via <see cref="AddProvider"/>.
/// Attempting to add any other provider type will result in an exception.</para>
/// </remarks>
internal sealed class TestLoggerFactory : ILoggerFactory
{
    private Serilog.Extensions.Logging.SerilogLoggerProvider _provider = null!;

    /// <summary>
    /// Adds a logging provider to the logger configuration.
    /// </summary>
    /// <param name="provider">The logging provider to add. Must be an instance of <see
    /// cref="Serilog.Extensions.Logging.SerilogLoggerProvider"/>.</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="provider"/> is not of type <see
    /// cref="Serilog.Extensions.Logging.SerilogLoggerProvider"/>.</exception>
    public void AddProvider(ILoggerProvider provider)
    {
        if (provider is not Serilog.Extensions.Logging.SerilogLoggerProvider p)
            throw new ArgumentException("Provider must be of type SerilogLoggerProvider", nameof(provider));
        
        _provider = p;
    }

    /// <inheritdoc/>
    public ILogger CreateLogger(string categoryName) => _provider.CreateLogger(categoryName);

    /// <inheritdoc/>
    public void Dispose() => _provider.Dispose();
}
