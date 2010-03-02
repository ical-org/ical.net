using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal
{
    public interface ICalendarDataType :
        ICalendarProperty
    {
        Type ValueType { get; }

        ICalendarDataType Parse(string value);
        bool TryParse(string value, ref ICalendarDataType obj);
    }
}
