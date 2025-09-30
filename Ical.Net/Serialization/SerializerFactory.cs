//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.Reflection;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization.DataTypes;

namespace Ical.Net.Serialization;

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
    /// The fallback for unknown <see cref="Type"/>s is <see cref="StringSerializer"/>.
    /// </summary>
    /// <param name="objectType">The type of object to be serialized.</param>
    /// <param name="ctx">The serialization context.</param>
    public virtual ISerializer? Build(Type? objectType, SerializationContext ctx)
    {
        return objectType switch
        {
            null => null,
            var t when typeof(Calendar).IsAssignableFrom(t)
                => new CalendarSerializer(ctx),
            var t when typeof(ICalendarComponent).IsAssignableFrom(t)
                => typeof(CalendarEvent).IsAssignableFrom(t)
                    ? new EventSerializer(ctx)
                    : new ComponentSerializer(ctx),
            var t when typeof(ICalendarProperty).IsAssignableFrom(t)
                => new PropertySerializer(ctx),
            var t when typeof(CalendarParameter).IsAssignableFrom(t)
                => new ParameterSerializer(ctx),
            var t when typeof(string).IsAssignableFrom(t)
                => new StringSerializer(ctx),
            var t when t.GetTypeInfo().IsEnum
                => new EnumSerializer(t, ctx),
            var t when typeof(Duration).IsAssignableFrom(t)
                => new DurationSerializer(ctx),
            var t when typeof(CalDateTime).IsAssignableFrom(t)
                => new DateTimeSerializer(ctx),
            var t when typeof(int).IsAssignableFrom(t)
                => new IntegerSerializer(ctx),
            var t when typeof(Uri).IsAssignableFrom(t)
                => new UriSerializer(ctx),
            var t when typeof(RecurrenceIdentifier).IsAssignableFrom(t)
                => new RecurrenceIdentifierSerializer(ctx),
            var t when typeof(ICalendarDataType).IsAssignableFrom(t)
                => _mDataTypeSerializerFactory.Build(t, ctx)!,
                // Default to a string serializer, which simply calls
                // ToString() on the value to serialize it.
            _ => new StringSerializer(ctx)
        };
    }
}
