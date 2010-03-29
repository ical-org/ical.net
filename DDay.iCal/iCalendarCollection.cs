using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;
using DDay.iCal.Serialization;

namespace DDay.iCal
{
    /// <summary>
    /// A collection of iCalendars that is enumerable, and contains
    /// several public properties for direct access to components
    /// from each individual iCalendar.
    /// </summary>
#if DATACONTRACT
    [CollectionDataContract(Name = "iCalendarCollection", Namespace = "http://www.ddaysoftware.com/dday.ical/2009/07/")]
#endif
    [Serializable]
    public class iCalendarCollection :
        List<IICalendar>,
        IICalendarCollection
    {
    }
}
