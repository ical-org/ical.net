using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal.Serialization.iCalendar
{
    public class ComponentPropertyConsolidator :
        ISerializationProcessor<ICalendarComponent>
    {
        #region ISerializationProcessor<ICalendarComponent> Members

        virtual public void PreSerialization(ICalendarComponent obj)
        {
        }

        virtual public void PostSerialization(ICalendarComponent obj)
        {
        }

        virtual public void PreDeserialization(ICalendarComponent obj)
        {
        }

        virtual public void PostDeserialization(ICalendarComponent obj)
        {
        }

        #endregion
    }
}
