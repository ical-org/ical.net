using System;
using System.Collections.Generic;
using ical.net.DataTypes;
using ical.net.Interfaces.DataTypes;

namespace ical.net.Evaluation
{
    public class PeriodListEvaluator : Evaluator
    {
        private readonly IPeriodList _mPeriodList;

        public PeriodListEvaluator(IPeriodList rdt)
        {
            _mPeriodList = rdt;
        }

        public override HashSet<IPeriod> Evaluate(IDateTime referenceDate, DateTime periodStart, DateTime periodEnd, bool includeReferenceDateInResults)
        {
            var periods = new HashSet<IPeriod>();

            if (includeReferenceDateInResults)
            {
                IPeriod p = new Period(referenceDate);
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
}