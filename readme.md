# <img src="https://github.com/ical-org/ical.net/raw/main/assets/logo.png" style="width:300px;">


| [![GitHub release](https://img.shields.io/github/release/ical-org/ical.net.svg?sort=semver)](https://github.com/ical-org/ical.net/releases/latest) | [![codecov](https://codecov.io/gh/ical-org/ical.net/branch/main/graph/badge.svg)](https://codecov.io/gh/ical-org/ical.net) | [![License: MIT](https://img.shields.io/badge/License-MIT-brightgreen.svg)](https://github.com/ical-org/ical.net/blob/main/license.md) |  
|----------|----------|----------|  
| [![NuGet Version](https://img.shields.io/nuget/v/ical.net)](https://www.nuget.org/packages/Ical.Net)  |   |   |  

## What is iCal.NET?
iCal.NET is a robust and feature-rich iCalendar (RFC 5545) library for .NET, designed to simplify working with calendar data while ensuring full compliance with the iCalendar standard. Here are the main features and benefits:

### Key Features

* **RFC 5545 Compliance**: Guarantees compatibility with the iCalendar standard, ensuring seamless integration with popular calendaring applications.
* **Event Management**: Easily create, modify, and manage calendar events programmatically.
* **Recurrence Rules**: Supports complex recurrence patterns, making it ideal for scheduling recurring events.
* **Serialization/Deserialization**: Effortlessly convert calendar data to and from iCalendar (.ics) files.
* **Time Zone Support**: Handle events across different time zones with ease.
* **Attachments**: Add attachments to calendar events (including invites) for enhanced functionality.

### Benefits

* **Performance and Usability**: The latest version (v5) is extensively rewritten for improved performance, correctness, and usability. Memory usage is optimized down to 50% of v4.
* **Compatibility**: Works seamlessly with .NET 8, .NET 6, .NET Standard, and .NET Framework, making it versatile for various projects.
* **Community-Driven**: Actively maintained and supported by a dedicated community, with extensive documentation and examples to get started quickly.
* **Open Source**: Open Source: Free to use and contribute to under the MIT license, fostering collaboration and innovation.

## Mission Statement

Our mission is to provide a robust and reliable iCalendar library for .NET, ensuring full RFC 5545 compliance and seamless integration with popular calendaring applications. We strive to enhance usability, performance, and compatibility, empowering developers to create exceptional calendaring solutions. 

**Join us in making iCal.NET the premier choice for .NET calendaring needs!**

## iCal.NET Versions

### iCal.NET v5

v5 is a comprehensive rewrite of the library, incorporating over 100 merged pull requests and focusing on enhanced performance, correctness, and usability. All reported issues from previous versions have been resolved, and unit tests have been added or enhanced for greater reliability.

See the **[API Changes Document](https://github.com/ical-org/ical.net/wiki/API-Changes-v4-to-v5)** and the **[Migration Guide for v4 to v5](https://github.com/ical-org/ical.net/wiki/Migrating-Guides)** in the wiki for detailed information.

### iCal.NET v4
is still available up to v4.3.1. Is is out of support and will not receive any further updates. We recommend using the v5 packages instead.

## Getting Started

### iCalendar Key Concepts

A basic understanding of the iCalendar standard (RFC 5545) is essential for using iCal.NET effectively. 
 **[The iCal.NET Wiki](https://github.com/ical-org/ical.net/wiki)** provides references to and information about the iCalendar specification.

### Install

Install the NuGet packing using the following command:

```sh
dotnet add package iCal.NET
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
* [Douglas Day](https://github.com/douglasday) founded and maintained the iCal.NET open-source project from 2007 to 2016. During this period, he contributed significantly to the development and enhancement of the library.

* iCal.NET logo adapted from [Love Calendar](https://thenounproject.com/term/love-calendar/116866/) by Sergey Demushkin
