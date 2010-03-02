using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal
{
    public interface IText :
        ICalendarDataType,
        IEscapable
    {
        string Value { get; set; }
    }
}
