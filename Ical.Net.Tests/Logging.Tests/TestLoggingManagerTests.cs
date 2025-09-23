//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//
#nullable enable
using System;
using System.Linq;
using Ical.Net.DataTypes;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace Ical.Net.Tests.Logging.Tests;

[TestFixture]
internal class TestLoggingManagerTests
{
    [Test]
    public void LogLevelsAndCategories_CanBeUsedAsFilters()
    {
        var pattern = nameof(LogLevelsAndCategories_CanBeUsedAsFilters);

        var options = new Options
        {
            LogToFileLogPath = null,
            LogToMemory = true,
            LogToConsole = false,
            DebugModeOnly = false,
            MinLogLevel = LogLevel.Trace
        };

        using var mgr = new TestLoggingManager(options);

        var logger = mgr.TestFactory.CreateLogger(pattern);

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

        logger.Log(LogLevel.None, "No logging");
        logger.Log(LogLevel.None, exception, string.Empty);

        var logs = mgr.Logs.ToList();
        Assert.That(logs, Has.Count.EqualTo(12));
    }

    [Test]
    public void DemoForOccurrences()
    {
        using var mgr = new TestLoggingManager(
            new Options { DebugModeOnly = false,
                MinLogLevel = LogLevel.Trace,
                LogToMemory = true,
                LogToConsole = true
            });

        var logger = mgr.TestFactory.CreateLogger("Occurrences");

        // Test
        var iCal = Calendar.Load(IcsFiles.YearlyComplex1)!;
        var evt = iCal.Events.First();
        var occurrences = evt.GetOccurrences(
                new CalDateTime(2025, 1, 1))
            .TakeWhileBefore(new CalDateTime(2027, 1, 1));

        // Log, using the extension methods for logging
        logger.LogTrace("{Occurrences}", occurrences.ToLog());

        // Analyze the logs
        var logs = mgr.Logs.ToList();
        /*
           2025-07-31 11:00:53.7022|TRACE|Occurrences|Occurrences:
           ...
         */
        Assert.That(logs, Has.Count.EqualTo(1));
        Assert.That(logs[0], Does.Contain("Occurrences"));
    }
}
