using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal
{
    public interface ICalendarProperty :
        ICalendarParameterListContainer,
        IKeyedObject<string>
    {
        string Value { get; set; }
    }
}
