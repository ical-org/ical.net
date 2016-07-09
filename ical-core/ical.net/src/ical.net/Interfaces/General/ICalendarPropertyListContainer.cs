namespace Ical.Net.Interfaces.General
{
    public interface ICalendarPropertyListContainer : ICalendarObject
    {
        ICalendarPropertyList Properties { get; }
    }
}