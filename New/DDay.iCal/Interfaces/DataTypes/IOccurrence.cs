using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal
{
    public interface IOccurrence :
        ICalendarDataType,
        IComparable<IOccurrence>
    {
        IPeriod Period { get; }
        IRecurringComponent Component { get; }
    }
}
