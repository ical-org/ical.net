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
        #region Constructors

        public ParameterSerializer() {}

        public ParameterSerializer(ISerializationContext ctx) : base(ctx) {}

        #endregion

        #region Overrides

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
            // Create a lexer for our text stream
            var lexer = new iCalLexer(tr);
            var parser = new iCalParser(lexer);

            // Get our serialization context
            var ctx = SerializationContext;

            // Parse the component!
            var p = parser.parameter(ctx, null);

            // Close our text stream
            tr.Close();

            // Return the parsed parameter
            return p;
        }

        #endregion
    }
}