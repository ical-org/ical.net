using System;
using System.Collections.Generic;
using System.Text;
using DDay.iCal.Serialization.iCalendar;

namespace DDay.iCal.Serialization.iCalendar
{
    public class SerializerFactory :
        ISerializerFactory
    {
        #region ISerializerFactory Members

        virtual public ISerializer Create(Type objectType, ISerializationContext ctx)
        {
            if (objectType != null)
            {
                ISerializer s = null;

                object contextObj = ctx.Peek();
                if (contextObj is IICalendar ||
                    contextObj is ICalendarComponent)
                {
                    if (typeof(IICalendar).IsAssignableFrom(objectType))
                        s = new iCalendarSerializer();
                    else if (typeof(ICalendarComponent).IsAssignableFrom(objectType))
                        s = new ComponentSerializer();                    
                    else if (typeof(ICalendarProperty).IsAssignableFrom(objectType))
                        s = new PropertySerializer();                    
                }
                else if (contextObj is ICalendarProperty)
                {
                    if (typeof(ICalendarParameter).IsAssignableFrom(objectType))
                        s = new ParameterSerializer();
                    else if (typeof(iCalDateTime).IsAssignableFrom(objectType))
                        s = new DateTimeSerializer();
                    else if (typeof(string).IsAssignableFrom(objectType))
                        s = new StringSerializer();
                }
                
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
