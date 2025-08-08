//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.Collections.Generic;
using Ical.Net.DataTypes;
using Ical.Net.Utility;
using NodaTime;
using NodaTime.Extensions;

namespace Ical.Net.Evaluation;

public abstract class Evaluator : IEvaluator
{
    protected static void IncrementDate(ref LocalDateTime dt, RecurrencePattern pattern, int interval)
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
                    dt = old.PlusDays(interval);
                    break;
                case FrequencyType.Weekly:
                    var isoDayOfWeek = pattern.FirstDayOfWeek.ToIsoDayOfWeek();
                    if (old.Date.DayOfWeek != isoDayOfWeek)
                    {
                        old = old.Previous(isoDayOfWeek);
                    }

                    dt = old.PlusWeeks(interval);
                    break;
                case FrequencyType.Monthly:
                    dt = old.PlusDays(-old.Day + 1).PlusMonths(interval);
                    break;
                case FrequencyType.Yearly:
                    dt = old.PlusDays(-old.DayOfYear + 1).PlusYears(interval);
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

    protected void IncrementDate(ref CalDateTime dt, RecurrencePattern pattern, int interval)
    {
        if (interval == 0)
            return;

        try
        {
            var old = dt;
            switch (pattern.Frequency)
            {
                case FrequencyType.Secondly:
                    dt = old.AddSeconds(interval);
                    break;
                case FrequencyType.Minutely:
                    dt = old.AddMinutes(interval);
                    break;
                case FrequencyType.Hourly:
                    dt = old.AddHours(interval);
                    break;
                case FrequencyType.Daily:
                    dt = old.AddDays(interval);
                    break;
                case FrequencyType.Weekly:
                    dt = DateUtil.AddWeeks(old, interval, pattern.FirstDayOfWeek);
                    break;
                case FrequencyType.Monthly:
                    dt = old.AddDays(-old.Day + 1).AddMonths(interval);
                    break;
                case FrequencyType.Yearly:
                    // RecurrencePatternEvaluator relies on the assumption that after incrementing, the new refDate
                    // is usually at the first day of an interval.
                    dt = old.AddDays(-old.DayOfYear + 1).AddYears(interval);
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

    public abstract IEnumerable<DataTypes.Period> Evaluate(CalDateTime referenceDate, CalDateTime? periodStart, EvaluationOptions? options);
}
