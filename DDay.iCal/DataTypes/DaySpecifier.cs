using System;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;

namespace DDay.iCal.DataTypes
{
    /// <summary>
    /// Represents an RFC 2445 "BYDAY" value.
    /// </summary>
    public partial class Recur : iCalDataType
    {
        public class DaySpecifier : iCalDataType, IComparable
        {
            #region Private Fields

            private int m_Num;            
            private DayOfWeek m_DayOfWeek;            

            #endregion

            #region Public Properties

            public int Num
            {
                get { return m_Num; }
                set { m_Num = value; }
            }

            public DayOfWeek DayOfWeek
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

            public DaySpecifier(string value)
                : this()
            {
                CopyFrom((DaySpecifier)Parse(value));
            }

            public DaySpecifier(DayOfWeek day)
                : this()
            {
                this.DayOfWeek = day;
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

            public override void CopyFrom(object obj)
            {
                if (obj is DaySpecifier)
                {
                    DaySpecifier bd = (DaySpecifier)obj;
                    this.Num = bd.Num;
                    this.DayOfWeek = bd.DayOfWeek;
                }
                base.CopyFrom(obj);
            }

            public override bool TryParse(string value, ref object obj)
            {
                DaySpecifier bd = (DaySpecifier)obj;

                Match bdMatch = Regex.Match(value, @"(\+|-)?(\d{1,2})?(\w{2})");
                if (bdMatch.Success)
                {
                    if (bdMatch.Groups[2].Success)
                    {
                        bd.Num = Convert.ToInt32(bdMatch.Groups[2].Value);
                        if (bdMatch.Groups[1].Success && bdMatch.Groups[1].Value.Contains("-"))
                            bd.Num *= -1;
                    }
                    bd.DayOfWeek = Recur.GetDayOfWeek(bdMatch.Groups[3].Value);
                    return true;
                }
                return false;
            }

            #endregion

            #region Public Methods

            public bool CheckValidDate(Recur r, Date_Time Date)
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
                        case Recur.FrequencyType.MONTHLY:
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

                        case Recur.FrequencyType.YEARLY:
                            {
                                // If BYMONTH is specified, then offset our tests
                                // by those months; otherwise, begin with Jan. 1st.
                                // NOTE: fixes USHolidays.ics eval
                                ArrayList months = new ArrayList();
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
                    bd = new DaySpecifier(obj as string);
                else if (obj is DaySpecifier)
                    bd = (DaySpecifier)obj;

                if (bd == null)
                    throw new ArgumentException();
                else return this.DayOfWeek.CompareTo(bd.DayOfWeek);
            }

            #endregion
        }
    }
}
