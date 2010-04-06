using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace DDay.iCal.Serialization.iCalendar
{
    public class StringSerializer :
        EncodableDataTypeSerializer
    {
        #region Constructors

        public StringSerializer()
        {
        }

        public StringSerializer(ISerializationContext ctx) : base(ctx)
        {
        }

        #endregion

        #region Protected Methods

        virtual protected string Unescape(string value)
        {
            // added null check - you can't call .Replace on a null
            // string, but you can just return null as a string
            if (value != null)
            {
                value = value.Replace(@"\n", "\n");
                value = value.Replace(@"\N", "\n");
                value = value.Replace(@"\;", ";");
                value = value.Replace(@"\,", ",");
                // NOTE: double quotes aren't escaped in RFC2445, but are in Mozilla Sunbird (0.5-)
                value = value.Replace("\\\"", "\"");

                // Replace all single-backslashes with double-backslashes.
                value = Regex.Replace(value, @"(?<!\\)\\(?!\\)", "\\\\");

                // Unescape double backslashes
                value = value.Replace(@"\\", @"\");                
            }
            return value;
        }

        virtual protected string Escape(string value)
        {
            // added null check - you can't call .Replace on a null
            // string, but you can just return null as a string
            if (value != null)
            {
                // NOTE: fixed a bug that caused text parsing to fail on
                // programmatically entered strings.
                // SEE unit test SERIALIZE25().
                value = value.Replace("\r\n", @"\n");
                value = value.Replace("\r", @"\n");
                value = value.Replace("\n", @"\n");
                value = value.Replace(";", @"\;");
                value = value.Replace(",", @"\,");                
            }
            return value;
        }

        #endregion

        #region Overrides

        public override Type TargetType
        {
            get { return typeof(string); }
        }

        public override string SerializeToString(object obj)
        {
            if (obj != null)
            {
                string value = obj.ToString();

                ICalendarObject co = SerializationContext.Peek() as ICalendarObject;
                if (co != null)
                {
                    // Encode the string as needed.
                    EncodableDataType dt = new EncodableDataType();
                    dt.AssociatedObject = co;
                    return Encode(dt, Escape(value));
                }
                return Escape(value);
            }
            return null;
        }

        public override object Deserialize(TextReader tr)
        {
            if (tr != null)
            {
                string value = tr.ReadToEnd();

                ICalendarObject co = SerializationContext.Peek() as ICalendarObject;
                if (co != null)
                {
                    // Try to decode the string
                    EncodableDataType dt = new EncodableDataType();
                    dt.AssociatedObject = co;
                    value = Decode(dt, value);
                }

                return Unescape(TextUtil.Normalize(value, SerializationContext).ReadToEnd());
            }
            return null;
        }

        #endregion
    }
}
