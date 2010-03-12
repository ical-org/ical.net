using System;
using System.Collections.Generic;
using System.Text;
using DDay.iCal.Serialization.iCalendar;
using System.Collections;

namespace DDay.iCal.Serialization.iCalendar
{
    public class DataTypeSerializerFactory :
        ISerializerFactory
    {
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

                if (typeof(IAttachment).IsAssignableFrom(objectType))
                    s = new AttachmentSerializer();
                else if (typeof(IGeographicLocation).IsAssignableFrom(objectType))
                    s = new GeographicLocationSerializer();
                else if (typeof(IRecurrencePattern).IsAssignableFrom(objectType))
                    s = new RecurrencePatternSerializer();
                else if (typeof(IUTCOffset).IsAssignableFrom(objectType))
                    s = new UTCOffsetSerializer();
                // Default to a string serializer, which simply calls
                // ToString() on the value to serialize it.
                else
                    s = new StringSerializer();
                
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
