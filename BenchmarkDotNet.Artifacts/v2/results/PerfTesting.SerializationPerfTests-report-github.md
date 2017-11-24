``` ini

BenchmarkDotNet=v0.10.10, OS=Windows 10 Redstone 2 [1703, Creators Update] (10.0.15063.726)
Processor=Intel Core i5-3210M CPU 2.50GHz (Ivy Bridge), ProcessorCount=4
Frequency=2435874 Hz, Resolution=410.5303 ns, Timer=TSC
  [Host] : .NET Framework 4.6.2 (CLR 4.0.30319.42000), 32bit LegacyJIT-v4.7.2115.0


```
|            Method |        Mean |      Error |     StdDev |
|------------------ |------------:|-----------:|-----------:|
|       Deserialize | 1,054.21 us | 23.7054 us | 40.2535 us |
| SerializeCalendar |    22.43 us |  0.4281 us |  0.5258 us |
