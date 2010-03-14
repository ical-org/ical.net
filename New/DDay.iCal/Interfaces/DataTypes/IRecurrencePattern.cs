using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal
{
    public interface IRecurrencePattern :
        IEncodableDataType
    {
        FrequencyType Frequency { get; set; }
        iCalDateTime Until { get; set; }
        int Count { get; set; }
        int Interval { get; set; }
        IList<int> BySecond { get; set; }
        IList<int> ByMinute { get; set; }
        IList<int> ByHour { get; set; }
        IList<IDaySpecifier> ByDay { get; set; }
        IList<int> ByMonthDay { get; set; }
        IList<int> ByYearDay { get; set; }
        IList<int> ByWeekNo { get; set; }
        IList<int> ByMonth { get; set; }
        IList<int> BySetPosition { get; set; }
        DayOfWeek WeekStart { get; set; }

        RecurrenceRestrictionType RestrictionType { get; set; }
        RecurrenceEvaluationModeType EvaluationMode { get; set; }
        
        bool CheckValidDate(iCalDateTime dt);
        bool IsValidDate(iCalDateTime dt);
    }
}
