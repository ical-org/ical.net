![iCal.NET for .NET](logo.png)

iCal.NET is an iCalendar (RFC 5545) class library for .NET aimed at providing RFC 5545 compliance, while providing full compatibility with popular calendaring applications and libraries.

## Getting iCal.NET

iCal.NET is available as [a nuget package](https://www.nuget.org/packages/Ical.Net).

## Migrating from dday.ical to ical.net

There's a guide just for you: **[Migrating from dday.ical](https://github.com/rianjs/ical.net/wiki/Migrating-from-dday.ical)**

## Examples

The wiki contains several pages of examples of common ical.net usage scenarios.

* [Simple event with a recurrence](https://github.com/rianjs/ical.net/wiki)
* [Deserializing an ics file](https://github.com/rianjs/ical.net/wiki/Deserialize-an-ics-file)
* [Working with attachments](https://github.com/rianjs/ical.net/wiki/Working-with-attachments)
* [Working with recurring elements](https://github.com/rianjs/ical.net/wiki/Working-with-recurring-elements)
* [Concurrency scenarios and PLINQ](https://github.com/rianjs/ical.net/wiki/Concurrency-scenarios-and-PLINQ)
* [Migrating from dday.ical](https://github.com/rianjs/ical.net/wiki/Migrating-from-dday.ical)

## Versioning

ical.net uses [semantic versioning](http://semver.org/). In a nutshell:

> Given a version number MAJOR.MINOR.PATCH, increment the:
>
> 1. MAJOR version when you make incompatible API changes,
> 2. MINOR version when you add functionality in a backwards-compatible manner, and
> 3. PATCH version when you make backwards-compatible bug fixes.

## Contributing

Fork and submit a pull request! If you haven't gotten feedback from me in a few days, please send me an email: rstockbower@gmail.com. Sometimes I don't see them.

A couple of guidelines to keep code quality high, and code reviews efficient:

* If you are submitting a fix, please include a unit test that tests for that bug. Unit tests are how we can be sure we haven't broken B while changing A. The unit test project has some good examples of how unit tests are structured. If you've never written (or run!) a unit test in Visual Studio, and you're uncertain how to do so, have a look at the [NUnit Test Adapter](http://nunit.org/index.php?p=vsTestAdapter&r=2.6.4), which is a free add-in with explicit nunit test support.
* Don't submit a change that has broken some of the unit tests. There are bugs in ical.net, as there are in all software. I have found cases where the unit tests themselves assert the wrong things. I have fixed several of these cases. Breaking a unit test that asserts the wrong thing is OK, but please make it assert the right thing so it's passing again.
* Please keep your commits and their messages meaningful. It is better to have many small commits, each with a message explaining the "why" of that specific change than it is to have a single, messy commit that does a dozen different things squashed together.  (Adding new features being the exception.) _Clean commits speed up the code review process._

### Immediate wins

#### v2 - Current version

Bug fixes and unit tests are the order of the day, particularly focusing on symmetric serialization and deserialization. dday.ical had many unit tests showing that various aspects of _deserialization_ worked properly, but did not have many (any?) tests that showed that _serialization_ worked. I am working on making sure these operations are reliably symmetrical.

Secondly, I am working on a .NET Core port. This work is largely done, but the tooling support for .NET Core isn't great. I have postponed work on that until such time as .NET Core versions can be emitted as part of a standard "Batch Build" alongside normal .NET binaries.

#### v3 - Future version

I have written a fairly detailed collection of things I'd like to get done for v3, which will involve some significant API changes, and simplifications of serialization and deserialization.

http://rianjs.net/2016/07/api-changes-for-ical-net-v3

I don't have clear plans beyond what I have outlined there. It may be that ical.net can be put on a shelf for a while after that.

## Creative Commons

iCal.NET logo adapted from [Love Calendar](https://thenounproject.com/term/love-calendar/116866/) By Sergey Demushkin, RU
