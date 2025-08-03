//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//
#nullable enable
using System;
using System.IO;
using System.Linq;
using System.Threading;
using Ical.Net.DataTypes;
using Ical.Net.Logging;
using Ical.Net.Logging.Internal;
using NUnit.Framework;

namespace Ical.Net.Tests.Logging;

// LoggingProvider.SetLoggerFactory is static and gets set in each test,
// so we need to ensure that the tests are not run in parallel.
[Parallelizable(ParallelScope.None)]
internal class LoggingProviderUnitTests
{
    [Test]
    public void InitializingLoggerFactoryTwice_ShouldThrow()
    {
        using var logging1 = new TestLoggingProvider(new Options { DebugModeOnly = false });

        using (Assert.EnterMultipleScope())
        {
            Assert.That(LoggingProvider.FactoryIsSet, Is.True);
            Assert.That(() =>
            {
                using var logging2 = new TestLoggingProvider(new Options { DebugModeOnly = false });
            }, Throws.InvalidOperationException);
        }
    }

    [Test]
    public void LoggerFactoryNotSet_ShouldFallBackToNullLoggerFactory()
    {
        using var logging1 = new TestLoggingProvider(new Options { DebugModeOnly = false });
        LoggingProvider.SetLoggerFactory(null, true);
        var logger = LoggingProvider.CreateLogger<LoggingProviderUnitTests>();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(LoggingProvider.LoggerFactory, Is.InstanceOf<NullLoggerFactory>());
            Assert.That(logger, Is.InstanceOf<Logger<LoggingProviderUnitTests>>());
            Assert.That(logger.IsEnabled(LogLevel.Information), Is.False);
            Assert.That(() => LoggingProvider.LoggerFactory.Dispose(), Throws.Nothing);
        }
    }

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
        genLogger.LogError(new ArgumentException(),"This is an error message.");
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

    [TestCase(true)]
    [TestCase(false)]
    public void DebugModeOnly_IsTrue_ShouldOnlyLogWhenDebugging(bool useFileTarget)
    {
        var logFile = useFileTarget
            ? Path.ChangeExtension(Path.GetTempFileName(), ".log")
            : null;

        using var logging = logFile is not null
            ? new TestLoggingProvider(logFile) // Use file target for logging
            : new TestLoggingProvider(); // Use in-memory target for logging

        var logger = LoggingProvider.CreateLogger<LoggingProviderUnitTests>();
        logger.LogInformation("##{Counter}##", 123);

        var logs = logging.Logs.ToList();

        if (System.Diagnostics.Debugger.IsAttached)
        {
            Assert.That(logs, Has.Count.EqualTo(1));
        }
        else
        {
            // If not in debug mode, no logs should be present
            Assert.That(logs, Is.Empty);
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
                MinLogLevel = (Microsoft.Extensions.Logging.LogLevel) l,
                MaxLogLevel = (Microsoft.Extensions.Logging.LogLevel) l,
                LoggerNamePattern = pattern
            }).ToList()
        };

        // Use in-memory logging provider with options
        using var logging = new TestLoggingProvider(options);

        var logger = LoggingProvider.CreateLogger<LoggingProviderUnitTests>();

        // Act: log one message for each level
        var exception = new InvalidOperationException("Test exception");
        foreach (var level in allLevels)
            switch (level)
            {
                case LogLevel.Trace:
                    logger.LogTrace("Trace message");
                    logger.LogTrace(exception, string.Empty);
                    break;
                case LogLevel.Debug:
                    logger.LogDebug("Debug message");
                    logger.LogDebug(exception, string.Empty);
                    break;
                case LogLevel.Information:
                    logger.LogInformation("Info message");
                    logger.LogInformation(exception, string.Empty);
                    break;
                case LogLevel.Warning:
                    logger.LogWarning("Warning message");
                    logger.LogWarning(exception, string.Empty);
                    break;
                case LogLevel.Error:
                    logger.LogError("Error message");
                    logger.LogError(exception, string.Empty);
                    break;
                case LogLevel.Critical:
                    logger.LogCritical("Critical message");
                    logger.LogCritical(exception, string.Empty);
                    break;
            }

        var logs = logging.Logs.ToList();
        Assert.That(logs, unrecognizedPattern ? Has.Count.EqualTo(12) : Has.Count.EqualTo(0));
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
                    MinLogLevel = (Microsoft.Extensions.Logging.LogLevel) LogLevel.Error,
                    MaxLogLevel = (Microsoft.Extensions.Logging.LogLevel) LogLevel.Error,
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
    public void ConcurrentLogging_ShouldLogAllMessagesCorrectly()
    {
        // Set up in-memory logging with (using 1 ILoggerFactory)
        using var logging = new TestLoggingProvider(new Options { DebugModeOnly = false });

        const int threadCount = 10;
        const int logsPerThread = 20;
        var runningThreads = 0;
        var maxParallelThreads = 0;

        // Create and start multiple threads to log messages concurrently

        var threads = Enumerable.Range(0, threadCount)
            .Select(t => new Thread(() => LogThread(t)))
            .ToArray();

        foreach (var thread in threads)
            thread.Start();
        foreach (var thread in threads)
            thread.Join();

        var logs = logging.Logs.ToList();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(logs, Has.Count.EqualTo(threadCount * logsPerThread));
            Assert.That(maxParallelThreads, Is.GreaterThan(1), "Threads did not run in parallel");
            for (var t = 0; t < threadCount; t++)
            {
                for (var i = 0; i < logsPerThread; i++)
                {
                    var expected = $"Log {i:D3} - Thread {t:D2}";
                    Assert.That(logs.Any(log => log.Contains(expected)), Is.True, $"Expected log message '{expected}' not found");
                }
            }
        }

        return;

        // Local functions

        void LogThread(int threadNo)
        {
            var current = Interlocked.Increment(ref runningThreads);
            UpdateMaxParallelThreads(current);

            var logger = LoggingProvider.CreateLogger<LoggingProviderUnitTests>();
            for (var i = 0; i < logsPerThread; i++)
            {
                logger.LogInformation("Log {Log:D3} - Thread {Thread:D2}", i, threadNo);
            }
            Interlocked.Decrement(ref runningThreads);
        }

        void UpdateMaxParallelThreads(int current)
        {
            do
            {
                var prevMax = maxParallelThreads;
                if (current > prevMax)
                {
                    Interlocked.CompareExchange(ref maxParallelThreads, current, prevMax);
                }
            } while (current > maxParallelThreads);
        }
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
