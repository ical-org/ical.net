//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.Runtime.Serialization;
using Ical.Net.DataTypes;

namespace Ical.Net.Serialization.Converters;

// T is object instead of Attendee because there is a test
// using a string value.
internal class AttendeeConverter : CalendarPropertyConverter<object>
{
    public override object? Read(CalendarReader reader, IParameterCollection parameters)
    {
        var value = reader.GetRawStringValue();

        // Prepend "mailto:" if necessary
        if (!value.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase))
        {
            value = "mailto:" + value;
        }

        try
        {
            return new Attendee(new Uri(value));
        }
        catch (UriFormatException ex)
        {
            throw new SerializationException("ATTENDEE value is invalid", ex);
        }
    }

    public override void Write(CalendarWriter writer, object value)
    {
        if (value is Attendee attendee)
        {
            if (attendee.Value == null)
            {
                writer.WriteValue(string.Empty);
                return;
            }

            writer.WriteValue(attendee.Value.OriginalString);
        }
        else if (value is string valueString)
        {
            // Value should not be escaped
            writer.WriteValueRaw(valueString);
        }
    }
}
