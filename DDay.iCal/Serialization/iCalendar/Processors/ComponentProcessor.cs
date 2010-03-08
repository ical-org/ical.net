using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal.Serialization.iCalendar
{
    public class ComponentProcessor :
        ISerializationProcessor<ICalendarComponent>
    {
        #region ISerializationProcessor<ICalendarComponent> Members

        public void Process(ICalendarComponent obj)
        {
            new ComponentPropertyConsolidator().Process(obj);
        }

        #endregion
    }
}
