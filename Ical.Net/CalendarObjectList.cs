//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using Ical.Net.Collections;

namespace Ical.Net;

/// <summary>
/// A collection of calendar objects.
/// </summary>
public class CalendarObjectList : GroupedList<string, ICalendarObject>, ICalendarObjectList<ICalendarObject>
{}
