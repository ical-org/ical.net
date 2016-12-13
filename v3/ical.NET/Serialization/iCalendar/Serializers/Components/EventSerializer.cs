using System;

namespace Ical.Net.Serialization.iCalendar.Serializers.Components
{
    public class EventSerializer : ComponentSerializer
    {
        public override Type TargetType => typeof (CalendarEvent);

        public override string SerializeToString(object obj)
        {
            var evt = obj as CalendarEvent;

            var actualEvent = evt.Properties.ContainsKey("DURATION") && evt.Properties.ContainsKey("DTEND")
                ? evt.Copy<CalendarEvent>()
                : evt;

            return base.SerializeToString(actualEvent);
        }
    }
}