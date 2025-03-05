//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

#nullable enable
using System;
using System.Collections.Generic;
using Ical.Net.DataTypes;

namespace Ical.Net.Evaluation;

internal class PeriodListEvaluator : Evaluator
{
    private readonly PeriodList _mPeriodList;

    public PeriodListEvaluator(PeriodList rdt)
    {
        _mPeriodList = rdt;
    }

    public override IEnumerable<Period> Evaluate(CalDateTime referenceDate, CalDateTime? periodStart, CalDateTime? periodEnd, bool includeReferenceDateInResults)
    {
        var periods = new SortedSet<Period>();

        if ((periodStart is not null) && (periodEnd is not null) && periodEnd.LessThan(periodStart))
        {
            return periods;
        }

        periods.UnionWith(_mPeriodList);
        return periods;
    }
}
