using System;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Runtime.Serialization;
using DDay.iCal.Serialization;
using System.IO;

namespace DDay.iCal
{
    /// <summary>
    /// Represents an RFC 5545 "BYDAY" value.
    /// </summary>
#if DATACONTRACT
    [DataContract(Name = "DaySpecifier", Namespace = "http://www.ddaysoftware.com/dday.ical/2009/07/")]
#endif
    [Serializable]
    public class DaySpecifier : 
        EncodableDataType,
        IDaySpecifier        
    {
        #region Private Fields

        private int m_Num = int.MinValue;            
        private DayOfWeek m_DayOfWeek;            

        #endregion

        #region Public Properties

#if DATACONTRACT
        [DataMember(Order = 1)]
#endif
        virtual public int Num
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

        public DaySpecifier()
        {
            Num = int.MinValue;
        }

        public DaySpecifier(DayOfWeek day)
            : this()
        {
            this.DayOfWeek = day;
        }

        public DaySpecifier(DayOfWeek day, int num)
            : this(day)
        {
            this.Num = num;
        }

        public DaySpecifier(DayOfWeek day, FrequencyOccurrence type)
            : this(day, (int)type)
        {
        }

        public DaySpecifier(string value)
        {
            DDay.iCal.Serialization.iCalendar.DaySpecifierSerializer serializer =
                new DDay.iCal.Serialization.iCalendar.DaySpecifierSerializer();
            CopyFrom(serializer.Deserialize(new StringReader(value)) as ICopyable);
        }

        #endregion

        #region Overrides

        public override bool Equals(object obj)
        {
            if (obj is DaySpecifier)
            {
                DaySpecifier ds = (DaySpecifier)obj;
                return ds.Num == Num &&
                    ds.DayOfWeek == DayOfWeek;
            }
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return Num.GetHashCode() ^ DayOfWeek.GetHashCode();
        }

        public override void CopyFrom(ICopyable obj)
        {
            base.CopyFrom(obj);
            if (obj is IDaySpecifier)
            {
                IDaySpecifier bd = (IDaySpecifier)obj;
                this.Num = bd.Num;
                this.DayOfWeek = bd.DayOfWeek;
            }
        }

        #endregion

        #region Public Methods

        public bool CheckValidDate(IRecurrencePattern r, IDateTime Date)
        {
            bool valid = false;

            if (this.DayOfWeek == Date.Value.DayOfWeek)
                valid = true;

            if (valid && this.Num != int.MinValue)
            {
                int mult = (this.Num < 0) ? -1 : 1;
                int offset = (this.Num < 0) ? 1 : 0;
                int abs = Math.Abs(this.Num);

                switch (r.Frequency)
                {
                    case FrequencyType.Monthly:
                        {
                            DateTime mondt = new DateTime(Date.Value.Year, Date.Value.Month, 1, Date.Value.Hour, Date.Value.Minute, Date.Value.Second, Date.Value.Kind);
                            mondt = DateTime.SpecifyKind(mondt, Date.Value.Kind);
                            if (offset > 0)
                                mondt = mondt.AddMonths(1).AddDays(-1);

                            while (mondt.DayOfWeek != this.DayOfWeek)
                                mondt = mondt.AddDays(mult);

                            for (int i = 1; i < abs; i++)
                                mondt = mondt.AddDays(7 * mult);

                            if (Date.Value.Date != mondt.Date)
                                valid = false;
                        } break;

                    case FrequencyType.Yearly:
                        {
                            // If BYMONTH is specified, then offset our tests
                            // by those months; otherwise, begin with Jan. 1st.
                            // NOTE: fixes USHolidays.ics eval
                            IList<int> months = new List<int>();
                            if (r.ByMonth.Count == 0)
                                months.Add(1);
                            else months = r.ByMonth;

                            bool found = false;
                            foreach (int month in months)
                            {
                                DateTime yeardt = new DateTime(Date.Value.Year, month, 1, Date.Value.Hour, Date.Value.Minute, Date.Value.Second, Date.Value.Kind);
                                yeardt = DateTime.SpecifyKind(yeardt, Date.Value.Kind);
                                if (offset > 0)
                                {
                                    // Start at end of year, or end of month if BYMONTH is specified
                                    if (r.ByMonth.Count == 0)
                                        yeardt = yeardt.AddYears(1).AddDays(-1);
                                    else yeardt = yeardt.AddMonths(1).AddDays(-1);
                                }

                                while (yeardt.DayOfWeek != this.DayOfWeek)
                                    yeardt = yeardt.AddDays(mult);

                                for (int i = 1; i < abs; i++)
                                    yeardt = yeardt.AddDays(7 * mult);

                                if (Date.Value == yeardt)
                                    found = true;
                            }

                            if (!found)
                                valid = false;
                        } break;

                    // Ignore other frequencies
                    default: break;
                }
            }
            return valid;
        }

        #endregion

        #region IComparable Members

        public int CompareTo(object obj)
        {
            DaySpecifier bd = null;
            if (obj is string)
                bd = new DaySpecifier(obj.ToString());
            else if (obj is DaySpecifier)
                bd = (DaySpecifier)obj;

            if (bd == null)
                throw new ArgumentException();
            else 
            {
                int compare = this.DayOfWeek.CompareTo(bd.DayOfWeek);
                if (compare == 0)
                    compare = this.Num.CompareTo(bd.Num);
                return compare;
            }
        }

        #endregion
    }    
}
