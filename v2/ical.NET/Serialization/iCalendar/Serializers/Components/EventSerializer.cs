using System;
using Ical.Net.Interfaces.Components;

namespace Ical.Net.Serialization.iCalendar.Serializers.Components
{
    public class EventSerializer : ComponentSerializer
    {
        public override Type TargetType => typeof (Event);

        public override string SerializeToString(object obj)
        {
            var evt = obj as IEvent;

            var actualEvent = evt.Properties.ContainsKey("DURATION") && evt.Properties.ContainsKey("DTEND")
                ? evt.Copy<IEvent>()
                : evt;

            return base.SerializeToString(actualEvent);
        }
    }
}