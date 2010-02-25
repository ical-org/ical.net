using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal.Test
{
    [ComponentBaseType(typeof(CustomComponentBase1))]
    public class CustomICal1 : iCalendar
    {
    }

    public class CustomComponentBase1 : Component
    {
        public CustomComponentBase1(CalendarObject obj) : base(obj) { }
        static public new Component Create(CalendarObject parent, string name)
        {
            switch (name.ToUpper())
            {
                case EVENT: return new CustomEvent1(parent);
                default: return Component.Create(parent, name);
            }
        }
    }

    public class CustomEvent1 : Event
    {
        private Text nonstandardProperty;

        [Serialized, Nonstandard]
        public Text NonstandardProperty
        {
            get { return nonstandardProperty; }
            set { nonstandardProperty = value; }
        }

        public CustomEvent1(CalendarObject parent) : base(parent) { }
    }
}
