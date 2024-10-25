﻿using System.IO;
using System.Reflection;

namespace Ical.Net.Tests
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

        internal static string Alarm1 => ReadStream("Ical.Net.Tests.Calendars.Alarm.ALARM1.ics");
        internal static string Alarm2 => ReadStream("Ical.Net.Tests.Calendars.Alarm.ALARM2.ics");
        internal static string Alarm3 => ReadStream("Ical.Net.Tests.Calendars.Alarm.ALARM3.ics");
        internal static string Alarm4 => ReadStream("Ical.Net.Tests.Calendars.Alarm.ALARM4.ics");
        internal static string Alarm5 => ReadStream("Ical.Net.Tests.Calendars.Alarm.ALARM5.ics");
        internal static string Alarm6 => ReadStream("Ical.Net.Tests.Calendars.Alarm.ALARM6.ics");
        internal static string Alarm7 => ReadStream("Ical.Net.Tests.Calendars.Alarm.ALARM7.ics");
        internal static string Attachment3 => ReadStream("Ical.Net.Tests.Calendars.Serialization.Attachment3.ics");
        internal static string Attachment4 => ReadStream("Ical.Net.Tests.Calendars.Serialization.Attachment4.ics");
        internal static string Attendee1 => ReadStream("Ical.Net.Tests.Calendars.Serialization.Attendee1.ics");
        internal static string Attendee2 => ReadStream("Ical.Net.Tests.Calendars.Serialization.Attendee2.ics");
        internal static string Bug1741093 => ReadStream("Ical.Net.Tests.Calendars.Recurrence.Bug1741093.ics");
        internal static string Bug2033495 => ReadStream("Ical.Net.Tests.Calendars.Serialization.Bug2033495.ics");
        internal static string Bug2148092 => ReadStream("Ical.Net.Tests.Calendars.Serialization.Bug2148092.ics");
        internal static string Bug2912657 => ReadStream("Ical.Net.Tests.Calendars.Recurrence.Bug2912657.ics");
        internal static string Bug2916581 => ReadStream("Ical.Net.Tests.Calendars.Recurrence.Bug2916581.ics");
        internal static string Bug2938007 => ReadStream("Ical.Net.Tests.Calendars.Serialization.Bug2938007.ics");
        internal static string Bug2959692 => ReadStream("Ical.Net.Tests.Calendars.Recurrence.Bug2959692.ics");
        internal static string Bug2966236 => ReadStream("Ical.Net.Tests.Calendars.Recurrence.Bug2966236.ics");
        internal static string Bug3007244 => ReadStream("Ical.Net.Tests.Calendars.Recurrence.Bug3007244.ics");
        internal static string ByMonth1 => ReadStream("Ical.Net.Tests.Calendars.Recurrence.ByMonth1.ics");
        internal static string ByMonth2 => ReadStream("Ical.Net.Tests.Calendars.Recurrence.ByMonth2.ics");
        internal static string ByMonthDay1 => ReadStream("Ical.Net.Tests.Calendars.Recurrence.ByMonthDay1.ics");
        internal static string Calendar1 => ReadStream("Ical.Net.Tests.Calendars.Serialization.Calendar1.ics");
        internal static string CalendarParameters2 => ReadStream("Ical.Net.Tests.Calendars.Serialization.CalendarParameters2.ics");
        internal static string CaseInsensitive1 => ReadStream("Ical.Net.Tests.Calendars.Serialization.CaseInsensitive1.ics");
        internal static string CaseInsensitive2 => ReadStream("Ical.Net.Tests.Calendars.Serialization.CaseInsensitive2.ics");
        internal static string CaseInsensitive3 => ReadStream("Ical.Net.Tests.Calendars.Serialization.CaseInsensitive3.ics");
        internal static string CaseInsensitive4 => ReadStream("Ical.Net.Tests.Calendars.Serialization.CaseInsensitive4.ics");
        internal static string Categories1 => ReadStream("Ical.Net.Tests.Calendars.Serialization.Categories1.ics");
        internal static string Daily1 => ReadStream("Ical.Net.Tests.Calendars.Recurrence.Daily1.ics");
        internal static string DailyByDay1 => ReadStream("Ical.Net.Tests.Calendars.Recurrence.DailyByDay1.ics");
        internal static string DailyByHourMinute1 => ReadStream("Ical.Net.Tests.Calendars.Recurrence.DailyByHourMinute1.ics");
        internal static string DailyCount1 => ReadStream("Ical.Net.Tests.Calendars.Recurrence.DailyCount1.ics");
        internal static string DailyCount2 => ReadStream("Ical.Net.Tests.Calendars.Recurrence.DailyCount2.ics");
        internal static string DailyInterval1 => ReadStream("Ical.Net.Tests.Calendars.Recurrence.DailyInterval1.ics");
        internal static string DailyInterval2 => ReadStream("Ical.Net.Tests.Calendars.Recurrence.DailyInterval2.ics");
        internal static string DailyUntil1 => ReadStream("Ical.Net.Tests.Calendars.Recurrence.DailyUntil1.ics");
        internal static string DateTime1 => ReadStream("Ical.Net.Tests.Calendars.Serialization.DateTime1.ics");
        internal static string DateTime2 => ReadStream("Ical.Net.Tests.Calendars.Serialization.DateTime2.ics");
        internal static string Duration1 => ReadStream("Ical.Net.Tests.Calendars.Serialization.Duration1.ics");
        internal static string Empty1 => ReadStream("Ical.Net.Tests.Calendars.Recurrence.Empty1.ics");
        internal static string EmptyLines1 => ReadStream("Ical.Net.Tests.Calendars.Serialization.EmptyLines1.ics");
        internal static string EmptyLines2 => ReadStream("Ical.Net.Tests.Calendars.Serialization.EmptyLines2.ics");
        internal static string EmptyLines3 => ReadStream("Ical.Net.Tests.Calendars.Serialization.EmptyLines3.ics");
        internal static string EmptyLines4 => ReadStream("Ical.Net.Tests.Calendars.Serialization.EmptyLines4.ics");
        internal static string Encoding1 => ReadStream("Ical.Net.Tests.Calendars.Serialization.Encoding1.ics");
        internal static string Encoding2 => ReadStream("Ical.Net.Tests.Calendars.Serialization.Encoding2.ics");
        internal static string Encoding3 => ReadStream("Ical.Net.Tests.Calendars.Serialization.Encoding3.ics");
        internal static string Event1 => ReadStream("Ical.Net.Tests.Calendars.Serialization.Event1.ics");
        internal static string Event2 => ReadStream("Ical.Net.Tests.Calendars.Serialization.Event2.ics");
        internal static string Event3 => ReadStream("Ical.Net.Tests.Calendars.Serialization.Event3.ics");
        internal static string Event4 => ReadStream("Ical.Net.Tests.Calendars.Serialization.Event4.ics");
        internal static string EventStatus => ReadStream("Ical.Net.Tests.Calendars.Serialization.EventStatus.ics");
        internal static string GeographicLocation1 => ReadStream("Ical.Net.Tests.Calendars.Serialization.GeographicLocation1.ics");
        internal static string Google1 => ReadStream("Ical.Net.Tests.Calendars.Serialization.Google1.ics");
        internal static string Hourly1 => ReadStream("Ical.Net.Tests.Calendars.Recurrence.Hourly1.ics");
        internal static string HourlyInterval1 => ReadStream("Ical.Net.Tests.Calendars.Recurrence.HourlyInterval1.ics");
        internal static string HourlyInterval2 => ReadStream("Ical.Net.Tests.Calendars.Recurrence.HourlyInterval2.ics");
        internal static string HourlyUntil1 => ReadStream("Ical.Net.Tests.Calendars.Recurrence.HourlyUntil1.ics");
        internal static string Journal1 => ReadStream("Ical.Net.Tests.Calendars.Journal.JOURNAL1.ics");
        internal static string Journal2 => ReadStream("Ical.Net.Tests.Calendars.Journal.JOURNAL2.ics");
        internal static string Language1 => ReadStream("Ical.Net.Tests.Calendars.Serialization.Language1.ics");
        internal static string Language2 => ReadStream("Ical.Net.Tests.Calendars.Serialization.Language2.ics");
        internal static string Language3 => ReadStream("Ical.Net.Tests.Calendars.Serialization.Language3.ics");
        internal static string Language4 => ReadStream("Ical.Net.Tests.Calendars.Serialization.Language4.ics");
        internal static string Minutely1 => ReadStream("Ical.Net.Tests.Calendars.Recurrence.Minutely1.ics");
        internal static string MinutelyByHour1 => ReadStream("Ical.Net.Tests.Calendars.Recurrence.MinutelyByHour1.ics");
        internal static string MinutelyCount1 => ReadStream("Ical.Net.Tests.Calendars.Recurrence.MinutelyCount1.ics");
        internal static string MinutelyCount2 => ReadStream("Ical.Net.Tests.Calendars.Recurrence.MinutelyCount2.ics");
        internal static string MinutelyCount3 => ReadStream("Ical.Net.Tests.Calendars.Recurrence.MinutelyCount3.ics");
        internal static string MinutelyCount4 => ReadStream("Ical.Net.Tests.Calendars.Recurrence.MinutelyCount4.ics");
        internal static string MinutelyInterval1 => ReadStream("Ical.Net.Tests.Calendars.Recurrence.MinutelyInterval1.ics");
        internal static string Monthly1 => ReadStream("Ical.Net.Tests.Calendars.Recurrence.Monthly1.ics");
        internal static string MonthlyByDay1 => ReadStream("Ical.Net.Tests.Calendars.Recurrence.MonthlyByDay1.ics");
        internal static string MonthlyByMonthDay1 => ReadStream("Ical.Net.Tests.Calendars.Recurrence.MonthlyByMonthDay1.ics");
        internal static string MonthlyByMonthDay2 => ReadStream("Ical.Net.Tests.Calendars.Recurrence.MonthlyByMonthDay2.ics");
        internal static string MonthlyBySetPos1 => ReadStream("Ical.Net.Tests.Calendars.Recurrence.MonthlyBySetPos1.ics");
        internal static string MonthlyBySetPos2 => ReadStream("Ical.Net.Tests.Calendars.Recurrence.MonthlyBySetPos2.ics");
        internal static string MonthlyCountByDay1 => ReadStream("Ical.Net.Tests.Calendars.Recurrence.MonthlyCountByDay1.ics");
        internal static string MonthlyCountByDay2 => ReadStream("Ical.Net.Tests.Calendars.Recurrence.MonthlyCountByDay2.ics");
        internal static string MonthlyCountByDay3 => ReadStream("Ical.Net.Tests.Calendars.Recurrence.MonthlyCountByDay3.ics");
        internal static string MonthlyCountByMonthDay1 => ReadStream("Ical.Net.Tests.Calendars.Recurrence.MonthlyCountByMonthDay1.ics");
        internal static string MonthlyCountByMonthDay2 => ReadStream("Ical.Net.Tests.Calendars.Recurrence.MonthlyCountByMonthDay2.ics");
        internal static string MonthlyCountByMonthDay3 => ReadStream("Ical.Net.Tests.Calendars.Recurrence.MonthlyCountByMonthDay3.ics");
        internal static string MonthlyInterval1 => ReadStream("Ical.Net.Tests.Calendars.Recurrence.MonthlyInterval1.ics");
        internal static string MonthlyUntilByDay1 => ReadStream("Ical.Net.Tests.Calendars.Recurrence.MonthlyUntilByDay1.ics");
        internal static string Outlook2007LineFolds => ReadStream("Ical.Net.Tests.Calendars.Serialization.Outlook2007LineFolds.ics");
        internal static string Parameter1 => ReadStream("Ical.Net.Tests.Calendars.Serialization.Parameter1.ics");
        internal static string Parameter2 => ReadStream("Ical.Net.Tests.Calendars.Serialization.Parameter2.ics");
        internal static string Parse1 => ReadStream("Ical.Net.Tests.Calendars.Serialization.Parse1.ics");
        internal static string Parse17 => ReadStream("Ical.Net.Tests.Calendars.Serialization.PARSE17.ics");
        internal static string ProdId1 => ReadStream("Ical.Net.Tests.Calendars.Serialization.ProdID1.ics");
        internal static string ProdId2 => ReadStream("Ical.Net.Tests.Calendars.Serialization.ProdID2.ics");
        internal static string Property1 => ReadStream("Ical.Net.Tests.Calendars.Serialization.Property1.ics");
        internal static string RecurrenceDates1 => ReadStream("Ical.Net.Tests.Calendars.Serialization.RecurrenceDates1.ics");
        internal static string RequestStatus1 => ReadStream("Ical.Net.Tests.Calendars.Serialization.RequestStatus1.ics");
        internal static string Secondly1 => ReadStream("Ical.Net.Tests.Calendars.Recurrence.Secondly1.ics");
        internal static string TimeZone1 => ReadStream("Ical.Net.Tests.Calendars.Serialization.TimeZone1.ics");
        internal static string TimeZone2 => ReadStream("Ical.Net.Tests.Calendars.Serialization.TimeZone2.ics");
        internal static string TimeZone3 => ReadStream("Ical.Net.Tests.Calendars.Serialization.TimeZone3.ics");
        internal static string Todo1 => ReadStream("Ical.Net.Tests.Calendars.Todo.Todo1.ics");
        internal static string Todo2 => ReadStream("Ical.Net.Tests.Calendars.Todo.Todo2.ics");
        internal static string Todo3 => ReadStream("Ical.Net.Tests.Calendars.Todo.Todo3.ics");
        internal static string Todo4 => ReadStream("Ical.Net.Tests.Calendars.Todo.Todo4.ics");
        internal static string Todo5 => ReadStream("Ical.Net.Tests.Calendars.Todo.Todo5.ics");
        internal static string Todo6 => ReadStream("Ical.Net.Tests.Calendars.Todo.Todo6.ics");
        internal static string Todo7 => ReadStream("Ical.Net.Tests.Calendars.Todo.Todo7.ics");
        internal static string Todo8 => ReadStream("Ical.Net.Tests.Calendars.Todo.Todo8.ics");
        internal static string Todo9 => ReadStream("Ical.Net.Tests.Calendars.Todo.Todo9.ics");
        internal static string Transparency1 => ReadStream("Ical.Net.Tests.Calendars.Serialization.Transparency1.ics");
        internal static string Transparency2 => ReadStream("Ical.Net.Tests.Calendars.Serialization.Transparency2.ics");
        internal static string Trigger1 => ReadStream("Ical.Net.Tests.Calendars.Serialization.Trigger1.ics");
        internal static string UsHolidays => ReadStream("Ical.Net.Tests.Calendars.Serialization.USHolidays.ics");
        internal static string WeeklyCount1 => ReadStream("Ical.Net.Tests.Calendars.Recurrence.WeeklyCount1.ics");
        internal static string WeeklyCountWkst1 => ReadStream("Ical.Net.Tests.Calendars.Recurrence.WeeklyCountWkst1.ics");
        internal static string WeeklyCountWkst2 => ReadStream("Ical.Net.Tests.Calendars.Recurrence.WeeklyCountWkst2.ics");
        internal static string WeeklyCountWkst3 => ReadStream("Ical.Net.Tests.Calendars.Recurrence.WeeklyCountWkst3.ics");
        internal static string WeeklyCountWkst4 => ReadStream("Ical.Net.Tests.Calendars.Recurrence.WeeklyCountWkst4.ics");
        internal static string WeeklyInterval1 => ReadStream("Ical.Net.Tests.Calendars.Recurrence.WeeklyInterval1.ics");
        internal static string WeeklyUntil1 => ReadStream("Ical.Net.Tests.Calendars.Recurrence.WeeklyUntil1.ics");
        internal static string WeeklyUntilWkst1 => ReadStream("Ical.Net.Tests.Calendars.Recurrence.WeeklyUntilWkst1.ics");
        internal static string WeeklyUntilWkst2 => ReadStream("Ical.Net.Tests.Calendars.Recurrence.WeeklyUntilWkst2.ics");
        internal static string WeeklyWeekStartsLastYear => ReadStream("Ical.Net.Tests.Calendars.Recurrence.WeeklyWeekStartsLastYear.ics");
        internal static string WeeklyWkst1 => ReadStream("Ical.Net.Tests.Calendars.Recurrence.WeeklyWkst1.ics");
        internal static string XProperty1 => ReadStream("Ical.Net.Tests.Calendars.Serialization.XProperty1.ics");
        internal static string XProperty2 => ReadStream("Ical.Net.Tests.Calendars.Serialization.XProperty2.ics");
        internal static string Yearly1 => ReadStream("Ical.Net.Tests.Calendars.Recurrence.Yearly1.ics");
        internal static string YearlyByDay1 => ReadStream("Ical.Net.Tests.Calendars.Recurrence.YearlyByDay1.ics");
        internal static string YearlyByMonth1 => ReadStream("Ical.Net.Tests.Calendars.Recurrence.YearlyByMonth1.ics");
        internal static string YearlyByMonth2 => ReadStream("Ical.Net.Tests.Calendars.Recurrence.YearlyByMonth2.ics");
        internal static string YearlyByMonth3 => ReadStream("Ical.Net.Tests.Calendars.Recurrence.YearlyByMonth3.ics");
        internal static string YearlyByMonthDay1 => ReadStream("Ical.Net.Tests.Calendars.Recurrence.YearlyByMonthDay1.ics");
        internal static string YearlyBySetPos1 => ReadStream("Ical.Net.Tests.Calendars.Recurrence.YearlyBySetPos1.ics");
        internal static string YearlyByWeekNo1 => ReadStream("Ical.Net.Tests.Calendars.Recurrence.YearlyByWeekNo1.ics");
        internal static string YearlyByWeekNo2 => ReadStream("Ical.Net.Tests.Calendars.Recurrence.YearlyByWeekNo2.ics");
        internal static string YearlyByWeekNo3 => ReadStream("Ical.Net.Tests.Calendars.Recurrence.YearlyByWeekNo3.ics");
        internal static string YearlyByWeekNo4 => ReadStream("Ical.Net.Tests.Calendars.Recurrence.YearlyByWeekNo4.ics");
        internal static string YearlyByWeekNo5 => ReadStream("Ical.Net.Tests.Calendars.Recurrence.YearlyByWeekNo5.ics");
        internal static string YearlyComplex1 => ReadStream("Ical.Net.Tests.Calendars.Recurrence.YearlyComplex1.ics");
        internal static string YearlyCountByMonth1 => ReadStream("Ical.Net.Tests.Calendars.Recurrence.YearlyCountByMonth1.ics");
        internal static string YearlyCountByYearDay1 => ReadStream("Ical.Net.Tests.Calendars.Recurrence.YearlyCountByYearDay1.ics");
        internal static string YearlyInterval1 => ReadStream("Ical.Net.Tests.Calendars.Recurrence.YearlyInterval1.ics");

        internal static string LibicalIcalrecurTest => ReadStream("Ical.Net.Tests.contrib.libical.icalrecur_test.out");

    }
}
