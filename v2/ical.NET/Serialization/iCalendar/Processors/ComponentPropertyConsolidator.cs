using Ical.Net.Interfaces.Components;
using Ical.Net.Interfaces.Serialization;

namespace Ical.Net.Serialization.iCalendar.Processors
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