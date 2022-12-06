using System;
using System.IO;
using Ical.Net.DataTypes;

namespace Ical.Net.Serialization.DataTypes
{
    public class FreeBusyEntrySerializer : PeriodSerializer
    {
        public FreeBusyEntrySerializer() { }

        public FreeBusyEntrySerializer(SerializationContext ctx) : base(ctx) { }

        public override Type TargetType => typeof (FreeBusyEntry);

        public override string SerializeToString(object obj)
        {
            if (!(obj is FreeBusyEntry entry))
            {
                return base.SerializeToString(obj);
            }

            switch (entry.Status)
            {
                case FreeBusyStatus.Busy:
                    entry.Parameters.Remove("FBTYPE");
                    break;
                case FreeBusyStatus.BusyTentative:
                    entry.Parameters.Set("FBTYPE", "BUSY-TENTATIVE");
                    break;
                case FreeBusyStatus.BusyUnavailable:
                    entry.Parameters.Set("FBTYPE", "BUSY-UNAVAILABLE");
                    break;
                case FreeBusyStatus.Free:
                    entry.Parameters.Set("FBTYPE", "FREE");
                    break;
            }

            return base.SerializeToString(obj);
        }

        public override object Deserialize(TextReader tr)
        {
            var entry = base.Deserialize(tr) as FreeBusyEntry;
            if (entry == null)
            {
                return entry;
            }

            if (!entry.Parameters.ContainsKey("FBTYPE"))
            {
                return entry;
            }

            var value = entry.Parameters.Get("FBTYPE");
            if (value == null)
            {
                return entry;
            }

            entry.Status = value.ToUpperInvariant() switch
            {
                "FREE" => FreeBusyStatus.Free,
                "BUSY" => FreeBusyStatus.Busy,
                "BUSY-UNAVAILABLE" => FreeBusyStatus.BusyUnavailable,
                "BUSY-TENTATIVE" => FreeBusyStatus.BusyTentative,
                _ => entry.Status
            };

            return entry;
        }
    }
}