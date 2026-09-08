//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.Runtime.Serialization;

namespace Ical.Net.Serialization.Converters;

internal class UriConverter : CalendarPropertyConverter<Uri>
{
    public override Uri? Read(CalendarReader reader, IParameterCollection parameters)
    {
        if (!reader.TryGetUri(out var uri))
        {
            throw new SerializationException($"Invalid URI value at line {reader.LineNumber}");
        }

        return uri;
    }

    public override void Write(CalendarWriter writer, Uri value)
    {
        writer.WriteValue(value);
    }
}
