``` ini

BenchmarkDotNet=v0.13.5, OS=Windows 11 (10.0.26100.6584)
Unknown processor
.NET SDK=9.0.304
  [Host]     : .NET 6.0.36 (6.0.3624.51421), X64 RyuJIT AVX2 DEBUG
  DefaultJob : .NET 6.0.36 (6.0.3624.51421), X64 RyuJIT AVX2


```
|                 Method |       Mean |   Error |  StdDev | Rank |     Gen0 |    Gen1 |  Allocated |
|----------------------- |-----------:|--------:|--------:|-----:|---------:|--------:|-----------:|
| SimpleCharacterParsing |   101.7 μs | 0.45 μs | 0.35 μs |    1 |  20.5078 |  0.1221 |  252.74 KB |
|        SequenceParsing |   185.4 μs | 1.72 μs | 1.60 μs |    3 |  39.3066 |  8.0566 |  483.27 KB |
|         PatternParsing |   388.9 μs | 4.34 μs | 4.06 μs |    5 |  98.1445 | 18.0664 | 1203.03 KB |
|       ModeBasedParsing |   155.1 μs | 1.61 μs | 1.43 μs |    2 |  31.0059 |  4.6387 |  381.04 KB |
|    TokenizationEnabled |   596.8 μs | 6.59 μs | 6.16 μs |    7 | 126.9531 | 30.2734 | 1562.02 KB |
|     ASTBuildingEnabled |   279.0 μs | 4.27 μs | 3.78 μs |    4 |  55.1758 |  9.7656 |  678.38 KB |
|      LargeInputParsing | 1,011.5 μs | 8.31 μs | 7.78 μs |    8 | 205.0781 | 13.6719 | 2520.32 KB |
|     FullFeatureParsing |   558.9 μs | 9.32 μs | 8.72 μs |    6 |  96.6797 | 23.4375 |  1195.9 KB |
