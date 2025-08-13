using System;

namespace Ical.Net.Logging;

internal static class LoggerExtensions // Make public when logging is used in library classes
{
    public static void LogTrace(this ILogger logger, string messageTemplate, params object[] args)
        => logger.Log(LogLevel.Trace, messageTemplate, args);

    public static void LogTrace(this ILogger logger, Exception exception, string messageTemplate, params object[] args)
        => logger.Log(LogLevel.Trace, exception, messageTemplate, args);

    public static void LogDebug(this ILogger logger, string messageTemplate, params object[] args)
        => logger.Log(LogLevel.Debug, messageTemplate, args);

    public static void LogDebug(this ILogger logger, Exception exception, string messageTemplate, params object[] args)
        => logger.Log(LogLevel.Debug, exception, messageTemplate, args);

    public static void LogInformation(this ILogger logger, string messageTemplate, params object[] args)
        => logger.Log(LogLevel.Information, messageTemplate, args);

    public static void LogInformation(this ILogger logger, Exception exception, string messageTemplate, params object[] args)
        => logger.Log(LogLevel.Information, exception, messageTemplate, args);

    public static void LogWarning(this ILogger logger, string messageTemplate, params object[] args)
        => logger.Log(LogLevel.Warning, messageTemplate, args);

    public static void LogWarning(this ILogger logger, Exception exception, string messageTemplate, params object[] args)
        => logger.Log(LogLevel.Warning, exception, messageTemplate, args);

    public static void LogError(this ILogger logger, string messageTemplate, params object[] args)
        => logger.Log(LogLevel.Error, messageTemplate, args);

    public static void LogError(this ILogger logger, Exception exception, string messageTemplate, params object[] args)
        => logger.Log(LogLevel.Error, exception, messageTemplate, args);

    public static void LogCritical(this ILogger logger, string messageTemplate, params object[] args)
        => logger.Log(LogLevel.Critical, messageTemplate, args);

    public static void LogCritical(this ILogger logger, Exception exception, string messageTemplate, params object[] args)
        => logger.Log(LogLevel.Critical, exception, messageTemplate, args);
}
