using ical.NET.Collections.Interfaces;

namespace Ical.Net.Interfaces.General
{
    public interface ICalendarParameter : ICalendarObject, IValueObject<string>
    {
        string Value { get; set; }
    }
}