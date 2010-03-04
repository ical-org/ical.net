using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace DDay.iCal.Serialization.iCalendar
{
    public class ComponentSerializer :
        SerializerBase
    {
        #region Overrides

        public override string SerializeToString(object obj)
        {
            throw new NotImplementedException();
        }

        public override object Deserialize(TextReader tr)
        {
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

        #endregion
    }
}
