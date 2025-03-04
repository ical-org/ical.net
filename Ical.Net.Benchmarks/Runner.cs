//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains.InProcess.NoEmit;
using System;
using System.Diagnostics;
using System.IO;

namespace Ical.Net.Benchmarks;

public class Runner
{
    private static void Main(string[] args)
    {
        if (Debugger.IsAttached)
        {
            var applicationWorkflows = new ApplicationWorkflows();
            applicationWorkflows.SingleThreaded();

            var benchmark = new OccurencePerfTests();
            benchmark.Setup();
            benchmark.GetOccurrences();
        }
        else
        {
            var config = ManualConfig.Create(DefaultConfig.Instance)
                .WithOptions(ConfigOptions.DisableOptimizationsValidator)
                .AddJob(Job.Default.WithToolchain(InProcessNoEmitToolchain.Instance));

            Console.WriteLine("Running ApplicationWorkflows benchmarks...");
            BenchmarkRunner.Run<ApplicationWorkflows>(config);

            Console.WriteLine("Running OccurencePerfTests benchmarks...");
            BenchmarkRunner.Run<OccurencePerfTests>(config);

            Console.WriteLine("Running CalDateTimePerfTests benchmarks...");
            BenchmarkRunner.Run<CalDateTimePerfTests>(config);

            Console.WriteLine("Running SerializationPerfTests benchmarks...");
            BenchmarkRunner.Run<SerializationPerfTests>(config);

            Console.WriteLine("Running ThroughputTests benchmarks...");
            BenchmarkRunner.Run<ThroughputTests>(config);
        }
    }

    /// <summary>
    /// Searches the parent directories of <paramref name="startPath"/> for
    /// the first directory with name of <paramref name="directoryName"/>.
    /// </summary>
    /// <param name="directoryName">The name of the directory to search.</param>
    /// <param name="startPath">The path where the search starts.</param>
    /// <returns>The full path of the found folder.</returns>
    /// <exception cref="DirectoryNotFoundException">
    /// The <paramref name="startPath"/> does not exist, or the <paramref name="directoryName"/>
    /// was not found in the parent directories.
    /// </exception>
    /// <exception cref="System.UnauthorizedAccessException">The caller does not have the required permission.</exception>
    /// <exception cref="System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length.</exception>
    public static string FindParentFolder(string directoryName, string startPath)
    {
        if (!Directory.Exists(startPath))
        {
            throw new DirectoryNotFoundException($"Start path '{startPath}' not found.");
        }

        var currentPath = startPath;
        var rootReached = false;

        while (!rootReached && !Directory.Exists(Path.Combine(currentPath, directoryName)))
        {
            currentPath = Directory.GetParent(currentPath)?.FullName;
            rootReached = currentPath == null;
            currentPath ??= Directory.GetDirectoryRoot(startPath);
        }

        var resultPath = Path.Combine(currentPath, directoryName);
        if (!Directory.Exists(resultPath)) throw new DirectoryNotFoundException($"Folder '{directoryName}' not found in parent directories.");
        return resultPath;
    }
}
