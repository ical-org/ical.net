//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.IO;
using Ical.Net.DataTypes;

namespace Ical.Net.Serialization.DataTypes;

public class TriggerSerializer : StringSerializer
{
    public TriggerSerializer() { }

    public TriggerSerializer(SerializationContext ctx) : base(ctx) { }

    public override Type TargetType => typeof(Trigger);

    public override string? SerializeToString(object? obj)
    {
        try
        {
            if (obj is not Trigger t)
            {
                return null;
            }

            // Push the trigger onto the serialization stack
            SerializationContext.Push(t);
            try
            {
                var factory = GetService<ISerializerFactory>();
                if (factory == null)
                {
                    return null;
                }

                var valueType = t.GetValueType() ?? typeof(Duration);
                if (!(factory.Build(valueType, SerializationContext) is IStringSerializer serializer))
                {
                    return null;
                }

                var value = valueType == typeof(CalDateTime)
                    ? t.DateTime
                    : (object?) t.Duration;

                return serializer.SerializeToString(value);
            }
            finally
            {
                // Pop the trigger off the serialization stack
                SerializationContext.Pop();
            }
        }
        catch
        {
            return null;
        }
    }

    public override object? Deserialize(TextReader? tr)
    {
        if (tr == null) return null;

        var value = tr.ReadToEnd();

        if (!(CreateAndAssociate() is Trigger t))
        {
            return null;
        }

        // Push the trigger onto the serialization stack
        SerializationContext.Push(t);
        try
        {
            // Decode the value as needed
            value = Decode(t, value);

            if (value == null) return null;

            // Set the trigger relation
            var relatedParameter = t.Parameters.Get("RELATED");
            if (relatedParameter is not null && relatedParameter.Equals("END"))
            {
                t.Related = TriggerRelation.End;
            }

            var factory = GetService<ISerializerFactory>();
            if (factory == null)
            {
                return null;
            }

            var valueType = t.GetValueType() ?? typeof(Duration);
            var serializer = factory.Build(valueType, SerializationContext) as IStringSerializer;
            var obj = serializer?.Deserialize(new StringReader(value));
            switch (obj)
            {
                case null:
                    return null;
                case CalDateTime dt:
                    t.DateTime = dt;
                    break;
                default:
                    t.Duration = (Duration) obj;
                    break;
            }

            return t;
        }
        finally
        {
            // Pop the trigger off the serialization stack
            SerializationContext.Pop();
        }
    }
}
