``` ini

BenchmarkDotNet=v0.10.10, OS=Windows 10 Redstone 2 [1703, Creators Update] (10.0.15063.726)
Processor=Intel Core i5-3210M CPU 2.50GHz (Ivy Bridge), ProcessorCount=4
Frequency=2435874 Hz, Resolution=410.5303 ns, Timer=TSC
.NET Core SDK=2.0.3
  [Host] : .NET Core 2.0.3 (Framework 4.6.25815.02), 64bit RyuJIT


```
|                                Method |     Mean |     Error |    StdDev |   Median |
|-------------------------------------- |---------:|----------:|----------:|---------:|
| DeserializeAndComputeUntilOccurrences | 18.97 ms | 0.3692 ms | 0.4534 ms | 18.78 ms |
| DeserializeAndComputeCountOccurrences | 18.98 ms | 0.3711 ms | 0.5778 ms | 18.65 ms |
