using System;
using System.IO;
using Ical.Net.Interfaces.DataTypes;
using Ical.Net.Interfaces.General;
using Ical.Net.Serialization.iCalendar.Serializers.DataTypes;

namespace Ical.Net.DataTypes
{
    /// <summary>
    /// Represents an RFC 5545 "BYDAY" value.
    /// </summary>
#if !SILVERLIGHT
    [Serializable]
#endif
    public class WeekDay : 
        EncodableDataType,
        IWeekDay        
    {
        #region Private Fields

        private int _mNum = int.MinValue;            
        private DayOfWeek _mDayOfWeek;            

        #endregion

        #region Public Properties

        virtual public int Offset
        {
            get { return _mNum; }
            set { _mNum = value; }
        }

        virtual public DayOfWeek DayOfWeek
        {
            get { return _mDayOfWeek; }
            set { _mDayOfWeek = value; }
        }

        #endregion

        #region Constructors

        public WeekDay()
        {
            Offset = int.MinValue;
        }

        public WeekDay(DayOfWeek day)
            : this()
        {
            this.DayOfWeek = day;
        }

        public WeekDay(DayOfWeek day, int num)
            : this(day)
        {
            this.Offset = num;
        }

        public WeekDay(DayOfWeek day, FrequencyOccurrence type)
            : this(day, (int)type)
        {
        }

        public WeekDay(string value)
        {
            var serializer =
                new WeekDaySerializer();
            CopyFrom(serializer.Deserialize(new StringReader(value)) as ICopyable);
        }

        #endregion

        #region Overrides

        public override bool Equals(object obj)
        {
            if (obj is WeekDay)
            {
                var ds = (WeekDay)obj;
                return ds.Offset == Offset &&
                    ds.DayOfWeek == DayOfWeek;
            }
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return Offset.GetHashCode() ^ DayOfWeek.GetHashCode();
        }

        public override void CopyFrom(ICopyable obj)
        {
            base.CopyFrom(obj);
            if (obj is IWeekDay)
            {
                var bd = (IWeekDay)obj;
                this.Offset = bd.Offset;
                this.DayOfWeek = bd.DayOfWeek;
            }
        }

        #endregion

        #region IComparable Members

        public int CompareTo(object obj)
        {
            IWeekDay bd = null;
            if (obj is string)
                bd = new WeekDay(obj.ToString());
            else if (obj is IWeekDay)
                bd = (IWeekDay)obj;

            if (bd == null)
                throw new ArgumentException();
            else 
            {
                var compare = this.DayOfWeek.CompareTo(bd.DayOfWeek);
                if (compare == 0)
                    compare = this.Offset.CompareTo(bd.Offset);
                return compare;
            }
        }

        #endregion
    }    
}
