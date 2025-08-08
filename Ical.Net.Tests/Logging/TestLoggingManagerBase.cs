// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.

#nullable enable
using System;
using System.Collections.Generic;
using Ical.Net.Tests.Logging.Abstractions;
using Serilog.Extensions.Logging;

namespace Ical.Net.Tests.Logging;

/// <summary>
/// Provides logging operations by configuring and providing a <see cref="Microsoft.Extensions.Logging.ILoggerFactory"/>.
/// </summary>
/// <remarks>
/// The <see cref="TestLoggingManagerBase"/> class initializes a logging configuration using
/// NLog and provides a logger factory that can be used to create loggers.
/// It supports custom logging rules and stores logs in memory for retrieval.
/// </remarks>
internal abstract class TestLoggingManagerBase : IDisposable
{
    private bool _isDisposed;
    private readonly InMemorySink? _inMemorySink;
    protected Options Options;

    /// <summary>
    /// Initializes a new instance of the <see cref="TestLoggingManagerBase"/> class with specified logging rules.
    /// </summary>
    /// <param name="options">
    /// The options for configuring the logging behavior.
    /// May be <see langword="null"/> to use default options.
    /// </param>
    protected TestLoggingManagerBase(Options? options)
    {
        Options = options ?? new Options();
        if (Options.LogToMemory)
            _inMemorySink = new InMemorySink
            {
                MaxLogsCount = Options.MaxLogsCount,
                OutputTemplate = Options.OutputTemplate
            };

        LoggerFactory = CreateFactory(Options);

        var testFactory = new TestLoggerFactory();
        testFactory.AddProvider(CreateConfiguredProvider(Options));
        TestFactory = testFactory;
    }

    /// <summary>
    /// Gets the <see cref="Ical.Net.Logging.ILoggerFactory"/>
    /// instance used for creating logger instances for use in Ical.Net.Logging.
    /// </summary>
    public Ical.Net.Logging.ILoggerFactory LoggerFactory { get; protected set; }

    /// <summary>
    /// Gets the <see cref="Microsoft.Extensions.Logging.ILoggerFactory"/>
    /// instance used for creating logger instances for unit tests and other purposes.
    /// </summary>
    public Microsoft.Extensions.Logging.ILoggerFactory TestFactory { get; protected set; }

    private Ical.Net.Logging.ILoggerFactory CreateFactory(Options options)
    {
        var serilogProvider = CreateConfiguredProvider(options);
        return SerilogAbstractions.CreateICalLoggingLoggerFactory(serilogProvider);
    }

    private SerilogLoggerProvider CreateConfiguredProvider(Options options)
        => SerilogAbstractions.CreateProvider(options, _inMemorySink);

    /// <summary>
    /// Gets a list of log entries when logging to memory is enabled.
    /// The number of returned logs is limited by <see cref="Options.MaxLogsCount"/>.
    /// When the limit is reached, the oldest logs are removed.
    /// </summary>
    public IEnumerable<string> Logs => _inMemorySink?.Logs ?? [];

    protected void Dispose(bool disposing)
    {
        if (_isDisposed) return;

        if (disposing)
        {
            LoggerFactory.Dispose();
            TestFactory.Dispose();
        }

        _isDisposed = true;
    }

    /// <inheritdoc/>
    public void Dispose() => Dispose(disposing: true);
}
