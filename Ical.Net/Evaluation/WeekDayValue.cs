//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using Ical.Net.DataTypes;
using NodaTime;
using NodaTime.Extensions;

namespace Ical.Net.Evaluation;

internal readonly struct WeekDayValue(int? offset, IsoDayOfWeek dayOfWeek)
{
    public WeekDayValue(WeekDay value) : this(value.Offset, value.DayOfWeek.ToIsoDayOfWeek()) { }

    public int? Offset { get; } = offset;

    public IsoDayOfWeek DayOfWeek { get; } = dayOfWeek;
}
