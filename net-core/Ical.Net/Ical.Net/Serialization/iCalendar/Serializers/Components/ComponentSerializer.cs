using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Ical.Net.Interfaces.Components;
using Ical.Net.Interfaces.General;
using Ical.Net.Interfaces.Serialization;
using Ical.Net.Interfaces.Serialization.Factory;
using Ical.Net.Utility;

namespace Ical.Net.Serialization.iCalendar.Serializers.Components
{
    public class ComponentSerializer : SerializerBase
    {
        protected virtual IComparer<ICalendarProperty> PropertySorter => new PropertyAlphabetizer();

        public ComponentSerializer() { }

        public ComponentSerializer(SerializationContext ctx) : base(ctx) { }

        public override Type TargetType => typeof(CalendarComponent);

        public override string SerializeToString(object obj)
        {
            var c = obj as ICalendarComponent;
            if (c == null)
            {
                return null;
            }

            var sb = new StringBuilder(512);
            var upperName = c.Name.ToUpperInvariant();
            sb.Append(TextUtil.FoldLines($"BEGIN:{upperName}"));

            // Get a serializer factory
            var sf = GetService<ISerializerFactory>();

            // Sort the calendar properties in alphabetical order before serializing them!
            var properties = c.Properties.OrderBy(p => p.Name).ToList();

            // Serialize properties
            foreach (var p in properties)
            {
                // Get a serializer for each property.
                var serializer = sf.Build(p.GetType(), SerializationContext) as IStringSerializer;
                sb.Append(serializer.SerializeToString(p));
            }

            // Serialize child objects
            foreach (var child in c.Children)
            {
                // Get a serializer for each child object.
                var serializer = sf.Build(child.GetType(), SerializationContext) as IStringSerializer;
                sb.Append(serializer.SerializeToString(child));
            }

            sb.Append(TextUtil.FoldLines($"END:{upperName}"));
            return sb.ToString();
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
                    // Create a lexer for our text stream
                    var lexer = new IcalLexer(normalized);
                    var parser = new IcalParser(lexer);

                    // Get a serializer factory from our serialization services
                    var sf = GetService<ISerializerFactory>();

                    // Get a calendar component factory from our serialization services
                    var cf = GetService<ICalendarComponentFactory>();

                    var parsedComponent = parser.component(ctx, sf, cf, null);
                    return parsedComponent;
                }
            }
        }

        public class PropertyAlphabetizer : IComparer<ICalendarProperty>
        {
            public int Compare(ICalendarProperty x, ICalendarProperty y)
            {
                if (x == y)
                {
                    return 0;
                }
                if (x == null)
                {
                    return -1;
                }
                return y == null
                    ? 1
                    : string.Compare(x.Name, y.Name, StringComparison.OrdinalIgnoreCase);
            }
        }
    }
}