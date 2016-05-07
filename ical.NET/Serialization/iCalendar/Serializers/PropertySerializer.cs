using System;
using System.Collections.Generic;
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
            var prop = obj as ICalendarProperty;
            if (prop != null && 
                prop.Values != null &&
                prop.Values.Any())
            {
                // Don't serialize the property if the value is null                

                // Push this object on the serialization context.
                SerializationContext.Push(prop);

                var mapper = GetService<IDataTypeMapper>();
                var serializedType = mapper.GetPropertyMapping(prop);
                
                // Get a serializer factory that we can use to serialize
                // the property and parameter values
                var sf = GetService<ISerializerFactory>();

                var result = new StringBuilder();
                foreach (var v in prop.Values)
                {
                    // Only serialize the value to a string if it
                    // is non-null.
                    if (v != null)
                    {
                        // Get a serializer to serialize the property's value.
                        // If we can't serialize the property's value, the next step is worthless anyway.
                        var valueSerializer = sf.Build(v.GetType(), SerializationContext) as IStringSerializer;
                        if (valueSerializer != null)
                        {
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
                                parameterList = ((ICalendarDataType)v).Parameters;

                            var sb = new StringBuilder(prop.Name);
                            if (parameterList.Any())
                            {
                                // Get a serializer for parameters
                                var parameterSerializer = sf.Build(typeof(ICalendarParameter), SerializationContext) as IStringSerializer;
                                if (parameterSerializer != null)
                                {
                                    // Serialize each parameter
                                    var parameters = new List<string>();
                                    foreach (var param in parameterList)
                                    {
                                        parameters.Add(parameterSerializer.SerializeToString(param));
                                    }

                                    // Separate parameters with semicolons
                                    sb.Append(";");
                                    sb.Append(string.Join(";", parameters.ToArray()));
                                }
                            }
                            sb.Append(":");
                            sb.Append(value);

                            result.Append(TextUtil.WrapLines(sb.ToString()));
                        }
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
                var lexer = new iCalLexer(tr);
                var parser = new iCalParser(lexer);

                // Get our serialization context
                var ctx = SerializationContext;

                // Parse the component!
                var p = parser.property(ctx, null);

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
