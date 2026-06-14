//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System.Collections.Generic;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using NodaTime;

namespace Ical.Net;

public interface IGetFreeBusy
{
    FreeBusy? GetFreeBusy(DateTimeZone timeZone, FreeBusy freeBusyRequest);
    FreeBusy? GetFreeBusy(DateTimeZone timeZone, CalDateTime fromInclusive, CalDateTime toExclusive);
    FreeBusy? GetFreeBusy(DateTimeZone timeZone, Organizer organizer, IEnumerable<Attendee> contacts, CalDateTime fromInclusive, CalDateTime toExclusive);
}
