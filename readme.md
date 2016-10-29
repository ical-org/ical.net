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

* [Submit a bug report or issue](https://github.com/rianjs/ical.net/wiki/Filing-a-(good)-bug-report)
* [Contribute code by submitting a pull request](https://github.com/rianjs/ical.net/wiki/Contributing-a-(good)-pull-request)
* [Ask a question](https://github.com/rianjs/ical.net/issues)

## Support

ical.net is a something I do in my spare time around my family, day job, friends, and other hobbies. That means support is "when I get to it". I recognize that sometimes this isn't good enough, especially if you have a production issue. To that end, I [offer paid support and bugfixes](http://rianjs.net/consulting). A few basic rules before you contact me:

* Changes made to the ical.net library are open source.
* You do not own the changes I make to the library.
* Congruence with my vision for the future for ical.net is required. That means I won't do things like add Exchange interop, or take  dependencies on third-party libraries that benefit on your use case.
* New versions of the library that result from changes made will be published on nuget for others to consume.

Please email me if paid support is something you require, and we can work out the details via email.

## The future: v3 and beyond

I have written a fairly detailed collection of things I'd like to get done for v3, which will involve some significant API changes, and simplifications of serialization and deserialization.

http://rianjs.net/2016/07/api-changes-for-ical-net-v3

I don't have clear plans beyond what I have outlined there. It may be that ical.net can be put on a shelf for a while after that.

## Creative Commons

iCal.NET logo adapted from [Love Calendar](https://thenounproject.com/term/love-calendar/116866/) By Sergey Demushkin, RU
