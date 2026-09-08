//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.Collections.Generic;
using Ical.Net.DataTypes;

namespace Ical.Net.Serialization;

public abstract class CalendarPropertyConverter<T> : CalendarPropertyConverter
{
    public sealed override Type Type => typeof(T);

    public override bool IsListValue => false;

    public override bool CanConvert(Type typeToConvert) => typeToConvert == typeof(T);

    public override bool IsValueBase64(ICalendarParameterCollectionContainer container)
        => container.Parameters.Get("ENCODING") == "BASE64";

    public abstract T? Read(CalendarReader reader, IParameterCollection parameters);

    public abstract void Write(CalendarWriter writer, T value);

    public virtual void WriteParameters(
        CalendarWriter writer,
        IEnumerable<CalendarParameter> parameters,
        ICalendarParameterCollectionContainer container)
    {
        foreach (var p in parameters)
        {
            if (p.ValueCount == 0)
            {
                continue;
            }

            writer.WriteParameterName(p.Name);

            // Since parameter values are validated as not null
            // and not empty, if value count is not zero, then
            // there is at least one non-empty value to write.
            foreach (var paramValue in p.Values)
            {
                writer.WriteParameterValue(paramValue!);
            }
        }
    }

    internal sealed override object? ReadObject(CalendarReader reader, IParameterCollection parameters)
        => Read(reader, parameters);

    internal sealed override void WriteObject(CalendarWriter writer, object value)
        => Write(writer, (T)value);

    internal sealed override void WriteObjectParameters(CalendarWriter writer, ICalendarParameterCollectionContainer value)
        => WriteParameters(writer, value.Parameters, value);
}
