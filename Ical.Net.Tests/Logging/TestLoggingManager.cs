//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//
#nullable enable

namespace Ical.Net.Tests.Logging;

/// <summary>
/// Enables iCal.NET logging operations, providing functionality to configure
/// log targets and retrieve log entries.
/// </summary>
/// <remarks>
/// The <see cref="TestLoggingManager"/> class allows for the configuration
/// using the <see cref="Options"/> class.
/// </remarks>
internal sealed class TestLoggingManager : TestLoggingManagerBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TestLoggingManager"/> class with default options.
    /// </summary>
    public TestLoggingManager() : this(options:null)
    {}

    /// <summary>
    /// Creates a new instance of the <see cref="TestLoggingManager"/> class with a specified log file.
    /// </summary>
    public TestLoggingManager(string logFile) : this(logFile, options: null)
    { }

    /// <summary>
    /// Creates a new instance of the <see cref="TestLoggingManager"/> class with a specified log file.
    /// </summary>
    /// <param name="logFile">
    /// The relative or absolute path of the file to store the logs.
    /// An existing file will be deleted before logging starts.
    /// </param>
    /// <param name="options">
    /// The options for configuring the logging behavior.
    /// May be <see langword="null"/> to use default options.
    /// </param>
    public TestLoggingManager(string logFile, Options? options)
        : base(options)
    {
        (options ?? new Options()).LogToFileLogPath = logFile;
    }

    /// <summary>
    /// Creates a new instance of the <see cref="TestLoggingManager"/> class with an in-memory log target.
    /// </summary>
    /// <param name="options">
    /// The options for configuring the logging behavior.
    /// May be <see langword="null"/> to use default options.
    /// </param>
    public TestLoggingManager(Options? options = null)
        : base(options)
    { }
}
