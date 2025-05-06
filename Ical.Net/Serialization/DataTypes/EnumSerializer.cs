//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

#nullable enable
using System;
using System.IO;
using Ical.Net.DataTypes;

namespace Ical.Net.Serialization.DataTypes;

public class EnumSerializer : EncodableDataTypeSerializer
{
    private readonly Type _mEnumType;

    public EnumSerializer(Type enumType)
    {
        _mEnumType = enumType;
    }

    public EnumSerializer(Type enumType, SerializationContext ctx) : base(ctx)
    {
        _mEnumType = enumType;
    }

    public override Type TargetType => _mEnumType;

    public override string? SerializeToString(object? obj)
    {
        try
        {
            if (SerializationContext.Peek() is ICalendarObject calObject)
            {
                // Encode the value as needed.
                var dt = new EncodableDataType
                {
                    AssociatedObject = calObject
                };
                return Encode(dt, obj?.ToString());
            }
            return obj?.ToString();
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

            // Remove "-" characters while parsing Enum values.
            if (value != null)
                return Enum.Parse(_mEnumType, value.Replace("-", ""), true);
        }
        catch
        {
            // Return null instead of throwing an exception
        }

        return value;
    }
}
