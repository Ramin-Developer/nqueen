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

## .NET 10 vs .NET 11 CountOnly comparison (2026-11-22)

Release builds of the same solver source at commit `031e087` were measured on the i7-14700K desktop with SDK 11.0.100 and BenchmarkDotNet 0.16.0-preview.2. The temporary comparison projects targeted .NET 10.0.15 / C# 14 and .NET 11.0.0 / C# 15, respectively; package versions and solver source were otherwise the same. Each case used one process launch, three warmups and 15 measured iterations. BenchmarkDotNet may exclude upper outliers.

| N | Mode | .NET 10 mean (ms) | .NET 11 mean (ms) | .NET 11 change |
|---:|---|---:|---:|---:|
| 16 | Unique | 61.25 | 60.49 | 1.2% faster |
| 16 | All | 60.57 | 60.84 | 0.4% slower |
| 17 | Unique | 415.84 | 413.32 | 0.6% faster |
| 17 | All | 417.68 | 405.03 | 3.0% faster |
| 18 | Unique | 3,000.05 | 2,995.45 | 0.2% faster |
| 18 | All | 3,017.01 | 3,003.63 | 0.4% faster |

Both modes used `BitmaskSolver` in Hide mode with parallel CountOnly, half-board restriction, adaptive depth and root split depth 3. Unique enabled prefix and partial-reflection pruning; All disabled both. Events were disabled. Global setup checked each result against the expected solution count. The .NET 10 cases ran before the .NET 11 cases; these single-launch results are exploratory and do not establish a general speedup.

### N=17 All repeat

A second comparison used the same setup, three process launches per run, three warmups and 15 measured iterations per launch. Runs were ordered .NET 11, .NET 10, .NET 10, .NET 11. Each launch verified the expected count of 95,815,104.

| Run | .NET 10 mean (ms) | .NET 11 mean (ms) |
|---|---:|---:|
| First | 419.3 | 415.2 |
| Second | 419.8 | 413.1 |

The average of the two run means is about 419.6 ms on .NET 10 and 414.2 ms on .NET 11, a roughly 1.3% difference rather than the initial 3%. These sequential blocks and their process-level variation limit causal and statistical conclusions. Results apply only to this machine and workload. The raw local reports and temporary harness are under the ignored `BenchmarkDotNet.Artifacts/runtime-comparison` directory and may be removed during cleanup.

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
