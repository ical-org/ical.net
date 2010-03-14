using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal
{
    public abstract class PeriodEvaluator :
        IPeriodEvaluator
    {
        #region Private Fields

        private System.Globalization.Calendar m_Calendar;
        private IList<iCalDateTime> m_StaticOccurrences;        
        private iCalDateTime m_EvaluationStartBounds;
        private iCalDateTime m_EvaluationEndBounds;
        protected List<Period> m_Periods;

        #endregion

        #region Constructors

        public PeriodEvaluator()
        {
            Initialize();
        }

        void Initialize()
        {
            m_Calendar = System.Globalization.CultureInfo.CurrentCulture.Calendar;
            m_StaticOccurrences = new List<iCalDateTime>();
            m_Periods = new List<Period>();
        }

        #endregion

        #region Protected Methods

        protected void IncrementDate(ref iCalDateTime dt, IRecurrencePattern pattern, int interval)
        {
            iCalDateTime old = dt;
            switch (pattern.Frequency)
            {
                case FrequencyType.Secondly: dt = old.AddSeconds(interval); break;
                case FrequencyType.Minutely: dt = old.AddMinutes(interval); break;
                case FrequencyType.Hourly: dt = old.AddHours(interval); break;
                case FrequencyType.Daily: dt = old.AddDays(interval); break;
                case FrequencyType.Weekly:
                    // How the week increments depends on the WKST indicated (defaults to Monday)
                    // So, basically, we determine the week of year using the necessary rules,
                    // and we increment the day until the week number matches our "goal" week number.
                    // So, if the current week number is 36, and our Interval is 2, then our goal
                    // week number is 38.
                    // NOTE: fixes RRULE12 eval.
                    int current = Calendar.GetWeekOfYear(old.Value, System.Globalization.CalendarWeekRule.FirstFourDayWeek, pattern.WeekStart),
                        lastLastYear = Calendar.GetWeekOfYear(new DateTime(old.Year - 1, 12, 31, 0, 0, 0, DateTimeKind.Local), System.Globalization.CalendarWeekRule.FirstFourDayWeek, pattern.WeekStart),
                        last = Calendar.GetWeekOfYear(new DateTime(old.Year, 12, 31, 0, 0, 0, DateTimeKind.Local), System.Globalization.CalendarWeekRule.FirstFourDayWeek, pattern.WeekStart),
                        goal = current + interval;

                    // If the goal week is greater than the last week of the year, wrap it!
                    if (goal > last)
                        goal = goal - last;
                    else if (goal <= 0)
                        goal = lastLastYear + goal;

                    int i = interval > 0 ? 1 : -1;
                    while (current != goal)
                    {
                        old = old.AddDays(i);
                        current = Calendar.GetWeekOfYear(old.Value, System.Globalization.CalendarWeekRule.FirstFourDayWeek, pattern.WeekStart);
                    }

                    dt = old;
                    break;
                case FrequencyType.Monthly: dt = old.AddDays(-old.Day + 1).AddMonths(interval); break;
                case FrequencyType.Yearly: dt = old.AddDays(-old.DayOfYear + 1).AddYears(interval); break;
                default: throw new Exception("FrequencyType.NONE cannot be evaluated. Please specify a FrequencyType before evaluating the recurrence.");
            }
        }

        #endregion

        #region IPeriodEvaluator Members

        public System.Globalization.Calendar Calendar
        {
            get { return m_Calendar; }
        }

        public IList<iCalDateTime> StaticOccurrences
        {
            get { return m_StaticOccurrences; }
        }

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

        public void Clear()
        {
            m_EvaluationStartBounds = default(iCalDateTime);
            m_EvaluationEndBounds = default(iCalDateTime);
            m_StaticOccurrences.Clear();
            m_Periods.Clear();
        }

        abstract public IList<Period> Evaluate(
            iCalDateTime startDate, 
            iCalDateTime fromDate, 
            iCalDateTime toDate);

        #endregion
    }
}
