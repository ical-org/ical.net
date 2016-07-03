using System;
using Ical.Net.Interfaces.Components;
using Ical.Net.Interfaces.Serialization;

namespace Ical.Net.Serialization.iCalendar.Serializers.Components
{
    public class EventSerializer : ComponentSerializer
    {
        public EventSerializer() {}

        public EventSerializer(ISerializationContext ctx) : base(ctx) {}

        public override Type TargetType => typeof (Event);

        public override string SerializeToString(object obj)
        {
            if (obj is IEvent)
            {
                var evt = ((IEvent) obj).Copy<IEvent>();

                // NOTE: DURATION and DTEND cannot co-exist on an event.
                // Some systems do not support DURATION, so we serialize
                // all events using DTEND instead.
                if (evt.Properties.ContainsKey("DURATION") && evt.Properties.ContainsKey("DTEND"))
                {
                    evt.Properties.Remove("DURATION");
                }

                return base.SerializeToString(evt);
            }
            return base.SerializeToString(obj);
        }
    }
}