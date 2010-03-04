using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal.Serialization
{
    public class SerializerFactory :
        ISerializerFactory
    {
        #region ISerializerFactory Members

        virtual public ISerializable Create(ICalendarObject obj, ISerializationContext ctx)
        {
            if (obj != null)
            {
                ISerializable s = null;

                Type type = obj.GetType();
                if (typeof(IICalendar).IsAssignableFrom(type))
                    s = new iCalendarSerializer();
                else if (typeof(ICalendarComponent).IsAssignableFrom(type))
                    s = new ComponentSerializer();
                
                // Set the serialization context
                if (s != null)
                    s.SerializationContext = ctx;

                return s;
            }
            return null;
        }

        #endregion
    }
}
