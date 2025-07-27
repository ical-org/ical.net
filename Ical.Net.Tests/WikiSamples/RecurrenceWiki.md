
It's impossible to provide an example of every type of recurrence scenario, so here are a few that represent some common use cases. If you'd like to see a specific example laid out, please [create an issue](https://github.com/rianjs/ical.net/issues).

## How to think about occurrences
If you create an event or an alarm that happens more than once, it typically has a start time, an end time, and some rules about how and when it repeats:
- "Daily, forever"
- "Every other Tuesday until the end of the year"
- "The fourth Thursday of every November"
You then want to *search* for occurrences of that event during a given time period. This kicks off the machinery which generates the set of occurrences of that event that match your search criteria.

### How to create a recurring event
It's like creating a normal event but we add one or more `RecurrencePattern` to the event to make it recurring.

> [!NOTE]
> Make sure that the event's start date is the first occurrence of the series.

```cs
var recurrence = new RecurrencePattern()
{
    Frequency = FrequencyType.Daily,
    Interval = 2,
    Count = 30
    // Add other parameters like ByDay, ByMonth, etc.
};

var calendarEvent = new CalendarEvent()
{
    DtStart = new CalDateTime(2025, 07, 10),
    DtEnd = new CalDateTime(2025, 07, 11),
    // Add the rule to the event.
    RecurrenceRules = [recurrence]
};
```
### How to get occurrences of a recurring event
After creating a recurring event we can call `GetOccurrences()` to get all occurrences of a series or `GetOccurrences(CalDateTime startTime)` to get all occurrences after a given date. We can limit the evaluation by adding a `TakeWhileBefore(CalDateTime periodEnd)` to get occurrences till a specific point in time.

> [!NOTE]
> The provided end-date in the `TakeWhileBefore` method calculate against the event start is meant in an exclusive manner. In our example, the occurrence on 2025-08-01 is not provided despite the event-start (2025-08-01) and the calculation-end (2025-08-01) is the same date (exclusive). To include it we have to set the calculation-end to 2025-08-01T00:00:01.

> [!WARNING]
> Calculating a series with no end may lead in a `EvaluationOutOfRangeException`. Make sure you use `TakeWhileBefore` to avoid this.

```cs
// Get all occurrences of the series.
IEnumerable<Occurrence> allOccurrences = calendarEvent.GetOccurrences();
Assert.That(allOccurrences.Count(), Is.EqualTo(30));

// Get the occurrences in July.
IEnumerable<Occurrence> julyOccurrences = calendarEvent
    .GetOccurrences(new CalDateTime(2025, 07, 01))
    .TakeWhileBefore(new CalDateTime(2025, 08, 01));
Assert.That(julyOccurrences.Count(), Is.EqualTo(11));
```

> [!TIP]
> We can call the `GetOccurrences` method on `CalendarEvent` and on `Calendar`.


## Recurrence Examples
### Daily, Interval, Count
Suppose we want to create an event for July 10, between 09:00 and 10:00 that recurs every other day for 2 occurrences.

```cs
// Create the CalendarEvent
var start = new CalDateTime(2025, 07, 10, 09, 00, 00, "Europe/Zurich");
var recurrence = new RecurrencePattern()
{
	Frequency = FrequencyType.Daily,
	Interval = 2,
	Count = 2
};

var calendarEvent = new CalendarEvent()
{
	DtStart = start,
	DtEnd = start.AddHours(1),
	RecurrenceRules = [recurrence]
};

// Add CalendarEvent to Calendar
var calendar = new Calendar();
calendar.Events.Add(calendarEvent);

// Serialize Calendar to string
var calendarSerializer = new CalendarSerializer();
var calendarAsIcs = calendarSerializer.SerializeToString(calendar);
```
This results in the following Ics string.
```ics
BEGIN:VCALENDAR
BEGIN:VEVENT
DTEND;TZID=Europe/Zurich:20250710T100000
DTSTART;TZID=Europe/Zurich:20250710T090000
RRULE:FREQ=DAILY;INTERVAL=2;COUNT=2
END:VEVENT
END:VCALENDAR
```
Now let's get all occurrences of this series.
```cs
var occurrences = calendar.GetOccurrences();
```
This will give us two occurrences with the following start and end dates.
```ics
DTEND;TZID=Europe/Zurich:20250710T100000
DTSTART;TZID=Europe/Zurich:20250710T090000

DTEND;TZID=Europe/Zurich:20250712T100000
DTSTART;TZID=Europe/Zurich:20250712T090000
```

### Yearly, ByMonthDay, Until
Suppose we want to create an series of events for July 10 and 12, between 09:00 and 10:00 that recurs every year until in two years.

> [!WARNING]
> The `UNTIL` date must have UTC or none time zone.

> [!NOTE]
> The `UNTIL` date is meant in an inclusive manner.

```cs
// Create the CalendarEvent
var start = new CalDateTime(2025, 07, 10, 09, 00, 00, "Europe/Zurich");
var recurrence = new RecurrencePattern()
{
    Frequency = FrequencyType.Yearly,
    ByMonthDay = [10, 12],
    // 2027-07-10 09:00:00 Europe/Zurich (07:00:00 UTC)
    Until = start.AddYears(2).ToTimeZone("UTC")
};

var calendarEvent = new CalendarEvent()
{
    DtStart = start,
    DtEnd = start.AddHours(1),
    RecurrenceRules = [recurrence]
};

// Add CalendarEvent to Calendar
var calendar = new Calendar();
calendar.Events.Add(calendarEvent);

// Serialize Calendar to string
var calendarSerializer = new CalendarSerializer();
var calendarAsIcs = calendarSerializer.SerializeToString(calendar);
```
This results in the following Ics string.
```ics
BEGIN:VCALENDAR
BEGIN:VEVENT
DTEND;TZID=Europe/Zurich:20250710T100000
DTSTART;TZID=Europe/Zurich:20250710T090000
RRULE:FREQ=YEARLY;UNTIL=20270710T070000Z;BYMONTHDAY=10,12
END:VEVENT
END:VCALENDAR
```
Now let's get all occurrences of this series.
```cs
var occurrences = calendar.GetOccurrences();
```
This will give us two occurrences with the following start and end dates.
```ics
DTEND;TZID=Europe/Zurich:20250710T100000
DTSTART;TZID=Europe/Zurich:20250710T090000

DTEND;TZID=Europe/Zurich:20250712T100000
DTSTART;TZID=Europe/Zurich:20250712T090000

DTEND;TZID=Europe/Zurich:20260710T100000
DTSTART;TZID=Europe/Zurich:20260710T090000

DTEND;TZID=Europe/Zurich:20260712T100000
DTSTART;TZID=Europe/Zurich:20260712T090000

DTEND;TZID=Europe/Zurich:20270710T100000
DTSTART;TZID=Europe/Zurich:20270710T090000
```
> [!TIP]
> Have a look at the last occurrence, it has the same start as the `UNTIL` of the recurrence, this is the meaning of 'inclusive' (valid as long as `DTSTART` <= `UNTIL`).

### Montly, ByDay, Count, RDate  (add occurrence)
Suppose we decided to play poker with our friends every last sunday per month for the next three months but also on July 10th, just to stay sharp.

```cs
// Create the CalendarEvent
var start = new CalDateTime(2025, 06, 29, 16, 00, 00, "Europe/Zurich");
var recurrence = new RecurrencePattern()
{
    Frequency = FrequencyType.Monthly,
    ByDay = [new(DayOfWeek.Sunday, FrequencyOccurrence.Last)],
    Count = 3,
};

// Create additional occurrence.
PeriodList periodList = new PeriodList();
periodList.Add(new CalDateTime(2025, 07, 10, 09, 00, 00, "Europe/Zurich"));

var calendarEvent = new CalendarEvent()
{
    DtStart = start,
    DtEnd = start.AddHours(4),
    RecurrenceRules = [recurrence],
    // Add the additional occurrence to the series.
    RecurrenceDatesPeriodLists = [periodList]
};

// Add CalendarEvent to Calendar
var calendar = new Calendar();
calendar.Events.Add(calendarEvent);

// Serialize Calendar to string
var calendarSerializer = new CalendarSerializer();
var calendarAsIcs = calendarSerializer.SerializeToString(calendar);
```
This results in the following Ics string.
> [!NOTE]
> Have a look at the `RDATE` property which defines our additional occurrence.
```ics
BEGIN:VCALENDAR
BEGIN:VEVENT
DTEND;TZID=Europe/Zurich:20250629T200000
DTSTART;TZID=Europe/Zurich:20250629T160000
RDATE;TZID=Europe/Zurich:20250710T090000
RRULE:FREQ=MONTHLY;COUNT=3;BYDAY=-1SU
END:VEVENT
END:VCALENDAR
```
Now let's get all occurrences of this series.
```cs
var occurrences = calendar.GetOccurrences();
```
This will give us two occurrences with the following start and end dates.
> [!NOTE]
> We only specified the start of our additional occurrence, the duration is inherit of the base-series.
```ics
DTEND;TZID=Europe/Zurich:20250629T200000
DTSTART;TZID=Europe/Zurich:20250629T160000

DTEND;TZID=Europe/Zurich:20250710T130000
DTSTART;TZID=Europe/Zurich:20250710T090000

DTEND;TZID=Europe/Zurich:20250727T200000
DTSTART;TZID=Europe/Zurich:20250727T160000

DTEND;TZID=Europe/Zurich:20250831T200000
DTSTART;TZID=Europe/Zurich:20250831T160000
```

### Hourly, Until, ExDate (remove occurrence)
Suppose we decided to read 15 minutes every full hour until midnight - except a 22:00 where we eat dinner (what a strange life we have 😅).

```cs
// Create the CalendarEvent
var start = new CalDateTime(2025, 07, 10, 20, 00, 00, "UTC");
var recurrence = new RecurrencePattern()
{
    Frequency = FrequencyType.Hourly,
    Until = start.AddHours(4)
};

// Create exception for an occurrence.
PeriodList periodList = new PeriodList();
periodList.Add(new CalDateTime(2025, 07, 10, 22, 00, 00, "UTC"));

var calendarEvent = new CalendarEvent()
{
    DtStart = start,
    DtEnd = start.AddMinutes(15),
    RecurrenceRules = [recurrence],
    // Add the exception date(s) to the series.
    ExceptionDatesPeriodLists = [periodList]
};

// Add CalendarEvent to Calendar
var calendar = new Calendar();
calendar.Events.Add(calendarEvent);

// Serialize Calendar to string
var calendarSerializer = new CalendarSerializer();
var calendarAsIcs = calendarSerializer.SerializeToString(calendar);
```
This results in the following Ics string.
> [!NOTE]
> Have a look at the `EXDATE` property which defines our removed occurrences.
```ics
BEGIN:VCALENDAR
BEGIN:VEVENT
DTEND:20250710T201500Z
DTSTART:20250710T200000Z
EXDATE:20250710T220000Z
RRULE:FREQ=HOURLY;UNTIL=20250711T000000Z
END:VEVENT
END:VCALENDAR
```
Now let's get all occurrences of this series.
```cs
var occurrences = calendar.GetOccurrences();
```
This will give us two occurrences with the following start and end dates.
```ics
DTEND:20250710T201500Z
DTSTART:20250710T200000Z

DTEND:20250710T211500Z
DTSTART:20250710T210000Z

DTEND:20250710T231500Z
DTSTART:20250710T230000Z

DTEND:20250711T001500Z
DTSTART:20250711T000000Z
```

### Daily, Interval, Count, Exception (moved occurrence)
Suppose we decided to go for a walk every other day at 09:00 (4 times) - but not the third occurrence, we have a packed schedule then and can only go for 13 minutes at 13:00. 

Since start, end, duration and title of the event are changing, we have to create that new event and tell the series to replace an occurrence of the series with this 'special' event (child event). We do this by linking the child event to the series by using the same `UID`. We also have to link the child event to the original occurrence of the series by adding the original occurrence start-date in the `RCURRENCE-ID` property.

> [!NOTE] Link moved events
> 1. Create a `CalendarEvent` with the changes (child event).
> 2. Link the child event to the series by using the same `Uid` (`UID`).
> 3. Link the child event to the original occurrence date by using the `RecurrenceId` (`RECURRENCE-ID`).

```cs
// Create the CalendarEvent
var start = new CalDateTime(2025, 07, 10, 09, 00, 00, "Europe/Zurich");
var recurrence = new RecurrencePattern()
{
    Frequency = FrequencyType.Daily,
    Interval = 2,
    Count = 4
};

var calendarEvent = new CalendarEvent()
{
    // UID links master with child.
    Uid = "my-custom-id",
    Summary = "Walking",
    DtStart = start,
    DtEnd = start.AddHours(1),
    RecurrenceRules = [recurrence],
};

var startMoved = new CalDateTime(2025, 07, 13, 13, 00, 00, "Europe/Zurich");
var movedEvent = new CalendarEvent()
{
    // UID links master with child.
    Uid = "my-custom-id",
    // Overwrite properties of the original occurrence.
    Summary = "Short after lunch walk",
    // Set new start and end time.
    DtStart = startMoved,
    DtEnd = startMoved.AddMinutes(13),
    // Set the original date of the occurrence (2025-07-14 09:00:00).
    RecurrenceId = start.AddDays(4)
};

// Add CalendarEvent to Calendar
var calendar = new Calendar();
calendar.Events.Add(calendarEvent);
calendar.Events.Add(movedEvent);

// Serialize Calendar to string
var calendarSerializer = new CalendarSerializer();
var calendarAsIcs = calendarSerializer.SerializeToString(calendar);
```
This results in the following Ics string.
> [!NOTE]
> Have a look at the `SUMMARY` property which we have overridden.
```ics
BEGIN:VCALENDAR
BEGIN:VEVENT
DTEND;TZID=Europe/Zurich:20250710T100000
DTSTART;TZID=Europe/Zurich:20250710T090000
RRULE:FREQ=DAILY;INTERVAL=2;COUNT=4
SUMMARY:Walking
UID:my-custom-id
END:VEVENT
BEGIN:VEVENT
DTEND;TZID=Europe/Zurich:20250713T131300
DTSTART;TZID=Europe/Zurich:20250713T130000
RECURRENCE-ID;TZID=Europe/Zurich:20250714T090000
SUMMARY:Short after lunch walk
UID:my-custom-id
END:VEVENT
END:VCALENDAR
```
Now let's get all occurrences of this series.
```cs
var occurrences = calendar.GetOccurrences();
```
This will give us two occurrences with the following start and end dates.
```ics
DTEND;TZID=Europe/Zurich:20250710T100000
DTSTART;TZID=Europe/Zurich:20250710T090000

DTEND;TZID=Europe/Zurich:20250712T100000
DTSTART;TZID=Europe/Zurich:20250712T090000

DTEND;TZID=Europe/Zurich:20250713T131300
DTSTART;TZID=Europe/Zurich:20250713T130000

DTEND;TZID=Europe/Zurich:20250716T100000
DTSTART;TZID=Europe/Zurich:20250716T090000
```

## FAQ (Recurrence)

### Can I add multiple recurrence rules?
Yes, you can, even it's not shown in the examples above.

### What about negative recurrrence rules?
You can use them with the `ExceptionRules` property on a e.g. `CalendarEvent`. There are two facts to consider on using:
1. It is marked as `deprecated` in [RFC 5545 (iCalendar)](https://www.rfc-editor.org/rfc/rfc5545#section-8.3.2) and maybe other software may not read this rule(s).
2. They are hard to understand by humans (as multiple rules are in general).
