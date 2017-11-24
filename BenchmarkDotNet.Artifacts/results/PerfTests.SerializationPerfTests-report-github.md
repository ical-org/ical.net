``` ini

BenchmarkDotNet=v0.10.10, OS=Windows 10 Redstone 2 [1703, Creators Update] (10.0.15063.726)
Processor=Intel Core i5-3210M CPU 2.50GHz (Ivy Bridge), ProcessorCount=4
Frequency=2435874 Hz, Resolution=410.5303 ns, Timer=TSC
.NET Core SDK=2.0.3
  [Host] : .NET Core 2.0.3 (Framework 4.6.25815.02), 64bit RyuJIT


```
|            Method |      Mean |      Error |     StdDev |
|------------------ |----------:|-----------:|-----------:|
|       Deserialize | 639.00 us | 12.6489 us | 15.9968 us |
| SerializeCalendar |  21.53 us |  0.4269 us |  0.5082 us |
