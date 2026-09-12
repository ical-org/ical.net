//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Ical.Net.DataTypes;

namespace Ical.Net.Serialization.Converters;

internal class AttachmentConverter : CalendarPropertyConverter<Attachment>
{
    public override Attachment? Read(CalendarReader reader, IParameterCollection parameters)
    {
        if (parameters.Get("VALUE") is string valueTypeName
            && valueTypeName == "BINARY")
        {
            var binaryData = reader.ReadBinaryValue();
            return new Attachment(binaryData);
        }

        if (reader.TryGetUri(out var uri))
        {
            return new Attachment
            {
                Uri = uri
            };
        }

        throw new SerializationException($"Attachment URI could not be parsed at line {reader.LineNumber}");
    }

    public override void Write(CalendarWriter writer, Attachment value)
    {
        if (value.Uri is { } uri)
        {
            writer.WriteValue(uri);
            return;
        }

        if (value.Data is { } data)
        {
            writer.WriteParameter("VALUE", "BINARY");
            writer.WriteParameter("ENCODING", "BASE64");

            writer.WriteBinaryValue(data);
            return;
        }

        throw new SerializationException("Attachment is missing data");
    }

    public override void WriteParameters(
        CalendarWriter writer,
        IEnumerable<CalendarParameter> parameters,
        ICalendarParameterCollectionContainer container)
    {
        // Set VALUE based on value instead
        var parametersExceptValue = parameters
            .Where(x => !x.Name.Equals("VALUE", StringComparison.OrdinalIgnoreCase)
                && !x.Name.Equals("ENCODING", StringComparison.OrdinalIgnoreCase));

        base.WriteParameters(writer, parametersExceptValue, container);
    }
}
