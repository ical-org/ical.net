using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Runtime.Serialization;

namespace DDay.iCal.Serialization.iCalendar
{
    public class DateTimeSerializer : 
        EncodableDataTypeSerializer
    {
        #region Private Methods

        private DateTime CoerceDateTime(int year, int month, int day, int hour, int minute, int second, DateTimeKind kind)
        {
            DateTime dt = DateTime.MinValue;

            // NOTE: determine if a date/time value exceeds the representable date/time values in .NET.
            // If so, let's automatically adjust the date/time to compensate.
            // FIXME: should we have a parsing setting that will throw an exception
            // instead of automatically adjusting the date/time value to the
            // closest representable date/time?
            try
            {
                if (year > 9999)
                    dt = DateTime.MaxValue;
                else if (year > 0)
                    dt = new DateTime(year, month, day, hour, minute, second, kind);
            }
            catch
            {
            }

            return dt;
        }

        #endregion

        #region Overrides

        public override Type TargetType
        {
            get { return typeof(iCalDateTime); }
        }

        public override string SerializeToString(object obj)
        {            
            if (obj is IDateTime)
            {
                string value = string.Empty;

                IDateTime dt = (IDateTime)obj;
                value += string.Format("{0:0000}{1:00}{2:00}", dt.Year, dt.Month, dt.Day);
                if (dt.HasTime)
                {
                    value += string.Format("T{0:00}{1:00}{2:00}", dt.Hour, dt.Minute, dt.Second);
                    if (dt.IsUniversalTime)
                        value += "Z";
                }

                // Encode the value as necessary
                return Encode(dt, value);
            }
            return null;
        }

        public override object Deserialize(TextReader tr)
        {
            string value = tr.ReadToEnd();

            IDateTime dt = CreateAndAssociate() as IDateTime;
            if (dt != null)
            {
                // Assign the TZID for the date/time value.
                if (dt.AssociatedParameters != null)
                    dt.TZID = dt.AssociatedParameters.Get("TZID");

                // Decode the value as necessary
                value = Decode(dt, value);
                string[] values = value.Split('T');

                Match match = Regex.Match(value, @"^((\d{4})(\d{2})(\d{2}))?T?((\d{2})(\d{2})(\d{2})(Z)?)?$", RegexOptions.IgnoreCase);
                if (!match.Success)
                    return null;
                else
                {
                    DateTime now = DateTime.Now;

                    int year = now.Year;
                    int month = now.Month;
                    int date = now.Day;
                    int hour = 0;
                    int minute = 0;
                    int second = 0;

                    if (match.Groups[1].Success)
                    {
                        dt.HasDate = true;
                        year = Convert.ToInt32(match.Groups[2].Value);
                        month = Convert.ToInt32(match.Groups[3].Value);
                        date = Convert.ToInt32(match.Groups[4].Value);
                    }
                    if (match.Groups[5].Success)
                    {
                        dt.HasTime = true;
                        hour = Convert.ToInt32(match.Groups[6].Value);
                        minute = Convert.ToInt32(match.Groups[7].Value);
                        second = Convert.ToInt32(match.Groups[8].Value);
                    }

                    if (match.Groups[9].Success)
                        dt.IsUniversalTime = true;

                    dt.Value = CoerceDateTime(year, month, date, hour, minute, second, DateTimeKind.Utc);
                    return dt;
                }
            }
            return null;
        }

        #endregion
    }
}
