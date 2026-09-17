//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System.Runtime.Serialization;
using Ical.Net.DataTypes;

namespace Ical.Net.Serialization.Converters;

internal class RequestStatusConverter : CalendarPropertyConverter<RequestStatus>
{
    public override RequestStatus? Read(CalendarReader reader, IParameterCollection parameters)
    {
        // REQUEST-STATUS value type is "structured" TEXT value

        if (!reader.TryGetStructuredTextValue(out var code)
            || !StatusCode.TryParse(code, out var statusCode))
        {
            return null;
        }

        if (!reader.TryGetStructuredTextValue(out var description))
        {
            return null;
        }

        var status = new RequestStatus
        {
            StatusCode = statusCode,
            Description = description
        };

        if (reader.TryGetStructuredTextValue(out var extraData))
        {
            status.ExtraData = extraData;
        }

        return status;
    }

    public override void Write(CalendarWriter writer, RequestStatus value)
    {
        if (value.StatusCode == null)
        {
            throw new SerializationException("Request status is missing a status code");
        }

        if (value.Description == null)
        {
            throw new SerializationException("Request status is missing a description");
        }

        writer.WriteStructuredTextSegment(value.StatusCode.ToString());

        writer.WriteStructuredTextSegment(value.Description);

        if (!string.IsNullOrEmpty(value.ExtraData))
        {
            writer.WriteStructuredTextSegment(value.Description);
        }
    }
}
