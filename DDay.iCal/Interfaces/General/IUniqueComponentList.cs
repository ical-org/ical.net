using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace DDay.iCal
{
    public interface IUniqueComponentList<T> :
        IFilteredCalendarObjectList<T>
        where T : IUniqueComponent
    {
        bool ContainsKey(string UID);
        T this[string uid] { get; set; }
    }
}
