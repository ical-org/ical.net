// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.

#nullable enable
using System;
using Ical.Net.Logging;
using Ical.Net.Tests.Logging.Adapters;
using Microsoft.Extensions.Logging;
using NLog.Config;
using NLog.Extensions.Logging;
using NLog.Targets;

namespace Ical.Net.Tests.Logging;

/// <summary>
/// Provides logging operations by configuring and providing a <see cref="Microsoft.Testing.Platform.Logging.ILoggerFactory"/>
/// and a <see cref="NLog.Targets.Target"/> for log storage.
/// </summary>
/// <remarks>
/// The <see cref="TestLoggingProviderBase"/> class initializes a logging configuration using
/// NLog and provides a logger factory that can be used to create loggers.
/// It supports custom logging rules and stores logs in memory for retrieval.
/// </remarks>
internal abstract class TestLoggingProviderBase : IDisposable
{
    private bool _isDisposed;
    protected Options Options;

    /// <summary>
    /// Initializes a new instance of the <see cref="TestLoggingProviderBase"/> class with specified logging rules.
    /// </summary>
    /// <param name="target">
    /// The target where logs will be written. This can be a memory target or any other NLog target.
    /// </param>
    /// <param name="options">
    /// The options for configuring the logging behavior.
    /// May be <see langword="null"/> to use default options.
    /// </param>
    protected TestLoggingProviderBase(Target target, Options? options)
    {
        Options = options ?? new Options();

        Target = target;
        if (Options.DebugModeOnly && !System.Diagnostics.Debugger.IsAttached)
        {
            Target = new NullTarget("null");
        }
        var config = Configure(Target, Options);
        LoggerFactory = CreateFactory(config);

        // Set the iCal.NET logging configuration
        // Don't force the logger factory to be set if it already exists!
        LoggingProvider.SetLoggerFactory(LoggerFactory);
    }

    /// <summary>
    /// Gets the logging target (<see cref="MemoryTarget"/> or <see cref="FileTarget"/>) where logs are stored.
    /// </summary>
    protected internal Target Target { get; }

    /// <summary>
    /// Gets the <see cref="Microsoft.Extensions.Logging.ILoggerFactory"/> instance used for creating logger instances.
    /// </summary>
    public Ical.Net.Logging.ILoggerFactory LoggerFactory { get; protected set; }

    private static Ical.Net.Logging.ILoggerFactory CreateFactory(LoggingConfiguration config)
    {
        // Create a LoggerFactory with NLog as the provider
        var msLoggerFactory = Microsoft.Extensions.Logging.LoggerFactory.Create(builder =>
        {
            builder.ClearProviders();
            builder.AddNLog(config);
        });

        // Wrap the Microsoft logger factory with the Ical adapter
        return new MicrosoftLoggerFactoryAdapter(msLoggerFactory);
    }

    private static LoggingConfiguration Configure(Target target, Options options)
    {
        var config = new LoggingConfiguration();
        config.AddTarget(target);

        if (options.Filters.Count == 0)
        {
            // Default rule for all log levels and all loggers
            config.AddRuleForAllLevels(target);
        }
        else
        {
            // Add rules based on the provided collection
            foreach (var filter in options.Filters)
            {
                config.AddRule(MapLogLevel(filter.MinLogLevel), MapLogLevel(filter.MaxLogLevel),
                    target, filter.LoggerNamePattern);
            }
        }

        return config;
    }

    private void Dispose(bool disposing)
    {
        if (_isDisposed) return;

        if (disposing)
        {
            Target.Dispose();
            LoggerFactory.Dispose();
            NLog.LogManager.Configuration = null;
            LoggingProvider.SetLoggerFactory(null, true);
        }

        _isDisposed = true;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc/>
    ~TestLoggingProviderBase() => Dispose(disposing: false);
    
    private static NLog.LogLevel MapLogLevel(Microsoft.Extensions.Logging.LogLevel logLevel)
    {
#pragma warning disable CS8509 // The switch expression does not handle all possible values of its input type (it is not exhaustive).
        return logLevel switch
        {
            Microsoft.Extensions.Logging.LogLevel.Trace => NLog.LogLevel.Trace,
            Microsoft.Extensions.Logging.LogLevel.Debug => NLog.LogLevel.Debug,
            Microsoft.Extensions.Logging.LogLevel.Information => NLog.LogLevel.Info,
            Microsoft.Extensions.Logging.LogLevel.Warning => NLog.LogLevel.Warn,
            Microsoft.Extensions.Logging.LogLevel.Error => NLog.LogLevel.Error,
            Microsoft.Extensions.Logging.LogLevel.Critical => NLog.LogLevel.Fatal,
        };
#pragma warning restore CS8509 // The switch expression does not handle all possible values of its input type (it is not exhaustive).
    }
}
