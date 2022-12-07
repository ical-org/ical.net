using System;
using BenchmarkDotNet.Attributes;
using Ical.Net.DataTypes;

namespace PerfTests
{
    public class CalDateTimePerfTests
    {
        const string _aTzid = "Australia/Sydney";
        const string _bTzid = "America/New_York";

        [Benchmark]
        public IDateTime EmptyTzid() => new CalDateTime(DateTime.Now);

        [Benchmark]
        public IDateTime SpecifiedTzid() => new CalDateTime(DateTime.Now, _aTzid);

        [Benchmark]
        public IDateTime UtcDateTime() => new CalDateTime(DateTime.UtcNow);

        [Benchmark]
        public IDateTime EmptyTzidToTzid() => EmptyTzid().ToTimeZone(_bTzid);

        [Benchmark]
        public IDateTime SpecifiedTzidToDifferentTzid() => SpecifiedTzid().ToTimeZone(_bTzid);

        [Benchmark]
        public IDateTime UtcToDifferentTzid() => UtcDateTime().ToTimeZone(_bTzid);
    }
}