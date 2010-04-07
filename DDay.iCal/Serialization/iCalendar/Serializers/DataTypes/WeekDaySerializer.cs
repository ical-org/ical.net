using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

namespace DDay.iCal.Serialization.iCalendar
{
    public class WeekDaySerializer :
        EncodableDataTypeSerializer
    {
        public override Type TargetType
        {
            get { return typeof(WeekDay); }
        }

        public override string SerializeToString(object obj)
        {
            IWeekDay ds = obj as IWeekDay;
            if (ds != null)
            {
                string value = string.Empty;
                if (ds.Offset != int.MinValue)
                    value += ds.Offset;
                value += Enum.GetName(typeof(DayOfWeek), ds.DayOfWeek).ToUpper().Substring(0, 2);

                return Encode(ds, value);
            }
            return null;
        }

        public override object Deserialize(TextReader tr)
        {
            string value = tr.ReadToEnd();

            // Create the day specifier and associate it with a calendar object
            IWeekDay ds = CreateAndAssociate() as IWeekDay;

            // Decode the value, if necessary
            value = Decode(ds, value);

            Match match = Regex.Match(value, @"(\+|-)?(\d{1,2})?(\w{2})");
            if (match.Success)
            {
                if (match.Groups[2].Success)
                {
                    ds.Offset = Convert.ToInt32(match.Groups[2].Value);
                    if (match.Groups[1].Success && match.Groups[1].Value.Contains("-"))
                        ds.Offset *= -1;
                }
                ds.DayOfWeek = RecurrencePatternSerializer.GetDayOfWeek(match.Groups[3].Value);
                return ds;
            }

            return null;
        }
    }
}
