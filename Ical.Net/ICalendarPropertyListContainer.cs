//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

namespace Ical.Net;

public interface ICalendarPropertyListContainer : ICalendarObject
{
    CalendarPropertyList Properties { get; }
}