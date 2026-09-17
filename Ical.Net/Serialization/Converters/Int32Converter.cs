//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.Globalization;
using System.Runtime.Serialization;

namespace Ical.Net.Serialization.Converters;

internal class Int32Converter : CalendarPropertyConverter<int>
{
    public override int Read(CalendarReader reader, IParameterCollection parameters)
    {
        if (reader.TryGetInteger(out var value))
        {
            return value;
        }

        throw new SerializationException($"Could not parse integer value at line {reader.LineNumber}");
    }

    public override void Write(CalendarWriter writer, int value) => writer.WriteValue(value);
}
