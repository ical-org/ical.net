using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace DDay.iCal.Serialization.iCalendar
{
    public class ComponentSerializer :
        SerializerBase
    {
        #region Protected Properties

        virtual protected IComparer<ICalendarProperty> PropertySorter
        {
            get
            {
                return new PropertyAlphabetizer();
            }
        }

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
                sb.Append(TextUtil.WrapLines("BEGIN:" + c.Name.ToUpper()));

                // Get a serializer factory
                ISerializerFactory sf = GetService<ISerializerFactory>();

                // Sort the calendar properties in alphabetical order before
                // serializing them!
                List<ICalendarProperty> properties = new List<ICalendarProperty>(c.Properties);
                try
                {                    
                    properties.Sort(PropertySorter);
                }
                catch (Exception e)
                {
                    throw;
                }

                // Serialize properties
                foreach (ICalendarProperty p in properties)
                {
                    // Get a serializer for each property.
                    IStringSerializer serializer = sf.Build(p.GetType(), SerializationContext) as IStringSerializer;
                    if (serializer != null)
                        sb.Append(serializer.SerializeToString(p));
                }

                // Serialize child objects
                if (sf != null)
                {
                    foreach (ICalendarObject child in c.Children)
                    {
                        // Get a serializer for each child object.
                        IStringSerializer serializer = sf.Build(child.GetType(), SerializationContext) as IStringSerializer;
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

        #region Helper Classes

        public class PropertyAlphabetizer : IComparer<ICalendarProperty>
        {
            #region IComparer<ICalendarProperty> Members

            public int Compare(ICalendarProperty x, ICalendarProperty y)
            {
                if (x == y || (x == null && y == null))
                    return 0;
                else if (x == null)
                    return -1;
                else if (y == null)
                    return 1;
                else
                    return string.Compare(x.Name, y.Name, true);
            }

            #endregion
        }

        #endregion
    }
}
