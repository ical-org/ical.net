//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;

namespace Ical.Net.Evaluation;

public class TimeZoneInfoEvaluator : RecurringEvaluator
{
    protected VTimeZoneInfo TimeZoneInfo
    {
        get => Recurrable as VTimeZoneInfo ?? throw new InvalidOperationException();
        set => Recurrable = value;
    }

    protected override NodaTime.ZonedDateTime GetEnd(NodaTime.ZonedDateTime start) => start;
    protected override EvaluationPeriod EvaluateRDate(Period rdate, NodaTime.DateTimeZone referenceTimeZone)
        => throw new NotImplementedException();

    public TimeZoneInfoEvaluator(IRecurrable tzi) : base(tzi) { }
}
