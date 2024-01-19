using BenchmarkDotNet.Running;

namespace Ical.Net.Benchmarks
{
    public class Runner
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run(BenchmarkConverter.TypeToBenchmarks(typeof(ApplicationWorkflows)));


            //BenchmarkRunnerCore.Run(BenchmarkConverter.TypeToBenchmarks(typeof(OccurencePerfTests)), t => InProcessToolchain.Instance);
            //BenchmarkRunnerCore.Run(BenchmarkConverter.TypeToBenchmarks(typeof(CalDateTimePerfTests)), t => InProcessToolchain.Instance);
            //BenchmarkRunnerCore.Run(BenchmarkConverter.TypeToBenchmarks(typeof(SerializationPerfTests)), t => InProcessToolchain.Instance);
            //BenchmarkRunnerCore.Run(BenchmarkConverter.TypeToBenchmarks(typeof(ThroughputTests)), t => InProcessToolchain.Instance);
        }
    }
}
