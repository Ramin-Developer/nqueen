# NQueen.Benchmarking

BenchmarkDotNet harness for solver performance investigations. Run benchmarks from the repository root in Release configuration.

## Profiles

`Program.cs` supports named local profiles through either a positional argument or the `BENCHMARK_PROFILE` environment variable:

| Profile | Benchmarks | Purpose |
|---|---|---|
| `canonical` | `UniqueFastHalfBoardEvenOddBenchmark`, `AllCountOnlyParallelScalingBenchmark`, `AllCountOnlyRecursiveVsIterativeBenchmark`, `FrontEndInvocationPathBenchmark` | Default evidence set before solver-performance work |
| `unique` | `UniqueFastHalfBoardEvenOddBenchmark` | Unique CountOnly half-board baseline at N=16/17/18 |
| `all` | `AllCountOnlyParallelScalingBenchmark`, `AllCountOnlyRecursiveVsIterativeBenchmark` | All CountOnly parallel and iterative-vs-recursive baselines |
| `frontend` | `FrontEndInvocationPathBenchmark` | GUI-style vs console-style invocation comparison |
| `console` | `ConsolePruningImpactAllBenchmark`, `ConsolePruningImpactUniqueBenchmark` | Historical console pruning comparison |

## Commands

```powershell
# Default canonical profile
dotnet run -c Release --project NQueen.Benchmarking

# A focused profile
dotnet run -c Release --project NQueen.Benchmarking -- unique
dotnet run -c Release --project NQueen.Benchmarking -- all
dotnet run -c Release --project NQueen.Benchmarking -- frontend

# Equivalent environment-variable form
$env:BENCHMARK_PROFILE = "canonical"
dotnet run -c Release --project NQueen.Benchmarking
Remove-Item Env:BENCHMARK_PROFILE

# BenchmarkDotNet filter/list/export arguments still go through BenchmarkSwitcher
dotnet run -c Release --project NQueen.Benchmarking -- --list flat
dotnet run -c Release --project NQueen.Benchmarking -- --filter *UniqueFastHalfBoardEvenOddBenchmark*
```

Set `BENCHMARK_MODE=1` to force BenchmarkDotNet assembly-switcher behavior even without `--` arguments.

## Fresh local baseline

Measured on 2026-10-01 with BenchmarkDotNet 0.15.8, Windows 11 10.0.26300.9550, Intel Core i7-14700K, .NET SDK 10.0.401, and .NET 10.0.12 runtime.

| Profile | Benchmark | BoardSize | Mean | Error | StdDev |
|---|---|---:|---:|---:|---:|
| `unique` | Unique Count-Only Fast Half-Board | 16 | 183.3 ms | 0.86 ms | 0.80 ms |
| `unique` | Unique Count-Only Fast Half-Board | 17 | 1,454.0 ms | 6.29 ms | 5.88 ms |
| `unique` | Unique Count-Only Fast Half-Board | 18 | 9,093.3 ms | 56.54 ms | 52.88 ms |
| `all` | All Parallel Count-Only Scaling | 16 | 139.6 ms | 0.43 ms | 0.36 ms |
| `all` | All Parallel Count-Only Scaling | 18 | 7,056.0 ms | 29.72 ms | 26.35 ms |
| `all` | Recursive Search | 16 | 145.7 ms | 0.72 ms | 0.67 ms |
| `all` | Iterative Search (production) | 16 | 140.4 ms | 0.71 ms | 0.66 ms |
| `all` | Recursive Search | 18 | 7,408.6 ms | 45.16 ms | 42.25 ms |
| `all` | Iterative Search (production) | 18 | 7,126.4 ms | 23.87 ms | 21.16 ms |

## Manual validation context

Manual GUI/console timings are useful context but should not replace BenchmarkDotNet baselines. Keep N=20 Unique CountOnly available as a real simulation path; verify the displayed count against `ExpectedSolutionCounts.GetUnique(20)` (`4,878,666,808`) before using a manual timing as evidence.

Recent user-reported manual observation to recheck on the same machine: N=20 Unique CountOnly, elapsed `641.5 s`, memory `190 MB`.

Manual command-line validation path:

```powershell
dotnet run -c Release --project NQueen.Console -- --mode unique --size 20 --count-only
```

Only use the elapsed time as performance evidence if the simulation completes and the output count is `4,878,666,808`.

## Optimization comparison notes

On `perf/solver-optimization-pass`, a short local comparison (`--warmupCount 1 --iterationCount 3`) of
`UniqueFastHalfBoardEvenOddBenchmark` measured the incremental-reflection/no-reflection DFS split against
the same-session baseline:

| BoardSize | Baseline mean | Optimized mean | Delta |
|---:|---:|---:|---:|
| 16 | 186.2 ms | 177.9 ms | -4.5% |
| 17 | 1,457.2 ms | 1,351.4 ms | -7.3% |
| 18 | 8,866.6 ms | 8,507.7 ms | -4.0% |

Manual N=20 Unique CountOnly validation on the same branch also improved from approximately
`575 s` to approximately `556 s`. Treat that as manual simulation evidence, not a replacement for
the focused BenchmarkDotNet comparison above.

Treat these as focused PR evidence; keep the full `unique` profile available for final release-grade confirmation.

## Console vs GUI invocation parity (2026-10-03, `perf/console-gui-parity`)

`frontend` profile, N = 8/12/14/16, all mode x storage scenarios, ShortRun + MemoryDiagnoser.
Both front ends use `BitmaskSolverRunConfigurator` -> `BitmaskSolver.Solve`; GUI-style adds
`GetSimResultsAsync` (Task.Run) plus progress/solution sinks.

| N | Scenario | Console (direct Solve) | GUI-style (async + sinks) |
|---|---|---|---|
| 8 | Single / Unique Mat / All Mat | 0.17 / 1.6 / 8.1 us | 3.0 / 6.1 / 14.7 us |
| 12 | Unique CountOnly / All CountOnly | 201 / 183 us | 205 / 194 us |
| 14 | All Mat / All CountOnly | 1.78 / 1.76 ms | 1.81 / 1.84 ms |
| 16 | Unique Mat / Unique CountOnly | 61.0 / 60.2 ms | 60.7 / 60.6 ms |
| 16 | All Mat / All CountOnly | 60.4 / 61.3 ms | 61.3 / 61.2 ms |

Findings: identical kernel path and allocations (Alloc Ratio 1.00-1.01, e.g. ~205 KB at N=16).
GUI-style adds a fixed ~3-13 us (thread-pool hop + sinks), visible only for sub-millisecond runs
(Single, N<=8) and within noise (<=5%) from N=12. App-reported memory (`Process.WorkingSet64`,
rounded to 10 MB) differs by host process (WPF vs console), not by solver path.
