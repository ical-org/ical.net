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

                // FIXME: should we deserialize something even when an ICalendarObject
                // cannot be found on the serialization stack?
                ICalendarObject co = SerializationContext.Peek() as ICalendarObject;
                if (co != null)
                {
                    bool serializeAsList = false;

                    if (co is ICalendarProperty)
                    {
                        IDataTypeMapper mapper = GetService<IDataTypeMapper>();
                        Type type = mapper.GetPropertyMapping((ICalendarProperty)co);
                        if (type != null && typeof(IList<string>).IsAssignableFrom(type))
                            serializeAsList = true;
                    }

                    value = TextUtil.Normalize(value, SerializationContext).ReadToEnd();

                    // Try to decode the string
                    EncodableDataType dt = new EncodableDataType();
                    dt.AssociatedObject = co;
                    
                    List<string> values = new List<string>();

                    int i = 0;
                    if (serializeAsList)
                    {
                        MatchCollection matches = Regex.Matches(value, @"[^\\](,)");
                        foreach (Match match in matches)
                        {
                            values.Add(Unescape(Decode(dt, value.Substring(i, match.Index - i + 1))));
                            i = match.Index + 2;
                        }
                    }

                    if (i < value.Length)
                        values.Add(Unescape(Decode(dt, value.Substring(i, value.Length - i))));

                    // Return either a single value, or the entire list.
                    if (values.Count == 1)
                        return values[0];
                    else
                        return values;
                }
            }
            return null;
        }

        #endregion
    }
}
