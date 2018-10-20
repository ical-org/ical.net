using System;
using System.IO;
using System.Linq;
using System.Reflection;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains.InProcess;

namespace PerfTests
{
    public class Runner
    {
        static void Main(string[] args)
        {
            BenchmarkRunnerCore.Run(BenchmarkConverter.TypeToBenchmarks(typeof(ApplicationWorkflows)), t => InProcessToolchain.Instance);


            //BenchmarkRunnerCore.Run(BenchmarkConverter.TypeToBenchmarks(typeof(OccurencePerfTests)), t => InProcessToolchain.Instance);
            //BenchmarkRunnerCore.Run(BenchmarkConverter.TypeToBenchmarks(typeof(CalDateTimePerfTests)), t => InProcessToolchain.Instance);
            //BenchmarkRunnerCore.Run(BenchmarkConverter.TypeToBenchmarks(typeof(SerializationPerfTests)), t => InProcessToolchain.Instance);
            //BenchmarkRunnerCore.Run(BenchmarkConverter.TypeToBenchmarks(typeof(ThroughputTests)), t => InProcessToolchain.Instance);
        }
    }
}
