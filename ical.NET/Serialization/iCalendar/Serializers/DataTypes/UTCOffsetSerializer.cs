using System;
using System.IO;
using System.Text.RegularExpressions;
using Ical.Net.DataTypes;
using Ical.Net.Interfaces.DataTypes;

namespace Ical.Net.Serialization.iCalendar.Serializers.DataTypes
{
    public class UtcOffsetSerializer : EncodableDataTypeSerializer
    {
        public override Type TargetType => typeof (UtcOffset);

        public override string SerializeToString(object obj)
        {
            var offset = obj as IUtcOffset;
            if (offset != null)
            {
                var value = (offset.Positive ? "+" : "-") + offset.Hours.ToString("00") + offset.Minutes.ToString("00") +
                            (offset.Seconds != 0 ? offset.Seconds.ToString("00") : string.Empty);

                // Encode the value as necessary
                return Encode(offset, value);
            }
            return null;
        }

        internal static readonly Regex DecodeOffset = new Regex(@"(\+|-)(\d{2})(\d{2})(\d{2})?", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public override object Deserialize(TextReader tr)
        {
            var offsetString = tr.ReadToEnd();
            var offset = new UtcOffset(offsetString);
            return offset;
        }

        public static TimeSpan GetOffset(string rawOffset)
        {
            if (rawOffset.EndsWith("00"))
            {
                rawOffset = rawOffset.Substring(0, rawOffset.Length - 2);
            }
            var dummyOffset = DateTimeOffset.Parse("2016-01-01 00:00:00 " + rawOffset);
            return dummyOffset.Offset;
        }
    }
}