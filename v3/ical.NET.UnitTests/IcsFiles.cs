﻿using System.IO;
using System.Reflection;

namespace Ical.Net.UnitTests
{
    internal class IcsFiles
    {
        private static readonly Assembly _assembly = typeof(IcsFiles).GetTypeInfo().Assembly;

        internal static string ReadStream(string manifestResource)
        {
            using (var stream = _assembly.GetManifestResourceStream(manifestResource))
            {
                return new StreamReader(stream).ReadToEnd();
            }
        }

        internal static string Alarm1 => ReadStream("Ical.Net.UnitTests.Calendars.Alarm.ALARM1.ics");
        internal static string ALARM1 => ReadStream("Ical.Net.UnitTests.Calendars.Alarm.ALARM1.ics");
        internal static string ALARM2 => ReadStream("Ical.Net.UnitTests.Calendars.Alarm.ALARM2.ics");
        internal static string ALARM3 => ReadStream("Ical.Net.UnitTests.Calendars.Alarm.ALARM3.ics");
        internal static string ALARM4 => ReadStream("Ical.Net.UnitTests.Calendars.Alarm.ALARM4.ics");
        internal static string ALARM5 => ReadStream("Ical.Net.UnitTests.Calendars.Alarm.ALARM5.ics");
        internal static string ALARM6 => ReadStream("Ical.Net.UnitTests.Calendars.Alarm.ALARM6.ics");
        internal static string ALARM7 => ReadStream("Ical.Net.UnitTests.Calendars.Alarm.ALARM7.ics");
        internal static string Attachment3 => ReadStream("Ical.Net.UnitTests.Calendars.Serialization.Attachment3.ics");
        internal static string Attachment4 => ReadStream("Ical.Net.UnitTests.Calendars.Serialization.Attachment4.ics");
        internal static string Attendee1 => ReadStream("Ical.Net.UnitTests.Calendars.Serialization.Attendee1.ics");
        internal static string Attendee2 => ReadStream("Ical.Net.UnitTests.Calendars.Serialization.Attendee2.ics");
        internal static string Attendee3 => ReadStream("Ical.Net.UnitTests.Calendars.Serialization.Attendee3.ics");
        internal static string Bug1741093 => ReadStream("Ical.Net.UnitTests.Calendars.Recurrence.Bug1741093.ics");
        internal static string Bug2033495 => ReadStream("Ical.Net.UnitTests.Calendars.Serialization.Bug2033495.ics");
        internal static string Bug2148092 => ReadStream("Ical.Net.UnitTests.Calendars.Serialization.Bug2148092.ics");
        internal static string Bug2912657 => ReadStream("Ical.Net.UnitTests.Calendars.Recurrence.Bug2912657.ics");
        internal static string Bug2916581 => ReadStream("Ical.Net.UnitTests.Calendars.Recurrence.Bug2916581.ics");
        internal static string Bug2938007 => ReadStream("Ical.Net.UnitTests.Calendars.Serialization.Bug2938007.ics");
        internal static string Bug2959692 => ReadStream("Ical.Net.UnitTests.Calendars.Recurrence.Bug2959692.ics");
        internal static string Bug2966236 => ReadStream("Ical.Net.UnitTests.Calendars.Recurrence.Bug2966236.ics");
        internal static string Bug3007244 => ReadStream("Ical.Net.UnitTests.Calendars.Recurrence.Bug3007244.ics");
        internal static string ByMonth1 => ReadStream("Ical.Net.UnitTests.Calendars.Recurrence.ByMonth1.ics");
        internal static string ByMonth2 => ReadStream("Ical.Net.UnitTests.Calendars.Recurrence.ByMonth2.ics");
        internal static string ByMonthDay1 => ReadStream("Ical.Net.UnitTests.Calendars.Recurrence.ByMonthDay1.ics");
        internal static string Calendar1 => ReadStream("Ical.Net.UnitTests.Calendars.Serialization.Calendar1.ics");
        internal static string CalendarParameters2 => ReadStream("Ical.Net.UnitTests.Calendars.Serialization.CalendarParameters2.ics");
        internal static string CaseInsensitive1 => ReadStream("Ical.Net.UnitTests.Calendars.Serialization.CaseInsensitive1.ics");
        internal static string CaseInsensitive2 => ReadStream("Ical.Net.UnitTests.Calendars.Serialization.CaseInsensitive2.ics");
        internal static string CaseInsensitive3 => ReadStream("Ical.Net.UnitTests.Calendars.Serialization.CaseInsensitive3.ics");
        internal static string CaseInsensitive4 => ReadStream("Ical.Net.UnitTests.Calendars.Serialization.CaseInsensitive4.ics");
        internal static string Categories1 => ReadStream("Ical.Net.UnitTests.Calendars.Serialization.Categories1.ics");
        internal static string Daily1 => ReadStream("Ical.Net.UnitTests.Calendars.Recurrence.Daily1.ics");
        internal static string DailyByDay1 => ReadStream("Ical.Net.UnitTests.Calendars.Recurrence.DailyByDay1.ics");
        internal static string DailyByHourMinute1 => ReadStream("Ical.Net.UnitTests.Calendars.Recurrence.DailyByHourMinute1.ics");
        internal static string DailyCount1 => ReadStream("Ical.Net.UnitTests.Calendars.Recurrence.DailyCount1.ics");
        internal static string DailyCount2 => ReadStream("Ical.Net.UnitTests.Calendars.Recurrence.DailyCount2.ics");
        internal static string DailyInterval1 => ReadStream("Ical.Net.UnitTests.Calendars.Recurrence.DailyInterval1.ics");
        internal static string DailyInterval2 => ReadStream("Ical.Net.UnitTests.Calendars.Recurrence.DailyInterval2.ics");
        internal static string DailyUntil1 => ReadStream("Ical.Net.UnitTests.Calendars.Recurrence.DailyUntil1.ics");
        internal static string DateTime1 => ReadStream("Ical.Net.UnitTests.Calendars.Serialization.DateTime1.ics");
        internal static string DateTime2 => ReadStream("Ical.Net.UnitTests.Calendars.Serialization.DateTime2.ics");
        internal static string Duration1 => ReadStream("Ical.Net.UnitTests.Calendars.Serialization.Duration1.ics");
        internal static string Empty1 => ReadStream("Ical.Net.UnitTests.Calendars.Recurrence.Empty1.ics");
        internal static string EmptyLines1 => ReadStream("Ical.Net.UnitTests.Calendars.Serialization.EmptyLines1.ics");
        internal static string EmptyLines2 => ReadStream("Ical.Net.UnitTests.Calendars.Serialization.EmptyLines2.ics");
        internal static string EmptyLines3 => ReadStream("Ical.Net.UnitTests.Calendars.Serialization.EmptyLines3.ics");
        internal static string EmptyLines4 => ReadStream("Ical.Net.UnitTests.Calendars.Serialization.EmptyLines4.ics");
        internal static string Encoding1 => ReadStream("Ical.Net.UnitTests.Calendars.Serialization.Encoding1.ics");
        internal static string Encoding2 => ReadStream("Ical.Net.UnitTests.Calendars.Serialization.Encoding2.ics");
        internal static string Encoding3 => ReadStream("Ical.Net.UnitTests.Calendars.Serialization.Encoding3.ics");
        internal static string Event1 => ReadStream("Ical.Net.UnitTests.Calendars.Serialization.Event1.ics");
        internal static string Event2 => ReadStream("Ical.Net.UnitTests.Calendars.Serialization.Event2.ics");
        internal static string Event3 => ReadStream("Ical.Net.UnitTests.Calendars.Serialization.Event3.ics");
        internal static string Event4 => ReadStream("Ical.Net.UnitTests.Calendars.Serialization.Event4.ics");
        internal static string GeographicLocation1 => ReadStream("Ical.Net.UnitTests.Calendars.Serialization.GeographicLocation1.ics");
        internal static string Google1 => ReadStream("Ical.Net.UnitTests.Calendars.Serialization.Google1.ics");
        internal static string Hourly1 => ReadStream("Ical.Net.UnitTests.Calendars.Recurrence.Hourly1.ics");
        internal static string HourlyInterval1 => ReadStream("Ical.Net.UnitTests.Calendars.Recurrence.HourlyInterval1.ics");
        internal static string HourlyInterval2 => ReadStream("Ical.Net.UnitTests.Calendars.Recurrence.HourlyInterval2.ics");
        internal static string HourlyUntil1 => ReadStream("Ical.Net.UnitTests.Calendars.Recurrence.HourlyUntil1.ics");
        internal static string JOURNAL1 => ReadStream("Ical.Net.UnitTests.Calendars.Journal.JOURNAL1.ics");
        internal static string JOURNAL2 => ReadStream("Ical.Net.UnitTests.Calendars.Journal.JOURNAL2.ics");
        internal static string Language1 => ReadStream("Ical.Net.UnitTests.Calendars.Serialization.Language1.ics");
        internal static string Language2 => ReadStream("Ical.Net.UnitTests.Calendars.Serialization.Language2.ics");
        internal static string Language3 => ReadStream("Ical.Net.UnitTests.Calendars.Serialization.Language3.ics");
        internal static string Language4 => ReadStream("Ical.Net.UnitTests.Calendars.Serialization.Language4.ics");
        internal static string Minutely1 => ReadStream("Ical.Net.UnitTests.Calendars.Recurrence.Minutely1.ics");
        internal static string MinutelyByHour1 => ReadStream("Ical.Net.UnitTests.Calendars.Recurrence.MinutelyByHour1.ics");
        internal static string MinutelyCount1 => ReadStream("Ical.Net.UnitTests.Calendars.Recurrence.MinutelyCount1.ics");
        internal static string MinutelyCount2 => ReadStream("Ical.Net.UnitTests.Calendars.Recurrence.MinutelyCount2.ics");
        internal static string MinutelyCount3 => ReadStream("Ical.Net.UnitTests.Calendars.Recurrence.MinutelyCount3.ics");
        internal static string MinutelyCount4 => ReadStream("Ical.Net.UnitTests.Calendars.Recurrence.MinutelyCount4.ics");
        internal static string MinutelyInterval1 => ReadStream("Ical.Net.UnitTests.Calendars.Recurrence.MinutelyInterval1.ics");
        internal static string Monthly1 => ReadStream("Ical.Net.UnitTests.Calendars.Recurrence.Monthly1.ics");
        internal static string MonthlyByDay1 => ReadStream("Ical.Net.UnitTests.Calendars.Recurrence.MonthlyByDay1.ics");
        internal static string MonthlyByMonthDay1 => ReadStream("Ical.Net.UnitTests.Calendars.Recurrence.MonthlyByMonthDay1.ics");
        internal static string MonthlyByMonthDay2 => ReadStream("Ical.Net.UnitTests.Calendars.Recurrence.MonthlyByMonthDay2.ics");
        internal static string MonthlyBySetPos1 => ReadStream("Ical.Net.UnitTests.Calendars.Recurrence.MonthlyBySetPos1.ics");
        internal static string MonthlyBySetPos2 => ReadStream("Ical.Net.UnitTests.Calendars.Recurrence.MonthlyBySetPos2.ics");
        internal static string MonthlyCountByDay1 => ReadStream("Ical.Net.UnitTests.Calendars.Recurrence.MonthlyCountByDay1.ics");
        internal static string MonthlyCountByDay2 => ReadStream("Ical.Net.UnitTests.Calendars.Recurrence.MonthlyCountByDay2.ics");
        internal static string MonthlyCountByDay3 => ReadStream("Ical.Net.UnitTests.Calendars.Recurrence.MonthlyCountByDay3.ics");
        internal static string MonthlyCountByMonthDay1 => ReadStream("Ical.Net.UnitTests.Calendars.Recurrence.MonthlyCountByMonthDay1.ics");
        internal static string MonthlyCountByMonthDay2 => ReadStream("Ical.Net.UnitTests.Calendars.Recurrence.MonthlyCountByMonthDay2.ics");
        internal static string MonthlyCountByMonthDay3 => ReadStream("Ical.Net.UnitTests.Calendars.Recurrence.MonthlyCountByMonthDay3.ics");
        internal static string MonthlyInterval1 => ReadStream("Ical.Net.UnitTests.Calendars.Recurrence.MonthlyInterval1.ics");
        internal static string MonthlyUntilByDay1 => ReadStream("Ical.Net.UnitTests.Calendars.Recurrence.MonthlyUntilByDay1.ics");
        internal static string Outlook2007LineFolds => ReadStream("Ical.Net.UnitTests.Calendars.Serialization.Outlook2007LineFolds.ics");
        internal static string Parameter1 => ReadStream("Ical.Net.UnitTests.Calendars.Serialization.Parameter1.ics");
        internal static string Parameter2 => ReadStream("Ical.Net.UnitTests.Calendars.Serialization.Parameter2.ics");
        internal static string Parse1 => ReadStream("Ical.Net.UnitTests.Calendars.Serialization.Parse1.ics");
        internal static string PARSE17 => ReadStream("Ical.Net.UnitTests.Calendars.Serialization.PARSE17.ics");
        internal static string ProdID1 => ReadStream("Ical.Net.UnitTests.Calendars.Serialization.ProdID1.ics");
        internal static string ProdID2 => ReadStream("Ical.Net.UnitTests.Calendars.Serialization.ProdID2.ics");
        internal static string Property1 => ReadStream("Ical.Net.UnitTests.Calendars.Serialization.Property1.ics");
        internal static string RecurrenceDates1 => ReadStream("Ical.Net.UnitTests.Calendars.Serialization.RecurrenceDates1.ics");
        internal static string RequestStatus1 => ReadStream("Ical.Net.UnitTests.Calendars.Serialization.RequestStatus1.ics");
        internal static string Secondly1 => ReadStream("Ical.Net.UnitTests.Calendars.Recurrence.Secondly1.ics");
        internal static string TimeZone1 => ReadStream("Ical.Net.UnitTests.Calendars.Serialization.TimeZone1.ics");
        internal static string TimeZone2 => ReadStream("Ical.Net.UnitTests.Calendars.Serialization.TimeZone2.ics");
        internal static string TimeZone3 => ReadStream("Ical.Net.UnitTests.Calendars.Serialization.TimeZone3.ics");
        internal static string Todo1 => ReadStream("Ical.Net.UnitTests.Calendars.Todo.Todo1.ics");
        internal static string Todo2 => ReadStream("Ical.Net.UnitTests.Calendars.Todo.Todo2.ics");
        internal static string Todo3 => ReadStream("Ical.Net.UnitTests.Calendars.Todo.Todo3.ics");
        internal static string Todo4 => ReadStream("Ical.Net.UnitTests.Calendars.Todo.Todo4.ics");
        internal static string Todo5 => ReadStream("Ical.Net.UnitTests.Calendars.Todo.Todo5.ics");
        internal static string Todo6 => ReadStream("Ical.Net.UnitTests.Calendars.Todo.Todo6.ics");
        internal static string Todo7 => ReadStream("Ical.Net.UnitTests.Calendars.Todo.Todo7.ics");
        internal static string Todo8 => ReadStream("Ical.Net.UnitTests.Calendars.Todo.Todo8.ics");
        internal static string Todo9 => ReadStream("Ical.Net.UnitTests.Calendars.Todo.Todo9.ics");
        internal static string Transparency1 => ReadStream("Ical.Net.UnitTests.Calendars.Serialization.Transparency1.ics");
        internal static string Transparency2 => ReadStream("Ical.Net.UnitTests.Calendars.Serialization.Transparency2.ics");
        internal static string Trigger1 => ReadStream("Ical.Net.UnitTests.Calendars.Serialization.Trigger1.ics");
        internal static string USHolidays => ReadStream("Ical.Net.UnitTests.Calendars.Serialization.USHolidays.ics");
        internal static string WeeklyCount1 => ReadStream("Ical.Net.UnitTests.Calendars.Recurrence.WeeklyCount1.ics");
        internal static string WeeklyCountWkst1 => ReadStream("Ical.Net.UnitTests.Calendars.Recurrence.WeeklyCountWkst1.ics");
        internal static string WeeklyCountWkst2 => ReadStream("Ical.Net.UnitTests.Calendars.Recurrence.WeeklyCountWkst2.ics");
        internal static string WeeklyCountWkst3 => ReadStream("Ical.Net.UnitTests.Calendars.Recurrence.WeeklyCountWkst3.ics");
        internal static string WeeklyCountWkst4 => ReadStream("Ical.Net.UnitTests.Calendars.Recurrence.WeeklyCountWkst4.ics");
        internal static string WeeklyInterval1 => ReadStream("Ical.Net.UnitTests.Calendars.Recurrence.WeeklyInterval1.ics");
        internal static string WeeklyUntil1 => ReadStream("Ical.Net.UnitTests.Calendars.Recurrence.WeeklyUntil1.ics");
        internal static string WeeklyUntilWkst1 => ReadStream("Ical.Net.UnitTests.Calendars.Recurrence.WeeklyUntilWkst1.ics");
        internal static string WeeklyUntilWkst2 => ReadStream("Ical.Net.UnitTests.Calendars.Recurrence.WeeklyUntilWkst2.ics");
        internal static string WeeklyWeekStartsLastYear => ReadStream("Ical.Net.UnitTests.Calendars.Recurrence.WeeklyWeekStartsLastYear.ics");
        internal static string WeeklyWkst1 => ReadStream("Ical.Net.UnitTests.Calendars.Recurrence.WeeklyWkst1.ics");
        internal static string XProperty1 => ReadStream("Ical.Net.UnitTests.Calendars.Serialization.XProperty1.ics");
        internal static string XProperty2 => ReadStream("Ical.Net.UnitTests.Calendars.Serialization.XProperty2.ics");
        internal static string Yearly1 => ReadStream("Ical.Net.UnitTests.Calendars.Recurrence.Yearly1.ics");
        internal static string YearlyByDay1 => ReadStream("Ical.Net.UnitTests.Calendars.Recurrence.YearlyByDay1.ics");
        internal static string YearlyByMonth1 => ReadStream("Ical.Net.UnitTests.Calendars.Recurrence.YearlyByMonth1.ics");
        internal static string YearlyByMonth2 => ReadStream("Ical.Net.UnitTests.Calendars.Recurrence.YearlyByMonth2.ics");
        internal static string YearlyByMonth3 => ReadStream("Ical.Net.UnitTests.Calendars.Recurrence.YearlyByMonth3.ics");
        internal static string YearlyByMonthDay1 => ReadStream("Ical.Net.UnitTests.Calendars.Recurrence.YearlyByMonthDay1.ics");
        internal static string YearlyBySetPos1 => ReadStream("Ical.Net.UnitTests.Calendars.Recurrence.YearlyBySetPos1.ics");
        internal static string YearlyByWeekNo1 => ReadStream("Ical.Net.UnitTests.Calendars.Recurrence.YearlyByWeekNo1.ics");
        internal static string YearlyByWeekNo2 => ReadStream("Ical.Net.UnitTests.Calendars.Recurrence.YearlyByWeekNo2.ics");
        internal static string YearlyByWeekNo3 => ReadStream("Ical.Net.UnitTests.Calendars.Recurrence.YearlyByWeekNo3.ics");
        internal static string YearlyByWeekNo4 => ReadStream("Ical.Net.UnitTests.Calendars.Recurrence.YearlyByWeekNo4.ics");
        internal static string YearlyByWeekNo5 => ReadStream("Ical.Net.UnitTests.Calendars.Recurrence.YearlyByWeekNo5.ics");
        internal static string YearlyComplex1 => ReadStream("Ical.Net.UnitTests.Calendars.Recurrence.YearlyComplex1.ics");
        internal static string YearlyCountByMonth1 => ReadStream("Ical.Net.UnitTests.Calendars.Recurrence.YearlyCountByMonth1.ics");
        internal static string YearlyCountByYearDay1 => ReadStream("Ical.Net.UnitTests.Calendars.Recurrence.YearlyCountByYearDay1.ics");
        internal static string YearlyInterval1 => ReadStream("Ical.Net.UnitTests.Calendars.Recurrence.YearlyInterval1.ics");
    }
}
