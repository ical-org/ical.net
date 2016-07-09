using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using Ical.Net.Evaluation;
using Ical.Net.Interfaces.DataTypes;
using Ical.Net.Interfaces.General;
using Ical.Net.Serialization.iCalendar.Serializers.DataTypes;

namespace Ical.Net.DataTypes
{
    /// <summary>
    /// An iCalendar list of recurring dates (or date exclusions)
    /// </summary>
    public class PeriodList : EncodableDataType, IPeriodList
    {
        public string TzId { get; set; }

        private IList<IPeriod> _periods = new List<IPeriod>(128);

        protected IList<IPeriod> Periods
        {
            get { return _periods; }
            set { _periods = value; }
        }

        public PeriodList()
        {
            Initialize();
        }

        public PeriodList(string value) : this()
        {
            var serializer = new PeriodListSerializer();
            CopyFrom(serializer.Deserialize(new StringReader(value)) as ICopyable);
        }

        private void Initialize()
        {
            SetService(new PeriodListEvaluator(this));
        }

        protected override void OnDeserializing(StreamingContext context)
        {
            base.OnDeserializing(context);
            Initialize();
        }

        protected bool Equals(PeriodList other)
        {
            return Equals(_periods, other._periods) && string.Equals(TzId, other.TzId);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((PeriodList)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((_periods?.GetHashCode() ?? 0) * 397) ^ (TzId?.GetHashCode() ?? 0);
            }
        }

        public override void CopyFrom(ICopyable obj)
        {
            base.CopyFrom(obj);
            if (obj is IPeriodList)
            {
                var rdt = (IPeriodList) obj;
                foreach (var p in rdt)
                {
                    Add(p.Copy<IPeriod>());
                }
            }
        }

        public override string ToString() => new PeriodListSerializer().SerializeToString(this);

        public List<Period> Evaluate(CalDateTime startDate, CalDateTime fromDate, CalDateTime endDate)
        {
            var periods = new List<Period>(Periods.Count);

            if (startDate > fromDate)
            {
                fromDate = startDate;
            }

            if (endDate < fromDate || fromDate > endDate)
            {
                return periods;
            }

            var uncollectedPeriodQuery = from Period p in Periods
                                         where !periods.Contains(p)
                                         select p;

            periods.AddRange(uncollectedPeriodQuery);
            return periods;
        }

        public virtual void Add(IDateTime dt) => Periods.Add(new Period(dt));

        public virtual void Remove(IDateTime dt) => Periods.Remove(new Period(dt));

        public IPeriod this[int index]
        {
            get { return _periods[index]; }
            set { _periods[index] = value; }
        }

        public virtual void Add(IPeriod item) => _periods.Add(item);

        public virtual void Clear() => _periods.Clear();

        public bool Contains(IPeriod item) => _periods.Contains(item);

        public void CopyTo(IPeriod[] array, int arrayIndex) => _periods.CopyTo(array, arrayIndex);

        public int Count => _periods.Count;

        public bool IsReadOnly => false;

        public bool Remove(IPeriod item) => _periods.Remove(item);

        public int IndexOf(IPeriod item) => _periods.IndexOf(item);

        public void Insert(int index, IPeriod item) => _periods.Insert(index, item);

        public void RemoveAt(int index) => _periods.RemoveAt(index);

        public IEnumerator<IPeriod> GetEnumerator() => _periods.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _periods.GetEnumerator();
    }
}