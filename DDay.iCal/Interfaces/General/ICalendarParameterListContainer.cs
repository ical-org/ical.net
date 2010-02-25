using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal
{
    public interface ICalendarParameterListContainer :
        ICalendarObject
    {
        ICalendarParameterList Parameters { get; }
    }
}
