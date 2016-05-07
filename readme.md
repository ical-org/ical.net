DDay.iCal is an iCalendar (RFC2445) class library for .NET. It's aimed at providing full RFC 2445 compliance, while providing full compatibility with popular calendaring applications.

## Getting ical.net

Coming soon to a nuget near you.

## Examples

Coming soon to a readme near you.

## Contributing

Fork, and submit a pull request! If you haven't gotten feedback from me in a few days, please send me an email: rstockbower@gmail.com. Sometimes I don't see them.

A couple of guidelines to keep code quality high, and code reviews efficient:

* If you are submitting a fix, please include a unit test that tests for that bug. Unit tests are how we can be sure we haven't broken B while changing A. The unit test project has some good examples of how unit tests are structured. If you've never written (or run!) a unit test in Visual Studio, and you're uncertain how to do so, have a look at the [NUnit Test Adapter](http://nunit.org/index.php?p=vsTestAdapter&r=2.6.4), which is a free add-in with explicit nunit test support.
* Don't submit a change that has broken some of the unit tests. There are bugs in ical.net, as there are in all software. I have found cases where the unit tests themselves assert the wrong things. I have fixed several of these cases. Breaking a unit test that asserts the wrong thing is OK, but please make it assert the right thing so it's passing again.
* Please keep your commits and their messages meaningful. It is better to have many small commits, each with a message explaining the "why" of that specific change than it is to have a single, messy commit that does a dozen different things squashed together.  (Adding new features being the exception.) _Clean commits speed up the code review process._

### Immediate wins

Performance is ical.net's biggest pain point right now. Most of the immediate wins are ways of addressing performance holistically, and is how I got interested in the project to begin with.

#### Circular type dependencies

(This one isn't for the faint of heart.)

Here's a sketch of one small area that shows what I mean:

```
IDateTime
    -> TimeZoneObservance
        -> IPeriod -> IDateTime (circular)
        -> ITimeZoneInfo
            -> IUTCOffset (DateTime ToUTC/Local, TimeSpan)
            -> TimeZoneObservance (circular)
```

There were many circular type dependencies in dday.ical, and they persist in ical.net. In general, this makes it difficult to change implementations. The normal pattern of making `A` and `B` depend on `C` instead of on each other is a non-trivial undertaking because each type depends on every other type. To that end, just flattening the interfaces so each type doesn't have a large, hidden dependency tree would be helpful.

At first glance, many of the properties associated with the `IDateTime` object, for example, can be getter-only, but have cases where doing so would break something you don't see.

#### NodaTime usage

Many of the concepts explicit in the diagram above disappear entirely if we move to NodaTime. Doing so everywhere is difficult due to the circular type dependencies, though I have managed to swap implementations in some of the hot paths in a few places.

#### Swapping service provider pattern for real dependency injection

This follows from the circular type dependencies. This would make unit testing smaller chunks of code easier, and it makes it easier to swap out implementations for serialization, parsing, recurrence computation, time zone checking, etc.

#### Modern serialization/deserialization

The patterns used in dday.ical are somewhat dated, and I suspect the machinery around the process doesn't need to be so ornate.

#### Update ANTLR

I suspect the version of ANTLR used is old. I would prefer not to have the DLLs committed. Even swapping these for a nuget reference would be a step forward.

#### Use of generic .NET collections

dday.ical had its own set of specialized collections that it used with the service provider pattern. In general, the performance of these are bad. I haven't fully investigated, yet, but I suspect getting rid of the specialized collections could improve performance.

Along similar lines, I replace many instances of `List<T>` usage with `ISet<T>` with comprehensive `GetHashCode()` implementations, which helped performance considerably. I suspect I didn't get them all; there are many corners of the code that I have not explored.

#### Get rid of the garbage collection + reallocation overhead

Taking this on as a top-level task is probably pointless. Moving away from the old dday.ical collections, moving to NodaTime, and changing/eliminating `Copy<T>` behavior from the serialization implementation and getting rid of the service provider pattern should reduce much of this thrashing as a natural course of events.
