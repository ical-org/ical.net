using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace DDay.iCal.Serialization.iCalendar
{
    public class EventSerializer :
        ComponentSerializer
    {
        #region Constructor

        public EventSerializer() : base()
        {
        }

        public EventSerializer(ISerializationContext ctx)
            : base(ctx)
        {
        }

        #endregion

        #region Overrides

        public override Type TargetType
        {
            get { return typeof(Event); }
        }

        public override string SerializeToString(object obj)
        {
            if (obj is IEvent)
            {
                IEvent evt = ((IEvent)obj).Copy<IEvent>();

                // NOTE: DURATION and DTEND cannot co-exist on an event.
                // Some systems do not support DURATION, so we serialize
                // all events using DTEND instead.
                if (evt.Properties.ContainsKey("DURATION") && evt.Properties.ContainsKey("DTEND"))
                    evt.Properties.Remove("DURATION");

                return base.SerializeToString(evt);
            }
            return base.SerializeToString(obj);
        }

        #endregion
    }
}
