using System;
using System.IO;
using Ical.Net.DataTypes;

namespace Ical.Net.Serialization.DataTypes
{
    public class AttendeeSerializer : StringSerializer
    {
        public AttendeeSerializer() { }

        public AttendeeSerializer(SerializationContext ctx) : base(ctx) { }

        public override Type TargetType => typeof (Attendee);

        public override string? SerializeToString(object? obj) => obj is Attendee a ? SerializeToString(a): null;

        public string? SerializeToString(Attendee a) => Encode(a, a.Value.OriginalString);

        public Attendee Deserialize(string attendee)
        {
            try
            {
                var a = CreateAndAssociate() as Attendee;
                var uriString = Unescape(Decode(a, attendee));

                // Prepend "mailto:" if necessary
                if (!uriString.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase))
                {
                    uriString = "mailto:" + uriString;
                }

                a.Value = new Uri(uriString);
                return a;
            }
            catch
            {
                // ignored
            }

            return null;
        }

        public override object Deserialize(TextReader tr) => Deserialize(tr.ReadToEnd());
    }
}