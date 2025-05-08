//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System.Collections.Generic;
using Ical.Net.CalendarComponents;

namespace Ical.Net.Proxies;

public interface IUniqueComponentList<TComponentType> :
    ICalendarObjectList<TComponentType> where TComponentType : class, IUniqueComponent
{
    TComponentType this[string uid] { get; set; }
    void AddRange(IEnumerable<TComponentType> collection);
}
