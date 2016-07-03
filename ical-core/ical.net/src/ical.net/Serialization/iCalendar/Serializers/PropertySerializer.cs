using System;
using System.IO;
using System.Linq;
using System.Text;
using Ical.Net.General;
using Ical.Net.Interfaces.DataTypes;
using Ical.Net.Interfaces.General;
using Ical.Net.Interfaces.Serialization;
using Ical.Net.Interfaces.Serialization.Factory;
using Ical.Net.Utility;

namespace Ical.Net.Serialization.iCalendar.Serializers
{
    public class PropertySerializer : SerializerBase
    {
        public PropertySerializer() {}

        public PropertySerializer(ISerializationContext ctx) : base(ctx) {}

        public override Type TargetType => typeof (CalendarProperty);

        public override string SerializeToString(object obj)
        {
            var prop = obj as ICalendarProperty;
            if (prop?.Values == null || !prop.Values.Any())
            {
                return null;
            }

            // Push this object on the serialization context.
            SerializationContext.Push(prop);

            // Get a serializer factory that we can use to serialize
            // the property and parameter values
            var sf = GetService<ISerializerFactory>();

            var result = new StringBuilder();
            foreach (var v in prop.Values)
            {
                if (v == null)
                {
                    continue;
                }

                // Get a serializer to serialize the property's value.
                // If we can't serialize the property's value, the next step is worthless anyway.
                var valueSerializer = sf.Build(v.GetType(), SerializationContext) as IStringSerializer;
                if (valueSerializer == null)
                {
                    continue;
                }

                // Iterate through each value to be serialized,
                // and give it a property (with parameters).
                // FIXME: this isn't always the way this is accomplished.
                // Multiple values can often be serialized within the
                // same property.  How should we fix this?

                // NOTE:
                // We Serialize the property's value first, as during 
                // serialization it may modify our parameters.
                // FIXME: the "parameter modification" operation should
                // be separated from serialization. Perhaps something
                // like PreSerialize(), etc.
                var value = valueSerializer.SerializeToString(v);

                // Get the list of parameters we'll be serializing
                var parameterList = prop.Parameters;
                if (v is ICalendarDataType)
                {
                    parameterList = ((ICalendarDataType) v).Parameters;
                }

                var sb = new StringBuilder(256);
                sb.Append(prop.Name);
                if (parameterList.Any())
                {
                    // Get a serializer for parameters
                    var parameterSerializer = sf.Build(typeof (ICalendarParameter), SerializationContext) as IStringSerializer;
                    if (parameterSerializer != null)
                    {
                        // Serialize each parameter
                        // Separate parameters with semicolons
                        sb.Append(";");
                        sb.Append(string.Join(";", parameterList.Select(param => parameterSerializer.SerializeToString(param))));
                    }
                }
                sb.Append(":");
                sb.Append(value);

                result.Append(TextUtil.WrapLines(sb.ToString()));
            }

            // Pop the object off the serialization context.
            SerializationContext.Pop();
            return result.ToString();
        }

        public override object Deserialize(TextReader tr)
        {
            if (tr == null)
            {
                return null;
            }

            using (tr)
            {
                var ctx = SerializationContext;
                var contents = tr.ReadToEnd();

                using (var normalized = TextUtil.Normalize(contents, ctx))
                {
                    var lexer = new iCalLexer(normalized);
                    var parser = new iCalParser(lexer);

                    // Get a serializer factory from our serialization services
                    var sf = GetService<ISerializerFactory>();

                    // Get a calendar component factory from our serialization services
                    var cf = GetService<ICalendarComponentFactory>();

                    var parsedComponent = parser.component(ctx, sf, cf, null);
                    return parsedComponent;
                }
            }
        }
    }
}