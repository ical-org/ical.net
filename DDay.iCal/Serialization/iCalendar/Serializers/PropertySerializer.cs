using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Collections;

namespace DDay.iCal.Serialization.iCalendar
{
    public class PropertySerializer :
        SerializerBase
    {
        #region Constructors

        public PropertySerializer()
        {
        }

        public PropertySerializer(ISerializationContext ctx) : base(ctx)
        {
        }

        #endregion

        #region Overrides

        public override Type TargetType
        {
            get { return typeof(CalendarProperty); }
        }

        public override string SerializeToString(object obj)
        {
            ICalendarProperty prop = obj as ICalendarProperty;
            if (prop != null &&
                prop.Value != null)
            {
                // Don't serialize the property if the value is null                

                // Push this object on the serialization context.
                SerializationContext.Push(prop);
                
                // Build a list of values that are to be serialized.
                List<object> objs = new List<object>();
                if (!(prop.Value is string) && prop.Value is IEnumerable)
                {
                    foreach (object v in (IEnumerable)prop.Value)
                        objs.Add(v);
                }
                else objs.Add(prop.Value);

                // Get a serializer factory that we can use to serialize
                // the property and parameter values
                ISerializerFactory sf = GetService<ISerializerFactory>();

                StringBuilder result = new StringBuilder();
                // Get a serialize to serialize the property's value.
                // If we can't serialize the property's value, the next step is worthless anyway.
                IStringSerializer valueSerializer = sf.Build(prop.Value.GetType(), SerializationContext) as IStringSerializer;
                if (valueSerializer != null)
                {                   
                    foreach (object v in objs)
                    {
                        // Iterate through each value to be serialized,
                        // and give it a property (with parameters).
                        // FIXME: this isn't always the way this is accomplished.
                        // Multiple values can often be serialized within the
                        // same property.  How should we fix this?
                        StringBuilder sb = new StringBuilder(prop.Name);
                        if (prop.Parameters.Count > 0)
                        {
                            // Get a serializer for parameters
                            IStringSerializer parmSerializer = sf.Build(typeof(ICalendarParameter), SerializationContext) as IStringSerializer;
                            if (parmSerializer != null)
                            {
                                // Serialize each parameter
                                List<string> parameters = new List<string>();
                                foreach (ICalendarParameter param in prop.Parameters)
                                {
                                    parameters.Add(parmSerializer.SerializeToString(param));
                                }

                                // Separate parameters with semicolons
                                sb.Append(";");
                                sb.Append(string.Join(";", parameters.ToArray()));
                            }
                        }
                        sb.Append(":");

                        // Serialize the property's value
                        sb.Append(valueSerializer.SerializeToString(prop.Value));

                        result.Append(TextUtil.WrapLines(sb.ToString()));
                    }
                }

                // Pop the object off the serialization context.
                SerializationContext.Pop();

                return result.ToString();
            }
            return null;
        }

        public override object Deserialize(TextReader tr)
        {
            if (tr != null)
            {
                // Normalize the text before parsing it
                tr = TextUtil.Normalize(tr, SerializationContext);

                // Create a lexer for our text stream
                iCalLexer lexer = new iCalLexer(tr);
                iCalParser parser = new iCalParser(lexer);

                // Get our serialization context
                ISerializationContext ctx = SerializationContext;

                // Parse the component!
                ICalendarProperty p = parser.property(ctx, null);

                // Close our text stream
                tr.Close();

                // Return the parsed property
                return p;
            }
            return null;
        } 

        #endregion
    }
}
