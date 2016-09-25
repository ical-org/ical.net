using System;
using System.Reflection;
using ical.net.General;
using ical.net.Interfaces;
using ical.net.Interfaces.Components;
using ical.net.Interfaces.DataTypes;
using ical.net.Interfaces.General;
using ical.net.Interfaces.Serialization;
using ical.net.Interfaces.Serialization.Factory;
using ical.net.Serialization.iCalendar.Serializers;
using ical.net.Serialization.iCalendar.Serializers.Components;
using ical.net.Serialization.iCalendar.Serializers.Other;

namespace ical.net.Serialization.iCalendar.Factory
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

                if (typeof (ICalendar).GetTypeInfo().IsAssignableFrom(objectType))
                {
                    s = new CalendarSerializer();
                }
                else if (typeof (ICalendarComponent).GetTypeInfo().IsAssignableFrom(objectType))
                {
                    s = typeof (IEvent).GetTypeInfo().IsAssignableFrom(objectType)
                        ? new EventSerializer()
                        : new ComponentSerializer();
                }
                else if (typeof (ICalendarProperty).GetTypeInfo().IsAssignableFrom(objectType))
                {
                    s = new PropertySerializer();
                }
                else if (typeof (CalendarParameter).GetTypeInfo().IsAssignableFrom(objectType))
                {
                    s = new ParameterSerializer();
                }
                else if (typeof (string).GetTypeInfo().IsAssignableFrom(objectType))
                {
                    s = new StringSerializer();
                }
#if NET_4
                else if (objectType.IsEnum)
                {
                    s = new EnumSerializer(objectType);
                }
#else
                else if (objectType.GetTypeInfo().IsEnum)
                {
                    s = new EnumSerializer(objectType);
                }
#endif
                else if (typeof (TimeSpan).GetTypeInfo().IsAssignableFrom(objectType))
                {
                    s = new TimeSpanSerializer();
                }
                else if (typeof (int).GetTypeInfo().IsAssignableFrom(objectType))
                {
                    s = new IntegerSerializer();
                }
                else if (typeof (Uri).GetTypeInfo().IsAssignableFrom(objectType))
                {
                    s = new UriSerializer();
                }
                else if (typeof (ICalendarDataType).GetTypeInfo().IsAssignableFrom(objectType))
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