//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using Microsoft.Extensions.Logging;

namespace Ical.Net.Tests.Logging.Adapters;

#pragma warning disable CA2254 // template should be a static expression

internal class MicrosoftLoggerAdapter(ILogger msLogger) : Ical.Net.Logging.ILogger
{
    public void Log(Ical.Net.Logging.LogLevel level, string messageTemplate, params object[] args)
        => msLogger.Log(ConvertLogLevel(level), messageTemplate, args);

    public void Log(Ical.Net.Logging.LogLevel level, Exception exception, string messageTemplate, params object[] args)
        => msLogger.Log(ConvertLogLevel(level), exception, messageTemplate, args);

    public bool IsEnabled(Ical.Net.Logging.LogLevel level) =>
        msLogger.IsEnabled(ConvertLogLevel(level));

    private static LogLevel ConvertLogLevel(Ical.Net.Logging.LogLevel level)
    {
        return level switch
        {
            Ical.Net.Logging.LogLevel.Trace => LogLevel.Trace,
            Ical.Net.Logging.LogLevel.Debug => LogLevel.Debug,
            Ical.Net.Logging.LogLevel.Information => LogLevel.Information,
            Ical.Net.Logging.LogLevel.Warning => LogLevel.Warning,
            Ical.Net.Logging.LogLevel.Error => LogLevel.Error,
            Ical.Net.Logging.LogLevel.Critical => LogLevel.Critical,
            _ => LogLevel.None
        };
    }
}
#pragma warning restore CA2254
