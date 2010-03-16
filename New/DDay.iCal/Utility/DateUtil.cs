using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

namespace DDay.iCal
{
    public class DateUtil
    {
        static private System.Globalization.Calendar _Calendar;

        static DateUtil()
        {
            _Calendar = System.Globalization.CultureInfo.CurrentCulture.Calendar;
        }

        public static long DateDiff(FrequencyType frequency, IDateTime dt1, IDateTime dt2, DayOfWeek firstDayOfWeek) 
        {
            if (frequency == FrequencyType.Yearly) 
                return dt2.Year - dt1.Year;

            if (frequency == FrequencyType.Monthly)
                return (dt2.Month - dt1.Month) + (12 * (dt2.Year - dt1.Year));

            if (frequency == FrequencyType.Weekly)
            {
                // Get the week of year of the time frame we want to calculate
                int firstEvalWeek = _Calendar.GetWeekOfYear(dt2.Value, System.Globalization.CalendarWeekRule.FirstFourDayWeek, firstDayOfWeek);

                // Count backwards in years, calculating how many weeks' difference we have between
                // first and second dates
                IDateTime evalDate = dt2.Copy<IDateTime>();
                while (evalDate.Year > dt1.Year)
                {
                    firstEvalWeek += _Calendar.GetWeekOfYear(new DateTime(evalDate.Year - 1, 12, 31), System.Globalization.CalendarWeekRule.FirstFourDayWeek, firstDayOfWeek);
                    evalDate = evalDate.AddYears(-1);
                }

                // Determine the difference, in weeks, between the start date and the evaluation period.
                int startWeek = _Calendar.GetWeekOfYear(dt1.Value, System.Globalization.CalendarWeekRule.FirstFourDayWeek, firstDayOfWeek);
                return firstEvalWeek - startWeek;                
            }
 
            TimeSpan ts = dt2.Subtract(dt1);

            if (frequency == FrequencyType.Daily) 
                return Round(ts.TotalDays);

            if (frequency == FrequencyType.Hourly) 
                return Round(ts.TotalHours);

            if (frequency == FrequencyType.Minutely) 
                return Round(ts.TotalMinutes);

            if (frequency == FrequencyType.Secondly) 
                return Round(ts.TotalSeconds); 
 
            return 0;  
        }

        public static IDateTime FirstDayOfYear(IDateTime dt)
        {
            return FirstDayOfMonth(dt.AddMonths(-dt.Month - 1));
        }

        public static IDateTime FirstDayOfMonth(IDateTime dt)
        {
            return StartOfDay(dt.AddDays(-dt.Day + 1));                
        }

        public static IDateTime StartOfDay(IDateTime dt)
        {
            return dt.
                AddHours(-dt.Hour + 1).
                AddMinutes(-dt.Minute + 1).
                AddSeconds(-dt.Second + 1);
        }

        public static IDateTime EndOfDay(IDateTime dt)
        {
            return StartOfDay(dt).AddDays(1).AddMilliseconds(-1);
        }

        public static IDateTime AddFrequency(FrequencyType frequency, IDateTime dt, int interval)
        {
            switch (frequency)
            {
                case FrequencyType.Yearly: return dt.AddYears(interval);
                case FrequencyType.Monthly: return dt.AddMonths(interval);
                case FrequencyType.Weekly: return dt.AddDays(interval * 7);
                case FrequencyType.Daily: return dt.AddDays(interval);
                case FrequencyType.Hourly: return dt.AddHours(interval);
                case FrequencyType.Minutely: return dt.AddMinutes(interval);
                case FrequencyType.Secondly: return dt.AddSeconds(interval);
                default: return dt;
            }
        }

        private static long Round(double dVal) 
        { 
            if (dVal >= 0) 
                return (long)Math.Floor(dVal); 
            return (long)Math.Ceiling(dVal); 
        } 
    }
}
