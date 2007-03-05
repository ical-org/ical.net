using System;
using System.Collections.Generic;
using System.Text;
using DDay.iCal.Components;
using DDay.iCal.DataTypes;
using DDay.iCal.Components;
using DDay.iCal.Serialization;

namespace DDay.iCal.Test
{
    [ComponentBaseType(typeof(CustomComponentBase1))]
    public class CustomICal1 : iCalendar
    {
    }

    public class CustomComponentBase1 : ComponentBase
    {
        public CustomComponentBase1(iCalObject obj) : base(obj) { }
        static public new ComponentBase Create(iCalObject parent, string name)
        {
            switch (name)
            {
                case "VEVENT": return new CustomEvent1(parent);
                default: return ComponentBase.Create(parent, name);
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

        public CustomEvent1(iCalObject parent) : base(parent) { }
    }
}
