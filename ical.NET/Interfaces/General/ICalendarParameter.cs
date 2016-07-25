using ical.Net.Collections.Interfaces;

namespace Ical.Net.Interfaces.General
{
    public interface ICalendarParameter : ICalendarObject, IValueObject<string>
    {
        string Value { get; set; }
    }
}