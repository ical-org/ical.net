using System;
using System.Collections.Generic;
using System.Text;
using DDay.iCal.Serialization;

namespace DDay.iCal.Test
{
    // FIXME: fix this custom calendar to function properly
    //[ComponentBaseType(typeof(CustomComponentBase1))]
    public class CustomICal1 : iCalendar
    {
    }

    //public class CustomComponentBase1 : CalendarComponent
    //{
    //    public CustomComponentBase1(ICalendarObject obj) : base(obj) { }
    //    public new ComponentBase Create(ICalendarObject parent, string name)
    //    {
    //        switch (name.ToUpper())
    //        {
    //            case Components.EVENT: return new CustomEvent1(parent);
    //            default: return base.Create(parent, name);
    //        }
    //    }
    //}

    public class CustomEvent1 : Event
    {
        public string NonstandardProperty
        {
            get { return Properties.Get<string>("X-NONSTANDARD-PROPERTY"); }
            set { Properties.Set("X-NONSTANDARD-PROPERTY", value); }
        }                
    }
}
