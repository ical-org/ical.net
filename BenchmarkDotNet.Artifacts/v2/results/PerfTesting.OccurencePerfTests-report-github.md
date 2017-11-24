``` ini

BenchmarkDotNet=v0.10.10, OS=Windows 10 Redstone 2 [1703, Creators Update] (10.0.15063.726)
Processor=Intel Core i5-3210M CPU 2.50GHz (Ivy Bridge), ProcessorCount=4
Frequency=2435874 Hz, Resolution=410.5303 ns, Timer=TSC
  [Host] : .NET Framework 4.6.2 (CLR 4.0.30319.42000), 32bit LegacyJIT-v4.7.2115.0


```
|                                                     Method |      Mean |     Error |    StdDev |
|----------------------------------------------------------- |----------:|----------:|----------:|
| MultipleEventsWithUntilOccurrencesSearchingByWholeCalendar |  2.225 ms | 0.0438 ms | 0.0732 ms |
|                         MultipleEventsWithUntilOccurrences |  1.625 ms | 0.0324 ms | 0.0360 ms |
|         MultipleEventsWithUntilOccurrencesEventsAsParallel |  1.210 ms | 0.0248 ms | 0.0295 ms |
| MultipleEventsWithCountOccurrencesSearchingByWholeCalendar | 19.753 ms | 0.3860 ms | 0.5283 ms |
|                         MultipleEventsWithCountOccurrences | 14.173 ms | 0.2756 ms | 0.3486 ms |
|         MultipleEventsWithCountOccurrencesEventsAsParallel |  9.230 ms | 0.0966 ms | 0.0806 ms |
