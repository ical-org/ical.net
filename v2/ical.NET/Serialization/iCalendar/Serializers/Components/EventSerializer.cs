﻿using System;
using ical.net.Interfaces.Components;

namespace ical.net.Serialization.iCalendar.Serializers.Components
{
    public class EventSerializer : ComponentSerializer
    {
        public override Type TargetType => typeof (Event);

        public override string SerializeToString(object obj)
        {
            var evt = obj as Event;

            // NOTE: DURATION and DTEND cannot co-exist on an event.
            // Some systems do not support DURATION, so we serialize
            // all events using DTEND instead.
            if (evt.Properties.ContainsKey("DURATION") && evt.Properties.ContainsKey("DTEND"))
            {
                evt.Properties.Remove("DURATION");
            }

            return base.SerializeToString(evt);
        }
    }
}