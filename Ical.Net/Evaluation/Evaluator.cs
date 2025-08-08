//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.Collections.Generic;
using Ical.Net.DataTypes;
using NodaTime;
using NodaTime.Extensions;
using NodaTime.TimeZones;

namespace Ical.Net.Evaluation;

public abstract class Evaluator : IEvaluator
{
    /// <summary>
    /// Resolve time zone offset based on the associated start value.
    /// </summary>
    /// <param name="start"></param>
    /// <returns></returns>
    internal static ZoneLocalMappingResolver ResolveFrom(ZonedDateTime start)
    {
        return map =>
        {
            if (map.Count == 1)
            {
                return map.Single();
            }
            else if (map.Count == 2)
            {
                // Only map forward in time
                var last = map.Last();
                if (last.Offset == start.Offset)
                {
                    return last;
                }
                else
                {
                    return map.First();
                }
            }
            else
            {
                // Invalid local time, shift forward
                return Resolvers.ReturnForwardShifted
                    .Invoke(map.LocalDateTime, map.Zone, map.EarlyInterval, map.LateInterval);
            }
        };
    }

    protected static void IncrementDate(ref ZonedDateTime dt, RecurrencePattern pattern, int interval)
    {
        if (interval == 0)
            return;

        try
        {
            var old = dt;
            switch (pattern.Frequency)
            {
                case FrequencyType.Secondly:
                    dt = old.PlusSeconds(interval);
                    break;
                case FrequencyType.Minutely:
                    dt = old.PlusMinutes(interval);
                    break;
                case FrequencyType.Hourly:
                    dt = old.PlusHours(interval);
                    break;
                case FrequencyType.Daily:
                    dt = old.LocalDateTime
                        .PlusDays(interval)
                        .InZoneLeniently(old.Zone);
                    break;
                case FrequencyType.Weekly:
                    var isoDayOfWeek = pattern.FirstDayOfWeek.ToIsoDayOfWeek();
                    if (old.Date.DayOfWeek != isoDayOfWeek)
                    {
                        old = old.LocalDateTime
                            .Previous(isoDayOfWeek)
                            .InZoneLeniently(old.Zone);
                    }

                    dt = old.LocalDateTime
                        .PlusWeeks(interval)
                        .InZoneLeniently(old.Zone);
                    break;
                case FrequencyType.Monthly:
                    dt = old.LocalDateTime
                        .PlusDays(-old.Day + 1)
                        .PlusMonths(interval)
                        .InZoneLeniently(old.Zone);
                    break;
                case FrequencyType.Yearly:
                    dt = old.LocalDateTime
                        .PlusDays(-old.DayOfYear + 1)
                        .PlusYears(interval)
                        .InZoneLeniently(old.Zone);
                    break;
                default:
                    // Frequency should always be valid at this stage.
                    System.Diagnostics.Debug.Fail($"'{pattern.Frequency}' as RecurrencePattern.Frequency is not implemented.");
                    break;
            }
        }
        catch (ArgumentOutOfRangeException)
        {
            // intentionally don't include the outer exception
            throw new EvaluationOutOfRangeException("Evaluation aborted: The maximum supported date-time was exceeded while enumerating a recurrence rule. This commonly happens when trying to enumerate an unbounded RRULE to its end. Consider applying the .TakeWhile() operator.");
        }
    }

    public virtual IEnumerable<EvaluationPeriod> Evaluate(CalDateTime referenceDate, ZonedDateTime periodStart, EvaluationOptions? options)
    {
        return Evaluate(referenceDate, periodStart.Zone, periodStart.ToInstant(), options);
    }

    public abstract IEnumerable<EvaluationPeriod> Evaluate(CalDateTime referenceDate, DateTimeZone timeZone, Instant? periodStart, EvaluationOptions? options);
}
