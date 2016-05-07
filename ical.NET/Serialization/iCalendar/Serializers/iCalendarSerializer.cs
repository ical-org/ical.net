using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Ical.Net.Interfaces;
using Ical.Net.Interfaces.General;
using Ical.Net.Interfaces.Serialization;
using Ical.Net.Serialization.iCalendar.Serializers.Components;
using Ical.Net.Utility;

namespace Ical.Net.Serialization.iCalendar.Serializers
{
    public class CalendarSerializer :
        ComponentSerializer
    {
        #region Private Fields

        ICalendar _mCalendar;

        #endregion

        #region Constructors

        public CalendarSerializer() : base()
        {
        }

        public CalendarSerializer(ICalendar cal)
        {
            _mCalendar = cal;
        }

        public CalendarSerializer(ISerializationContext ctx) : base(ctx)
        {
        }

        #endregion

        #region Public Methods

        [Obsolete("Use the Serialize(IICalendar iCal, string filename) method instead.")]
        public virtual void Serialize(string filename)
        {
            if (_mCalendar != null)
                Serialize(_mCalendar, filename);
        }

        [Obsolete("Use the SerializeToString(ICalendarObject obj) method instead.")]
        public virtual string SerializeToString()
        {
            return SerializeToString(_mCalendar);
        }

        public virtual void Serialize(ICalendar cal, string filename)
        {
            using (var fs = new FileStream(filename, FileMode.Create))
            {
                Serialize(cal, fs, new UTF8Encoding());
            }
        }        

        #endregion

        #region Overrides

        protected override IComparer<ICalendarProperty> PropertySorter
        {
            get
            {
                return new CalendarPropertySorter();
            }
        }

        public override string SerializeToString(object obj)
        {
            var iCal = obj as ICalendar;
            if (iCal != null)
            {
                // Ensure VERSION and PRODUCTID are both set,
                // as they are required by RFC5545.
                var copy = iCal.Copy<ICalendar>();
                if (string.IsNullOrEmpty(copy.Version))
                    copy.Version = CalendarVersions.V20;                    
                if (string.IsNullOrEmpty(copy.ProductId))
                    copy.ProductId = CalendarProductIDs.Default;

                return base.SerializeToString(copy);
            }

            return base.SerializeToString(obj);
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

                // Parse the iCalendar(s)!
                var iCalendars = parser.icalendar(SerializationContext);

                // Close our text stream
                tr.Close();

                // Return the parsed iCalendar(s)
                return iCalendars;
            }
            return null;
        }

        #endregion

        private class CalendarPropertySorter :
            IComparer<ICalendarProperty>
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
                {
                    // Alphabetize all properties except VERSION, which should appear first. 
                    if (string.Equals("VERSION", x.Name, StringComparison.InvariantCultureIgnoreCase))
                        return -1;
                    else if (string.Equals("VERSION", y.Name, StringComparison.InvariantCultureIgnoreCase))
                        return 1;
                    return string.Compare(x.Name, y.Name);
                }
            }

            #endregion
        }
    }
}
