using System;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
using DDay.iCal.Components;

namespace DDay.iCal.DataTypes
{
    /// <summary>
    /// An iCalendar list of recurring dates (or date exclusions)
    /// </summary>
    public class RDate : iCalDataType
    {
        #region Private Fields

        private ArrayList m_Items = new ArrayList();
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

        public ArrayList Items
        {
          get { return m_Items; }
          set { m_Items = value; }
        }

        #endregion

        #region Constructors

        public RDate() { }
        public RDate(string value) : this()
        {
            CopyFrom((RDate)Parse(value));
        }

        #endregion

        #region Overrides

        public override bool Equals(object obj)
        {
            if (obj is RDate)
            {
                RDate r = (RDate)obj;
                for (int i = 0; i < m_Items.Count; i++)
                    if (!Items[i].Equals(r.Items[i]))
                        return false;
                return true;
            }
            return base.Equals(obj);
        }
 
        public override void CopyFrom(object obj)
        {
            if (obj is RDate)
            {
                RDate rdt = (RDate)obj;
                foreach (object o in rdt.Items)
                    Items.Add(o);
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
                    Items.Add(dt);
                }
                else if (((Period)p).TryParse(v, ref p))
                {
                    ((Period)p).StartTime.iCalendar = ((Period)p).EndTime.iCalendar = iCalendar;
                    ((Period)p).StartTime.TZID = ((Period)p).EndTime.TZID = TZID;
                    Items.Add(p);
                }
                else return false;
            }
            return true;
        }

        #endregion

        #region public Methods

        public ArrayList Evaluate(Date_Time StartDate, Date_Time FromDate, Date_Time EndDate)
        {
            ArrayList Periods = new ArrayList();

            if (StartDate > FromDate)
                FromDate = StartDate;

            if (EndDate < FromDate ||
                FromDate > EndDate)
                return Periods;
            
            foreach (object obj in Items)
                if (!Periods.Contains(obj))
                    Periods.Add(obj);                

            return Periods;
        }

        #endregion
    }
}
