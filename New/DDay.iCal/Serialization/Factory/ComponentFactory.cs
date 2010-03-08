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

            switch (objectName.ToUpper())
            {
                // FIXME: implement

                //case ALARM: return new Alarm();
                case Components.EVENT: 
                    type = typeof(Event);
                    break;
                //case FREEBUSY: return new FreeBusy();
                //case JOURNAL: return new Journal();
                //case TIMEZONE: return new iCalTimeZone();
                case Components.TODO:
                    type = typeof(Todo);
                    break;
                //case DAYLIGHT:
                //case STANDARD:
                //    return new iCalTimeZoneInfo(objectName.ToUpper());
                default:
                    type = typeof(CalendarComponent);
                    break;
            }

            ICalendarComponent c = null;
            if (uninitialized)
                c = SerializationUtil.GetUninitializedObject(type) as ICalendarComponent;
            else
                c = Activator.CreateInstance(type) as ICalendarComponent;

            if (c != null)
                c.Name = objectName.ToUpper();

            return c;
        }

        #endregion
    }
}
