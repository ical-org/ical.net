using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace DDay.iCal.Serialization
{
    public class ComponentSerializer :
        SerializerBase
    {
        #region Overrides

        public override string SerializeToString(ICalendarObject obj)
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

            // FIXME: what if the serialization context is null?

            // Add a string parser factory to our serialization services,
            // if one is not already present!
            IStringParserFactory spf = GetService<IStringParserFactory>();
            if (spf == null)
            {
                spf = new StringParserFactory();
                ctx.SetService(spf);
            }
            
            // Get a calendar component factory from our serialization services
            ICalendarComponentFactory cf = GetService<ICalendarComponentFactory>();

            // Parse the component!
            ICalendarComponent component = parser.component(ctx, spf, cf, null);

            // Close our text stream
            tr.Close();

            // Return the parsed component
            return component;
        }

        #endregion
    }
}
