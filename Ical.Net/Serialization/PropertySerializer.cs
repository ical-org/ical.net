//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Ical.Net.DataTypes;
using Ical.Net.Utility;

namespace Ical.Net.Serialization;

public class PropertySerializer : SerializerBase
{
    public PropertySerializer() { }

    public PropertySerializer(SerializationContext ctx) : base(ctx) { }

    public override Type TargetType => typeof(CalendarProperty);

    public override string? SerializeToString(object obj)
    {
        var prop = obj as ICalendarProperty;
        if (prop?.Values == null || !prop.Values.Any())
        {
            return null;
        }

        // Push this object on the serialization context.
        SerializationContext.Push(prop);

        // Get a serializer factory that we can use to serialize
        // the property and parameter values
        var sf = GetService<ISerializerFactory>();

        var result = new StringBuilder();
        foreach (var v in prop.Values.Where(value => value != null))
        {
            SerializeValue(result, prop, v, sf);
        }

        // Pop the object off the serialization context.
        SerializationContext.Pop();
        return result.ToString();
    }

    private void SerializeValue(StringBuilder result, ICalendarProperty prop, object value, ISerializerFactory sf)
    {
        // Get a serializer to serialize the property's value.
        // If we can't serialize the property's value, the next step is worthless anyway.
        var valueSerializer = sf.Build(value.GetType(), SerializationContext) as IStringSerializer;

        // Iterate through each value to be serialized,
        // and give it a property (with parameters).
        // FIXME: this isn't always the way this is accomplished.
        // Multiple values can often be serialized within the
        // same property. How should we fix this?

        // NOTE:
        // We Serialize the property's value first, as during
        // serialization it may modify our parameters.
        // FIXME: the "parameter modification" operation should
        // be separated from serialization. Perhaps something
        // like PreSerialize(), etc.
        var serializedValue = valueSerializer?.SerializeToString(value);

        // Get the list of parameters we'll be serializing
        var parameterList =
            (IList<CalendarParameter>?)(value as ICalendarDataType)?.Parameters
            ?? (valueSerializer as IParameterProvider)?.GetParameters(value).ToList()
            ?? (IList<CalendarParameter>)prop.Parameters;

        var sb = new StringBuilder();
        sb.Append(prop.Name);
        if (parameterList.Count != 0)
        {
            // Get a serializer for parameters
            var parameterSerializer = sf.Build(typeof(CalendarParameter), SerializationContext) as IStringSerializer;
            if (parameterSerializer != null)
            {
                sb.Append(';');
                var first = true;
                // Serialize each parameter and append to the StringBuilder
                foreach (var param in parameterList)
                {
                    if (!first) sb.Append(';');

                    sb.Append(parameterSerializer.SerializeToString(param));
                    first = false;
                }
            }
        }

        sb.Append(':');
        sb.Append(serializedValue);

        result.FoldLines(sb.ToString());
    }

    public override object? Deserialize(TextReader tr) => null;
}
