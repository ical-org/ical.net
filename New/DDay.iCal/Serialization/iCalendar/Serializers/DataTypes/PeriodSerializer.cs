using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Runtime.Serialization;

namespace DDay.iCal.Serialization.iCalendar
{
    public class PeriodSerializer : 
        EncodableDataTypeSerializer
    {
        #region Overrides

        public override Type TargetType
        {
            get { return typeof(Period); }
        }

        public override string SerializeToString(object obj)
        {    
            IPeriod p = obj as IPeriod;
            ISerializerFactory factory = GetService<ISerializerFactory>();

            if (p != null && factory != null)
            {
                IStringSerializer dtSerializer = factory.Build(typeof(IDateTime), SerializationContext) as IStringSerializer;
                IStringSerializer durationSerializer = factory.Build(typeof(TimeSpan), SerializationContext) as IStringSerializer;
                if (dtSerializer != null && durationSerializer != null)
                {
                    StringBuilder sb = new StringBuilder();

                    // Serialize the start time                    
                    sb.Append(dtSerializer.SerializeToString(p.StartTime));

                    // Serialize the duration
                    if (p.Duration != null)
                    {
                        sb.Append("/");
                        sb.Append(durationSerializer.SerializeToString(p.Duration));
                    }

                    // Encode the value as necessary
                    return Encode(p, sb.ToString());
                }
            }
            return null;
        }

        public override object Deserialize(TextReader tr)
        {
            string value = tr.ReadToEnd();

            IPeriod p = CreateAndAssociate() as IPeriod;
            ISerializerFactory factory = GetService<ISerializerFactory>();
            if (p != null && factory != null)
            {
                IStringSerializer dtSerializer = factory.Build(typeof(IDateTime), SerializationContext) as IStringSerializer;
                IStringSerializer durationSerializer = factory.Build(typeof(TimeSpan), SerializationContext) as IStringSerializer;
                if (dtSerializer != null && durationSerializer != null)
                {
                    // Decode the value as necessary
                    value = Decode(p, value);

                    string[] values = value.Split('/');
                    if (values.Length != 2)
                        return false;

                    p.StartTime = dtSerializer.Deserialize(new StringReader(values[0])) as IDateTime;
                    p.EndTime = dtSerializer.Deserialize(new StringReader(values[1])) as IDateTime;
                    if (p.EndTime == null)
                        p.Duration = (TimeSpan)durationSerializer.Deserialize(new StringReader(values[1]));

                    // Only return an object if it has been deserialized correctly.
                    if (p.StartTime != null && p.Duration != null)
                        return p;
                }
            }

            return null;
        }

        #endregion
    }
}
