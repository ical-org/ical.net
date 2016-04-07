using System;
using System.Collections.Generic;

namespace DDay.iCal
{
    public class PeriodListEvaluator :
        Evaluator
    {
        #region Private Fields

        IPeriodList m_PeriodList;

        #endregion

        #region Constructors

        public PeriodListEvaluator(IPeriodList rdt)
        {
            m_PeriodList = rdt;
        }

        #endregion

        #region Overrides

        public override HashSet<IPeriod> Evaluate(IDateTime referenceDate, DateTime periodStart, DateTime periodEnd, bool includeReferenceDateInResults)
        {
            var periods = new HashSet<IPeriod>();

            if (includeReferenceDateInResults)
            {
                IPeriod p = new Period(referenceDate);
                if (!periods.Contains(p))
                    periods.Add(p);
            }

            if (periodEnd < periodStart)
                return periods;

            foreach (var p in m_PeriodList)
            {
                if (!periods.Contains(p))
                    periods.Add(p);
            }

            return periods;
        }

        #endregion
    }
}
