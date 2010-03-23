using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

namespace DDay.iCal.Serialization.iCalendar
{
    public class PeriodListSerializer :
        EncodableDataTypeSerializer
    {
        public override Type TargetType
        {
            get { return typeof(PeriodList); }
        }

        public override string SerializeToString(object obj)
        {
            IPeriodList rdt = obj as IPeriodList;
            ISerializerFactory factory = GetService<ISerializerFactory>();

            if (rdt != null && factory != null)
            {
                IStringSerializer dtSerializer = factory.Build(typeof(IDateTime), SerializationContext) as IStringSerializer;
                IStringSerializer periodSerializer = factory.Build(typeof(IPeriod), SerializationContext) as IStringSerializer;
                if (dtSerializer != null && periodSerializer != null)
                {
                    List<string> parts = new List<string>();

                    foreach (IPeriod p in rdt)
                    {
                        if (p.EndTime != null)
                            parts.Add(periodSerializer.SerializeToString(p));
                        else if (p.StartTime != null)
                            parts.Add(dtSerializer.SerializeToString(p.StartTime));
                    }

                    return Encode(rdt, string.Join(",", parts.ToArray()));
                }
            }
            return null;
        }

        public override object Deserialize(TextReader tr)
        {
            string value = tr.ReadToEnd();

            // Create the day specifier and associate it with a calendar object
            IPeriodList rdt = CreateAndAssociate() as IPeriodList;
            ISerializerFactory factory = GetService<ISerializerFactory>();
            if (rdt != null && factory != null)
            {
                // Decode the value, if necessary
                value = Decode(rdt, value);

                IStringSerializer dtSerializer = factory.Build(typeof(IDateTime), SerializationContext) as IStringSerializer;
                IStringSerializer periodSerializer = factory.Build(typeof(IPeriod), SerializationContext) as IStringSerializer;
                if (dtSerializer != null && periodSerializer != null)
                {
                    string[] values = value.Split(',');
                    foreach (string v in values)
                    {
                        IDateTime dt = dtSerializer.Deserialize(new StringReader(v)) as IDateTime;
                        IPeriod p = periodSerializer.Deserialize(new StringReader(v)) as IPeriod;

                        if (dt != null)
                        {
                            dt.AssociatedObject = rdt.AssociatedObject;
                            rdt.Add(dt);
                        }
                        else if (p != null)
                        {
                            p.AssociatedObject = rdt.AssociatedObject;
                            rdt.Add(p);
                        }
                    }
                    return rdt;
                }
            }

            return null;
        }
    }
}
