using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal
{
    public interface ICalendarValueObject :
        ICalendarObject
    {
        object Value { get; set; }
        IList<object> Values { get; set; }
    }
}
