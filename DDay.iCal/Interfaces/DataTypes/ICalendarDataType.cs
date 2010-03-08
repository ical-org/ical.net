using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal
{
    public interface ICalendarDataType :
        ICopyable        
    {
        IICalendar Calendar { get; }
        void AssociateWith(ICalendarObject obj);
        void Deassociate();
    }
}
