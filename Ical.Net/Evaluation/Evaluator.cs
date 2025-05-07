//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.Collections.Generic;
using Ical.Net.DataTypes;
using Ical.Net.Utility;

namespace Ical.Net.Evaluation;

public abstract class Evaluator : IEvaluator
{
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
                    dt = old.AddDays(-old.DayOfYear + 1).AddYears(interval);
                    break;
                // FIXME: use a more specific exception.
                default:
                    throw new Exception("FrequencyType.NONE cannot be evaluated. Please specify a FrequencyType before evaluating the recurrence.");
            }
        }
        catch (ArgumentOutOfRangeException)
        {
            // intentionally don't include the outer exception
            throw new EvaluationOutOfRangeException("Evaluation was aborted because an event's start time exceeded the maxium supported date/time value.");
        }
    }

    public abstract IEnumerable<Period> Evaluate(CalDateTime referenceDate, CalDateTime? periodStart, EvaluationOptions? options);
}
