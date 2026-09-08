//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.Collections;
using System.Runtime.Serialization;
using Ical.Net.DataTypes;

namespace Ical.Net.Serialization;

public abstract class CalendarPropertyConverter
{
    public abstract Type Type { get; }

    public abstract bool IsListValue { get; }

    public abstract bool CanConvert(Type typeToConvert);

    public abstract bool IsValueBase64(ICalendarParameterCollectionContainer container);

    internal abstract object? ReadObject(CalendarReader reader, IParameterCollection parameters);

    internal abstract void WriteObject(CalendarWriter writer, object value);

    internal abstract void WriteObjectParameters(CalendarWriter writer, ICalendarParameterCollectionContainer container);

    internal CalendarProperty ReadProperty(
        CalendarReader reader,
        string name,
        CalendarSerializerOptions options)
    {
        var property = new CalendarProperty(name);

        ReadParameters(reader, property, options);

        if (IsValueBase64(property))
        {
            reader.ReadNextValueAsBase64();
        }

        object? propertyValue;

        try
        {
            propertyValue = ReadObject(reader, property.Parameters);
        }
        catch (Exception ex) when (ex is not SerializationException)
        {
            throw new SerializationException(
                $"Failed to read value for {name} at line no. {reader.LineNumber}", ex);
        }

        if (propertyValue is ICalendarDataType calendarDataType)
        {
            calendarDataType.AssociatedObject = property;
        }

        // TODO: What if a custom value is List<string> that should
        // not be enumerated like this?
        if (IsListValue && propertyValue is IEnumerable valueList)
        {
            foreach (var s in valueList)
            {
                property.AddValue(s);
            }
        }
        else
        {
            property.AddValue(propertyValue);
        }

        return property;
    }

    private static void ReadParameters(
        CalendarReader reader,
        CalendarProperty property,
        CalendarSerializerOptions options)
    {
        while (reader.TryReadParameterName(out var parameterName))
        {
            var parameter = new CalendarParameter(parameterName);

            // Read parameter values
            while (reader.TryReadParameterValue(out var parameterValue))
            {
                parameter.AddValue(parameterValue);
            }

            if (parameter.ValueCount == 0)
            {
                if (options.StrictParsing)
                {
                    throw new SerializationException($"Property parameter is missing a value at line {reader.LineNumber}");
                }

                // Ignore empty parameter
                continue;
            }

            property.AddParameter(parameter);
        }
    }


    internal void WriteProperty(CalendarWriter writer, ICalendarProperty property)
    {
        if (IsListValue)
        {
            // Write comma-separated list of values
            WritePropertyLine(writer, property, property.Values);
        }
        else if (property.ValueCount > 1)
        {
            // Write multiple values as separate properties
            foreach (var singleValue in property.Values)
            {
                if (singleValue is not null)
                {
                    WritePropertyLine(writer, property, singleValue);
                }
            }
        }
        else if (property.Value is { } propertyValue)
        {
            // Write a single value property
            WritePropertyLine(writer, property, propertyValue);
        }
    }

    private void WritePropertyLine(
        CalendarWriter writer,
        ICalendarProperty property,
        object propertyValue)
    {
        writer.WriteName(property.Name);

        try
        {
            // The parameter proxy system has issues. If the value is a proxy,
            // use the proxy as the parameters instead of the actual property
            // paramters.
            ICalendarParameterCollectionContainer parameterContainer = propertyValue is CalendarDataType calDataValue
                ? calDataValue : property;

            WriteObjectParameters(writer, parameterContainer);
        }
        catch (Exception ex) when (ex is not SerializationException)
        {
            throw new SerializationException(
                $"Failed to write property parameters for \"{property.Name}\"", ex);
        }

        if (IsValueBase64(property))
        {
            writer.WriteNextValueAsBase64();
        }

        try
        {
            WriteObject(writer, propertyValue);
        }
        catch (InvalidCastException)
        {
            throw new SerializationException($"Value type of property \"{property.Name}\""
                + $" does not match converter type");
        }
        catch (Exception ex) when (ex is not SerializationException)
        {
            throw new SerializationException(
                $"Failed to write value for property \"{property.Name}\". See inner exception.", ex);
        }

        writer.EndLine();
    }
}
