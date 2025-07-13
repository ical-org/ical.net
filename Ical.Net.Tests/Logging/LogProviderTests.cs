//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//
#nullable enable
using System;
using System.Collections.Generic;
using Ical.Net.Logging;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace Ical.Net.Tests.Logging;

internal class LoggingProviderTests
{
    [Test]
    public void Logger_ShouldCaptureLoggingMessages()
    {
        // Heads up: Each logger instance created by this logger factory instance
        // will write to this list.
        var logEntries = new List<LogEntry>();
        using var loggerFactory = InMemoryLoggerProvider.CreateLoggerFactory(logEntries);

        // This sets the static logger factory for Ical.Net
        // It should be called once at application startup by the consuming application.
        LoggingProvider.SetLoggerFactory(loggerFactory);

        // Create loggers for Ical.Net from the registered logger factory.
        var genLogger = LoggingProvider.CreateLogger<LoggingProviderTests>();
        var catLogger = LoggingProvider.CreateLogger(typeof(LoggingProviderTests).FullName!);

        catLogger.LogInformation("This is an info message.");
        genLogger.LogError(new ArgumentException(), "This is an error message.");
        genLogger.LogWarning("This is a warning message with structured data. {Data}", new { Key1 = "Value1", Key2 = "Value2" });

        using (Assert.EnterMultipleScope())
        {
            Assert.That(logEntries, Has.Count.EqualTo(3));

            Assert.That(logEntries[0].LogLevel, Is.EqualTo(LogLevel.Information));
            Assert.That(logEntries[0].Message, Does.Contain($"[0] [Information] [{typeof(LoggingProviderTests).FullName}] This is an info message."));

            Assert.That(logEntries[1].Exception, Is.TypeOf<ArgumentException>());
            Assert.That(logEntries[1].Message, Does.Contain($"[1] [Error] [{typeof(LoggingProviderTests).FullName}] This is an error message. | Exception: System.ArgumentException:"));
            Assert.That(logEntries[1].EventId, Is.EqualTo(new EventId(1)));

            Assert.That(logEntries[2].Message, Does.Contain($"[2] [Warning] [{typeof(LoggingProviderTests).FullName}] This is a warning message with structured data. {{Data}} | Data: {{ Key1 = Value1, Key2 = Value2 }}"));

            Assert.That(genLogger.BeginScope("Scope"), Is.Null);
            Assert.That(genLogger.IsEnabled(LogLevel.Information), Is.True);
            Assert.That(LoggingProvider.LoggerFactory, Is.InstanceOf<ILoggerFactory>());
        }
    }
}
