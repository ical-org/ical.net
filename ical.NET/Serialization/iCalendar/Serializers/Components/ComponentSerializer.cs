using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Ical.Net.Interfaces.Components;
using Ical.Net.Interfaces.General;
using Ical.Net.Interfaces.Serialization;
using Ical.Net.Interfaces.Serialization.Factory;
using Ical.Net.Utility;

namespace Ical.Net.Serialization.iCalendar.Serializers.Components
{
    public class ComponentSerializer :
        SerializerBase
    {
        #region Protected Properties

        protected virtual IComparer<ICalendarProperty> PropertySorter => new PropertyAlphabetizer();

        #endregion

        #region Constructor

        public ComponentSerializer()
        {
        }

        public ComponentSerializer(ISerializationContext ctx) : base(ctx)
        {
        }

        #endregion

        #region Overrides

        public override Type TargetType => typeof(CalendarComponent);

        public override string SerializeToString(object obj)
        {
            var c = obj as ICalendarComponent;
            if (c != null)
            {
                var sb = new StringBuilder();
                sb.Append(TextUtil.WrapLines("BEGIN:" + c.Name.ToUpper()));

                // Get a serializer factory
                var sf = GetService<ISerializerFactory>();

                // Sort the calendar properties in alphabetical order before
                // serializing them!
                var properties = new List<ICalendarProperty>(c.Properties);
                
                // FIXME: remove this try/catch
                try
                {                    
                    properties.Sort(PropertySorter);
                }
                catch (Exception e)
                {
                    throw;
                }

                // Serialize properties
                foreach (var p in properties)
                {
                    // Get a serializer for each property.
                    var serializer = sf.Build(p.GetType(), SerializationContext) as IStringSerializer;
                    if (serializer != null)
                        sb.Append(serializer.SerializeToString(p));
                }

                // Serialize child objects
                if (sf != null)
                {
                    foreach (var child in c.Children)
                    {
                        // Get a serializer for each child object.
                        var serializer = sf.Build(child.GetType(), SerializationContext) as IStringSerializer;
                        if (serializer != null)
                            sb.Append(serializer.SerializeToString(child));
                    }
                }

                sb.Append(TextUtil.WrapLines("END:" + c.Name.ToUpper()));
                return sb.ToString();
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

                // Get a serializer factory from our serialization services
                var sf = GetService<ISerializerFactory>();

                // Get a calendar component factory from our serialization services
                var cf = GetService<ICalendarComponentFactory>();

                // Parse the component!
                var component = parser.component(ctx, sf, cf, null);

                // Close our text stream
                tr.Close();

                // Return the parsed component
                return component;
            }
            return null;
        }

        #endregion

        #region Helper Classes

        public class PropertyAlphabetizer : IComparer<ICalendarProperty>
        {
            #region IComparer<ICalendarProperty> Members

            public int Compare(ICalendarProperty x, ICalendarProperty y)
            {
                if (x == y || (x == null && y == null))
                    return 0;
                if (x == null)
                    return -1;
                if (y == null)
                    return 1;
                return string.Compare(x.Name, y.Name, true);
            }

            #endregion
        }

        #endregion
    }
}
