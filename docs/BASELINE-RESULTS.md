# Baseline Simulation Results

Reference timings for comparing later optimizations and .NET upgrades.
Source: `Elapsed Times - Max N = 27.xlsm` (user's desktop run, saved 2026-11-08).

| Item | Value |
|---|---|
| Machine | Intel i7-14700K (20 cores / 28 threads), desktop |
| Runtime | .NET 10 (SDK 10.0.403), C# 14 |
| Solver | `BitmaskSolver`, symmetry-class (Takaken) counter, CountOnly |
| Not recorded | Front end (GUI/Console), build configuration, exact runtime patch |

## Elapsed time and memory (N = 16..20)

| N | Unique (s) | All (s) | Unique max memory (MB) | All max memory (MB) |
|---:|---:|---:|---:|---:|
| 16 | 0.1 | 0.1 | 160 | 160 |
| 17 | 0.1 | 0.4 | 160 | 160 |
| 18 | 3.0 | 3.1 | 160 | 170 |
| 19 | 23.8 | 24.1 | 160 | 170 |
| 20 | 193.4 | 193.4 | 160 | 170 |

Single mode: 0 s (below display resolution) for N=1..27. N <= 15 Unique/All: 0 s (below display resolution).
Memory is the process working set rounded to 10 MB (`NumericUtility.UpdateMemoryUsage`).

## Reference counts (OEIS A000170 / A002562)

N=27 is the largest N for which both counts are known exactly (Q27 project, 2016).

| N | Unique | All |
|---:|---:|---:|
| 21 | 39,333,324,973 | 314,666,222,712 |
| 22 | 336,376,244,042 | 2,691,008,701,644 |
| 23 | 3,029,242,658,210 | 24,233,937,684,440 |
| 24 | 28,439,272,956,934 | 227,514,171,973,736 |
| 25 | 275,986,683,743,434 | 2,207,893,435,808,352 |
| 26 | 2,789,712,466,510,289 | 22,317,699,616,364,044 |
| 27 | 29,363,495,934,315,694 | 234,907,967,154,122,528 |

N <= 20 counts match `ExpectedSolutionCounts`. The workbook stores N=26/27 as floating point; exact values above.

## How to compare

Re-run the same N/mode with CountOnly on an idle machine and add a column (date, runtime, change) here.
