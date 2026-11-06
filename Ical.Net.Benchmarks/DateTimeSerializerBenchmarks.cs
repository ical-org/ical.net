//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using BenchmarkDotNet.Attributes;
using Ical.Net.DataTypes;
using Ical.Net.Serialization.DataTypes;

namespace Ical.Net.Benchmarks;

[MemoryDiagnoser]
public class DateTimeSerializerBenchmarks
{
    private static readonly DateTimeSerializer serializer = new();

    private static readonly CalDateTime DateOnly = new(2025, 1, 2);
    private static readonly CalDateTime Floating = new(2025, 1, 2, 3, 4, 5);
    private static readonly CalDateTime Utc = new(2025, 1, 2, 3, 4, 5, CalDateTime.UtcTzId);

    [Benchmark]
    public string? SerializeDateOnly() => serializer.SerializeToString(DateOnly);

    [Benchmark]
    public string? SerializeFloating() => serializer.SerializeToString(Floating);

    [Benchmark]
    public string? SerializeUtc() => serializer.SerializeToString(Utc);
}
