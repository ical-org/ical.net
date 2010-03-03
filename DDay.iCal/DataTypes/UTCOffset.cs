using System;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
using System.Runtime.Serialization;
using DDay.iCal.Serialization;

namespace DDay.iCal
{
    /// <summary>
    /// Represents a time offset from UTC (Coordinated Universal Time).
    /// </summary>
#if DATACONTRACT
    [DataContract(Name = "UTCOffset", Namespace = "http://www.ddaysoftware.com/dday.ical/2009/07/")]
#endif
    [Serializable]
    public class UTCOffset : 
        CalendarDataType,
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
        virtual public bool Positive
        {
            get { return m_Positive; }
            set { m_Positive = value; }
        }

#if DATACONTRACT
        [DataMember(Order = 2)]
#endif
        virtual public int Hours
        {
            get { return m_Hours; }
            set { m_Hours = value; }
        }

#if DATACONTRACT
        [DataMember(Order = 3)]
#endif
        virtual public int Minutes
        {
            get { return m_Minutes; }
            set { m_Minutes = value; }
        }

#if DATACONTRACT
        [DataMember(Order = 4)]
#endif
        virtual public int Seconds
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
            CopyFrom((UTCOffset)Parse(value));
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
            if (obj is UTCOffset)
            {
                UTCOffset utco = (UTCOffset)obj;
                this.Positive = utco.Positive;
                this.Hours = utco.Hours;
                this.Minutes = utco.Minutes;
                this.Seconds = utco.Seconds;
            }
            base.CopyFrom(obj);
        }

        public override bool TryParse(string value, ref ICalendarDataType obj)
        {
            UTCOffset utco = (UTCOffset)obj;
            Match match = Regex.Match(value, @"(\+|-)(\d{2})(\d{2})(\d{2})?");
            if (match.Success)
            {
                try
                {
                    // NOTE: Fixes bug #1874174 - TimeZone positive UTC_Offsets don't parse correctly
                    if (match.Groups[1].Value == "+")
                        utco.Positive = true;
                    utco.Hours = Int32.Parse(match.Groups[2].Value);
                    utco.Minutes = Int32.Parse(match.Groups[3].Value);
                    if (match.Groups[4].Success)
                        utco.Seconds = Int32.Parse(match.Groups[4].Value);
                }
                catch
                {
                    return false;
                }
                return true;
            }

            return false;
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
    }
}
