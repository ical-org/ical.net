//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

#nullable enable
using Ical.Net.Collections;

namespace Ical.Net;

public interface ICalendarObjectList<TType> :
    IGroupedCollection<string, TType> where TType : class, ICalendarObject
{
    TType? this[int index] { get; }
}
