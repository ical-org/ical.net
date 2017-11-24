``` ini

BenchmarkDotNet=v0.10.10, OS=Windows 10 Redstone 2 [1703, Creators Update] (10.0.15063.726)
Processor=Intel Core i5-3210M CPU 2.50GHz (Ivy Bridge), ProcessorCount=4
Frequency=2435874 Hz, Resolution=410.5303 ns, Timer=TSC
  [Host] : .NET Framework 4.6.2 (CLR 4.0.30319.42000), 32bit LegacyJIT-v4.7.2115.0


```
|            Method |        Mean |      Error |     StdDev |
|------------------ |------------:|-----------:|-----------:|
|       Deserialize | 1,019.14 us | 24.9981 us | 24.5515 us |
| SerializeCalendar |    21.97 us |  0.4247 us |  0.6225 us |
