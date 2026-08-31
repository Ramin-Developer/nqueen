# NQueen

**NQueen — high-performance N-Queens solver with WPF visualisation, console runner, and BenchmarkDotNet benchmarks.**

[![CI](https://github.com/Ramin-Developer/NQueen/actions/workflows/ci.yml/badge.svg)](https://github.com/Ramin-Developer/NQueen/actions/workflows/ci.yml)
[![codecov](https://codecov.io/gh/Ramin-Developer/NQueen/branch/main/graph/badge.svg)](https://codecov.io/gh/Ramin-Developer/NQueen)

---

## Table of Contents

- [What it does](#what-it-does)
- [Features](#features)
- [Solution layout](#solution-layout)
- [Tech stack](#tech-stack)
- [Prerequisites](#prerequisites)
- [Build, test, and run](#build-test-and-run)
- [Benchmarks](#benchmarks)
- [Contributing](#contributing)
- [License](#license)

---

## What it does

Solves the N-Queens problem — placing N non-attacking queens on an N×N chessboard — across three modes:

| Mode | Description |
|---|---|
| **All** | Enumerate every distinct solution (or just count them) |
| **Unique** | Enumerate symmetry-reduced canonical solutions (or just count them) |
| **Single** | Find one valid placement quickly |

Results are streamed in real time to both a WPF chessboard visualisation and a solution-list panel. Progress reporting and cancellation are fully supported.

---

## Features

| Feature | Detail |
|---|---|
| **Bitmask solver** | `BitmaskSolver` — the single production solver, split across partial-class files for clarity |
| **Bitboard count** | `BitboardNQueenSolver` — static pure-count utility with half-board symmetry reduction and parallel partitioning |
| **Symmetry reduction** | Unique mode prunes reflections and rotations via a canonical prefix gate |
| **Parallelism** | Chunk-of-1 dynamic partitioner over depth-2 work items; tuned for N ≥ 16 |
| **WPF UI** | MVVM front end — animated queen placement, solution list, board-size selector, progress bar |
| **Console runner** | Headless runner for scripted experiments; comparable hide-mode paths share the same solver configurator as the GUI |
| **Benchmarks** | BenchmarkDotNet suite (`FrontEndInvocationPathBenchmark`, `UniqueFastHalfBoardEvenOddBenchmark`, …) |
| **Tests** | 667 passing tests across unit, view-model, and shared test infrastructure |

---

## Solution layout

```
NQueen/
├── NQueen.Domain/          Interfaces, models, enums, context records, settings, utilities
├── NQueen.Kernel/          Solver implementations (BitmaskSolver, BitboardNQueenSolver) + DI extensions
├── NQueen.Shared/          Cross-cutting helpers (parsing, numerics)
├── NQueen.GUI/             WPF MVVM front end (net10.0-windows)
├── NQueen.Console/         Console runner
├── NQueen.UnitTests/       Kernel / domain unit tests
├── NQueen.ViewModelTests/  ViewModel-level integration tests
├── NQueen.TestShared/      Shared test infrastructure
├── NQueen.Benchmarking/    BenchmarkDotNet benchmarks
├── docs/                   ROADMAP.md, GUI audit, benchmark artefacts
├── .github/workflows/      CI (build-test fast gate + non-blocking coverage report)
├── README.md
└── LICENSE
```

---

## Tech stack

- **.NET 10** (`net10.0` / `net10.0-windows` for the GUI)
- **WPF** (MVVM via `CommunityToolkit.Mvvm`) for the GUI
- **BenchmarkDotNet** for reproducible microbenchmarks
- **xUnit** + **Shouldly** for unit and view-model tests
- **FluentValidation** for input validation
- **Microsoft.Extensions.DependencyInjection** for DI composition
- Central package management via `Directory.Packages.props`; shared build settings in `Directory.Build.props`

---

## Prerequisites

| Requirement | Version |
|---|---|
| .NET SDK | **10.0** or later |
| OS (GUI) | Windows 10 / 11 (`net10.0-windows`) |
| OS (Console / tests) | Windows, Linux, or macOS |

---

## Build, test, and run

From the repository root:

```bash
# Restore and build the whole solution
dotnet build --configuration Release

# Run all tests (fast subset — skips heavy enumeration tests)
dotnet test --configuration Release --filter "Category!=Slow"

# Run every test including slow enumeration tests
dotnet test --configuration Release

# Run the console runner
dotnet run --project NQueen.Console --configuration Release

# Non-interactive examples
dotnet run --project NQueen.Console --configuration Release -- --mode unique --size 19 --count-only
dotnet run --project NQueen.Console --configuration Release -- --mode all --size 15 --count-only
```

For comparable non-visual runs, the GUI and Console both use the shared
`BitmaskSolverRunConfigurator` in `NQueen.Kernel`. That keeps parallelism, split depth,
pruning, count-only/materialize storage, and All-mode half-board rules aligned. The Console
always runs with `DisplayMode.Hide`; GUI-only visualization paths remain separate by design.
For `All + Hide + N >= 15`, half-board restriction is enabled automatically through the shared
configuration.

---

## Benchmarks

```bash
cd NQueen.Benchmarking
dotnet run -c Release

# Focus the GUI-vs-Console path comparison
dotnet run -c Release -- --filter "*FrontEndInvocationPathBenchmark*"
```

Key benchmark classes: `FrontEndInvocationPathBenchmark`, `UniqueFastHalfBoardEvenOddBenchmark`,
`AllCountOnlyRecursiveVsIterativeBenchmark`.
