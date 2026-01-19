//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System.Globalization;
using Ical.Net.Tests.Logging.Adapters;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Extensions.Logging;

namespace Ical.Net.Tests.Logging.Abstractions;

internal class SerilogAbstractions
{
    public static Serilog.Events.LogEventLevel MapLogLevel(LogLevel logLevel)
    {
        return logLevel switch
        {
            LogLevel.Trace => Serilog.Events.LogEventLevel.Verbose,
            LogLevel.Debug => Serilog.Events.LogEventLevel.Debug,
            LogLevel.Information => Serilog.Events.LogEventLevel.Information,
            LogLevel.Warning => Serilog.Events.LogEventLevel.Warning,
            LogLevel.Error => Serilog.Events.LogEventLevel.Error,
            LogLevel.Critical => Serilog.Events.LogEventLevel.Fatal,
            // Workaround for LogLevel.None that is not directly supported by Serilog.
            LogLevel.None => Serilog.Events.LogEventLevel.Fatal + 1,
            _ => Serilog.Events.LogEventLevel.Fatal + 1
        };
    }

    /// <summary>
    /// Creates an <see cref="Ical.Net.Logging.ILoggerFactory"/> instance
    /// using Serilog as the logging provider.
    /// The factory is backed by the <see cref="MicrosoftLoggerFactoryAdapter"/>.
    /// </summary>
    /// <param name="serilogProvider">The provider used to create the logger factory.</param>
    /// <returns>An <see cref="Ical.Net.Logging.ILoggerFactory"/></returns>
    public static Ical.Net.Logging.ILoggerFactory CreateICalLoggingLoggerFactory(SerilogLoggerProvider serilogProvider)
    {
        // The logger factory is implementing Microsoft.Extensions.Logging.ILoggerFactory
        var msLoggerFactory = new TestLoggerFactory();
        msLoggerFactory.AddProvider(serilogProvider);

        // Wrap the Microsoft logger factory with the ICal adapter
        return new MicrosoftLoggerFactoryAdapter(msLoggerFactory);
    }

    public static SerilogLoggerProvider CreateProvider(Options options, InMemorySink? inMemorySink)
    {
        // Create a Serilog logger used by the provider to pipe events through.
        // Without a custom logger, the provider uses the Serilog.Log class.

        // The logger can be used to create sub-loggers for specific categories.
        var standardLogger = CreateLogger(options, inMemorySink);

        // Create a SerilogLoggerProvider that provides loggers.
        // SerilogLoggerProvider.CreateLogger(...) returns a Microsoft.Extensions.Logging.ILogger
        var serilogProvider = new SerilogLoggerProvider(standardLogger);

        return serilogProvider;
    }
    
    public static Serilog.ILogger CreateLogger(Options options, InMemorySink? inMemorySink)
    {
        if (options.DebugModeOnly && !System.Diagnostics.Debugger.IsAttached)
        {
            return Serilog.Core.Logger.None;
        }

        // For thread isolation, create separate Logger instances (not just providers)
        // and avoid using the static Log.Logger.
        // If Log.Logger is set in one thread, it affects all threads because it's static.
        var loggerConfiguration = new LoggerConfiguration();
        if (options.LogToFileLogPath != null)
        {
            loggerConfiguration.WriteTo.File(
                // path gets timestamped when other than RollingInterval.Infinite is used
                path: options.LogToFileLogPath,
                // ignored when using RollingInterval.Infinite
                retainedFileCountLimit: 50,
                outputTemplate: options.OutputTemplate,
                rollingInterval: RollingInterval.Minute,
                formatProvider: CultureInfo.InvariantCulture);
        }

        loggerConfiguration.MinimumLevel.Is(MapLogLevel(options.MinLogLevel));
        
        foreach (var filter in options.Filters)
        {
            var logEventLevel = MapLogLevel(filter.MinLogLevel);
            loggerConfiguration.MinimumLevel.Override(filter.LoggerNamePattern, logEventLevel);
        }

        if (options.LogToConsole)
        {
            loggerConfiguration
                .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture, outputTemplate: options.OutputTemplate);
        }

        if (options.LogToMemory && inMemorySink != null)
        {
            loggerConfiguration.WriteTo.Sink(inMemorySink);
        }

        return loggerConfiguration.CreateLogger();
    }
}
