using Ical.Net.DataTypes;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Ical.Net.Serialization.DataTypes
{
    public class QuotedPrintableStringSerializer : EncodableDataTypeSerializer
    {
        private readonly QuotedPrintableString _encoding;

        public QuotedPrintableStringSerializer() 
        {
            _encoding = new QuotedPrintableString{
                Encoding = "UTF8"
            };
        }

        public QuotedPrintableStringSerializer(string charset)
        {
            _encoding = new QuotedPrintableString
            {
                Encoding = charset
            };
        }

        public QuotedPrintableStringSerializer(SerializationContext ctx) : base(ctx) 
        {
            if (ctx.Peek() is Ical.Net.CalendarProperty calPro && calPro.Parameters.ContainsKey("CHARSET"))
            {
                var charset = calPro.Parameters.Get("CHARSET");
                _encoding = _encoding = new QuotedPrintableString
                {
                    Encoding = charset
                };
            }
            else
            {
                _encoding = new QuotedPrintableString
                {
                    Encoding = "UTF8"
                };
            }
        }

        public override Type TargetType => typeof(QuotedPrintableString);

        public override object Deserialize(TextReader tr)
        {
            if (tr == null)
            {
                return null;
            }

            var value = tr.ReadToEnd();

            var bytes = new List<byte>(value.Length);

            for (int i = 0; i < value.Length; i++)
            {
                var c = value[i];
                switch (c)
                {
                    case '=':
                        if (!ConsumeWhiteSpace(ref i, ref value))
                        {
                            var sHex = value.Substring(i + 1, 2);
                            int hex = Convert.ToInt32(sHex, 16);
                            byte b = Convert.ToByte(hex);
                            bytes.Add(b);
                            i += 2;
                        }
                        break;
                    default:
                        {
                            var b = Convert.ToByte(c);
                            bytes.Add(b);
                        }
                        break;
                }
            }
            return Encode(_encoding, bytes.ToArray());
            //return _encoding.GetString(bytes, 0 , index);
        }

        private bool ConsumeWhiteSpace(ref int index, ref string value)
        {
            bool foundws = false;
            var max = value.Length - 1;
            while (index < max && char.IsWhiteSpace(value[index + 1]))
            {
                index++;
                foundws = true;
            }
            return foundws;
        }

        public override string SerializeToString(object obj)
        {
            if (obj == null)
            {
                return null;
            }
            throw new NotImplementedException();
        }
    }
}
