//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using BenchmarkDotNet.Attributes;
using Ical.Net.DataTypes;
using Ical.Net.Serialization.DataTypes;

namespace Ical.Net.Benchmarks;

[MemoryDiagnoser]
public class UtcOffsetSerializerBenchmarks
{
    private static readonly UtcOffsetSerializer serializer = new();

    private static readonly UtcOffset UtcOffset = new(TimeSpan.FromHours(-5.5));

    [Benchmark]
    public string? SerializeUtcOffset() => serializer.SerializeToString(UtcOffset);
}
