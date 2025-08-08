//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//
#nullable enable
using System;
using System.IO;
using Ical.Net.Tests.Logging.Abstractions;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Serilog;
using Serilog.Events;

namespace Ical.Net.Tests.Logging.Tests;

[TestFixture]
internal class SerilogAbstractionTests
{
    [TestCase(LogLevel.Trace, LogEventLevel.Verbose)]
    [TestCase(LogLevel.Debug, LogEventLevel.Debug)]
    [TestCase(LogLevel.Information, LogEventLevel.Information)]
    [TestCase(LogLevel.Warning, LogEventLevel.Warning)]
    [TestCase(LogLevel.Error, LogEventLevel.Error)]
    [TestCase(LogLevel.Critical, LogEventLevel.Fatal)]
    [TestCase(LogLevel.None, LogEventLevel.Fatal + 1)]
    public void MapLogLevel_ReturnsExpectedSerilogLevel(LogLevel input, LogEventLevel expected)
    {
        var result = SerilogAbstractions.MapLogLevel(input);
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void MapLogLevel_UnknownValue_ReturnsFatalPlusOne()
    {
        var unknown = (LogLevel) 999;
        var result = SerilogAbstractions.MapLogLevel(unknown);
        Assert.That(result, Is.EqualTo(LogEventLevel.Fatal + 1));
    }

    [Test]
    public void CreateLogger_WritesLogToFile_WithRollingInterval()
    {
        var baseFilename = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "serilog_test.log");
        var options = new Options
        {
            LogToFileLogPath = baseFilename,
            OutputTemplate = Options.DefaultOutputTemplate,
            MinLogLevel = LogLevel.Information,
            Filters = [],
            LogToConsole = false,
            LogToMemory = false,
            DebugModeOnly = false
        };

        var logger = SerilogAbstractions.CreateLogger(options, null);
        var testMessage = $"Test log entry {Guid.NewGuid():N}";
        logger.Information(testMessage);

        // Ensure the logger is flushed and disposed properly
        // to ensure the log files can be found and read.
        if (logger is IDisposable disposableLogger)
            disposableLogger.Dispose();

        var logDir = Path.GetDirectoryName(baseFilename)!;

        var logFiles = Directory.GetFiles(logDir, "*.log");

        Assert.That(logFiles.Length, Is.GreaterThan(0), $"Log file should exist. Searched for: *.log in {logDir}");

        var found = false;
        foreach (var file in logFiles)
        {
            var fileContent = File.ReadAllText(file);
            if (!fileContent.Contains(testMessage)) continue;

            found = true;
            break;
        }
        Assert.That(found, Is.True, $"Log file(s) found: {string.Join(", ", logFiles)}. None contained the test message: {testMessage}");
    }
}
