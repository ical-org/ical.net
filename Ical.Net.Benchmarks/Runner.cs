//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using System.IO;

namespace Ical.Net.Benchmarks;

public class Runner
{
    private static void Main(string[] args)
    {
#if DEBUG
        BenchmarkSwitcher.FromAssembly(typeof(ApplicationWorkflows).Assembly).Run(args, new DebugInProcessConfig());
#else
            #region * ApplicationWorkflows results *
            /*
               // * Summary *
               
               BenchmarkDotNet v0.14.0, Windows 11 (10.0.22631.4317/23H2/2023Update/SunValley3)
               13th Gen Intel Core i7-13700K, 1 CPU, 24 logical and 16 physical cores
               .NET SDK 8.0.403
                 [Host]     : .NET 8.0.10 (8.0.1024.46610), X64 RyuJIT AVX2
                 DefaultJob : .NET 8.0.10 (8.0.1024.46610), X64 RyuJIT AVX2
               
               | Method                                                          | Mean     | Error    | StdDev   |
               |---------------------------------------------------------------- |---------:|---------:|---------:|
               | SingleThreaded                                                  | 18.20 ms | 0.184 ms | 0.163 ms |
               | ParallelUponDeserialize                                         | 17.56 ms | 0.350 ms | 0.739 ms |
               | ParallelUponGetOccurrences                                      | 26.39 ms | 0.294 ms | 0.275 ms |
               | ParallelDeserializeSequentialGatherEventsParallelGetOccurrences | 19.39 ms | 0.373 ms | 0.399 ms |
               
               // * Hints *
               Outliers
                 ApplicationWorkflows.SingleThreaded: Default             -> 1 outlier  was  removed (18.95 ms)
                 ApplicationWorkflows.ParallelUponGetOccurrences: Default -> 1 outlier  was  detected (25.79 ms)
               
               // * Legends *
                 Mean   : Arithmetic mean of all measurements
                 Error  : Half of 99.9% confidence interval
                 StdDev : Standard deviation of all measurements
                 1 ms   : 1 Millisecond (0.001 sec)
               
               // ***** BenchmarkRunner: End *****
             */
            #endregion
            BenchmarkRunner.Run(BenchmarkConverter.TypeToBenchmarks(typeof(ApplicationWorkflows)));

            #region * OccurencePerfTests results *
            /*
               // * Summary *
               
               BenchmarkDotNet v0.14.0, Windows 11 (10.0.22631.4317/23H2/2023Update/SunValley3)
               13th Gen Intel Core i7-13700K, 1 CPU, 24 logical and 16 physical cores
               .NET SDK 8.0.403
                 [Host]     : .NET 8.0.10 (8.0.1024.46610), X64 RyuJIT AVX2
                 DefaultJob : .NET 8.0.10 (8.0.1024.46610), X64 RyuJIT AVX2
               
               | Method                                                     | Mean       | Error    | StdDev   |
               |----------------------------------------------------------- |-----------:|---------:|---------:|
               | MultipleEventsWithUntilOccurrencesSearchingByWholeCalendar |   175.2 us |  1.87 us |  1.66 us |
               | MultipleEventsWithUntilOccurrences                         |   126.6 us |  1.43 us |  1.34 us |
               | MultipleEventsWithUntilOccurrencesEventsAsParallel         |         NA |       NA |       NA |
               | MultipleEventsWithCountOccurrencesSearchingByWholeCalendar | 1,672.7 us | 33.29 us | 31.14 us |
               | MultipleEventsWithCountOccurrences                         | 1,066.6 us | 20.83 us | 30.53 us |
               | MultipleEventsWithCountOccurrencesEventsAsParallel         |         NA |       NA |       NA |
               
               Benchmarks with issues (System.AggregateException: One or more errors occurred. (Operations that change non-concurrent collections must have exclusive access. A concurrent update was performed on this collection and corrupted its state. The collection's state is no longer correct.
                 OccurencePerfTests.MultipleEventsWithUntilOccurrencesEventsAsParallel: DefaultJob
                 OccurencePerfTests.MultipleEventsWithCountOccurrencesEventsAsParallel: DefaultJob
               
             */
            #endregion
            BenchmarkRunner.Run(BenchmarkConverter.TypeToBenchmarks(typeof(OccurencePerfTests)));

            #region * CalDateTimePerfTests results *
            /*
               // * Summary *
               
               BenchmarkDotNet v0.14.0, Windows 11 (10.0.22631.4317/23H2/2023Update/SunValley3)
               13th Gen Intel Core i7-13700K, 1 CPU, 24 logical and 16 physical cores
               .NET SDK 8.0.403
                 [Host]     : .NET 8.0.10 (8.0.1024.46610), X64 RyuJIT AVX2
                 DefaultJob : .NET 8.0.10 (8.0.1024.46610), X64 RyuJIT AVX2
               
               | Method                       | Mean      | Error    | StdDev   |
               |----------------------------- |----------:|---------:|---------:|
               | EmptyTzid                    |  97.35 ns | 0.700 ns | 0.620 ns |
               | SpecifiedTzid                | 219.31 ns | 4.406 ns | 5.729 ns |
               | UtcDateTime                  | 193.75 ns | 3.563 ns | 3.333 ns |
               | EmptyTzidToTzid              | 412.57 ns | 6.857 ns | 6.414 ns |
               | SpecifiedTzidToDifferentTzid | 494.44 ns | 8.299 ns | 7.763 ns |
               | UtcToDifferentTzid           | 437.86 ns | 3.880 ns | 3.630 ns |
               
               // * Hints *
               Outliers
                 CalDateTimePerfTests.EmptyTzid: Default -> 1 outlier  was  removed (101.43 ns)
             */
            #endregion
            BenchmarkRunner.Run(BenchmarkConverter.TypeToBenchmarks(typeof(CalDateTimePerfTests)));

            #region * SerializationPerfTests results *
            /*
               // * Summary *
               
               BenchmarkDotNet v0.14.0, Windows 11 (10.0.22631.4317/23H2/2023Update/SunValley3)
               13th Gen Intel Core i7-13700K, 1 CPU, 24 logical and 16 physical cores
               .NET SDK 8.0.403
                 [Host]     : .NET 8.0.10 (8.0.1024.46610), X64 RyuJIT AVX2
                 DefaultJob : .NET 8.0.10 (8.0.1024.46610), X64 RyuJIT AVX2
               
               
               | Method                     | Mean     | Error    | StdDev   |
               |--------------------------- |---------:|---------:|---------:|
               | Deserialize                | 53.40 us | 0.456 us | 0.404 us |
               | BenchmarkSerializeCalendar | 13.76 us | 0.266 us | 0.285 us |
               
               // * Hints *
               Outliers
                 SerializationPerfTests.Deserialize: Default -> 1 outlier  was  removed (54.96 us)
               
               // * Legends *
                 Mean   : Arithmetic mean of all measurements
                 Error  : Half of 99.9% confidence interval
                 StdDev : Standard deviation of all measurements
                 1 us   : 1 Microsecond (0.000001 sec)
               
               // ***** BenchmarkRunner: End *****
             */
            #endregion
            BenchmarkRunner.Run(BenchmarkConverter.TypeToBenchmarks(typeof(SerializationPerfTests)));

            #region * ThroughputTests results *
            /*
               // * Summary *
               
               BenchmarkDotNet v0.14.0, Windows 11 (10.0.22631.4317/23H2/2023Update/SunValley3)
               13th Gen Intel Core i7-13700K, 1 CPU, 24 logical and 16 physical cores
               .NET SDK 8.0.403
                 [Host]     : .NET 8.0.10 (8.0.1024.46610), X64 RyuJIT AVX2
                 DefaultJob : .NET 8.0.10 (8.0.1024.46610), X64 RyuJIT AVX2
               
               
               | Method                                | Mean     | Error     | StdDev    |
               |-------------------------------------- |---------:|----------:|----------:|
               | DeserializeAndComputeUntilOccurrences | 2.178 ms | 0.0410 ms | 0.0403 ms |
               | DeserializeAndComputeCountOccurrences | 2.116 ms | 0.0277 ms | 0.0245 ms |
               
               // * Hints *
               Outliers
                 ThroughputTests.DeserializeAndComputeCountOccurrences: Default -> 1 outlier  was  removed (2.20 ms)
               
               // * Legends *
                 Mean   : Arithmetic mean of all measurements
                 Error  : Half of 99.9% confidence interval
                 StdDev : Standard deviation of all measurements
                 1 ms   : 1 Millisecond (0.001 sec)
               
               // ***** BenchmarkRunner: End *****
             */
            #endregion
            BenchmarkRunner.Run(BenchmarkConverter.TypeToBenchmarks(typeof(ThroughputTests)));
#endif
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