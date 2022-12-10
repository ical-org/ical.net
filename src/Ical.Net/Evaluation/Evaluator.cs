using System;
using System.Collections.Generic;
using System.Globalization;
using Ical.Net.DataTypes;

namespace Ical.Net.Evaluation
{
    public abstract class Evaluator : IEvaluator
    {
        ICalendarObject? _AssociatedObject;
        readonly ICalendarDataType _AssociatedDataType;

        protected HashSet<Period> MPeriods;

        protected Evaluator()
        {
            Initialize();
        }

        protected Evaluator(ICalendarObject associatedObject)
        {
            _AssociatedObject = associatedObject;

            Initialize();
        }

        protected Evaluator(ICalendarDataType dataType)
        {
            _AssociatedDataType = dataType;

            Initialize();
        }

        void Initialize()
        {
            Calendar = CultureInfo.CurrentCulture.Calendar;
            MPeriods = new HashSet<Period>();
        }

        protected static IDateTime ConvertToIDateTime(DateTime dt, IDateTime referenceDate)
        {
            IDateTime newDt = new CalDateTime(dt, referenceDate.TzId);
            newDt.AssociateWith(referenceDate);
            return newDt;
        }

        public System.Globalization.Calendar Calendar { get; private set; }

        public DateTime EvaluationStartBounds { get; set; } = DateTime.MaxValue;

        public DateTime EvaluationEndBounds { get; set; } = DateTime.MinValue;

        public ICalendarObject AssociatedObject
        {
            get => _AssociatedObject ?? _AssociatedDataType?.AssociatedObject;
            protected set => _AssociatedObject = value;
        }

        public HashSet<Period> Periods => MPeriods;

        public virtual void Clear()
        {
            EvaluationStartBounds = DateTime.MaxValue;
            EvaluationEndBounds = DateTime.MinValue;
            MPeriods.Clear();
        }

        public abstract HashSet<Period> Evaluate(IDateTime referenceDate, DateTime periodStart, DateTime periodEnd, bool includeReferenceDateInResults);
    }
}