using System;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
using System.Runtime.Serialization;
using System.IO;
using DDay.iCal.Serialization.iCalendar;

namespace DDay.iCal
{
    /// <summary>
    /// Represents a time offset from UTC (Coordinated Universal Time).
    /// </summary>
#if DATACONTRACT
    [DataContract(Name = "UTC_Offset", Namespace = "http://www.ddaysoftware.com/dday.ical/2009/07/")]
#endif
    [Serializable]
    public class UTCOffset : 
        EncodableDataType,
        IUTCOffset
    {
        #region Private Fields

        private bool m_Positive = false;
        private int m_Hours;
        private int m_Minutes;
        private int m_Seconds = 0;
        
        #endregion

        #region Public Properties

#if DATACONTRACT
        [DataMember(Order = 1)]
#endif
        public bool Positive
        {
            get { return m_Positive; }
            set { m_Positive = value; }
        }

#if DATACONTRACT
        [DataMember(Order = 2)]
#endif
        public int Hours
        {
            get { return m_Hours; }
            set { m_Hours = value; }
        }

#if DATACONTRACT
        [DataMember(Order = 3)]
#endif
        public int Minutes
        {
            get { return m_Minutes; }
            set { m_Minutes = value; }
        }

#if DATACONTRACT
        [DataMember(Order = 4)]
#endif
        public int Seconds
        {
            get { return m_Seconds; }
            set { m_Seconds = value; }
        }

        #endregion

        #region Constructors

        public UTCOffset() { }
        public UTCOffset(string value)
            : this()
        {
            UTCOffsetSerializer serializer = new UTCOffsetSerializer();            
            CopyFrom(serializer.Deserialize(new StringReader(value)) as ICopyable);
        }

        public UTCOffset(TimeSpan ts)
        {            
            if (ts.Ticks >= 0)
                Positive = true;
            Hours = Math.Abs(ts.Hours);
            Minutes = Math.Abs(ts.Minutes);
            Seconds = Math.Abs(ts.Seconds);
        }

        #endregion

        #region Overrides

        public override bool Equals(object obj)
        {
            UTCOffset o = obj as UTCOffset;
            if (o != null)
            {
                return object.Equals(Positive, o.Positive) &&
                    object.Equals(Hours, o.Hours) &&
                    object.Equals(Minutes, o.Minutes) &&
                    object.Equals(Seconds, o.Seconds);
            }
            return base.Equals(obj);
        }

        public override void CopyFrom(ICopyable obj)
        {
            base.CopyFrom(obj);
            if (obj is IUTCOffset)
            {
                IUTCOffset utco = (IUTCOffset)obj;
                this.Positive = utco.Positive;
                this.Hours = utco.Hours;
                this.Minutes = utco.Minutes;
                this.Seconds = utco.Seconds;
            }
        }

        public override string ToString()
        {
            return (this.Positive ? "+" : "-") +
                this.Hours.ToString("00") +
                this.Minutes.ToString("00") +
                (this.Seconds != 0 ? this.Seconds.ToString("00") : string.Empty);
        }

        #endregion

        #region Operators

        static public implicit operator UTCOffset(TimeSpan ts)
        {
            return new UTCOffset(ts);
        }

        static public explicit operator TimeSpan(UTCOffset o)
        {
            TimeSpan ts = new TimeSpan(0);
            ts = ts.Add(TimeSpan.FromHours(o.Positive ? o.Hours : -o.Hours));
            ts = ts.Add(TimeSpan.FromMinutes(o.Positive ? o.Minutes : -o.Minutes));
            ts = ts.Add(TimeSpan.FromSeconds(o.Positive ? o.Seconds : -o.Seconds));
            return ts;
        }

        #endregion

        #region IUTCOffset Members

        virtual public DateTime Offset(DateTime dt)
        {
            if ((dt == DateTime.MinValue && !Positive) ||
                (dt == DateTime.MaxValue && Positive))
                return dt;

            int mult = Positive ? -1 : 1;
            dt = dt.AddHours(Hours * mult);
            dt = dt.AddMinutes(Minutes * mult);
            dt = dt.AddSeconds(Seconds * mult);
            return dt;
        }

        #endregion
    }
}
