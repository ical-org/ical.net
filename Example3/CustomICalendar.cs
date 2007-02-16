using System;
using DDay.iCal;
using DDay.iCal.Objects;

namespace Example3
{
    /// <summary>
    /// A custom iCalendar class that contains additional information
    /// </summary>
    [ComponentBaseType(typeof(CustomComponentBase))]
    class CustomICalendar : iCalendar
    {        
    }
}
