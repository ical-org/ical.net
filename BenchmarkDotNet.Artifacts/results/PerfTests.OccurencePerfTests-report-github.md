``` ini

BenchmarkDotNet=v0.10.10, OS=Windows 10 Redstone 2 [1703, Creators Update] (10.0.15063.726)
Processor=Intel Core i5-3210M CPU 2.50GHz (Ivy Bridge), ProcessorCount=4
Frequency=2435874 Hz, Resolution=410.5303 ns, Timer=TSC
.NET Core SDK=2.0.3
  [Host] : .NET Core 2.0.3 (Framework 4.6.25815.02), 64bit RyuJIT


```
|                                                     Method |        Mean |     Error |    StdDev |
|----------------------------------------------------------- |------------:|----------:|----------:|
| MultipleEventsWithUntilOccurrencesSearchingByWholeCalendar |  1,483.1 us |  29.59 us |  37.43 us |
|                         MultipleEventsWithUntilOccurrences |  1,123.2 us |  21.72 us |  25.01 us |
|         MultipleEventsWithUntilOccurrencesEventsAsParallel |    891.1 us |  17.73 us |  18.20 us |
| MultipleEventsWithCountOccurrencesSearchingByWholeCalendar | 14,174.4 us | 276.96 us | 284.42 us |
|                         MultipleEventsWithCountOccurrences |  9,314.6 us | 180.01 us | 227.66 us |
|         MultipleEventsWithCountOccurrencesEventsAsParallel |  6,603.5 us | 130.28 us | 173.92 us |
