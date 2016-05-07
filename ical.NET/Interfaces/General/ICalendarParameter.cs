using DDay.Collections;

namespace DDay.iCal
{
    public interface ICalendarParameter :
        ICalendarObject,
        IValueObject<string>
    {
        string Value { get; set; }
    }
}
