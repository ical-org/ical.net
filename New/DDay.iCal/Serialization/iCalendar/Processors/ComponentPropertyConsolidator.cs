using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal.Serialization.iCalendar
{
    public class ComponentPropertyConsolidator :
        ISerializationProcessor<ICalendarComponent>
    {
        #region ISerializationProcessor<ICalendarComponent> Members

        virtual public void Process(ICalendarComponent obj)
        {
            // FIXME: this should probably have a lot more special-case-handling
            // and smarts built into it?

            //List<ICalendarProperty> resulting = new List<ICalendarProperty>();
            //foreach (ICalendarProperty p in obj.Properties)
            //{
            //    if (obj.Properties.CountOf(p.Name) > 1)
            //    {
                    
            //    }
            //}
            
            //foreach (ICalendarProperty p in obj.Properties)
            //{
                

            //}
        }

        #endregion
    }
}
