using ical.net.Interfaces.Components;
using ical.net.Interfaces.Serialization;

namespace ical.net.Serialization.iCalendar.Processors
{
    /// <summary>
    /// FIXME: implement this.
    /// </summary>
    public class ComponentPropertyConsolidator : ISerializationProcessor<ICalendarComponent>
    {
        public virtual void PreSerialization(ICalendarComponent obj) {}

        public virtual void PostSerialization(ICalendarComponent obj) {}

        public virtual void PreDeserialization(ICalendarComponent obj) {}

        public virtual void PostDeserialization(ICalendarComponent obj) {}
    }
}