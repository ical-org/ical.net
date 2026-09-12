//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using Ical.Net.Serialization;

namespace Ical.Net.Tests;

public enum SerializerVersion
{
    Current,
    Obsolete,
}

internal class SerializerSwitch(SerializerVersion serializerVersion)
{
    public string Serialize(Calendar calendar)
    {
        if (serializerVersion == SerializerVersion.Current)
        {
            return CalendarSerializer.Serialize(calendar);
        }

        return new CalendarSerializer().SerializeToString(calendar)!;
    }

    public T Deserialize<T>(string value) where T : Calendar
    {
        if (serializerVersion == SerializerVersion.Current)
        {
            return CalendarSerializer.Deserialize<T>(value);
        }

#pragma warning disable CS0618 // Type or member is obsolete
        return (T) Calendar.Load(value)!;
#pragma warning restore CS0618 // Type or member is obsolete
    }
}

