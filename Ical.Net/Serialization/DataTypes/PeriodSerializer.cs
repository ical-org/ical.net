//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

#nullable enable
using System;
using System.IO;
using System.Text;
using Ical.Net.DataTypes;

namespace Ical.Net.Serialization.DataTypes;

public class PeriodSerializer : EncodableDataTypeSerializer
{
    public PeriodSerializer() { }

    public PeriodSerializer(SerializationContext ctx) : base(ctx) { }

    public override Type TargetType => typeof(Period);

    public override string? SerializeToString(object? obj)
    {
        var factory = GetService<ISerializerFactory>();

        if (obj is not Period p || factory == null)
        {
            return null;
        }

        // Push the period onto the serialization context stack
        SerializationContext.Push(p);

        try
        {
            var dtSerializer = factory.Build(typeof(CalDateTime), SerializationContext) as IStringSerializer;
            var durationSerializer = factory.Build(typeof(Duration), SerializationContext) as IStringSerializer;
            if (dtSerializer == null || durationSerializer == null)
            {
                return null;
            }

            var sb = new StringBuilder();

            // Serialize the start time
            sb.Append(dtSerializer.SerializeToString(p.StartTime));

            // RFC 5545 section 3.6.1:
            // For cases where a "VEVENT" calendar component
            // specifies a "DTSTART" property with a DATE value type but no
            // "DTEND" nor "DURATION" property, the event’s duration is taken to
            // be one day:

            if (p.EndTime is { } endtime)
            {
                // Serialize the end date and time...
                sb.Append('/');
                sb.Append(dtSerializer.SerializeToString(endtime));
            }
            if (p.Duration is { } duration) 
            {
                // Serialize the duration
                sb.Append('/');
                sb.Append(durationSerializer.SerializeToString(duration));
            }
            // else, just the start time gets serialized to comply with the RFC 5545 section 3.6.1

            // Encode the value as necessary
            return Encode(p, sb.ToString());
        }
        finally
        {
            // Pop the period off the serialization context stack
            SerializationContext?.Pop();
        }
    }

    public override object? Deserialize(TextReader tr)
    {
        var value = tr.ReadToEnd();

        var p = CreateAndAssociate() as Period;
        var factory = GetService<ISerializerFactory>();
        if (p == null || factory == null)
        {
            return null;
        }

        var dtSerializer = factory.Build(typeof(CalDateTime), SerializationContext) as IStringSerializer;
        var durationSerializer = factory.Build(typeof(Duration), SerializationContext) as IStringSerializer;
        if (dtSerializer == null || durationSerializer == null)
        {
            return null;
        }

        // Decode the value as necessary
        value = Decode(p, value);
        if (value == null) return null;

        var values = value.Split('/');
        if (values.Length != 2)
        {
            return null;
        }

        var start = dtSerializer.Deserialize(new StringReader(values[0])) as CalDateTime;
        var end = dtSerializer.Deserialize(new StringReader(values[1])) as CalDateTime;
        var duration = durationSerializer.Deserialize(new StringReader(values[1])) as Duration?;

        return start is null ? null : Period.Create(start, end, duration, p.AssociatedObject);
    }
}
