using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using DDay.iCal.DataTypes;
using DDay.iCal.Components;

namespace DDay.iCal.Serialization.iCalendar.DataTypes
{
    public class RecurSerializer : FieldSerializer
    {
        #region Private Fields

        private DDay.iCal.DataTypes.Recur m_Recur;

        #endregion

        #region Constructors

        public RecurSerializer(DDay.iCal.DataTypes.Recur recur)
            : base(recur)
        {
            this.m_Recur = recur;
        }

        #endregion

        #region ISerializable Members

        public override string SerializeToString()
        {
            List<string> values = new List<string>();

            values.Add("FREQ=" + Enum.GetName(typeof(Recur.FrequencyType), m_Recur.Frequency));

            //-- FROM RFC2445 --
            //The INTERVAL rule part contains a positive integer representing how
            //often the recurrence rule repeats. The default value is "1", meaning
            //every second for a SECONDLY rule, or every minute for a MINUTELY
            //rule, every hour for an HOURLY rule, every day for a DAILY rule,
            //every week for a WEEKLY rule, every month for a MONTHLY rule and
            //every year for a YEARLY rule.
            
            int interval = m_Recur.Interval;
            if (interval == int.MinValue)
                interval = 1;

            if (interval != 1)
                values.Add("INTERVAL=" + interval);

            if (m_Recur.Until != null)
            {
                ISerializable serializer = new Date_TimeUTCSerializer(m_Recur.Until);
                if (serializer != null)
                    values.Add("UNTIL=" + serializer.SerializeToString());
            }

            if (m_Recur.Wkst != DayOfWeek.Monday)
                values.Add("WKST=" + Enum.GetName(typeof(DayOfWeek), m_Recur.Wkst).ToUpper().Substring(0, 2));

            if (m_Recur.Count != int.MinValue)
                values.Add("COUNT=" + m_Recur.Count);

            if (m_Recur.ByDay.Count > 0)
            {
                List<string> bydayValues = new List<string>();

                foreach (Recur.DaySpecifier byday in m_Recur.ByDay)
                {
                    ISerializable serializer = new DaySpecifierSerializer(byday);
                    if (serializer != null)
                        bydayValues.Add(serializer.SerializeToString());
                }

                values.Add("BYDAY=" + string.Join(",", bydayValues.ToArray()));
            }

            SerializeByValue(values, m_Recur.ByHour, "BYHOUR");
            SerializeByValue(values, m_Recur.ByMinute, "BYMINUTE");
            SerializeByValue(values, m_Recur.ByMonth, "BYMONTH");
            SerializeByValue(values, m_Recur.ByMonthDay, "BYMONTHDAY");
            SerializeByValue(values, m_Recur.BySecond, "BYSECOND");
            SerializeByValue(values, m_Recur.BySetPos, "BYSETPOS");
            SerializeByValue(values, m_Recur.ByWeekNo, "BYWEEKNO");
            SerializeByValue(values, m_Recur.ByYearDay, "BYYEARDAY");

            return string.Join(";", values.ToArray());
        }

        private void SerializeByValue(List<string> Aggregate, List<int> ByValue, string Name)
        {
            if (ByValue.Count > 0)
            {
                List<string> byValues = new List<string>();
                foreach (int i in ByValue)
                    byValues.Add(i.ToString());

                Aggregate.Add(Name + "=" + string.Join(",", byValues.ToArray()));
            }
        }

        #endregion
    }
}
