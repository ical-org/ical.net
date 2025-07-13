// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.

#nullable enable
using System.Collections.Generic;

namespace Ical.Net.Tests.Logging;

internal record Options
{
    /// <summary>
    /// The default maximum number of logs to keep in memory or return from file.
    /// </summary>
    public const int DefaultMaxLogsCount = 1000;

    /// <summary>
    /// Gets or sets the collection of filters applied to logging.
    /// </summary>
    public List<Filter> Filters { get; set; } = new();

    /// <summary>
    /// Limits the number of logs stored in memory or latest logs returned from file.
    /// When the limit is reached, <b>the oldest logs are discarded</b>.
    /// </summary>
    public int MaxLogsCount { get; set; } = DefaultMaxLogsCount;

    /// <summary>
    /// Gets or sets a value indicating whether logging is enabled only in debug mode.
    /// <see langword="true"/> is the default, so logging won't be active in release builds unless explicitly enabled.
    /// Be careful when asserting log entries in tests, as they may not be present in release builds.
    /// </summary>
    public bool DebugModeOnly { get; set; } = true;
}
