using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains.InProcess;

namespace PerfTesting
{
    class Runner
    {
        static void Main(string[] args)
        {
            //BenchmarkRunnerCore.Run(BenchmarkConverter.TypeToBenchmarks(typeof(OccurencePerfTests)), t => InProcessToolchain.Instance);
            BenchmarkRunner.Run(BenchmarkConverter.TypeToBenchmarks(typeof(CalDateTimePerfTests)));
            //BenchmarkRunnerCore.Run(BenchmarkConverter.TypeToBenchmarks(typeof(SerializationPerfTests)), t => InProcessToolchain.Instance);
            //var c = new CalDateTimePerfTests();
            //c.EmptyTzidToTzid();
        }
    }
}
