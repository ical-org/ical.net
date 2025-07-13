//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//
#nullable enable
using System;
using System.IO;
using System.Linq;
using Ical.Net.DataTypes;
using Ical.Net.Logging;
using Microsoft.Extensions.Logging;
using NUnit.Framework;


namespace Ical.Net.Tests.Logging;

internal class LoggingProviderUnitTests
{
    [TestCase(true)]
    [TestCase(false)]
    public void SubmittedLogMessages_ShouldBeInTheLogs(bool useFileTarget)
    {
        var logFile = useFileTarget
            ? Path.ChangeExtension(Path.GetTempFileName(), ".log")
            : null;

        // Set up the logging provider that will supply
        // ILoggerFactory and ILogger instances.
        using var logging = logFile is not null
            ? new TestLoggingProvider(logFile, new Options { DebugModeOnly = false }) // Use file target for logging
            : new TestLoggingProvider(new Options { DebugModeOnly = false }); // Use in-memory target for logging

        // Create loggers for Ical.Net from the registered logger factory.
        var genLogger = LoggingProvider.CreateLogger<LoggingProviderUnitTests>();
        var catLogger = LoggingProvider.CreateLogger(typeof(LoggingProviderUnitTests).FullName!);

        catLogger.LogInformation("This is an info message.");
        genLogger.LogError(new ArgumentException(), "This is an error message.");
        genLogger.LogWarning("This is a warning message with structured data. {Data}", new { Key1 = "Value1", Key2 = "Value2" });
        var logs = logging.Logs.ToList();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(logs, Has.Count.EqualTo(3));

            Assert.That(logs[0], Does.Contain("|INFO|"));
            Assert.That(logs[0], Does.Contain($"{typeof(LoggingProviderUnitTests).FullName}|This is an info message."));

            Assert.That(logs[1], Does.Contain(nameof(ArgumentException)));
            Assert.That(logs[1], Does.Contain($"|ERROR|{typeof(LoggingProviderUnitTests).FullName}|This is an error message."));

            Assert.That(logs[2], Does.Contain($"|WARN|{typeof(LoggingProviderUnitTests).FullName}|This is a warning message with structured data. {{ Key1 = Value1, Key2 = Value2 }}"));

            Assert.That(LoggingProvider.LoggerFactory, Is.InstanceOf<ILoggerFactory>());
        }
    }

    [TestCase(true)]
    [TestCase(false)]
    public void OnlyLatestMaxLogsCount_ShouldBeReturned(bool useFileTarget)
    {
        var logFile = useFileTarget
            ? Path.ChangeExtension(Path.GetTempFileName(), ".log")
            : null;

        // Set up the logging provider that will supply
        // ILoggerFactory and ILogger instances.
        var options = new Options { MaxLogsCount = 15, DebugModeOnly = false};
        using var logging = logFile is not null
            ? new TestLoggingProvider(logFile, options) // Use file target for logging
            : new TestLoggingProvider(options); // Use in-memory target for logging

        var logger = LoggingProvider.CreateLogger<LoggingProviderUnitTests>();

        for (var i = 0; i < options.MaxLogsCount + 10; i++)
            logger.LogInformation("##{Counter}##", i);

        var logs = logging.Logs.ToList();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(logs, Has.Count.EqualTo(options.MaxLogsCount));
            // Assert that the last 10 logs exceeding MaxLogsCount exist in the logs list
            for (var i = options.MaxLogsCount; i < options.MaxLogsCount + 10; i++)
            {
                var expected = $"##{i}##";
                Assert.That(logs.Any(log => log.Contains(expected)), Is.True, $"Log for counter {i} not found in logs.");
            }
        }
    }

    [Test]
    public void MissingFileTargetToReadFrom_ShouldThrow()
    {
        var logFile = Path.ChangeExtension(Path.GetTempFileName(), ".log");

        using var logging = new TestLoggingProvider(logFile, new Options { DebugModeOnly = false });
        // Nothing will be logged, so the file should not exist.

        // ReSharper disable once AccessToDisposedClosure
        Assert.That(() => { _ = logging.Logs.ToList(); }, Throws.TypeOf<FileNotFoundException>());
    }

    [Test]
    public void DisposingProviderTwice_ShouldNotThrow()
    {
        var logging = new TestLoggingProvider();
        
        Assert.That(() => { logging.Dispose(); logging.Dispose(); }, Throws.Nothing);
    }

    [TestCase(true)]
    [TestCase(false)]
    public void AllLogLevels_CanBeUsedAsFilters(bool unrecognizedPattern)
    {
        var pattern = unrecognizedPattern
            ? $"{typeof(LoggingProviderUnitTests).FullName}"
            : "UnrecognizedPattern";

        // Create filters for each LogLevel
        var allLevels = Enum.GetValues(typeof(LogLevel)).Cast<LogLevel>()
            .Where(l => l != LogLevel.None).ToArray();

        var options = new Options
        {
            DebugModeOnly = false,
            Filters = allLevels.Select(l => new Filter
            {
                MinLogLevel = l,
                MaxLogLevel = l,
                LoggerNamePattern = pattern
            }).ToList()
        };

        // Use in-memory logging provider with options
        using var logging = new TestLoggingProvider(options);

        var logger = LoggingProvider.CreateLogger<LoggingProviderUnitTests>();

        // Act: log one message for each level
        foreach (var level in allLevels)
            switch (level)
            {
                case LogLevel.Trace:
                    logger.LogTrace("Trace message");
                    break;
                case LogLevel.Debug:
                    logger.LogDebug("Debug message");
                    break;
                case LogLevel.Information:
                    logger.LogInformation("Info message");
                    break;
                case LogLevel.Warning:
                    logger.LogWarning("Warning message");
                    break;
                case LogLevel.Error:
                    logger.LogError("Error message");
                    break;
                case LogLevel.Critical:
                    logger.LogCritical("Critical message");
                    break;
            }

        var logs = logging.Logs.ToList();
        Assert.That(logs, unrecognizedPattern ? Has.Count.EqualTo(6) : Has.Count.EqualTo(0));
    }

    [Test]
    public void UsingFilters_ShouldExcludeLogMessages()
    {
        var options = new Options
        {
            DebugModeOnly = false,
            Filters =
            [
                new Filter
                {
                    MinLogLevel = LogLevel.Error,
                    MaxLogLevel = LogLevel.Error,
                    LoggerNamePattern = $"{typeof(LoggingProviderUnitTests).FullName}"
                }
            ]
        };

        // Use in-memory logging provider with options
        using var logging = new TestLoggingProvider(options);

        var logger = LoggingProvider.CreateLogger<LoggingProviderUnitTests>();
        logger.LogWarning("Warning message"); // Should be excluded
        logger.LogError("Error message");

        var catLogger = LoggingProvider.CreateLogger("SomeCategory");
        catLogger.LogError("Error message"); // Should be excluded

        var logs = logging.Logs.ToList();

        Assert.That(logs, Has.Count.EqualTo(1));
    }

    [Test]
    public void LoggingForDebugModeOnly_ShouldLog()
    {
        var options = new Options
        {
            DebugModeOnly = true,
        };

        // Use in-memory logging provider with options
        using var logging = new TestLoggingProvider(options);

        var logger = LoggingProvider.CreateLogger<LoggingProviderUnitTests>();
        logger.LogError("Error message");
        var logs = logging.Logs.ToList();

        Assert.That(logs, System.Diagnostics.Debugger.IsAttached
            ? Has.Count.EqualTo(1)
            : Has.Count.EqualTo(0));
    }

    [Test]
    public void DemoForOccurrences()
    {
        // Set up the logging provider
        // By default, logging will only be active in debug builds.
        using var logging = new TestLoggingProvider(new Options { DebugModeOnly = false });
        var logger = LoggingProvider.CreateLogger("Occurrences");

        // Test
        var iCal = Calendar.Load(IcsFiles.YearlyComplex1)!;
        var evt = iCal.Events.First();
        var occurrences = evt.GetOccurrences(
                new CalDateTime(2025, 1, 1))
            .TakeWhileBefore(new CalDateTime(2027, 1, 1));

        // Log, using the extension methods for logging
        logger.LogTrace("{Occurrences}", occurrences.ToLog());

        // Analyze the logs
        var logs = logging.Logs.ToList();
        /*
           2025-07-31 11:00:53.7022|TRACE|Occurrences|Occurrences:
           Start: 01/05/2025 08:30:00 -05:00 US-Eastern Period: PT1H End: 01/05/2025 09:30:00 -05:00 US-Eastern
           Start: 01/05/2025 09:30:00 -05:00 US-Eastern Period: PT1H End: 01/05/2025 10:30:00 -05:00 US-Eastern
           Start: 01/12/2025 08:30:00 -05:00 US-Eastern Period: PT1H End: 01/12/2025 09:30:00 -05:00 US-Eastern
           Start: 01/12/2025 09:30:00 -05:00 US-Eastern Period: PT1H End: 01/12/2025 10:30:00 -05:00 US-Eastern
           Start: 01/19/2025 08:30:00 -05:00 US-Eastern Period: PT1H End: 01/19/2025 09:30:00 -05:00 US-Eastern
           Start: 01/19/2025 09:30:00 -05:00 US-Eastern Period: PT1H End: 01/19/2025 10:30:00 -05:00 US-Eastern
           Start: 01/26/2025 08:30:00 -05:00 US-Eastern Period: PT1H End: 01/26/2025 09:30:00 -05:00 US-Eastern
           Start: 01/26/2025 09:30:00 -05:00 US-Eastern Period: PT1H End: 01/26/2025 10:30:00 -05:00 US-Eastern
         */
        Assert.That(logs, Has.Count.EqualTo(1));
        Assert.That(logs[0], Does.Contain("Occurrences"));
    }
}
