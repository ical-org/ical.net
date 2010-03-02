using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal
{
    public interface IText :
        ICalendarDataType
    {
        string Value { get; set; }
    }
}
