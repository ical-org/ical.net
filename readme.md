# ![iCal.NET](https://raw.githubusercontent.com/ical-org/ical.net/main/assets/logo_300px.png)

| [![GitHub release](https://img.shields.io/github/release/ical-org/ical.net.svg?sort=semver)](https://github.com/ical-org/ical.net/releases/latest) | [![codecov](https://codecov.io/gh/ical-org/ical.net/branch/main/graph/badge.svg)](https://codecov.io/gh/ical-org/ical.net) | [![License: MIT](https://img.shields.io/badge/License-MIT-brightgreen.svg)](https://github.com/ical-org/ical.net/blob/main/license.md) |  
|----------|----------|----------|  
| [![NuGet Version](https://img.shields.io/nuget/v/ical.net)](https://www.nuget.org/packages/Ical.Net)  | [![NuGet Pre-release](https://img.shields.io/nuget/vpre/ical.net?label=nuget%20pre-release)](https://www.nuget.org/packages/Ical.Net/absoluteLatest)  |   |  

## What is iCal.NET?
iCal.NET is a robust and feature-rich iCalendar (RFC 5545) library for .NET, designed to simplify working with calendar data while ensuring full compliance with the iCalendar standard. Here are the main features and benefits:

### Key Features

* **RFC 5545 Compliance**: Guarantees compatibility with the iCalendar standard, ensuring seamless integration with popular calendaring applications.
* **Event Management**: Easily create, modify, and manage calendar events programmatically.
* **Recurrence Rules**: Supports complex recurrence rules, making it ideal for scheduling recurring events.
* **Serialization/Deserialization**: Effortlessly convert calendar data to and from iCalendar (.ics) files.
* **Time Zone Support**: Handle events across different time zones with ease.
* **Attachments**: Add attachments to calendar events (including invites) for enhanced functionality.

### Benefits

* **Performance and Usability**: Version 5 was an extensive rewrite for improved performance, correctness, and usability, with memory usage optimized down to 50% of v4. The upcoming v6 pre-release builds further on this foundation with a new recurrence engine, simplified date/time handling, and overhauled time zone and alarm support.
* **Compatibility**: Works seamlessly with .NET 10, .NET 8, .NET Standard, and .NET Framework, making it versatile for various projects.
* **Community-Driven**: Actively maintained and supported by a dedicated community, with extensive documentation and examples to get started quickly.
* **Open Source**: Free to use and contribute to under the MIT license, fostering collaboration and innovation.

## Mission Statement

Our mission is to provide a robust and reliable iCalendar library for .NET, ensuring full RFC 5545 compliance and seamless integration with popular calendaring applications. We strive to enhance usability, performance, and compatibility, empowering developers to create exceptional calendaring solutions. 

**Join us in making iCal.NET the premier choice for .NET calendaring needs!**

## iCal.NET Versions

### iCal.NET v6

v6 is the best iCal.NET yet - even as a pre-release. Built on over 100 commits since v5.2.0, it introduces a simplified, unambiguous `CalDateTime`, a brand-new `NodaTime`-based recurrence and occurrence evaluation engine, a flexible per-`Calendar` time zone provider, a fully overhauled RFC 5545-compliant alarm evaluator, and a faster serializer.

We encourage you to try it out and share your feedback — it's the most correct, performant, and well-tested version of iCal.NET to date.

See **[release-notes.md](release-notes.md)** for the full list of changes, including breaking changes to be aware of when upgrading from v5.

### iCal.NET v5

v5 is a comprehensive rewrite of the library, incorporating over 100 merged pull requests and focusing on enhanced performance, correctness, and usability. All reported issues from previous versions have been resolved, and unit tests have been added or enhanced for greater reliability.

See the **[API Changes Document](https://github.com/ical-org/ical.net/wiki/API-Changes-v4-to-v5)** and the **[Migration Guide for v4 to v5](https://github.com/ical-org/ical.net/wiki/Migrating-Guides)** in the wiki for detailed information. See **[release-notes-v5.md](release-notes-v5.md)** for the full v5.x release history.

### iCal.NET v4
is still available up to v4.3.1. It is out of support and will not receive any further updates. We recommend using the v6 pre-release or v5 packages instead. See **[release-notes-v4.md](release-notes-v4.md)** for the v4.x (and earlier) release history.

## Getting Started

### iCalendar Key Concepts

A basic understanding of the iCalendar standard (RFC 5545) is essential for using iCal.NET effectively. 
 **[The iCal.NET Wiki](https://github.com/ical-org/ical.net/wiki)** provides references to and information about the iCalendar specification.

### Install

Install the NuGet package using the following command:

```sh
dotnet add package iCal.NET
```

To try the upcoming v6 pre-release, include pre-release versions explicitly:

```sh
dotnet add package iCal.NET --prerelease
```

## Examples

The **[The iCal.NET Wiki](https://github.com/ical-org/ical.net/wiki)** contains several pages of examples of common iCal.NET usage scenarios.

* [Simple event with a recurrence](https://github.com/ical-org/ical.net/wiki)
* [Deserializing an ics file](https://github.com/ical-org/ical.net/wiki/Deserialize-an-ics-file)
* [Working with attachments](https://github.com/ical-org/ical.net/wiki/Working-with-attachments)
* [Working with recurring elements](https://github.com/ical-org/ical.net/wiki/Working-with-recurring-elements)
* [Concurrency scenarios and PLINQ](https://github.com/ical-org/ical.net/wiki/Concurrency-scenarios-and-PLINQ)

## Versioning

iCal.NET uses [semantic versioning](http://semver.org/).

## Contributing

* [Submit a bug report or issue](https://github.com/ical-org/ical.net/wiki/Filing-a-(good)-bug-report)
* [Contribute code by submitting a pull request](https://github.com/ical-org/ical.net/wiki/Contributing-a-(good)-pull-request). **Always open an issue first**, so we can discuss necessary changes.
* [Ask a question](https://github.com/ical-org/ical.net/discussions). **Please only use the discussion area for questions.**

## Support

* We ask and encourage you to contribute back to the project. This is especially true if you are using the library in a commercial product.
* Questions asked in the discussion area are open to the community or experienced users to answer. Give maintainers a helping hand by answering questions whenever you can.
* Remember that keeping ical.net up is something ical.net maintainers and contributors do in their spare time.

## Credits

Big thanks to [JetBrains](https://www.jetbrains.com/) for supporting the project with free licenses of their fantastic tools.

<img src="https://resources.jetbrains.com/storage/products/company/brand/logos/jetbrains.svg" alt="JetBrains logo" width="200"><br/>

Without those two guys, iCal.NET would not exist today:

* [Rian Stockbower](https://github.com/rianjs/) took over the project in 2016, after obtaining permission from Douglas Day to relicense and continue developing the library. Rian maintained the library until Sept 2024. He added support to newer versions of .NET and focused on performance.
* [Doug Day](mailto:doug@ddaysoftware.com) founded and maintained the original dday.ical open-source project from 2007 to 2016. During this period, he contributed significantly to the development and enhancement of the library.

* iCal.NET logo adapted from [Love Calendar](https://thenounproject.com/term/love-calendar/116866/) by Sergey Demushkin
