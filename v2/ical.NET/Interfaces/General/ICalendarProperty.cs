using ical.net.collections.Interfaces;

namespace ical.net.Interfaces.General
{
    public interface ICalendarProperty : ICalendarParameterCollectionContainer, ICalendarObject, IValueObject<object>
    {
        object Value { get; set; }
    }
}