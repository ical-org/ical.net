using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains.InProcess;

namespace PerfTests
{
    public class Runner
    {
        static void Main(string[] args)
        {
            BenchmarkRunnerCore.Run(BenchmarkConverter.TypeToBenchmarks(typeof(OccurencePerfTests)), t => InProcessToolchain.Instance);
        }
    }
}
