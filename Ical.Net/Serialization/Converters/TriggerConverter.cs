//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using Ical.Net.DataTypes;

namespace Ical.Net.Serialization.Converters;

internal class TriggerConverter : CalendarPropertyConverter<Trigger>
{
    public override Trigger? Read(CalendarReader reader, IParameterCollection parameters)
    {
        var valueStr = reader.GetTextValue();

        var t = new Trigger();

        if (parameters.Get("VALUE") is { } valueType && valueType is "DATE-TIME" or "DATE")
        {
            t.DateTime = CalDateTime.Parse(valueStr, tzId: null);
        }
        else
        {
            t.Duration = Duration.Parse(valueStr);
        }

        if (parameters.Get("RELATED") is { } p && p == TriggerRelation.End)
        {
            t.Related = TriggerRelation.End;
        }

        return t;
    }

    public override void Write(CalendarWriter writer, Trigger value)
    {
        if (value.DateTime is { } dt)
        {
            writer.WriteParameter("VALUE", dt.HasTime ? "DATE-TIME" : "DATE");
            writer.WriteValue(dt.ToBasicIso());
        }
        else if (value.Duration is { } duration)
        {
            // Duration is default
            // writer.WriteParameter("VALUE", "DURATION");
            writer.WriteValue(duration.ToBasicIso());
        }
        else
        {
            writer.WriteValue(string.Empty);
        }
    }
}
