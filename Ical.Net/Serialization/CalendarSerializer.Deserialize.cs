//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ical.Net.CalendarComponents;

namespace Ical.Net.Serialization;

public partial class CalendarSerializer
{
    public static List<T> DeserializeCollection<T>(
        string value,
        CalendarSerializerOptions? options = default) where T : CalendarComponent
    {
        var utf8Bytes = Encoding.UTF8.GetBytes(value);
        using var stream = new MemoryStream(utf8Bytes);

        return DeserializeCollection<T>(stream, options);
    }

    public static List<T> DeserializeCollection<T>(
        Stream utf8Stream,
        CalendarSerializerOptions? options = default) where T : CalendarComponent
    {
        var reader = new CalendarReader(utf8Stream);

        var components = new List<T>();

        while (true)
        {
            var component = InternalDeserialize<T>(reader, options);

            if (component is null)
            {
                break;
            }

            components.Add(component);
        }

        return components;
    }

    public static async Task<List<T>> DeserializeCollectionAsync<T>(
        Stream utf8Stream,
        CalendarSerializerOptions? options = default,
        CancellationToken cancellationToken = default) where T : CalendarComponent
    {
        var reader = new CalendarReader(utf8Stream);

        var components = new List<T>();

        while (true)
        {
            var component = await InternalDeserializeAsync<T>(reader, options, cancellationToken)
                .ConfigureAwait(false);

            if (component is null)
            {
                break;
            }

            components.Add(component);
        }

        return components;
    }

#if NET10_0_OR_GREATER
    public static async IAsyncEnumerable<Calendar> DeserializeAsyncEnumerable(
        Stream utf8Stream,
        CalendarSerializerOptions? options = default,
        [EnumeratorCancellation]
        CancellationToken cancellationToken = default)
    {
        var reader = new CalendarReader(utf8Stream);

        while (!cancellationToken.IsCancellationRequested)
        {
            var component = await InternalDeserializeAsync<Calendar>(reader, options, cancellationToken)
                .ConfigureAwait(false);

            if (component is null)
            {
                break;
            }

            yield return component;
        }
    }
#endif

    public static T Deserialize<T>(
        string value,
        CalendarSerializerOptions? options = default) where T : CalendarComponent
    {
        var utf8Bytes = Encoding.UTF8.GetBytes(value);
        var stream = new MemoryStream(utf8Bytes);
        return Deserialize<T>(stream, options);
    }

    public static T Deserialize<T>(
        Stream utf8Stream,
        CalendarSerializerOptions? options = default) where T : CalendarComponent
    {
        var reader = new CalendarReader(utf8Stream);

        var component = InternalDeserialize<T>(reader, options)
            ?? throw new SerializationException("Unable to deserialize component");

        return component;
    }

    public static async Task<T> DeserializeAsync<T>(
        Stream utf8Stream,
        CalendarSerializerOptions? options = default,
        CancellationToken cancellationToken = default) where T : CalendarComponent
    {
        var reader = new CalendarReader(utf8Stream);
        var component = await InternalDeserializeAsync<T>(reader, options, cancellationToken).ConfigureAwait(false);

        if (component is null)
        {
            throw new SerializationException("Unable to deserialize component");
        }

        return component;
    }

    private static T? InternalDeserialize<T>(
        CalendarReader reader,
        CalendarSerializerOptions? options = default) where T : CalendarComponent
    {
        options ??= new();

        ContentLineResult result = default;
        var components = new Stack<CalendarComponent>();

        while (reader.ReadContentLine())
        {
            result = ProcessContentLine<T>(reader, components, options);

            if (result == ContentLineResult.End)
            {
                break;
            }
        }

        if ((result != ContentLineResult.End && components.Count > 0) || components.Count > 1)
        {
            throw new SerializationException($"Missing end of component at line {reader.LineNumber}");
        }

        if (components.Count == 1 && components.Pop() is T finalComponent)
        {
            return finalComponent;
        }

        return default;
    }

    private static async Task<T?> InternalDeserializeAsync<T>(
        CalendarReader reader,
        CalendarSerializerOptions? options = default,
        CancellationToken cancellationToken = default) where T : CalendarComponent
    {
        options ??= new();

        ContentLineResult result = default;
        var components = new Stack<CalendarComponent>();

        while (await reader.ReadContentLineAsync(cancellationToken).ConfigureAwait(false))
        {
            result = ProcessContentLine<T>(reader, components, options);

            if (result == ContentLineResult.End)
            {
                break;
            }
        }

        if (result != ContentLineResult.End || components.Count > 1)
        {
            throw new SerializationException($"Missing end of component at line {reader.LineNumber}");
        }

        if (components.Count == 1 && components.Pop() is T finalComponent)
        {
            return finalComponent;
        }

        return default;
    }

    private enum ContentLineResult
    {
        Continue,
        End,
    }

    private static ContentLineResult ProcessContentLine<T>(
        CalendarReader reader,
        Stack<CalendarComponent> components,
        CalendarSerializerOptions options) where T : CalendarComponent
    {
        var name = reader.ReadName();

        CalendarComponent component;

        // Name is already uppercase, so compare ordinal
        if (string.Equals(name, "BEGIN", StringComparison.Ordinal))
        {
            var componentName = reader.GetRawStringValue();

            if (componentName == string.Empty)
            {
                throw new SerializationException($"Missing component name at line {reader.LineNumber}");
            }

            component = ConverterMap.GetCalendarComponent(componentName);

            // Make sure first component is the requested type
            if (components.Count == 0 && component is not T)
            {
                throw new SerializationException($"Unexpected component type \"{componentName}\" at line {reader.LineNumber}");
            }

            // Remove all default properties
            component.Properties.Clear();

            components.Push(component);
            return ContentLineResult.Continue;
        }

        if (components.Count == 0)
        {
            throw new SerializationException("Expected start of component (BEGIN:COMPONENT)");
        }

        if (string.Equals(name, "END", StringComparison.Ordinal))
        {
            var componentName = reader.GetRawStringValue();

            component = components.Pop();

            if (!string.Equals(componentName, component.Name, StringComparison.OrdinalIgnoreCase))
            {
                throw new SerializationException($"Unmatched END of component \"{component.Name.ToUpperInvariant()}\""
                    + $" at line no. {reader.LineNumber}");
            }

            if (components.Count == 0)
            {
                if (component is T typedComponent)
                {
                    components.Push(typedComponent);
                    return ContentLineResult.End;
                }

                throw new SerializationException("Unexpected final component type");
            }

            var parent = components.Peek();
            component.Parent = parent;

            parent.AddChild(component);
            return ContentLineResult.Continue;
        }

        component = components.Peek();

        try
        {
            // Get converter based on content line name and
            // allowed VALUE override.
            var converter = options.GetConverter(name);

            var property = converter.ReadProperty(reader, name, options);

            component.Properties.Add(property);
        }
        catch (SerializationException)
        {
            throw;
        }
        catch (Exception) when (!options.StrictParsing)
        {
            // Ignore error
        }

        return ContentLineResult.Continue;
    }
}
