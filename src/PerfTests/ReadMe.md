# Expected Test Results

BenchmarkDotNet=v0.12.0, OS=Windows 10.0.19044
Intel Core i7-10610U CPU 1.80GHz, 1 CPU, 8 logical and 4 physical cores
.NET Core SDK=7.0.100
  [Host]     : .NET Core 3.1.31 (CoreCLR 4.700.22.51102, CoreFX 4.700.22.51303), X64 RyuJIT  [AttachedDebugger]
  DefaultJob : .NET Core 3.1.31 (CoreCLR 4.700.22.51102, CoreFX 4.700.22.51303), X64 RyuJIT


|                                                          Method |     Mean |    Error |   StdDev |   Median |
|---------------------------------------------------------------- |---------:|---------:|---------:|---------:|
|                                                  SingleThreaded | 70.48 ms | 1.472 ms | 3.800 ms | 69.72 ms |
|                                         ParallelUponDeserialize | 67.22 ms | 1.543 ms | 2.310 ms | 66.75 ms |
|                                      ParallelUponGetOccurrences | 68.30 ms | 2.128 ms | 6.003 ms | 66.59 ms |
| ParallelDeserializeSequentialGatherEventsParallelGetOccurrences | 68.87 ms | 1.511 ms | 4.186 ms | 68.19 ms |

|                       Method |       Mean |     Error |    StdDev |     Median |
|----------------------------- |-----------:|----------:|----------:|-----------:|
|                    EmptyTzid |   599.1 ns |  10.11 ns |   8.44 ns |   597.1 ns |
|                SpecifiedTzid | 1,235.1 ns |  24.34 ns |  29.89 ns | 1,228.5 ns |
|                  UtcDateTime |   958.9 ns |  18.83 ns |  25.13 ns |   963.7 ns |
|              EmptyTzidToTzid | 2,479.6 ns |  43.99 ns |  39.00 ns | 2,489.7 ns |
| SpecifiedTzidToDifferentTzid | 2,822.4 ns |  89.15 ns | 249.98 ns | 2,749.9 ns |
|           UtcToDifferentTzid | 2,726.5 ns | 110.13 ns | 312.42 ns | 2,678.9 ns |

|                                                     Method |       Mean |     Error |     StdDev |     Median |
|----------------------------------------------------------- |-----------:|----------:|-----------:|-----------:|
| MultipleEventsWithUntilOccurrencesSearchingByWholeCalendar | 1,006.8 us |  60.48 us |   174.5 us |   956.8 us |
|                         MultipleEventsWithUntilOccurrences |   861.4 us |  59.69 us |   174.1 us |   809.8 us |
|         MultipleEventsWithUntilOccurrencesEventsAsParallel |         NA |        NA |         NA |         NA |
| MultipleEventsWithCountOccurrencesSearchingByWholeCalendar | 9,631.7 us | 626.00 us | 1,806.2 us | 9,082.5 us |
|                         MultipleEventsWithCountOccurrences | 5,736.6 us | 290.15 us |   837.1 us | 5,513.1 us |
|         MultipleEventsWithCountOccurrencesEventsAsParallel |         NA |        NA |         NA |         NA |


|            Method |      Mean |     Error |    StdDev |     Median |
|------------------ |----------:|----------:|----------:|-----------:|
|       Deserialize | 299.60 us | 13.559 us | 38.245 us | 290.744 us |
| SerializeCalendar |  10.41 us |  0.604 us |  1.770 us |   9.623 us |

|                                Method |     Mean |    Error |   StdDev |
|-------------------------------------- |---------:|---------:|---------:|
| DeserializeAndComputeUntilOccurrences | 12.30 ms | 0.421 ms | 1.193 ms |
| DeserializeAndComputeCountOccurrences | 11.92 ms | 0.471 ms | 1.353 ms |
