//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.Collections.Generic;
using System.Linq;
using Ical.Net.DataTypes;
using Ical.Net.Serialization.DataTypes;

namespace Ical.Net.Serialization;

public class DataTypeSerializerFactory : ISerializerFactory
{
    private static readonly Dictionary<Type, Func<SerializationContext, ISerializer>> _serializerMap =
        new()
        {
            { typeof(Attachment), ctx => new AttachmentSerializer(ctx) },
            { typeof(Attendee), ctx => new AttendeeSerializer(ctx) },
            { typeof(CalDateTime), ctx => new DateTimeSerializer(ctx) },
            { typeof(FreeBusyEntry), ctx => new FreeBusyEntrySerializer(ctx) },
            { typeof(GeographicLocation), ctx => new GeographicLocationSerializer(ctx) },
            { typeof(Organizer), ctx => new OrganizerSerializer(ctx) },
            { typeof(Period), ctx => new PeriodSerializer(ctx) },
            { typeof(PeriodList), ctx => new PeriodListSerializer(ctx) },
            { typeof(RecurrenceRule), ctx => new RecurrenceRuleSerializer(ctx) },
            { typeof(RequestStatus), ctx => new RequestStatusSerializer(ctx) },
            { typeof(StatusCode), ctx => new StatusCodeSerializer(ctx) },
            { typeof(Trigger), ctx => new TriggerSerializer(ctx) },
            { typeof(UtcOffset), ctx => new UtcOffsetSerializer(ctx) },
            { typeof(WeekDay), ctx => new WeekDaySerializer(ctx) }
        };

    /// <summary>
    /// Returns a serializer that can be used to serialize and object
    /// of type <paramref name="objectType"/>.
    /// </summary>
    /// <param name="objectType">The type of object to be serialized.</param>
    /// <param name="ctx">The serialization context.</param>
    public virtual ISerializer? Build(Type? objectType, SerializationContext ctx)
    {
        if (objectType == null) return null;

        // Check if the type exists in the map
        var serializer = _serializerMap
            .Where(entry => entry.Key.IsAssignableFrom(objectType))
            .Select(entry => entry.Value(ctx))
            .FirstOrDefault();

        // Return the found serializer or default to a string serializer
        return serializer ?? new StringSerializer(ctx);
    }
}
