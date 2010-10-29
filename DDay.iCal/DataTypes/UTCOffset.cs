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
#if !SILVERLIGHT
    [Serializable]
#endif
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

        public bool Positive
        {
            get { return m_Positive; }
            set { m_Positive = value; }
        }

        public int Hours
        {
            get { return m_Hours; }
            set { m_Hours = value; }
        }

        public int Minutes
        {
            get { return m_Minutes; }
            set { m_Minutes = value; }
        }

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

        #region Private Methods

        private DateTime Offset(DateTime dt, bool positive)
        {
            if ((dt == DateTime.MinValue && positive) ||
                (dt == DateTime.MaxValue && !positive))
                return dt;

            int mult = positive ? 1 : -1;
            dt = dt.AddHours(Hours * mult);
            dt = dt.AddMinutes(Minutes * mult);
            dt = dt.AddSeconds(Seconds * mult);
            return dt;
        }

        #endregion

        #region IUTCOffset Members

        virtual public DateTime ToUTC(DateTime dt)
        {
            return DateTime.SpecifyKind(Offset(dt, !Positive), DateTimeKind.Utc);
        }

        virtual public DateTime ToLocal(DateTime dt)
        {
            return DateTime.SpecifyKind(Offset(dt, Positive), DateTimeKind.Local);
        }

        #endregion
    }
}
