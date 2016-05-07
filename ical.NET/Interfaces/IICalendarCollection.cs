using System.Collections.Generic;

namespace DDay.iCal
{
    public interface IICalendarCollection :
        IGetOccurrencesTyped,
        IList<IICalendar>
    {        
    }
}
