using System;
using System.IO;
using System.Linq;
using Ical.Net.General;
using Ical.Net.Interfaces.General;
using Ical.Net.Interfaces.Serialization;

namespace Ical.Net.Serialization.iCalendar.Serializers
{
    public class ParameterSerializer : SerializerBase
    {
        public ParameterSerializer() {}

        public ParameterSerializer(ISerializationContext ctx) : base(ctx) {}

        public override Type TargetType => typeof (CalendarParameter);

        public override string SerializeToString(object obj)
        {
            var p = obj as ICalendarParameter;
            if (p != null)
            {
                var result = p.Name + "=";
                var value = string.Join(",", p.Values.ToArray());

                // "Section 3.2:  Property parameter values MUST NOT contain the DQUOTE character."
                // Therefore, let's strip any double quotes from the value.                
                value = value.Replace("\"", string.Empty);

                // Surround the parameter value with double quotes, if the value
                // contains any problematic characters.
                if (value.IndexOfAny(new[] {';', ':', ','}) >= 0)
                {
                    value = "\"" + value + "\"";
                }
                return result + value;
            }
            return string.Empty;
        }

        public override object Deserialize(TextReader tr)
        {
            using (tr)
            {
                var lexer = new iCalLexer(tr);
                var parser = new iCalParser(lexer);
                var p = parser.parameter(SerializationContext, null);
                return p;
            }
        }
    }
}