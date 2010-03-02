using System;
using System.Collections.Generic;
using System.Text;
using DDay.iCal.Serialization;

namespace DDay.iCal.Test
{
    [ComponentFactory(typeof(CustomComponentBase1))]
    public class CustomICal1 : iCalendar
    {
    }

    public class CustomComponentBase1 : ComponentFactory
    {
        public override ICalendarComponent Create(string objectName)
        {
            switch (objectName.ToUpper())
            {
                case EVENT: return new CustomEvent1();
                default: return base.Create(objectName);
            }
        }
    }

    public class CustomEvent1 : Event
    {
        private Text nonstandardProperty;

        public Text NonstandardProperty
        {
            get { return nonstandardProperty; }
            set { nonstandardProperty = value; }
        }
    }
}
