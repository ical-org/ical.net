using System;
using System.IO;
using Ical.Net.DataTypes;

namespace Ical.Net.Serialization.DataTypes
{
    public class UriSerializer : EncodableDataTypeSerializer
    {
        public UriSerializer() {}

        public UriSerializer(SerializationContext ctx) : base(ctx) {}

        public override Type TargetType => typeof (string);

        public override string? SerializeToString(object? obj) => obj is Uri uri ? SerializeToString(uri) : null;

        public string SerializeToString(Uri uri)
        {
            if (!(SerializationContext.Peek() is ICalendarObject co))
            {
                return uri.OriginalString;
            }
            var dt = new EncodableDataType
            {
                AssociatedObject = co
            };
            return Encode(dt, uri.OriginalString);
        }

        public override object Deserialize(TextReader tr)
        {
            if (tr == null)
            {
                return null;
            }

            var value = tr.ReadToEnd();

            if (SerializationContext.Peek() is ICalendarObject co)
            {
                var dt = new EncodableDataType
                {
                    AssociatedObject = co
                };
                value = Decode(dt, value);
            }

            try
            {
                var uri = new Uri(value);
                return uri;
            }
            catch {}
            return null;
        }
    }
}