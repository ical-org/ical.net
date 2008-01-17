using System;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using DDay.iCal.Components;

namespace DDay.iCal.DataTypes
{
    /// <summary>
    /// An iCalendar list of recurring dates (or date exclusions)
    /// </summary>
    public class RDate : iCalDataType
    {
        #region Private Fields

        private List<Period> m_Periods = new List<Period>();
        private TZID m_TZID;

        #endregion

        #region Public Properties

        public TZID TZID
        {
            get
            {
                if (m_TZID == null && Parameters.ContainsKey("TZID"))
                    m_TZID = new TZID(((Parameter)Parameters["TZID"]).Values[0]);
                return m_TZID;
            }
            set { m_TZID = value; }
        }

        public List<Period> Periods
        {
            get { return m_Periods; }
            set { m_Periods = value; }
        }

        #endregion

        #region Constructors

        public RDate() { }
        public RDate(string value) : this()
        {
            CopyFrom((RDate)Parse(value));
        }

        #endregion

        #region Public Methods

        public void Add(Date_Time dt)
        {
            Periods.Add(new Period(dt));
        }

        public void Add(Period p)
        {
            Periods.Add(p);
        }

        public void Remove(Date_Time dt)
        {
            Periods.Remove(new Period(dt));
        }

        public void Remove(Period p)
        {
            Periods.Remove(p);
        }

        #endregion

        #region Overrides

        public override bool Equals(object obj)
        {
            if (obj is RDate)
            {
                RDate r = (RDate)obj;

                if (!Periods.Count.Equals(r.Periods.Count))
                    return false;

                for (int i = 0; i < Periods.Count; i++)
                    if (!Periods[i].Equals(r.Periods[i]))
                        return false;

                return true;
            }
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            int hashCode = 0;
            foreach (Period p in Periods)
                hashCode ^= p.GetHashCode();
            return hashCode;
        }
 
        public override void CopyFrom(object obj)
        {
            base.CopyFrom(obj);
            if (obj is RDate)
            {
                RDate rdt = (RDate)obj;
                foreach (Period p in rdt.Periods)
                    Periods.Add(p.Copy());
            }
            base.CopyFrom(obj);
        }

        public override bool TryParse(string value, ref object obj)
        {
            string[] values = value.Split(',');
            foreach (string v in values)
            {
                object dt = new Date_Time();
                object p = new Period();
                
                //
                // Set the iCalendar for each Date_Time object here,
                // so that any time zones applied to these objects will be
                // handled correctly.
                // NOTE: fixes RRULE30 eval, where EXDATE references a 
                // DATE-TIME without a time zone; therefore, the time zone
                // is implied from DTSTART, and would fail to do a proper
                // time zone lookup, because it wasn't assigned an iCalendar
                // object.
                //
                if (((Date_Time)dt).TryParse(v, ref dt))
                {
                    ((Date_Time)dt).iCalendar = iCalendar;
                    ((Date_Time)dt).TZID = TZID;
                    Periods.Add(new Period((Date_Time)dt));
                }
                else if (((Period)p).TryParse(v, ref p))
                {
                    ((Period)p).StartTime.iCalendar = ((Period)p).EndTime.iCalendar = iCalendar;
                    ((Period)p).StartTime.TZID = ((Period)p).EndTime.TZID = TZID;
                    Periods.Add((Period)p);
                }
                else return false;
            }
            return true;
        }

        #endregion

        #region Public Methods

        public List<Period> Evaluate(Date_Time StartDate, Date_Time FromDate, Date_Time EndDate)
        {
            List<Period> periods = new List<Period>();

            if (StartDate > FromDate)
                FromDate = StartDate;

            if (EndDate < FromDate ||
                FromDate > EndDate)
                return periods;

            foreach (Period p in Periods)
                if (!periods.Contains(p))
                    periods.Add(p);

            return periods;
        }

        #endregion
    }
}
