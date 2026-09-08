//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
#if NET8_0_OR_GREATER
using System.Collections.Frozen;
#endif
using System.Collections.Generic;
using Ical.Net.CalendarComponents;
using Ical.Net.Serialization.Converters;

namespace Ical.Net.Serialization;

internal static class ConverterMap
{
    internal static CalendarPropertyConverter GetConverter(string propertyName)
    {
        if (_converters.TryGetValue(propertyName, out var converter))
        {
            return converter;
        }

        // Property name is unknown. Treat value as a single text value.
        // The VALUE parameter will not change the parsed value type.
        return objectToStringConverter;
    }

    internal static CalendarComponent GetCalendarComponent(string name)
    {
        if (_components.TryGetValue(name, out var createInstance))
        {
            return createInstance();
        }

        return new CalendarComponent
        {
            Name = name.ToUpperInvariant()
        };
    }

#if NET8_0_OR_GREATER
    private static readonly FrozenDictionary<string, CalendarPropertyConverter> _converters = InitializeMap().ToFrozenDictionary();
#else
    private static readonly Dictionary<string, CalendarPropertyConverter> _converters = InitializeMap();
#endif

#if NET8_0_OR_GREATER
    private static readonly FrozenDictionary<string, Func<CalendarComponent>> _components = InitializeComponentMap().ToFrozenDictionary();
#else
    private static readonly Dictionary<string, Func<CalendarComponent>> _components = InitializeComponentMap();
#endif

    private static readonly ObjectToStringConverter objectToStringConverter = new();

    private static Dictionary<string, CalendarPropertyConverter> InitializeMap()
    {
        DateTimeConverter dateTimeConverter = new();
        StringListConverter stringListConverter = new();
        UriConverter uriConverter = new();
        PeriodListConverter periodListConverter = new();
        UtcOffsetConverter utcOffsetConverter = new();
        Int32Converter int32Converter = new();
        StringConverter stringConverter = new();

        return new (StringComparer.OrdinalIgnoreCase)
        {
            { "ATTACH", new AttachmentConverter() },
            { "ATTENDEE", new AttendeeConverter() },
            { "CATEGORIES", stringListConverter},
            { "COMPLETED", dateTimeConverter},
            { "CREATED", dateTimeConverter},
            { "DTEND", dateTimeConverter},
            { "DTSTAMP", dateTimeConverter},
            { "DTSTART", dateTimeConverter},
            { "DUE", dateTimeConverter},
            { "DURATION", new DurationConverter()},
            { "EXDATE", periodListConverter},
            { "FREEBUSY", new FreeBusyConverter()},
            { "GEO", new GeoConverter()},
            { "LAST-MODIFIED", dateTimeConverter},
            { "ORGANIZER", new OrganizerConverter()},
            { "PERCENT-COMPLETE", int32Converter},
            { "PRIORITY", int32Converter},
            { "RDATE", periodListConverter},
            { "RECURRENCE-ID", new RecurrenceIdConverter()},
            { "REQUEST-STATUS", new RequestStatusConverter()},
            { "REPEAT", int32Converter},
            { "RESOURCES", stringListConverter},
            { "RRULE", new RruleConverter()},
            { "SEQUENCE", int32Converter},
            { "STATUS", stringConverter},
            { "TRANSP", stringConverter},
            { TriggerRelation.Name, new TriggerConverter()},
            { "TZOFFSETFROM", utcOffsetConverter},
            { "TZOFFSETTO", utcOffsetConverter},
            { "TZURL", uriConverter},
            { "URL", uriConverter},
        };
    }

    private static Dictionary<string, Func<CalendarComponent>> InitializeComponentMap()
    {
        return new(StringComparer.OrdinalIgnoreCase)
        {
            { Components.Alarm, static () => new Alarm() },
            { EventStatus.Name, static () => new CalendarEvent() },
            { Components.Freebusy, static () => new FreeBusy() },
            { JournalStatus.Name, static () => new Journal() },
            { Components.Timezone, static () => new VTimeZone() },
            { TodoStatus.Name, static () => new Todo() },
            { Components.Calendar, static () => new Calendar() },
            { Components.Daylight, static () => new VTimeZoneInfo(Components.Daylight) },
            { Components.Standard, static () => new VTimeZoneInfo(Components.Standard) },
        };
    }
}
