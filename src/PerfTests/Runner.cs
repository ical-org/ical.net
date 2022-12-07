using System.Reflection;
using BenchmarkDotNet.Running;

namespace PerfTests
{
    public class Runner
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run(Assembly.GetExecutingAssembly());
            //BenchmarkRunner.Run(BenchmarkConverter.TypeToBenchmarks(typeof(ApplicationWorkflows)));
            //BenchmarkRunner.Run(BenchmarkConverter.TypeToBenchmarks(typeof(OccurrencePerfTests)));//, t => InProcessToolchain.Instance);
            //BenchmarkRunner.Run(BenchmarkConverter.TypeToBenchmarks(typeof(CalDateTimePerfTests)));//, t => InProcessToolchain.Instance);
            //BenchmarkRunner.Run(BenchmarkConverter.TypeToBenchmarks(typeof(SerializationPerfTests)));//, t => InProcessToolchain.Instance);
            //BenchmarkRunner.Run(BenchmarkConverter.TypeToBenchmarks(typeof(ThroughputTests)));//, t => InProcessToolchain.Instance);
        }
    }
}
