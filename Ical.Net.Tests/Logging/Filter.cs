// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.

#nullable enable
using Microsoft.Extensions.Logging;

namespace Ical.Net.Tests.Logging;

internal record Filter
{
    public LogLevel MinLogLevel { get; set; } = LogLevel.Trace;
    public LogLevel MaxLogLevel { get; set; } = LogLevel.Critical;
    public string LoggerNamePattern { get; set; } = "*";
}
