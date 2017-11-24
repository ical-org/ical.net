``` ini

BenchmarkDotNet=v0.10.10, OS=Windows 10 Redstone 2 [1703, Creators Update] (10.0.15063.726)
Processor=Intel Core i5-3210M CPU 2.50GHz (Ivy Bridge), ProcessorCount=4
Frequency=2435874 Hz, Resolution=410.5303 ns, Timer=TSC
  [Host] : .NET Framework 4.6.2 (CLR 4.0.30319.42000), 32bit LegacyJIT-v4.7.2115.0


```
|                       Method |       Mean |    Error |    StdDev |
|----------------------------- |-----------:|---------:|----------:|
|                    EmptyTzid | 1,400.0 ns | 27.41 ns |  25.64 ns |
|                SpecifiedTzid | 2,043.0 ns | 40.73 ns |  60.97 ns |
|                  UtcDateTime |   961.3 ns | 19.29 ns |  27.04 ns |
|              EmptyTzidToTzid | 4,002.6 ns | 78.20 ns | 114.62 ns |
| SpecifiedTzidToDifferentTzid | 5,007.1 ns | 97.24 ns | 139.46 ns |
|           UtcToDifferentTzid | 3,654.3 ns | 71.20 ns |  84.76 ns |
