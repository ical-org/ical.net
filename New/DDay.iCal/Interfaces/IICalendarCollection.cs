using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal
{
    public interface IICalendarCollection :
        IList<IICalendar>,
        IICalendar        
    {
        IICalendar this[int index] { get; set; }
    }
}
