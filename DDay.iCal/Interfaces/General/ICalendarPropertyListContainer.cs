namespace DDay.iCal
{
    public interface ICalendarPropertyListContainer :
        ICalendarObject
    {
        ICalendarPropertyList Properties { get; }
    }
}
