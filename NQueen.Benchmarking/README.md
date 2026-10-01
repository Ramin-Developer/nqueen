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
| `unique` | Unique Count-Only Fast Half-Board | 16 | 192.7 ms | 1.37 ms | 1.28 ms |
| `unique` | Unique Count-Only Fast Half-Board | 17 | 1,523.5 ms | 14.23 ms | 13.31 ms |
| `unique` | Unique Count-Only Fast Half-Board | 18 | 9,383.3 ms | 102.16 ms | 95.56 ms |
| `all` | All Parallel Count-Only Scaling | 16 | 141.9 ms | 0.84 ms | 0.74 ms |
| `all` | All Parallel Count-Only Scaling | 18 | 7,297.5 ms | 89.97 ms | 84.16 ms |
| `all` | Recursive Search | 16 | 150.6 ms | 1.04 ms | 0.93 ms |
| `all` | Iterative Search (production) | 16 | 144.8 ms | 0.81 ms | 0.72 ms |
| `all` | Recursive Search | 18 | 7,513.8 ms | 67.81 ms | 63.43 ms |
| `all` | Iterative Search (production) | 18 | 7,252.1 ms | 87.37 ms | 81.73 ms |

## Manual validation context

Manual GUI/console timings are useful context but should not replace BenchmarkDotNet baselines. For N=20 Unique CountOnly, verify the displayed count against `ExpectedSolutionCounts.GetUnique(20)` (`4,878,666,808`) before using a manual timing as evidence.

Recent user-reported manual observation to recheck on the same machine: N=20 Unique CountOnly, elapsed `641.5 s`, memory `190 MB`.
