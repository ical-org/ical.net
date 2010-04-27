using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections;

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
                ISerializationSettings settings = GetService<ISerializationSettings>();                

                List<string> values = new List<string>();
                if (obj is string)
                {
                    // Object to be serialied is a string already
                    values.Add((string)obj);
                }
                else if (obj is IEnumerable)
                {
                    // Object is a list of objects (probably IList<string>).
                    foreach (object child in (IEnumerable)obj)
                        values.Add(child.ToString());
                }
                else
                {
                    // Serialize the object as a string.
                    values.Add(obj.ToString());
                }

                ICalendarObject co = SerializationContext.Peek() as ICalendarObject;
                if (co != null)
                {
                    // Encode the string as needed.
                    EncodableDataType dt = new EncodableDataType();
                    dt.AssociatedObject = co;
                    for (int i = 0; i < values.Count; i++)
                        values[i] = Encode(dt, Escape(values[i]));

                    return string.Join(",", values.ToArray());
                }
                
                for (int i = 0; i < values.Count; i++)
                    values[i] = Escape(values[i]);
                return string.Join(",", values.ToArray());
            }
            return null;
        }

        public override object Deserialize(TextReader tr)
        {
            if (tr != null)
            {
                string value = tr.ReadToEnd();

                // NOTE: this can deserialize into an IList<string> or simply a string,
                // depending on the input text.  Anything that uses this serializer should
                // be prepared to receive either a string, or an IList<string>.

                bool serializeAsList = false;

                // Determine if we can serialize this property
                // with multiple values per line.
                ICalendarObject co = SerializationContext.Peek() as ICalendarObject;
                if (co is ICalendarProperty)
                    serializeAsList = GetService<IDataTypeMapper>().GetPropertyAllowsMultipleValues(co);

                value = TextUtil.Normalize(value, SerializationContext).ReadToEnd();

                // Try to decode the string
                EncodableDataType dt = null;
                if (co != null)
                {
                    dt = new EncodableDataType();
                    dt.AssociatedObject = co;
                }

                List<string> escapedValues = new List<string>();
                List<string> values = new List<string>();

                int i = 0;
                if (serializeAsList)
                {
                    MatchCollection matches = Regex.Matches(value, @"[^\\](,)");
                    foreach (Match match in matches)
                    {
                        string newValue = dt != null ? Decode(dt, value.Substring(i, match.Index - i + 1)) : value.Substring(i, match.Index - i + 1);
                        escapedValues.Add(newValue);
                        values.Add(Unescape(newValue));
                        i = match.Index + 2;
                    }
                }

                if (i < value.Length)
                {
                    string newValue = dt != null ? Decode(dt, value.Substring(i, value.Length - i)) : value.Substring(i, value.Length - i);
                    escapedValues.Add(newValue);
                    values.Add(Unescape(newValue));
                }

                if (co is ICalendarProperty)
                {
                    // Determine if our we're supposed to store extra information during
                    // the serialization process.  If so, let's store the escaped value.
                    ICalendarProperty property = (ICalendarProperty)co;
                    ISerializationSettings settings = GetService<ISerializationSettings>();
                    if (settings != null &&
                        settings.StoreExtraSerializationData)
                    {
                        // Store the escaped value
                        co.SetService("EscapedValue", escapedValues.Count == 1 ? 
                            (object)escapedValues[0] :
                            (object)escapedValues);
                    }
                }

                // Return either a single value, or the entire list.
                if (values.Count == 1)
                    return values[0];
                else
                    return values;                
            }
            return null;
        }

        #endregion
    }
}
