using System;
using System.Collections.Generic;
using Ical.Net.DataTypes;

namespace Ical.Net.Interfaces.DataTypes
{
    public interface IRecurrencePattern : IEncodableDataType
    {
        FrequencyType Frequency { get; set; }
        DateTime Until { get; set; }
        int Count { get; set; }
        int Interval { get; set; }
        IList<int> BySecond { get; set; }
        IList<int> ByMinute { get; set; }
        IList<int> ByHour { get; set; }
        IList<WeekDay> ByDay { get; set; }
        IList<int> ByMonthDay { get; set; }
        IList<int> ByYearDay { get; set; }
        IList<int> ByWeekNo { get; set; }
        IList<int> ByMonth { get; set; }
        IList<int> BySetPosition { get; set; }
        DayOfWeek FirstDayOfWeek { get; set; }

        RecurrenceRestrictionType RestrictionType { get; set; }
        RecurrenceEvaluationModeType EvaluationMode { get; set; }

        //IPeriod GetNextOccurrence(IDateTime dt);
    }
}