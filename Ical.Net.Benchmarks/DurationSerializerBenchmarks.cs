//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using BenchmarkDotNet.Attributes;
using Ical.Net.DataTypes;
using Ical.Net.Serialization.DataTypes;

namespace Ical.Net.Benchmarks;

[MemoryDiagnoser]
public class DurationSerializerBenchmarks
{
    private static readonly DurationSerializer serializer = new();

    private static readonly Duration Duration = new(days: -1, hours: -2, minutes: -3, seconds: -4);

    [Benchmark]
    public string? SerializeDuration() => serializer.SerializeToString(Duration);
}
