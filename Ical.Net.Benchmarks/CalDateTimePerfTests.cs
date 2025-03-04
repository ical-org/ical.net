//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using BenchmarkDotNet.Attributes;
using Ical.Net.DataTypes;
using System;

namespace Ical.Net.Benchmarks;

public class CalDateTimePerfTests
{
    private const string _aTzid = "Australia/Sydney";
    private const string _bTzid = "America/New_York";

    [Benchmark]
    public CalDateTime EmptyTzid() => CalDateTime.Now;

    [Benchmark]
    public CalDateTime SpecifiedTzid() => new CalDateTime(DateTime.Now, _aTzid);

    [Benchmark]
    public CalDateTime UtcDateTime() => new CalDateTime(DateTime.UtcNow);

    [Benchmark]
    public CalDateTime EmptyTzidToTzid() => EmptyTzid().ToTimeZone(_bTzid);

    [Benchmark]
    public CalDateTime SpecifiedTzidToDifferentTzid() => SpecifiedTzid().ToTimeZone(_bTzid);

    [Benchmark]
    public CalDateTime UtcToDifferentTzid() => UtcDateTime().ToTimeZone(_bTzid);
}
