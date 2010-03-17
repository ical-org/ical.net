using System;
using System.Collections.Generic;
using System.Text;

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

        public override IList<IPeriod> Evaluate(IDateTime startDate, IDateTime fromDate, IDateTime toDate)
        {
            List<IPeriod> periods = new List<IPeriod>();

            if (startDate.GreaterThan(fromDate))
                fromDate = startDate;

            if (toDate.LessThan(fromDate) ||
                fromDate.GreaterThan(toDate))
                return periods;

            foreach (IPeriod p in m_PeriodList)
            {
                if (!periods.Contains(p))
                    periods.Add(p);
            }

            return periods;
        }

        #endregion
    }
}
