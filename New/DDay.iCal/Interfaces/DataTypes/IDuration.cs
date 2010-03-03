using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal
{
    public interface IDuration :
        ICalendarDataType
    {
        TimeSpan Value { get; set; }
    }
}
