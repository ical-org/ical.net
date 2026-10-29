//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.IO;
using System.Linq;
using Ical.Net.CalendarComponents;
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

    public string Serialize(CalendarEvent calendarEvent)
    {
        if (serializerVersion == SerializerVersion.Current)
        {
            return CalendarSerializer.Serialize(calendarEvent);
        }

        return new EventSerializer().SerializeToString(calendarEvent)!;
    }

    public void Serialize(Stream utf8Destination, Calendar component)
    {
        if (serializerVersion == SerializerVersion.Current)
        {
            CalendarSerializer.Serialize(utf8Destination, component);
            return;
        }

        new CalendarSerializer().Serialize(component, utf8Destination);
    }

    public T Deserialize<T>(string value) where T : CalendarComponent
    {
        if (serializerVersion == SerializerVersion.Current)
        {
            return CalendarSerializer.Deserialize<T>(value);
        }

        using var reader = new StringReader(value);
        return (T) SimpleDeserializer.Default.Deserialize(reader).First();
    }

    public T Deserialize<T>(Stream stream) where T : CalendarComponent
    {
        if (serializerVersion == SerializerVersion.Current)
        {
            return CalendarSerializer.Deserialize<T>(stream);
        }

        using var reader = new StreamReader(stream);
        return (T) SimpleDeserializer.Default.Deserialize(reader).First();
    }
}

