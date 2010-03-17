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
                    // Assign the TZID for the date/time value.
                    if (rdt.AssociatedParameters != null)
                        rdt.AssociatedParameters.Set("TZID", rdt.TZID);

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
                // Assign the TZID for the date/time value.
                if (rdt.AssociatedParameters != null)
                    rdt.TZID = rdt.AssociatedParameters.Get("TZID");

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
                            dt.AssociateWith(rdt.AssociatedObject);
                            rdt.Add(dt);
                        }
                        else if (p != null)
                        {
                            p.AssociateWith(rdt.AssociatedObject);
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
