using DDay.Collections;

namespace DDay.iCal
{
    public interface ICalendarProperty :        
        ICalendarParameterCollectionContainer,
        ICalendarObject,
        IValueObject<object>
    {
        object Value { get; set; }
    }
}
