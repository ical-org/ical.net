using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal
{
    public abstract class PeriodEvaluator :
        IPeriodEvaluator
    {
        #region Private Fields

        private iCalDateTime m_EvaluationStartBounds;
        private iCalDateTime m_EvaluationEndBounds;
        private IList<Period> m_Periods;

        #endregion

        #region Constructors

        public PeriodEvaluator()
        {
            Initialize();
        }

        void Initialize()
        {
            m_Periods = new List<Period>();
        }

        #endregion

        #region IPeriodEvaluator Members

        virtual public iCalDateTime EvaluationStartBounds
        {
            get { return m_EvaluationStartBounds; }
            set { m_EvaluationStartBounds = value; }
        }

        virtual public iCalDateTime EvaluationEndBounds
        {
            get { return m_EvaluationEndBounds; }
            set { m_EvaluationEndBounds = value; }
        }

        public IList<Period> Periods
        {
            get { return m_Periods; }
        }

        public void ClearEvaluation()
        {
            m_EvaluationStartBounds = default(iCalDateTime);
            m_EvaluationEndBounds = default(iCalDateTime);
            m_Periods.Clear();
        }

        abstract public IList<Period> Evaluate(
            iCalDateTime startDate, 
            iCalDateTime fromDate, 
            iCalDateTime toDate);

        #endregion
    }
}
