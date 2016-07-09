using System;
using System.Reflection;
using Ical.Net.Interfaces;
using Ical.Net.Interfaces.Components;
using Ical.Net.Interfaces.DataTypes;
using Ical.Net.Interfaces.General;
using Ical.Net.Interfaces.Serialization;
using Ical.Net.Interfaces.Serialization.Factory;
using Ical.Net.Serialization.iCalendar.Serializers;
using Ical.Net.Serialization.iCalendar.Serializers.Components;
using Ical.Net.Serialization.iCalendar.Serializers.Other;

namespace Ical.Net.Serialization.iCalendar.Factory
{
    public class SerializerFactory : ISerializerFactory
    {
        private readonly ISerializerFactory _mDataTypeSerializerFactory;

        public SerializerFactory()
        {
            _mDataTypeSerializerFactory = new DataTypeSerializerFactory();
        }

        /// <summary>
        /// Returns a serializer that can be used to serialize and object
        /// of type <paramref name="objectType"/>.
        /// <note>
        ///     TODO: Add support for caching.
        /// </note>
        /// </summary>
        /// <param name="objectType">The type of object to be serialized.</param>
        /// <param name="ctx">The serialization context.</param>
        public virtual ISerializer Build(Type objectType, ISerializationContext ctx)
        {
            if (objectType != null)
            {
                ISerializer s;

                if (typeof (ICalendar).IsAssignableFrom(objectType))
                {
                    s = new CalendarSerializer();
                }
                else if (typeof (ICalendarComponent).IsAssignableFrom(objectType))
                {
                    s = typeof (IEvent).IsAssignableFrom(objectType)
                        ? new EventSerializer()
                        : new ComponentSerializer();
                }
                else if (typeof (ICalendarProperty).IsAssignableFrom(objectType))
                {
                    s = new PropertySerializer();
                }
                else if (typeof (ICalendarParameter).IsAssignableFrom(objectType))
                {
                    s = new ParameterSerializer();
                }
                else if (typeof (string).IsAssignableFrom(objectType))
                {
                    s = new StringSerializer();
                }
                else if (objectType.GetTypeInfo().IsEnum)
                {
                    s = new EnumSerializer(objectType);
                }
                else if (typeof (TimeSpan).IsAssignableFrom(objectType))
                {
                    s = new TimeSpanSerializer();
                }
                else if (typeof (int).IsAssignableFrom(objectType))
                {
                    s = new IntegerSerializer();
                }
                else if (typeof (Uri).IsAssignableFrom(objectType))
                {
                    s = new UriSerializer();
                }
                else if (typeof (ICalendarDataType).IsAssignableFrom(objectType))
                {
                    s = _mDataTypeSerializerFactory.Build(objectType, ctx);
                }
                // Default to a string serializer, which simply calls
                // ToString() on the value to serialize it.
                else
                {
                    s = new StringSerializer();
                }

                // Set the serialization context
                if (s != null)
                {
                    s.SerializationContext = ctx;
                }

                return s;
            }
            return null;
        }
    }
}