ical.net is an iCalendar (RFC2445) class library for .NET. It's aimed at providing full RFC 2445 compliance, while providing full compatibility with popular calendaring applications.

## Getting ical.net

ical.net is available as a nuget package: https://www.nuget.org/packages/Ical.Net

## Examples

### Creating a calendar with a recurring event:

```csharp
var now = DateTime.Now;
var later = now.AddHours(1);

//Repeat daily for 5 days
var rrule = new RecurrencePattern(FrequencyType.Daily, 1)
{
    Count = 5
};

var e = new Event
{
    DtStart = new CalDateTime(now),
    DtEnd = new CalDateTime(later),
    Duration = TimeSpan.FromHours(1),
    RecurrenceRules = new List<IRecurrencePattern> {rrule},
};

var foo = new Calendar();
foo.Events.Add(e);

var serializer = new CalendarSerializer(new SerializationContext());
var serializedCalendar = serializer.SerializeToString(calendar);

Console.WriteLine(serializedCalendar);

//Displays something like:

// BEGIN: VCALENDAR
// VERSION:2.0
// PRODID: -//github.com/rianjs/ical.net//NONSGML ical.net 2.1//EN
// BEGIN:VEVENT
// DTEND:20160704T172520
// DTSTAMP:20160704T162520
// DTSTART:20160704T162520
// RRULE:FREQ = DAILY; COUNT = 5
// SEQUENCE: 0
// UID: f4693a88 - 0a57 - 4761 - b949 - 8822b8a507d2
// END:VEVENT
// END:VCALENDAR
```

### Deserializing an ics file

```csharp
// Loads a collection of calendars (each ics file can have more than one VCALENDAR in it)
var calendarCollection = Calendar.LoadFromFile(@"path\to\file.ics");

//A VCALENDAR may contain many VEVENTs, so get the first calendar's first event
var firstEvent = calendarCollection
    .First()
    .Events
    .First();

Console.WriteLine(firstEvent.Duration);
```

## Contributing

Fork and submit a pull request! If you haven't gotten feedback from me in a few days, please send me an email: rstockbower@gmail.com. Sometimes I don't see them.

A couple of guidelines to keep code quality high, and code reviews efficient:

* If you are submitting a fix, please include a unit test that tests for that bug. Unit tests are how we can be sure we haven't broken B while changing A. The unit test project has some good examples of how unit tests are structured. If you've never written (or run!) a unit test in Visual Studio, and you're uncertain how to do so, have a look at the [NUnit Test Adapter](http://nunit.org/index.php?p=vsTestAdapter&r=2.6.4), which is a free add-in with explicit nunit test support.
* Don't submit a change that has broken some of the unit tests. There are bugs in ical.net, as there are in all software. I have found cases where the unit tests themselves assert the wrong things. I have fixed several of these cases. Breaking a unit test that asserts the wrong thing is OK, but please make it assert the right thing so it's passing again.
* Please keep your commits and their messages meaningful. It is better to have many small commits, each with a message explaining the "why" of that specific change than it is to have a single, messy commit that does a dozen different things squashed together.  (Adding new features being the exception.) _Clean commits speed up the code review process._

### Immediate wins

#### v2 - Current version

I am working on a .NET Core port. Getting that finished is my first priority, then I will begin work on v3.

#### v3 - Future version

I have written a fairly detailed collection of things I'd like to get done for v3, which will involve some significant API changes, and simplifications of serialization and deserialization.

http://rianjs.net/2016/07/api-changes-for-ical-net-v3

I don't have clear plans beyond what I have outlined there. It may be that ical.net can be put on a shelf for a while after that.
