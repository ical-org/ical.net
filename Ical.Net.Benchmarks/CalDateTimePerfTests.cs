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

    [Benchmark]
    public CalDateTime EmptyTzid() => CalDateTime.Now;

    [Benchmark]
    public CalDateTime SpecifiedTzid() => CalDateTime.FromDateTime(DateTime.Now, _aTzid);

    [Benchmark]
    public CalDateTime UtcDateTime() => CalDateTime.FromDateTime(DateTime.UtcNow);
}
