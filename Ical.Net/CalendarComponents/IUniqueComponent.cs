//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.Collections.Generic;
using Ical.Net.DataTypes;

namespace Ical.Net.CalendarComponents;

public interface IUniqueComponent : ICalendarComponent
{
    string? Uid { get; set; }

    IList<Attendee> Attendees { get; set; }
    IList<string> Comments { get; set; }
    CalDateTime? DtStamp { get; set; }
    Organizer? Organizer { get; set; }
    IList<RequestStatus> RequestStatuses { get; set; }
    Uri? Url { get; set; }
}
