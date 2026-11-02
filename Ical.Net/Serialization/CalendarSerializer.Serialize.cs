//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ical.Net.CalendarComponents;

namespace Ical.Net.Serialization;

public partial class CalendarSerializer
{
    public static string Serialize<T>(
        T component,
        CalendarSerializerOptions? options = null) where T : CalendarComponent
    {
        using var memory = new MemoryStream();
        Serialize(memory, component, options);

        var buffer = memory.GetBuffer();

        // Max string length is int.MaxValue
        var length = (int) memory.Length;

        return Encoding.UTF8.GetString(buffer, 0, length);
    }

    public static void Serialize(
        Stream utf8Destination,
        CalendarComponent component,
        CalendarSerializerOptions? options = null)
    {
        var writer = new CalendarWriter(utf8Destination);

        options ??= new();

        Serialize(writer, component, options);
    }

    private static void Serialize(
        CalendarWriter writer,
        CalendarComponent component,
        CalendarSerializerOptions options)
    {
        BeginComponent(writer, component, options);

        // Write children
        foreach (var child in component.Children)
        {
            if (child is CalendarComponent childComponent)
            {
                Serialize(writer, childComponent, options);
            }
        }

        EndComponent(writer, component);

        writer.Flush();
    }

    public static Task SerializeAsync(
        Stream utf8Destination,
        CalendarComponent component,
        CalendarSerializerOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var writer = new CalendarWriter(utf8Destination);

        options ??= new();

        return SerializeAsync(writer, component, options, cancellationToken);
    }

    private static async Task SerializeAsync(
        CalendarWriter writer,
        CalendarComponent component,
        CalendarSerializerOptions options,
        CancellationToken cancellationToken = default)
    {
        BeginComponent(writer, component, options);

        // Write children
        foreach (var child in component.Children)
        {
            if (child is CalendarComponent childComponent)
            {
                await SerializeAsync(writer, childComponent, options, cancellationToken);
            }
        }

        EndComponent(writer, component);

        // Flush after every component
        await writer.FlushAsync(cancellationToken).ConfigureAwait(false);
    }

    private static void BeginComponent(
        CalendarWriter writer,
        CalendarComponent component,
        CalendarSerializerOptions options)
    {
        writer.WriteRawContentLine("BEGIN"u8, component.Name);

        IEnumerable<ICalendarProperty> properties = component.Properties;
        if (options.OrderComponentProperties)
        {
            properties = properties.OrderBy(x => x.Name);
        }

        // Write properties
        foreach (var property in properties)
        {
            // Validate component properties
            if (!component.ShouldSerializeProperty(property))
            {
                continue;
            }

            var converter = options.GetConverter(property.Name);

            converter.WriteProperty(writer, property);
        }
    }

    private static void EndComponent(CalendarWriter writer, CalendarComponent component)
        => writer.WriteRawContentLine("END"u8, component.Name);
}
