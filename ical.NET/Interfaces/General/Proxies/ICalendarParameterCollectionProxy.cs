using DDay.Collections;

namespace DDay.iCal
{
    public interface ICalendarParameterCollectionProxy :
        ICalendarParameterCollection,
        IGroupedCollectionProxy<string, ICalendarParameter, ICalendarParameter>
    {                
    }
}
