using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Ical.Net.DataTypes;
using Ical.Net.Evaluation;
using Ical.Net.Proxies;
using Ical.Net.Utility;

namespace Ical.Net.CalendarComponents
{
    /// <summary> iCalendar component that contains multiple <see cref="RecurrencePattern"/>s in <see cref="RecurrenceRules"/> and <see cref="ExceptionRules"/>. </summary>
    /// <remarks>
    /// All <see cref="RecurrencePattern"/>s have individual End Dates, but share the common <see cref="Start"/> Date.
    /// <see cref=""/>
    /// This component automatically handles
    /// RRULEs, RDATE, EXRULEs, and EXDATEs, as well as the DTSTART
    /// for the recurring item (all recurring items must have a DTSTART).
    /// </remarks>
    public class RecurringComponent : UniqueComponent, IRecurringComponent
    {
        public static IEnumerable<IRecurringComponent> SortByDate(IEnumerable<IRecurringComponent> list) => SortByDate<IRecurringComponent>(list);

        public static IEnumerable<TRecurringComponent> SortByDate<TRecurringComponent>(IEnumerable<TRecurringComponent> list) => list.OrderBy(d => d);

        protected virtual bool EvaluationIncludesReferenceDate => false;

        public IList<Attachment> Attachments
        {
            get => Properties.GetMany<Attachment>("ATTACH");
            set => Properties.Set("ATTACH", value);
        }

        /// <summary> AKA Tags; a List of <see cref="string"/> Category Names </summary>
        public IList<string> Categories
        {
            get => Properties.GetMany<string>("CATEGORIES");
            set => Properties.Set("CATEGORIES", value);
        }

        /// <summary> String for the distinguished 'class' in the <see cref="Categories"/> </summary>
        public string Class
        {
            get => Properties.Get<string>("CLASS");
            set => Properties.Set("CLASS", value);
        }

        public IList<string> Contacts
        {
            get => Properties.GetMany<string>("CONTACT");
            set => Properties.Set("CONTACT", value);
        }

        public IDateTime Created
        {
            get => Properties.Get<IDateTime>("CREATED");
            set => Properties.Set("CREATED", value);
        }

        /// <summary> Longer than the <see cref="Summary"/> </summary>
        public string Description
        {
            get => Properties.Get<string>("DESCRIPTION");
            set => Properties.Set("DESCRIPTION", value);
        }

        /// <summary> The start date/time of the component. </summary>
        /// <remarks>common to all <see cref="RecurrenceRules"/> and <see cref="ExceptionRules"/></remarks>
        public virtual IDateTime DtStart
        {
            get => Properties.Get<IDateTime>("DTSTART");
            set => Properties.Set("DTSTART", value);
        }

        /// <summary>
        /// 
        /// </summary>
        public IList<PeriodList> ExceptionDates
        {
            get => Properties.GetMany<PeriodList>("EXDATE");
            set => Properties.Set("EXDATE", value);
        }

        public IList<RecurrencePattern> ExceptionRules
        {
            get => Properties.GetMany<RecurrencePattern>("EXRULE");
            set => Properties.Set("EXRULE", value);
        }

        public IDateTime LastModified
        {
            get => Properties.Get<IDateTime>("LAST-MODIFIED");
            set => Properties.Set("LAST-MODIFIED", value);
        }

        public int Priority
        {
            get => Properties.Get<int>("PRIORITY");
            set => Properties.Set("PRIORITY", value);
        }

        public IList<PeriodList> RecurrenceDates
        {
            get => Properties.GetMany<PeriodList>("RDATE");
            set => Properties.Set("RDATE", value);
        }

        public IList<RecurrencePattern> RecurrenceRules
        {
            get => Properties.GetMany<RecurrencePattern>("RRULE");
            set => Properties.Set("RRULE", value);
        }

        public IDateTime RecurrenceId
        {
            get => Properties.Get<IDateTime>("RECURRENCE-ID");
            set => Properties.Set("RECURRENCE-ID", value);
        }

        public IList<string> RelatedComponents
        {
            get => Properties.GetMany<string>("RELATED-TO");
            set => Properties.Set("RELATED-TO", value);
        }

        public int Sequence
        {
            get => Properties.Get<int>("SEQUENCE");
            set => Properties.Set("SEQUENCE", value);
        }

