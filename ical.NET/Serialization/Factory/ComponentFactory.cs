using System;
using Ical.Net.Interfaces.Components;
using Ical.Net.Interfaces.Serialization.Factory;
using Ical.Net.Utility;

namespace Ical.Net.Serialization.Factory
{
    public class ComponentFactory : ICalendarComponentFactory
    {
        public virtual ICalendarComponent Build(string objectName, bool uninitialized)
        {
            Type type = null;

            // Determine the type of component to build.
            switch (objectName.ToUpper())
            {
                case Components.Alarm:
                    type = typeof (Alarm);
                    break;
                case Components.Event:
                    type = typeof (Event);
                    break;
                case Components.Freebusy:
                    type = typeof (FreeBusy);
                    break;
                case Components.Journal:
                    type = typeof (Journal);
                    break;
                case Components.Timezone:
                    type = typeof (VTimeZone);
                    break;
                case Components.Todo:
                    type = typeof (Todo);
                    break;
                case Components.Daylight:
                case Components.Standard:
                    type = typeof (CalTimeZoneInfo);
                    break;
                default:
                    type = typeof (CalendarComponent);
                    break;
            }

            ICalendarComponent c = null;
            if (uninitialized)
            {
                // Create a new, uninitialized object (i.e. no constructor has been called).
                c = SerializationUtil.GetUninitializedObject(type) as ICalendarComponent;
            }
            else
            {
                // Create a new, initialized object.
                c = Activator.CreateInstance(type) as ICalendarComponent;
            }

            if (c != null)
            {
                // Assign the name of this component.
                c.Name = objectName.ToUpper();
            }

            return c;
        }
    }
}