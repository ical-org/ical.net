using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal
{
    public interface ICalendarProperty :        
        ICalendarParameterListContainer,
        ICalendarObject,
        IKeyedObject<string>
    {
        object Value { get; set; }
    }
}
