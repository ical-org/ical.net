using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal
{
    public interface IDaySpecifier :
        ICalendarDataType,
        IComparable
    {
        int Num { get; set; }
        DayOfWeek DayOfWeek { get; set; }
    }
}
