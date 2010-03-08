using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

namespace DDay.iCal
{
    public class DateUtils
    {
        static private System.Globalization.Calendar _Calendar;

        static DateUtils()
        {
            _Calendar = System.Globalization.CultureInfo.CurrentCulture.Calendar;
        }

        public static long DateDiff(FrequencyType frequency, iCalDateTime dt1, iCalDateTime dt2, DayOfWeek firstDayOfWeek) 
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
                iCalDateTime evalDate = dt2;
                while (evalDate.Year > dt1.Year)
                {
                    firstEvalWeek += _Calendar.GetWeekOfYear(new DateTime(evalDate.Year - 1, 12, 31), System.Globalization.CalendarWeekRule.FirstFourDayWeek, firstDayOfWeek);
                    evalDate = evalDate.AddYears(-1);
                }

                // Determine the difference, in weeks, between the start date and the evaluation period.
                int startWeek = _Calendar.GetWeekOfYear(dt1.Value, System.Globalization.CalendarWeekRule.FirstFourDayWeek, firstDayOfWeek);
                return firstEvalWeek - startWeek;                
            }
 
            TimeSpan ts = dt2 - dt1;

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

        public static iCalDateTime AddFrequency(FrequencyType frequency, iCalDateTime dt, int interval)
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
