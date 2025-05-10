//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ical.Net.DataTypes;

namespace Ical.Net.Serialization.DataTypes;

public class PeriodListSerializer : EncodableDataTypeSerializer
{
    public PeriodListSerializer() { }

    public PeriodListSerializer(SerializationContext ctx) : base(ctx) { }

    public override Type TargetType => typeof(PeriodList);

    public override string? SerializeToString(object? obj)
    {
        var factory = GetService<ISerializerFactory>();
        if (obj is not PeriodList periodList)
        {
            return null;
        }

        var dtSerializer = factory.Build(typeof(CalDateTime), SerializationContext) as IStringSerializer;
        var periodSerializer = factory.Build(typeof(Period), SerializationContext) as IStringSerializer;
        if (dtSerializer == null || periodSerializer == null)
        {
            return null;
        }

        var parts = new List<string?>(periodList.Count);

        var firstPeriod = periodList.FirstOrDefault();

        // Set TzId before ValueType, so that it serializes first
        if (firstPeriod != null && !string.IsNullOrEmpty(firstPeriod.TzId) && firstPeriod.TzId != "UTC")
        {
            periodList.Parameters.Set("TZID", periodList[0].TzId);
        }

        switch (firstPeriod?.PeriodKind) // default type is DATE-TIME
        {
            case PeriodKind.Period:
                periodList.SetValueType("PERIOD");
                break;
            case PeriodKind.DateOnly:
                periodList.SetValueType("DATE");
                break;
        }

        foreach (var p in periodList)
        {
            parts.Add(p.EffectiveDuration != null
                ? periodSerializer.SerializeToString(p)
                : dtSerializer.SerializeToString(p.StartTime));
        }

        return Encode(periodList, string.Join(",", parts));
    }

    public override object? Deserialize(TextReader tr)
    {
        var value = tr.ReadToEnd();

        // Create the day specifier and associate it with a calendar object
        if (CreateAndAssociate() is not PeriodList rdt) return null;

        var factory = GetService<ISerializerFactory>();

        // Decode the value, if necessary
        value = Decode(rdt, value);

        var dtSerializer = factory.Build(typeof(CalDateTime), SerializationContext) as IStringSerializer;
        var periodSerializer = factory.Build(typeof(Period), SerializationContext) as IStringSerializer;
        if (value == null || dtSerializer == null || periodSerializer == null)
        {
            return null;
        }

        var values = value.Split(',');
        foreach (var v in values)
        {
            var dt = dtSerializer.Deserialize(new StringReader(v)) as CalDateTime;
            var p = periodSerializer.Deserialize(new StringReader(v)) as Period;

            if (dt != null)
            {
                rdt.Add(dt);
            }
            else if (p != null)
            {
                rdt.Add(p);
            }
        }
        return rdt;
    }
}
