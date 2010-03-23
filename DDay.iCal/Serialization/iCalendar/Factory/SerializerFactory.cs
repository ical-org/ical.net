using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace DDay.iCal.Serialization.iCalendar
{
    public class SerializerFactory :
        ISerializerFactory
    {
        #region Private Fields

        ISerializerFactory m_DataTypeSerializerFactory;

        #endregion

        #region Constructors

        public SerializerFactory()
        {
            m_DataTypeSerializerFactory = new DataTypeSerializerFactory();
        }

        #endregion

        #region ISerializerFactory Members

        /// <summary>
        /// Returns a serializer that can be used to serialize and object
        /// of type <paramref name="objectType"/>.
        /// <note>
        ///     TODO: Add support for caching.
        /// </note>
        /// </summary>
        /// <param name="objectType">The type of object to be serialized.</param>
        /// <param name="ctx">The serialization context.</param>
        virtual public ISerializer Build(Type objectType, ISerializationContext ctx)
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
                else
                {
                    if (typeof(string).IsAssignableFrom(objectType))
                        s = new StringSerializer();
                    else if (objectType.IsGenericType && typeof(IList<>).IsAssignableFrom(objectType.GetGenericTypeDefinition()))
                        s = new GenericListSerializer(objectType);
                    else if (typeof(ICalendarParameter).IsAssignableFrom(objectType))
                        s = new ParameterSerializer();                    
                    else if (typeof(TimeSpan).IsAssignableFrom(objectType))
                        s = new TimeSpanSerializer();
                    else if (typeof(int).IsAssignableFrom(objectType))
                        s = new IntegerSerializer();
                    else if (typeof(ICalendarDataType).IsAssignableFrom(objectType))
                        s = m_DataTypeSerializerFactory.Build(objectType, ctx);
                    // Default to a string serializer, which simply calls
                    // ToString() on the value to serialize it.
                    else
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
