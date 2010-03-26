using System;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.IO;

namespace DDay.iCal
{
    /// <summary>
    /// Represents an RFC 5545 "BYDAY" value.
    /// </summary>
#if DATACONTRACT
    [DataContract(Name = "WeekDay", Namespace = "http://www.ddaysoftware.com/dday.ical/2009/07/")]
#endif
    [Serializable]
    public class WeekDay : 
        EncodableDataType,
        IWeekDay        
    {
        #region Private Fields

        private int m_Num = int.MinValue;            
        private DayOfWeek m_DayOfWeek;            

        #endregion

        #region Public Properties

#if DATACONTRACT
        [DataMember(Order = 1)]
#endif
        virtual public int Offset
        {
            get { return m_Num; }
            set { m_Num = value; }
        }

#if DATACONTRACT
        [DataMember(Order = 2)]
#endif
        virtual public DayOfWeek DayOfWeek
        {
            get { return m_DayOfWeek; }
            set { m_DayOfWeek = value; }
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
            DDay.iCal.Serialization.iCalendar.WeekDaySerializer serializer =
                new DDay.iCal.Serialization.iCalendar.WeekDaySerializer();
            CopyFrom(serializer.Deserialize(new StringReader(value)) as ICopyable);
        }

        #endregion

        #region Overrides

        public override bool Equals(object obj)
        {
            if (obj is WeekDay)
            {
                WeekDay ds = (WeekDay)obj;
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
                IWeekDay bd = (IWeekDay)obj;
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
                int compare = this.DayOfWeek.CompareTo(bd.DayOfWeek);
                if (compare == 0)
                    compare = this.Offset.CompareTo(bd.Offset);
                return compare;
            }
        }

        #endregion

        #region Operators

        public static implicit operator WeekDay(DateTime dt)
        {
            return new WeekDay(dt.DayOfWeek, 0);
        }

        #endregion
    }    
}
