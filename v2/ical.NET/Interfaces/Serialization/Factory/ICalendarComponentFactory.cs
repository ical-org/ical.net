using ical.net.Interfaces.Components;

namespace ical.net.Interfaces.Serialization.Factory
{
    public interface ICalendarComponentFactory
    {
        ICalendarComponent Build(string objectName);
    }
}