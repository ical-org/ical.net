using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace DDay.iCal
{
    public interface IUniqueComponentList<TComponentType> :
        ICalendarObjectList<TComponentType>
        where TComponentType : class, IUniqueComponent
    {
        TComponentType this[string uid] { get; set; }
    }
}
