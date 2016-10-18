﻿using System;
using System.IO;
using ical.net.DataTypes;
using ical.net.Interfaces.DataTypes;
using ical.net.Interfaces.Serialization;
using ical.net.Interfaces.Serialization.Factory;
using ical.net.Serialization.iCalendar.Serializers.Other;

namespace ical.net.Serialization.iCalendar.Serializers.DataTypes
{
    public class TriggerSerializer : StringSerializer
    {
        public override Type TargetType => typeof (Trigger);

        public override string SerializeToString(object obj)
        {
            try
            {
                var t = obj as ITrigger;
                if (t != null)
                {
                    // Push the trigger onto the serialization stack
                    SerializationContext.Push(t);
                    try
                    {
                        var factory = GetService<ISerializerFactory>();
                        if (factory != null)
                        {
                            var valueType = t.GetValueType() ?? typeof (TimeSpan);
                            var serializer = factory.Build(valueType, SerializationContext) as IStringSerializer;
                            if (serializer != null)
                            {
                                var value = (valueType == typeof (IDateTime)) ? t.DateTime : (object) t.Duration;
                                return serializer.SerializeToString(value);
                            }
                        }
                    }
                    finally
                    {
                        // Pop the trigger off the serialization stack
                        SerializationContext.Pop();
                    }
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        public override object Deserialize(TextReader tr)
        {
            var value = tr.ReadToEnd();

            var t = CreateAndAssociate() as ITrigger;
            if (t != null)
            {
                // Push the trigger onto the serialization stack
                SerializationContext.Push(t);
                try
                {
                    // Decode the value as needed
                    value = Decode(t, value);

                    // Set the trigger relation
                    if (t.Parameters.ContainsKey("RELATED") && t.Parameters.Get("RELATED").Equals("END"))
                    {
                        t.Related = TriggerRelation.End;
                    }

                    var factory = GetService<ISerializerFactory>();
                    if (factory != null)
                    {
                        var valueType = t.GetValueType() ?? typeof (TimeSpan);
                        var serializer = factory.Build(valueType, SerializationContext) as IStringSerializer;
                        var obj = serializer?.Deserialize(new StringReader(value));
                        if (obj != null)
                        {
                            if (obj is IDateTime)
                            {
                                t.DateTime = (IDateTime) obj;
                            }
                            else
                            {
                                t.Duration = (TimeSpan) obj;
                            }

                            return t;
                        }
                    }
                }
                finally
                {
                    // Pop the trigger off the serialization stack
                    SerializationContext.Pop();
                }
            }
            return null;
        }
    }
}