//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using BenchmarkDotNet.Attributes;
using Ical.Net.Serialization;

namespace Ical.Net.Benchmarks;

[MemoryDiagnoser]
public class ParameterSerializerBenchmarks
{
    private static readonly ParameterSerializer serializer = new();

    private static readonly CalendarParameter Parameter = new("TZID", "Europe/Vienna");

    [Benchmark]
    public string? SerializeParameter() => serializer.SerializeToString(Parameter);
}
