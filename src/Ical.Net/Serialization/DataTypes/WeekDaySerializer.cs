using System;
using System.IO;
using Ical.Net.DataTypes;
using Ical.Net.Utility;

namespace Ical.Net.Serialization.DataTypes
{
    public class WeekDaySerializer : EncodableDataTypeSerializer
    {
        public WeekDaySerializer() { }

        public WeekDaySerializer(SerializationContext ctx) : base(ctx) { }

        public override Type TargetType => typeof (WeekDay);

        public override string SerializeToString(object obj)
        {
            if (!(obj is WeekDay ds))
            {
                return null;
            }

            var value = string.Empty;
            if (ds.Offset != int.MinValue)
            {
                value += ds.Offset;
            }
            value += Enum.GetName(typeof (DayOfWeek), ds.DayOfWeek).ToUpper().Substring(0, 2);

            return Encode(ds, value);
        }

        public override object Deserialize(TextReader tr)
        {
            var value = tr.ReadToEnd();

            // Create the day specifier and associate it with a calendar object
            var ds = CreateAndAssociate() as WeekDay;

            // Decode the value, if necessary
            value = Decode(ds, value);

            var match = CompiledRegularExpressions.DayOfWeek.Match(value);
            if (!match.Success)
            {
                return null;
            }

            if (match.Groups[2].Success)
            {
                ds.Offset = Convert.ToInt32(match.Groups[2].Value);
                if (match.Groups[1].Success && match.Groups[1].Value.Contains("-"))
                {
                    ds.Offset *= -1;
                }
            }
            ds.DayOfWeek = RecurrencePatternSerializer.GetDayOfWeek(match.Groups[3].Value);
            return ds;
        }
    }
}