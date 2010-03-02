using System;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using DDay.iCal;
using System.Runtime.Serialization;
using DDay.iCal.Serialization;

namespace DDay.iCal
{
    /// <summary>
    /// An iCalendar list of recurring dates (or date exclusions)
    /// </summary>
#if DATACONTRACT
    [DataContract(Name = "RecurrenceDates", Namespace = "http://www.ddaysoftware.com/dday.ical/2009/07/")]
#endif
    [Serializable]
    public class RecurrenceDates : iCalDataType
    {
        #region Private Fields

        private List<Period> m_Periods = new List<Period>();

        #endregion

        #region Public Properties

        public TZID TZID
        {
            get { return Parameters.Get<TZID>("TZID"); }
            set { Parameters.Set("TZID", value); }
        }

#if DATACONTRACT
        [DataMember(Order = 1)]
#endif
        public List<Period> Periods
        {
            get { return m_Periods; }
            set { m_Periods = value; }
        }

        #endregion

        #region Constructors

        public RecurrenceDates() { }
        public RecurrenceDates(string value) : this() 
        {
            CopyFrom((RecurrenceDates)Parse(value));
        }

        #endregion

        #region Public Methods

        public void Add(iCalDateTime dt)
        {
            Periods.Add(new Period(dt));
        }

        public void Add(Period p)
        {
            Periods.Add(p);
        }

        public void Remove(iCalDateTime dt)
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
            if (obj is RecurrenceDates)
            {
                RecurrenceDates r = (RecurrenceDates)obj;

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

        public override void CopyFrom(ICopyable obj)
        {
            base.CopyFrom(obj);
            if (obj is RecurrenceDates)
            {
                RecurrenceDates rdt = (RecurrenceDates)obj;
                foreach (Period p in rdt.Periods)
                    Periods.Add(p.Copy<Period>());
            }
            base.CopyFrom(obj);
        }

        public override bool TryParse(string value, ref ICalendarDataType obj)
        {
            string[] values = value.Split(',');
            foreach (string v in values)
            {
                ICalendarDataType dt = new iCalDateTime();
                ICalendarDataType p = new Period();
                
                // FIXME: can we remove the following comment?
                // Seems like it no longer applies...
                
                //
                // Set the iCalendar for each iCalDateTime object here,
                // so that any time zones applied to these objects will be
                // handled correctly.
                // NOTE: fixes RRULE30 eval, where EXDATE references a 
                // DATE-TIME without a time zone; therefore, the time zone
                // is implied from DTSTART, and would fail to do a proper
                // time zone lookup, because it wasn't assigned an iCalendar
                // object.
                //
                if (((iCalDateTime)dt).TryParse(v, ref dt))
                {
                    ((iCalDateTime)dt).TZID = TZID;
                    Periods.Add(new Period((iCalDateTime)dt));
                }
                else if (((Period)p).TryParse(v, ref p))
                {
                    ((Period)p).StartTime.TZID = ((Period)p).EndTime.TZID = TZID;
                    Periods.Add((Period)p);
                }
                else return false;
            }
            return true;
        }

        #endregion

        #region Public Methods

        public List<Period> Evaluate(iCalDateTime StartDate, iCalDateTime FromDate, iCalDateTime EndDate)
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
