using System;
using System.Collections.Generic;
using Ical.Net.DataTypes;
using Ical.Net.Interfaces.DataTypes;

namespace Ical.Net.Evaluation
{
    public class PeriodListEvaluator : Evaluator
    {
        IPeriodList _mPeriodList;

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
                if (!periods.Contains(p))
                {
                    periods.Add(p);
                }
            }

            if (periodEnd < periodStart)
            {
                return periods;
            }

            foreach (var p in _mPeriodList)
            {
                if (!periods.Contains(p))
                {
                    periods.Add(p);
                }
            }

            return periods;
        }
    }
}