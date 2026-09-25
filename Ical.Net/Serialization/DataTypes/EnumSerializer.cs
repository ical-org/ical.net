//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

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

        if (SerializationContext.Peek() is ICalendarObject obj)
        {
            // Decode the value, if necessary!
            var dt = new EncodableDataType
            {
                AssociatedObject = obj
            };
            value = Decode(dt, value);
        }

        if (value == null)
        {
            return null;
        }

        try
        {
            // Hyphens are stripped before matching.
            return Enum.Parse(_mEnumType, value.Replace("-", ""), true);
        }
        catch (Exception ex) when (ex is ArgumentException or OverflowException)
        {
            // The value is not a member of the enum (ArgumentException) or does not fit its
            // underlying type (OverflowException). Both are common in real-world .ics files, so
            // fall back to the raw string. Decoding above is deliberately outside this, so a
            // decoding failure propagates rather than yielding a silently mistyped value.
            return value;
        }
    }
}
