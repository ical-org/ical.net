using System;
using System.Collections.Generic;
using System.Text;
using DDay.iCal;
using System.IO;
using DDay.iCal.Serialization;
using System.Runtime.Serialization;

namespace DDay.iCal
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
                // FIXME: implement

                //case ALARM: return new Alarm();
                case Components.EVENT: 
                    type = typeof(Event);
                    break;
                //case FREEBUSY: return new FreeBusy();
                //case JOURNAL: return new Journal();
                case Components.TIMEZONE:
                    type = typeof(iCalTimeZone);
                    break;
                case Components.TODO:
                    type = typeof(Todo);
                    break;
                case Components.DAYLIGHT:
                case Components.STANDARD:
                    type = typeof(iCalTimeZoneInfo);
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
