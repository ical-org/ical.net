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
