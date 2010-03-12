using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal
{
    public interface ICalendarDataType :
        ICopyable,
        IServiceProvider
    {
        ICalendarParameterList AssociatedParameters { get; }
        ICalendarObject AssociatedObject { get; }
        IICalendar Calendar { get; }
        void AssociateWith(ICalendarObject obj);
        void Deassociate();
    }
}
