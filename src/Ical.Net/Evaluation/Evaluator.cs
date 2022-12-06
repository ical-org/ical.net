using System;
using System.Collections.Generic;
using System.Globalization;
using Ical.Net.DataTypes;
using Ical.Net.Utility;

namespace Ical.Net.Evaluation
{
    public abstract class Evaluator : IEvaluator
    {
        private DateTime _mEvaluationStartBounds = DateTime.MaxValue;
        private DateTime _mEvaluationEndBounds = DateTime.MinValue;

        private ICalendarObject _mAssociatedObject;
        private readonly ICalendarDataType _mAssociatedDataType;

        protected HashSet<Period> MPeriods;

        protected Evaluator()
        {
            Initialize();
        }

        protected Evaluator(ICalendarObject associatedObject)
        {
            _mAssociatedObject = associatedObject;

            Initialize();
        }

        protected Evaluator(ICalendarDataType dataType)
        {
            _mAssociatedDataType = dataType;

            Initialize();
        }

        private void Initialize()
        {
            Calendar = CultureInfo.CurrentCulture.Calendar;
            MPeriods = new HashSet<Period>();
        }

        protected IDateTime ConvertToIDateTime(DateTime dt, IDateTime referenceDate)
        {
            IDateTime newDt = new CalDateTime(dt, referenceDate.TzId);
            newDt.AssociateWith(referenceDate);
            return newDt;
        }

        protected void IncrementDate(RecurrencePattern pattern, ref DateTime dt, int interval)
        {
            // FIXME: use a more specific exception.
            if (interval == 0)
            {
                throw new Exception("Cannot evaluate with an interval of zero.  Please use an interval other than zero.");
            }

            var old = dt;
            dt = pattern.Frequency switch
            {
                FrequencyType.Secondly => old.AddSeconds(interval),
                FrequencyType.Minutely => old.AddMinutes(interval),
                FrequencyType.Hourly => old.AddHours(interval),
                FrequencyType.Daily => old.AddDays(interval),
                FrequencyType.Weekly => old.AddWeeks(interval, pattern.FirstDayOfWeek),
                FrequencyType.Monthly => old.AddDays(-old.Day + 1).AddMonths(interval),
                FrequencyType.Yearly => old.AddDays(-old.DayOfYear + 1).AddYears(interval),
                _ => throw new Exception(
                    "FrequencyType.NONE cannot be evaluated. Please specify a FrequencyType before evaluating the recurrence.")
            };
        }

        public System.Globalization.Calendar Calendar { get; private set; }

        public DateTime EvaluationStartBounds
        {
            get => _mEvaluationStartBounds;
            set => _mEvaluationStartBounds = value;
        }

        public DateTime EvaluationEndBounds
        {
            get => _mEvaluationEndBounds;
            set => _mEvaluationEndBounds = value;
        }

        public ICalendarObject AssociatedObject
        {
            get => _mAssociatedObject ?? _mAssociatedDataType?.AssociatedObject;
            protected set => _mAssociatedObject = value;
        }

        public HashSet<Period> Periods => MPeriods;

        public virtual void Clear()
        {
            _mEvaluationStartBounds = DateTime.MaxValue;
            _mEvaluationEndBounds = DateTime.MinValue;
            MPeriods.Clear();
        }

        public abstract HashSet<Period> Evaluate(IDateTime referenceDate, DateTime periodStart, DateTime periodEnd, bool includeReferenceDateInResults);
    }
}