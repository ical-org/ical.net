using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DDay.Collections;

namespace DDay.iCal
{
    public interface ICalendarParameterCollectionProxy :
        ICalendarParameterCollection,
        IGroupedCollectionProxy<string, ICalendarParameter, ICalendarParameter>
    {                
    }
}
