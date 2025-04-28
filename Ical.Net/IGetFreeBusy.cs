//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

#nullable enable
using System.Collections.Generic;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;

namespace Ical.Net;

public interface IGetFreeBusy
{
    FreeBusy? GetFreeBusy(FreeBusy freeBusyRequest);
    FreeBusy? GetFreeBusy(CalDateTime fromInclusive, CalDateTime toExclusive);
    FreeBusy? GetFreeBusy(Organizer organizer, IEnumerable<Attendee> contacts, CalDateTime fromInclusive, CalDateTime toExclusive);
}
