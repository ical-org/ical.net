
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


## Examples
### Daily every second day, 2 times
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

// TODO: Add recurrence with By... property  
// TODO: Try recurrence with RDATE (never used before)  
// TODO: Add recurrence with EXDATE  
// TODO: Add recurrence with moved occurrence  
// No example with EXRULE because it's deprecated in RFC 5545  