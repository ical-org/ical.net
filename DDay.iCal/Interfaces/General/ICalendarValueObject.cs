using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal
{
    public interface ICalendarValueObject :
        ICalendarObject
    {
        string Value { get; set; }
        IList<string> Values { get; set; }
        string[] ValueArray { get; set; }
    }
}
