//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using BenchmarkDotNet.Attributes;
using Ical.Net.DataTypes;
using Ical.Net.Serialization.DataTypes;

namespace Ical.Net.Benchmarks;

[MemoryDiagnoser]
public class PeriodSerializerBenchmarks
{
    private static readonly PeriodSerializer serializer = new();

    private static readonly Period Period = new(new CalDateTime(2025, 1, 2, 3, 4, 5, CalDateTime.UtcTzId), Duration.FromHours(1));

    [Benchmark]
    public string? SerializePeriod() => serializer.SerializeToString(Period);
}
