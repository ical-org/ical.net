//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Ical.Net.DataTypes;

namespace Ical.Net.Tests;

public static class TestHelpers
{
    public static IEnumerable<Period> TakeUntil(this IEnumerable<Period> sequence, CalDateTime periodEnd)
        => (periodEnd == null) ? sequence : sequence.TakeWhile(p => p.StartTime < periodEnd);

    public static IEnumerable<Occurrence> TakeUntil(this IEnumerable<Occurrence> sequence, CalDateTime periodEnd)
        => (periodEnd == null) ? sequence : sequence.TakeWhile(p => p.Period.StartTime < periodEnd);
}
