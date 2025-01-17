//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

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

    public override IEnumerable<Period> Evaluate(IDateTime referenceDate, DateTime? periodStart, DateTime? periodEnd, bool includeReferenceDateInResults)
    {
        var periods = new SortedSet<Period>();

        if (includeReferenceDateInResults)
        {
            var p = new Period(referenceDate);
            periods.Add(p);
        }

        if (periodEnd < periodStart)
        {
            return periods;
        }

        periods.UnionWith(_mPeriodList);
        return periods;
    }
}
