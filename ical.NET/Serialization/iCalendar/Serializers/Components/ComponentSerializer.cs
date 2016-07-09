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

        public ComponentSerializer() {}

        public ComponentSerializer(ISerializationContext ctx) : base(ctx) {}

        public override Type TargetType => typeof (CalendarComponent);

        public override string SerializeToString(object obj)
        {
            var component = obj as ICalendarComponent;

            var sb = new StringBuilder(1024);
            sb.Append(TextUtil.WrapLines("BEGIN:" + component.Name.ToUpper()));

            var sf = GetService<ISerializerFactory>();

            var properties = new List<ICalendarProperty>(component.Properties.OrderBy(c => c.Name));

            var serializer = sf.Build(properties.First().GetType(), SerializationContext) as IStringSerializer;
            foreach (var p in properties)
            {
                sb.Append(serializer.SerializeToString(p));
            }

            foreach (var child in component.Children)
            {
                sb.Append(serializer.SerializeToString(child));
            }

            sb.Append(TextUtil.WrapLines("END:" + component.Name.ToUpper()));
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

        public class PropertyAlphabetizer : IComparer<ICalendarProperty>
        {
            public int Compare(ICalendarProperty x, ICalendarProperty y)
            {
                if (x == y || (x == null && y == null))
                {
                    return 0;
                }
                if (x == null)
                {
                    return -1;
                }
                if (y == null)
                {
                    return 1;
                }
                return string.Compare(x.Name, y.Name, StringComparison.OrdinalIgnoreCase);
            }
        }
    }
}