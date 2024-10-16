﻿using Ical.Net.DataTypes;
using System;
using System.IO;

namespace Ical.Net.Serialization.DataTypes
{
    public class UriSerializer : EncodableDataTypeSerializer
    {
        public UriSerializer() { }

        public UriSerializer(SerializationContext ctx) : base(ctx) { }

        public override Type TargetType => typeof(string);

        public override string SerializeToString(object obj)
        {
            if (!(obj is Uri))
            {
                return null;
            }

            var uri = (Uri)obj;

            if (SerializationContext.Peek() is ICalendarObject co)
            {
                var dt = new EncodableDataType
                {
                    AssociatedObject = co
                };
                return Encode(dt, uri.OriginalString);
            }
            return uri.OriginalString;
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
                var uri = new Uri(value, UriKind.RelativeOrAbsolute);
                return uri;
            }
            catch { }
            return null;
        }
    }
}