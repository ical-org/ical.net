using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace DDay.iCal.Serialization.iCalendar
{
    public class TriggerSerializer :
        StringSerializer
    {
        public override Type TargetType
        {
            get { return typeof(Trigger); }
        }

        public override string SerializeToString(object obj)
        {
            try
            {
                ITrigger t = obj as ITrigger;
                if (t != null)
                {
                    // Push the trigger onto the serialization stack
                    SerializationContext.Push(t);
                    try
                    {
                        ISerializerFactory factory = GetService<ISerializerFactory>();
                        if (factory != null)
                        {
                            Type valueType = t.GetValueType() ?? typeof(TimeSpan);
                            IStringSerializer serializer = factory.Build(valueType, SerializationContext) as IStringSerializer;
                            if (serializer != null)
                            {
                                object value = (valueType == typeof(IDateTime)) ? (object)t.DateTime : (object)t.Duration;
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
            string value = tr.ReadToEnd();

            ITrigger t = CreateAndAssociate() as ITrigger;
            if (t != null)
            {
                // Push the trigger onto the serialization stack
                SerializationContext.Push(t);
                try
                {
                    // Decode the value as needed
                    value = Decode(t, value);

                    // Set the trigger relation
                    if (t.Parameters.ContainsKey("RELATED") &&
                        t.Parameters.Get("RELATED").Equals("END"))
                    {
                        t.Related = TriggerRelation.End;
                    }

                    ISerializerFactory factory = GetService<ISerializerFactory>();
                    if (factory != null)
                    {
                        Type valueType = t.GetValueType() ?? typeof(TimeSpan);
                        IStringSerializer serializer = factory.Build(valueType, SerializationContext) as IStringSerializer;
                        if (serializer != null)
                        {
                            object obj = serializer.Deserialize(new StringReader(value));
                            if (obj is IDateTime)
                                t.DateTime = (IDateTime)obj;
                            else
                                t.Duration = (TimeSpan)obj;

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
