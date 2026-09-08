//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.Runtime.Serialization;
using Ical.Net.DataTypes;

namespace Ical.Net.Serialization.Converters;

internal class OrganizerConverter : CalendarPropertyConverter<Organizer>
{
    public override Organizer? Read(CalendarReader reader, IParameterCollection parameters)
    {
        if (!reader.TryGetUri(out var uri))
        {
            throw new SerializationException($"Invalid URI value at line {reader.LineNumber}");
        }

        // Make sure scheme is always "mailto"
        if (!uri.Scheme.Equals("mailto", StringComparison.OrdinalIgnoreCase))
        {
            var builder = new UriBuilder(uri)
            {
                Scheme = "mailto"
            };

            uri = builder.Uri;
        }

        return new Organizer
        {
            Value = uri
        };
    }
    public override void Write(CalendarWriter writer, Organizer value)
    {
        if (value.Value is Uri uri)
        {
            writer.WriteValue(uri);
        }
        else
        {
            writer.WriteValue(string.Empty);
        }
    }
}
