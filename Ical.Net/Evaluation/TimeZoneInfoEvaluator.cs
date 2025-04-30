//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

#nullable enable
using System;
using System.Collections.Generic;
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

    public TimeZoneInfoEvaluator(IRecurrable tzi) : base(tzi) { }
}
