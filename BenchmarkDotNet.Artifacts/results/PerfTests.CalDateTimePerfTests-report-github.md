``` ini

BenchmarkDotNet=v0.10.10, OS=Windows 10 Redstone 2 [1703, Creators Update] (10.0.15063.726)
Processor=Intel Core i5-3210M CPU 2.50GHz (Ivy Bridge), ProcessorCount=4
Frequency=2435874 Hz, Resolution=410.5303 ns, Timer=TSC
.NET Core SDK=2.0.3
  [Host] : .NET Core 2.0.3 (Framework 4.6.25815.02), 64bit RyuJIT


```
|                       Method |       Mean |    Error |    StdDev |     Median |
|----------------------------- |-----------:|---------:|----------:|-----------:|
|                    EmptyTzid |   719.4 ns | 14.03 ns |  13.78 ns |   711.3 ns |
|                SpecifiedTzid | 1,828.2 ns | 38.02 ns |  48.08 ns | 1,802.2 ns |
|                  UtcDateTime | 1,435.8 ns | 28.07 ns |  40.26 ns | 1,408.9 ns |
|              EmptyTzidToTzid | 4,968.9 ns | 98.92 ns | 125.10 ns | 4,903.4 ns |
| SpecifiedTzidToDifferentTzid | 4,892.1 ns | 96.39 ns | 135.13 ns | 4,815.4 ns |
|           UtcToDifferentTzid | 4,229.6 ns | 84.41 ns | 128.91 ns | 4,173.1 ns |
