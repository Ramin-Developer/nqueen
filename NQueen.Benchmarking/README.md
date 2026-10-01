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

## Manual validation context

Manual GUI/console timings are useful context but should not replace BenchmarkDotNet baselines. For N=20 Unique CountOnly, verify the displayed count against `ExpectedSolutionCounts.GetUnique(20)` (`4,878,666,808`) before using a manual timing as evidence.

Recent user-reported manual observation to recheck on the same machine: N=20 Unique CountOnly, elapsed `641.5 s`, memory `190 MB`.
