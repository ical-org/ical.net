using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal
{
    public interface ICalendarProperty :
        ICalendarValueObject,
        ICalendarParameterListContainer,
        IKeyedObject<string>
    {
    }
}
