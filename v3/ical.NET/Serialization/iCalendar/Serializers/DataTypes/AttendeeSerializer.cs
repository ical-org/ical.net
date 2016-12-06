using System;
using System.IO;
using Ical.Net.DataTypes;
using Ical.Net.Serialization.iCalendar.Serializers.Other;

namespace Ical.Net.Serialization.iCalendar.Serializers.DataTypes
{
    public class AttendeeSerializer : StringSerializer
    {
        public override Type TargetType => typeof (Attendee);

        public override string SerializeToString(object obj)
        {
            var a = obj as Attendee;
            return a == null
                ? null
                : Encode(a, a.UserAddress);
        }

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

                a.UserAddress = uriString;

                Uri uri;
                if (Uri.TryCreate(uriString, UriKind.RelativeOrAbsolute, out uri))
                {
                    a.Value = uri;
                }
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