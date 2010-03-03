using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal
{
    public interface IInteger :
        ICalendarDataType
    {
        int Value { get; set; }
    }
}
