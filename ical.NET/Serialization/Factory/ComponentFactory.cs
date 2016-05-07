using System;
using Ical.Net.Interfaces.Components;
using Ical.Net.Interfaces.Serialization.Factory;
using Ical.Net.Utility;

namespace Ical.Net.Serialization.Factory
{
    public class ComponentFactory :
        ICalendarComponentFactory
    {
        #region ICalendarComponentFactory Members

        virtual public ICalendarComponent Build(string objectName, bool uninitialized)
        {
            Type type = null;

            // Determine the type of component to build.
            switch (objectName.ToUpper())
            {
                case Components.ALARM:
                    type = typeof(Alarm);
                    break;
                case Components.EVENT: 
                    type = typeof(Event);
                    break;
                case Components.FREEBUSY:
                    type = typeof(FreeBusy);
                    break;
                case Components.JOURNAL:
                    type = typeof(Journal);
                    break;
                case Components.TIMEZONE:
                    type = typeof(ICalTimeZone);
                    break;
                case Components.TODO:
                    type = typeof(Todo);
                    break;
                case Components.DAYLIGHT:
                case Components.STANDARD:
                    type = typeof(ICalTimeZoneInfo);
                    break;
                default:
                    type = typeof(CalendarComponent);
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

        #endregion
    }
}
