using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace DDay.iCal.Serialization.iCalendar
{
    public class ComponentSerializer :
        SerializerBase
    {
        #region Constructor

        public ComponentSerializer()
        {
        }

        public ComponentSerializer(ISerializationContext ctx) : base(ctx)
        {
        }

        #endregion

        #region Overrides

        public override Type TargetType
        {
            get { return typeof(CalendarComponent); }
        }

        public override string SerializeToString(object obj)
        {
            ICalendarComponent c = obj as ICalendarComponent;
            if (c != null)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(TextUtil.WrapLines("BEGIN:" + c.Name));

                // Get a serializer factory
                ISerializerFactory sf = GetService<ISerializerFactory>();

                // FIXME: we should alphabetize the properties here first
                // before serializing them.

                // Serialize properties
                foreach (ICalendarProperty p in c.Properties)
                {
                    ISerializer serializer = sf.Build(p.GetType(), SerializationContext);
                    if (serializer != null)
                        sb.Append(serializer.SerializeToString(p));
                }

                // Serialize child objects
                if (sf != null)
                {
                    foreach (ICalendarObject child in c.Children)
                    {
                        ISerializer serializer = sf.Build(child.GetType(), SerializationContext);
                        if (serializer != null)
                            sb.Append(serializer.SerializeToString(child));
                    }
                }

                sb.Append(TextUtil.WrapLines("END:" + c.Name));
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
                iCalLexer lexer = new iCalLexer(tr);
                iCalParser parser = new iCalParser(lexer);

                // Get our serialization context
                ISerializationContext ctx = SerializationContext;

                // Get a serializer factory from our serialization services
                ISerializerFactory sf = GetService<ISerializerFactory>();

                // Get a calendar component factory from our serialization services
                ICalendarComponentFactory cf = GetService<ICalendarComponentFactory>();

                // Parse the component!
                ICalendarComponent component = parser.component(ctx, sf, cf, null);

                // Close our text stream
                tr.Close();

                // Return the parsed component
                return component;
            }
            return null;
        }

        #endregion
    }
}
