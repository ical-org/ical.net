//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

#nullable enable
namespace Ical.Net;

public interface ICalendarPropertyListContainer : ICalendarObject
{
    CalendarPropertyList Properties { get; }
}
