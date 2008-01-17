using System;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;

namespace DDay.iCal.DataTypes
{
    /// <summary>
    /// Represents a time offset from UTC (Coordinated Universal Time).
    /// </summary>
    public class UTC_Offset : iCalDataType
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

        public UTC_Offset() { }
        public UTC_Offset(string value)
            : this()
        {
            CopyFrom((UTC_Offset)Parse(value));
        }

        #endregion

        #region Overrides

        public override void CopyFrom(object obj)
        {
            base.CopyFrom(obj);
            if (obj is UTC_Offset)
            {
                UTC_Offset utco = (UTC_Offset)obj;
                this.Positive = utco.Positive;
                this.Hours = utco.Hours;
                this.Minutes = utco.Minutes;
                this.Seconds = utco.Seconds;
            }
            base.CopyFrom(obj);
        }

        public override bool TryParse(string value, ref object obj)
        {
            UTC_Offset utco = (UTC_Offset)obj;
            Match match = Regex.Match(value, @"(\+|-)(\d{2})(\d{2})(\d{2})?");
            if (match.Success)
            {
                try
                {
                    if (match.Groups[0].Value == "+")
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

        /// <summary>
        /// Returns a typed copy of the object.
        /// </summary>
        /// <returns>A typed copy of the object.</returns>
        public new UTC_Offset Copy()
        {
            return (UTC_Offset)base.Copy();
        }

        #endregion

        #region Operators

        static public implicit operator UTC_Offset(TimeSpan ts)
        {
            UTC_Offset off = new UTC_Offset();
            if (ts.Ticks >= 0)
                off.Positive = true;
            off.Hours = Math.Abs(ts.Hours);
            off.Minutes = Math.Abs(ts.Minutes);
            off.Seconds = Math.Abs(ts.Seconds);
            return off;
        }

        #endregion
    }
}
