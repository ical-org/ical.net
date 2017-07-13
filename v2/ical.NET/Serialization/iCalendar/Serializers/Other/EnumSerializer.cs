using System;
using System.IO;
using Ical.Net.DataTypes;
using Ical.Net.Interfaces.General;
using Ical.Net.Serialization.iCalendar.Serializers.DataTypes;
using Ical.Net.Interfaces.Serialization;

namespace Ical.Net.Serialization.iCalendar.Serializers.Other
{
    public class EnumSerializer : EncodableDataTypeSerializer
    {
        private readonly Type _mEnumType;

        public EnumSerializer(Type enumType)
        {
            _mEnumType = enumType;
        }

        public EnumSerializer(Type enumType, ISerializationContext ctx) : base(ctx)
        {
            _mEnumType = enumType;
        }

        public override Type TargetType => _mEnumType;

        public override string SerializeToString(object enumValue)
        {
            try
            {
                string value = enumValue.ToString();

                if (TargetType == typeof(EventStatus) || TargetType == typeof(TransparencyType))
                    value = value.ToUpperInvariant();

                var obj = SerializationContext.Peek() as ICalendarObject;

                if (obj != null)
                {
                    // Encode the value as needed.
                    var dt = new EncodableDataType
                    {
                        AssociatedObject = obj
                    };
                    return Encode(dt, value);
                }

                return value;
            }
            catch
            {
                return null;
            }
        }

        public override object Deserialize(TextReader tr)
        {
            var value = tr.ReadToEnd();

            try
            {
                var obj = SerializationContext.Peek() as ICalendarObject;
                if (obj != null)
                {
                    // Decode the value, if necessary!
                    var dt = new EncodableDataType
                    {
                        AssociatedObject = obj
                    };
                    value = Decode(dt, value);
                }

                // Remove "-" characters while parsing Enum values.
                return Enum.Parse(_mEnumType, value.Replace("-", ""), true);
            }
            catch {}

            return value;
        }
    }
}