        /// <summary>
        /// An alias to the DTStart field (i.e. start date/time).
        /// </summary>
        public IDateTime Start
        {
            get => DtStart;
            set => DtStart = value;
        }

        /// <summary> Summary of the long <see cref="Description"/> </summary>
        public string Summary
        {
            get => Properties.Get<string>("SUMMARY");
            set => Properties.Set("SUMMARY", value);
        }

        /// <summary>
        /// A list of <see cref="Alarm"/>s for this recurring component.
        /// </summary>
        public ICalendarObjectList<Alarm> Alarms => new CalendarObjectListProxy<Alarm>(Children);

        public RecurringComponent()
        {
            Initialize();
            EnsureProperties();
        }

        public RecurringComponent(string name) : base(name)
        {
            Initialize();
            EnsureProperties();
        }

        void Initialize() => SetService(new RecurringEvaluator(this));

        void EnsureProperties()
        {
            if (!Properties.ContainsKey("SEQUENCE"))
            {
                Sequence = 0;
            }
        }

        protected override void OnDeserializing(StreamingContext context)
        {
            base.OnDeserializing(context);

            Initialize();
        }

        public void ClearEvaluation() => RecurrenceUtil.ClearEvaluation(this);

        public HashSet<Occurrence> GetOccurrences(IDateTime dt) => RecurrenceUtil.GetOccurrences(this, dt, EvaluationIncludesReferenceDate);

        public HashSet<Occurrence> GetOccurrences(DateTime dt)
            => RecurrenceUtil.GetOccurrences(this, new CalDateTime(dt), EvaluationIncludesReferenceDate);

        public HashSet<Occurrence> GetOccurrences(IDateTime startTime, IDateTime endTime)
            => RecurrenceUtil.GetOccurrences(this, startTime, endTime, EvaluationIncludesReferenceDate);

        public HashSet<Occurrence> GetOccurrences(DateTime startTime, DateTime endTime)
            => RecurrenceUtil.GetOccurrences(this, new CalDateTime(startTime), new CalDateTime(endTime), EvaluationIncludesReferenceDate);

        public IList<AlarmOccurrence> PollAlarms() => PollAlarms(null, null);

        public IList<AlarmOccurrence> PollAlarms(IDateTime startTime, IDateTime endTime)
            => Alarms?.SelectMany(a => a.Poll(startTime, endTime)).ToList()
                ?? new List<AlarmOccurrence>();

        protected bool Equals(RecurringComponent other)
        {
            var result = Equals(DtStart, other.DtStart)
                && Equals(Priority, other.Priority)
                && string.Equals(Summary, other.Summary, StringComparison.OrdinalIgnoreCase)
                && string.Equals(Class, other.Class, StringComparison.OrdinalIgnoreCase)
                && string.Equals(Description, other.Description, StringComparison.OrdinalIgnoreCase)
                && Equals(RecurrenceId, other.RecurrenceId)
                && Attachments.SequenceEqual(other.Attachments)
                && CollectionHelpers.Equals(Categories, other.Categories)
                && CollectionHelpers.Equals(Contacts, other.Contacts)
                && CollectionHelpers.Equals(ExceptionDates, other.ExceptionDates)
                && CollectionHelpers.Equals(ExceptionRules, other.ExceptionRules)
                && CollectionHelpers.Equals(RecurrenceDates, other.RecurrenceDates, orderSignificant: true)
                && CollectionHelpers.Equals(RecurrenceRules, other.RecurrenceRules, orderSignificant: true);

            return result;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((RecurringComponent)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = DtStart?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ Priority.GetHashCode();
                hashCode = (hashCode * 397) ^ (Summary?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (Class?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (Description?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (RecurrenceId?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ CollectionHelpers.GetHashCode(Attachments);
                hashCode = (hashCode * 397) ^ CollectionHelpers.GetHashCode(Categories);
                hashCode = (hashCode * 397) ^ CollectionHelpers.GetHashCode(Contacts);
                hashCode = (hashCode * 397) ^ CollectionHelpers.GetHashCode(ExceptionDates);
                hashCode = (hashCode * 397) ^ CollectionHelpers.GetHashCode(ExceptionRules);
                hashCode = (hashCode * 397) ^ CollectionHelpers.GetHashCode(RecurrenceDates);
                hashCode = (hashCode * 397) ^ CollectionHelpers.GetHashCode(RecurrenceRules);
                return hashCode;
            }
        }
    }
}