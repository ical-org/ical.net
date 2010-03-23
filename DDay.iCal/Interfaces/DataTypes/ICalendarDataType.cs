using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal
{
    public interface ICalendarDataType :
        ICalendarParameterListContainer,
        ICopyable,
        IServiceProvider
    {
        ICalendarObject AssociatedObject { get; set; }
        IICalendar Calendar { get; }
    }
}
