//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//
#nullable enable
using System.Collections.Generic;
using System.IO;
using System.Text;
using NLog.Targets;

namespace Ical.Net.Tests.Logging;

/// <summary>
/// Enables iCal.NET logging operations, providing functionality to configure
/// log targets and retrieve log entries.
/// </summary>
/// <remarks>
/// The <see cref="TestLoggingProvider"/> class allows for the configuration
/// using the <see cref="Options"/> class.
/// </remarks>
internal sealed class TestLoggingProvider : TestLoggingProviderBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TestLoggingProvider"/> class with default options.
    /// </summary>
    public TestLoggingProvider() : this(options:null)
    {}

    /// <summary>
    /// Creates a new instance of the <see cref="TestLoggingProvider"/> class with a specified log file.
    /// </summary>
    public TestLoggingProvider(string logFile) : this(logFile, options: null)
    { }

    /// <summary>
    /// Creates a new instance of the <see cref="TestLoggingProvider"/> class with a specified log file.
    /// </summary>
    /// <param name="logFile">
    /// The relative or absolute path of the file to store the logs.
    /// An existing file will be deleted before logging starts.
    /// </param>
    /// <param name="options">
    /// The options for configuring the logging behavior.
    /// May be <see langword="null"/> to use default options.
    /// </param>
    public TestLoggingProvider(string logFile, Options? options)
        : base(
            new FileTarget("file")
                {
                    FileName = logFile,
                    KeepFileOpen = false,
                    DeleteOldFileOnStartup = true,
                    Encoding = Encoding.UTF8
                },
            options)
    { }

    /// <summary>
    /// Creates a new instance of the <see cref="TestLoggingProvider"/> class with an in-memory log target.
    /// </summary>
    /// <param name="options">
    /// The options for configuring the logging behavior.
    /// May be <see langword="null"/> to use default options.
    /// </param>
    public TestLoggingProvider(Options? options = null)
        : base(new MemoryTarget("memory"){ MaxLogsCount = options?.MaxLogsCount ?? Logging.Options.DefaultMaxLogsCount }, options)
    { }

    /// <summary>
    /// Gets a list of log entries.
    /// The number of returned logs is limited by <see cref="Options.MaxLogsCount"/>.
    /// When the limit is reached, the oldest logs are discarded.
    /// </summary>
    public IEnumerable<string> Logs
    {
        get
        {
            if (Target is MemoryTarget memoryTarget)
            {
                foreach (var log in memoryTarget.Logs)
                {
                    yield return log;
                }
                yield break;
            }

            if (Target is FileTarget fileTarget)
            {
                var fileName = fileTarget.FileName.Render(new NLog.LogEventInfo());

                if (!File.Exists(fileName))
                {
                    throw new FileNotFoundException("Log file not found.", fileName);
                }

                using var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                using var reader = new StreamReader(stream, Encoding.UTF8);
                var queue = new Queue<string>(Options.MaxLogsCount);
                while (reader.ReadLine() is { } line)
                {
                    if (queue.Count == Options.MaxLogsCount)
                    {
                        queue.Dequeue();
                    }
                    queue.Enqueue(line);
                }
                foreach (var log in queue)
                {
                    yield return log;
                }
            }
        }
    }
}
