//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.Linq;
using System.Threading;
using Ical.Net.Logging;
using Ical.Net.Logging.Internal;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace Ical.Net.Tests.Logging.Tests;

/// <summary>
/// LoggingProvider.SetLoggerFactory is static and gets
/// <list type="bullet">
/// <item><description>reset before each test</description></item>
/// <item><description>set in each test for different test scenarios that can't run in parallel</description></item>
/// </list>
/// </summary>
[Parallelizable(ParallelScope.None)]
[TestFixture]
internal class LoggingProviderTests
{
    [SetUp]
    public void Setup()
    {
        // Reset the LoggingProvider before each test
        LoggingProvider.SetLoggerFactory(null, force: true);
    }

    [Test]
    public void Loggers_FromTestFactory_AndLoggingProvider_ShouldLogToSameOutput()
    {
        // Options determine the configuration of the logging provider
        // the factories and the loggers created from them.
        var options = new Options { DebugModeOnly = false, LogToMemory = true, LogToConsole = true };
        using var mgr = new TestLoggingManager(options);
        // Set the iCal.NET logging configuration
        // Throws an exception if the factory is already set, unless force is true.
        LoggingProvider.SetLoggerFactory(mgr.LoggerFactory, force: false);

        // Create a logger using the TestFactory (can create multiple types of loggers)
        var testLogger = mgr.TestFactory.CreateLogger(nameof(mgr.TestFactory));
        testLogger.LogInformation("TestFactory logger");

        // Create a logger for the LoggingProvider
        // (it has a fixed ILoggerFactory, and creates 1 type of logger that cannot change)
        var providerLogger = LoggingProvider.CreateLogger(nameof(LoggingProvider));
        providerLogger.LogInformation("Provider logger");

        var logs = mgr.Logs.ToList();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(logs, Has.Count.EqualTo(2));
            Assert.That(logs[0], Does.Contain("TestFactory logger"));
            Assert.That(logs[1], Does.Contain("Provider logger"));
            Assert.That(mgr.TestFactory, Is.InstanceOf<Abstractions.TestLoggerFactory>());
        }
    }

    [Test]
    public void InitializingLoggerFactoryTwice_ShouldThrow()
    {
        using var mgr1 = new TestLoggingManager(new Options { DebugModeOnly = false });
        LoggingProvider.SetLoggerFactory(mgr1.LoggerFactory, force: false);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(LoggingProvider.FactoryIsSet, Is.True);
            Assert.That(() =>
            {
                using var mgr2 = new TestLoggingManager(new Options { DebugModeOnly = false });
                LoggingProvider.SetLoggerFactory(mgr2.LoggerFactory, force: false);
            }, Throws.InvalidOperationException);
        }
    }

    [Test]
    public void LoggerFactoryNotSet_ShouldFallBackToNullLoggerFactory()
    {
        LoggingProvider.SetLoggerFactory(null, true);
        var logger = LoggingProvider.CreateLogger<LoggingProviderTests>();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(LoggingProvider.LoggerFactory, Is.InstanceOf<NullLoggerFactory>());
            Assert.That(logger, Is.InstanceOf<Net.Logging.Internal.Logger<LoggingProviderTests>>());
            // All log levels should be disabled for NullLoggers
            Assert.That(logger.IsEnabled(Net.Logging.LogLevel.Trace), Is.False);
            Assert.That(logger.IsEnabled(Net.Logging.LogLevel.Critical), Is.False);
            Assert.That(() => LoggingProvider.LoggerFactory.Dispose(), Throws.Nothing);
        }
    }

    [Test]
    public void SubmittedLogMessages_ShouldBeInTheLogs()
    {
        // Set up the logging provider that will supply
        // ILoggerFactory and ILogger instances.
        using var mgr = new TestLoggingManager(new Options { DebugModeOnly = false });
        LoggingProvider.SetLoggerFactory(mgr.LoggerFactory, force: false);

        // Create loggers for Ical.Net from the registered logger factory.
        var genLogger = LoggingProvider.CreateLogger<LoggingProviderTests>();
        var catLogger = LoggingProvider.CreateLogger(typeof(LoggingProviderTests).FullName!);

        catLogger.LogInformation("Info Message");
        genLogger.LogError(new ArgumentException("Test Exception"),"Error Message");
        genLogger.LogWarning("Warning Message. {Data}", new { Key1 = "Value1", Key2 = "Value2" });
        genLogger.Log(Net.Logging.LogLevel.None, "Should not log");
        var logs = mgr.Logs.ToList();

        // Information, Error, and Warning are logged, None is not.
        Assert.That(logs, Has.Count.EqualTo(3));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(logs[0], Does.Contain($"{typeof(LoggingProviderTests).FullName}|Info Message"));

            Assert.That(logs[1], Does.Contain(nameof(ArgumentException)));
            Assert.That(logs[1], Does.Contain($"{typeof(LoggingProviderTests).FullName}|Error Message"));

            Assert.That(logs[2], Does.Contain($"{typeof(LoggingProviderTests).FullName}|Warning Message. {{ Key1 = Value1, Key2 = Value2 }}"));

            Assert.That(LoggingProvider.LoggerFactory, Is.InstanceOf<Net.Logging.ILoggerFactory>());
        }
    }

    [Test]
    public void OnlyLatestMaxLogsCount_ShouldBeReturned()
    {
        // Set up the logging provider that will supply
        // ILoggerFactory and ILogger instances.
        var options = new Options { MaxLogsCount = 15, DebugModeOnly = false};
        using var mgr = new TestLoggingManager(options);
        LoggingProvider.SetLoggerFactory(mgr.LoggerFactory, force: false);

        var logger = LoggingProvider.CreateLogger<LoggingProviderTests>();

        for (var i = 0; i < options.MaxLogsCount + 10; i++)
            logger.LogInformation("##{Counter}##", i);

        var logs = mgr.Logs.ToList();

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
    public void DebugModeOnly_IsTrue_ShouldOnlyLogWhenDebugging()
    {
        using var mgr = new TestLoggingManager(new Options { DebugModeOnly = true });
        LoggingProvider.SetLoggerFactory(mgr.LoggerFactory, force: false);

        var logger = LoggingProvider.CreateLogger<LoggingProviderTests>();
        logger.LogInformation("##{Counter}##", 123);

        var logs = mgr.Logs.ToList();

        if (System.Diagnostics.Debugger.IsAttached)
            Assert.That(logs, Has.Count.EqualTo(1));
        else
            // If not in debug mode, no logs should be present
            Assert.That(logs, Is.Empty);
    }

    [Test]
    public void DisposingProviderTwice_ShouldNotThrow()
    {
        var logging = new TestLoggingManager();
        
        Assert.That(() => { logging.Dispose(); logging.Dispose(); }, Throws.Nothing);
    }

    [TestCase(true)]
    [TestCase(false)]
    public void LogLevelsAndCategories_CanBeUsedAsFilters(bool isExcludedPattern)
    {
        var pattern = isExcludedPattern
            ? "ExcludedPattern"
            : "IncludedPattern";

        var options = new Options
        {
            LogToFileLogPath = null,
            DebugModeOnly = false,
            MinLogLevel = isExcludedPattern
                ? Microsoft.Extensions.Logging.LogLevel.Trace
                : Microsoft.Extensions.Logging.LogLevel.Critical,
            Filters =
            [
                // Override the default minimum log level
                new Filter(isExcludedPattern
                        ? Microsoft.Extensions.Logging.LogLevel.None
                        : Microsoft.Extensions.Logging.LogLevel.Trace
                    , pattern)
            ]
        };

        using var mgr = new TestLoggingManager(options);
        LoggingProvider.SetLoggerFactory(mgr.LoggerFactory, force: false);

        var logger = LoggingProvider.CreateLogger(pattern);

        var exception = new InvalidOperationException("Test exception");

        logger.LogTrace("Trace message");
        logger.LogTrace(exception, string.Empty);

        logger.LogDebug("Debug message");
        logger.LogDebug(exception, string.Empty);

        logger.LogInformation("Info message");
        logger.LogInformation(exception, string.Empty);

        logger.LogWarning("Warning message");
        logger.LogWarning(exception, string.Empty);

        logger.LogError("Error message");
        logger.LogError(exception, string.Empty);

        logger.LogCritical("Critical message");
        logger.LogCritical(exception, string.Empty);

        var logs = mgr.Logs.ToList();
        Assert.That(logs, isExcludedPattern ? Has.Count.EqualTo(0) : Has.Count.EqualTo(12));
    }

    [Test]
    public void UsingFilters_ShouldExcludeLogMessages()
    {
        const string excludedCategory = "ExcludedCategory";
        var options = new Options
        {
            DebugModeOnly = false,
            Filters =
            [
                new Filter(Microsoft.Extensions.Logging.LogLevel.Error, $"{typeof(LoggingProviderTests).FullName}"),
                new Filter(Microsoft.Extensions.Logging.LogLevel.None, excludedCategory)
            ]
        };

        using var mgr = new TestLoggingManager(options);
        LoggingProvider.SetLoggerFactory(mgr.LoggerFactory, force: false);

        var logger = LoggingProvider.CreateLogger<LoggingProviderTests>();
        logger.LogWarning("Warning message"); // Should be excluded
        logger.LogError("Error message");

        var catLogger = LoggingProvider.CreateLogger(excludedCategory);
        catLogger.LogError("Error message"); // Should be excluded

        var logs = mgr.Logs.ToList();

        Assert.That(logs, Has.Count.EqualTo(1));
    }

    [Test]
    public void LoggingForDebugModeOnly_ShouldLog()
    {
        var options = new Options
        {
            LogToMemory = true,
            DebugModeOnly = true,
            MinLogLevel = Microsoft.Extensions.Logging.LogLevel.Error
        };

        using var mgr = new TestLoggingManager(options);
        LoggingProvider.SetLoggerFactory(mgr.LoggerFactory, force: false);

        var logger = LoggingProvider.CreateLogger<LoggingProviderTests>();
        logger.LogError("Error message");
        var logs = mgr.Logs.ToList();

        Assert.That(logs, System.Diagnostics.Debugger.IsAttached
            ? Has.Count.EqualTo(1)
            : Has.Count.EqualTo(0));
    }

    [Test]
    public void ConcurrentLogging_ShouldLogAllMessagesCorrectly()
    {
        // Set up in-memory logging with (using 1 ILoggerFactory)
        using var mgr = new TestLoggingManager(new Options { DebugModeOnly = false });
        LoggingProvider.SetLoggerFactory(mgr.LoggerFactory, force: false);

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

        var logs = mgr.Logs.ToList();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(logs, Has.Count.EqualTo(threadCount * logsPerThread));
            Assert.That(maxParallelThreads, Is.GreaterThan(1), "Threads did not run in parallel");
            for (var t = 0; t < threadCount; t++)
                for (var i = 0; i < logsPerThread; i++)
                {
                    var expected = $"Log {i:D3} - Thread {t:D2}";
                    Assert.That(logs.Any(log => log.Contains(expected)), Is.True, $"Expected log message '{expected}' not found");
                }
        }

        return;

        // Local functions

        void LogThread(int threadNo)
        {
            var current = Interlocked.Increment(ref runningThreads);
            UpdateMaxParallelThreads(current);

            var logger = LoggingProvider.CreateLogger<LoggingProviderTests>();
            for (var i = 0; i < logsPerThread; i++)
                logger.LogInformation("Log {Log:D3} - Thread {Thread:D2}", i, threadNo);
            Interlocked.Decrement(ref runningThreads);
        }

        void UpdateMaxParallelThreads(int current)
        {
            do
            {
                var prevMax = maxParallelThreads;
                if (current > prevMax)
                    Interlocked.CompareExchange(ref maxParallelThreads, current, prevMax);
            } while (current > maxParallelThreads);
        }
    }
}
