using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal
{
    public class RecurrencePatternPeriodEvaluator :
        PeriodEvaluator
    {
        #region Private Fields

        IRecurrencePattern m_Pattern;

        #endregion

        #region Protected Properties

        protected IRecurrencePattern Pattern
        {
            get { return m_Pattern; }
            set { m_Pattern = value; }
        }

        #endregion

        #region Constructors

        public RecurrencePatternPeriodEvaluator(IRecurrencePattern pattern)
        {
            Pattern = pattern;
        }

        #endregion        

        #region Overrides

        public override IList<Period> Evaluate(
            iCalDateTime startDate, 
            iCalDateTime fromDate, 
            iCalDateTime toDate)
        {
            throw new NotImplementedException();            
        }

        #endregion
    }
}
