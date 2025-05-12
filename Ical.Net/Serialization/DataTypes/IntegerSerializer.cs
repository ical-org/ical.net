//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.Globalization;
using System.IO;
using Ical.Net.DataTypes;

namespace Ical.Net.Serialization.DataTypes;

public class IntegerSerializer : EncodableDataTypeSerializer
{
    public IntegerSerializer() { }

    public IntegerSerializer(SerializationContext ctx) : base(ctx) { }

    public override Type TargetType => typeof(int);

    public override string? SerializeToString(object? obj)
    {
        try
        {
            var i = Convert.ToInt32(obj, CultureInfo.InvariantCulture);

            if (SerializationContext.Peek() is ICalendarObject calObject)
            {
                // Encode the value as needed.
                var dt = new EncodableDataType
                {
                    AssociatedObject = calObject
                };
                return Encode(dt, i.ToString(CultureInfo.InvariantCulture));
            }
            return i.ToString(CultureInfo.InvariantCulture);
        }
        catch
        {
            return null;
        }
    }

    public override object? Deserialize(TextReader tr)
    {
        var value = tr.ReadToEnd();

        try
        {
            if (SerializationContext.Peek() is ICalendarObject obj)
            {
                // Decode the value, if necessary!
                var dt = new EncodableDataType
                {
                    AssociatedObject = obj
                };
                value = Decode(dt, value);
            }

            if (int.TryParse(value, out var i))
            {
                return i;
            }
        }
        catch
        {
            // Return null instead of throwing an exception
        }

        return null;
    }
}
