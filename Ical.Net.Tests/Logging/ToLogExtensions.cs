//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//
#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ical.Net.DataTypes;

namespace Ical.Net.Tests.Logging;

internal static class ToLogExtensions
{
    public static string ToLog(this Exception? exception)
    {
        if (exception == null) return string.Empty;
        var sb = new StringBuilder();
        sb.Append(exception.GetType().Name);
        sb.Append(": ");
        sb.Append(exception.Message);
        if (exception.StackTrace != null)
        {
            sb.Append(Environment.NewLine);
            sb.Append(exception.StackTrace);
        }
        return sb.ToString();
    }

    public static string ToLog(this IEnumerable<Occurrence>? occurrences)
    {
        var sb = new StringBuilder();
        if (occurrences == null || !occurrences.Any())
        {
            return "No occurrences found.";
        }

        sb.AppendLine("Occurrences:");

        foreach (var occurrence in occurrences)
        {
            sb.AppendLine(occurrence.ToLog());
        }

        return sb.ToString();
    }

    public static string ToLog(this Occurrence occurrence)
    {
        var o = occurrence;
        return $"Start: {o.Period.StartTime} Period: {o.Period.Duration?.ToString() ?? "null"} End: {o.Period.EffectiveEndTime ?? o.Period.StartTime.Add(o.Period.Duration!.Value)}";
    }
}
