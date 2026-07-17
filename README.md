# SearchComparisonNet

**SearchComparisonNet — C#/.NET app comparing search methods with benchmarks and visualizations.**

SearchComparisonNet is a small .NET 10 application that compares the efficiency of **linear search** and **binary search** over a generated, sorted integer dataset. It runs many randomized lookups with each strategy and reports average iteration counts and elapsed time side by side. The project includes a WPF UI for driving simulations and inspecting results, a console runner for automated experiments, and BenchmarkDotNet benchmarks for reproducible performance measurements.

[![CI](https://github.com/Ramin-Developer/SearchComparisonNet/actions/workflows/ci.yml/badge.svg)](https://github.com/Ramin-Developer/SearchComparisonNet/actions/workflows/ci.yml)

---

## Table of Contents

- [What it does](#what-it-does)
- [Features](#features)
- [Solution layout](#solution-layout)
- [Tech stack](#tech-stack)
- [Prerequisites](#prerequisites)
- [Build, test, and run](#build-test-and-run)
- [Run examples](#run-examples)
- [Benchmarks](#benchmarks)
- [Contributing](#contributing)
- [License](#license)

---

## What it does

- Generates a sorted integer dataset of configurable size (Number of Entries).  
- Runs a configurable number of randomized searches (Number of Searches) using both **linear** and **binary** strategies over the same dataset.  
- Reports per-strategy averages (iteration count and elapsed time) so the two approaches can be compared directly.  
- Supports single on‑demand lookups for a specific target value, showing its index (or `-1` when not found).  
- Shows a compact preview of the generated collection (first, middle, and last values).  
- Provides export helpers (CSV/JSON) so results can be plotted or analyzed externally.

---

## Features

| Feature | Detail |
|---|---|
| **Algorithms** | Linear scan; binary search; pluggable strategy interface for adding new algorithms |
| **Benchmarking** | BenchmarkDotNet benchmarks for microbenchmarks and reproducible results |
| **Runners** | WPF GUI for interactive experiments; console runner for scripted runs |
| **Export** | CSV/JSON export of results for external visualization |
| **Configurable** | Dataset size, number of searches, seed, and algorithm selection via UI or CLI |
| **Tests** | Unit tests for algorithms and data generation; view‑model tests for GUI logic |

---

## Solution layout

SearchComparisonNet/
├── src/
│   ├── SearchComparisonNet.Kernel/     Core algorithms, data generation, models, interfaces
│   ├── SearchComparisonNet.GUI/        WPF MVVM front end (net10.0-windows)
│   ├── SearchComparisonNet.Console/    Console runner and CLI
│   └── SearchComparisonNet.Bench/      BenchmarkDotNet benchmarks
├── tests/
│   ├── SearchComparisonNet.Tests/      Unit tests for kernel
│   └── SearchComparisonNet.ViewModelTests/
├── docs/                               Project notes and TODOs
├── .github/                            CI workflows and issue templates
├── README.md
└── LICENSE


---

## Tech stack

- **.NET 10** (`net10.0` / `net10.0-windows`)  
- **WPF** (MVVM) for the GUI  
- **BenchmarkDotNet** for benchmarks  
- **xUnit** for unit tests  
- **CommunityToolkit.Mvvm**, **FluentValidation**, and **Microsoft.Extensions.DependencyInjection** in the GUI  
- Central package management via `Directory.Packages.props` and shared build settings in `Directory.Build.props`

---

## Prerequisites

| Requirement | Version |
|---|---|
| .NET SDK | **10.0** or later |
| OS (GUI) | Windows 10 / 11 (WPF requires `net10.0-windows`) |
| OS (Console) | Windows, Linux, or macOS |

---

## Build, test, and run

From the repository root:

```bash
# Restore and build the whole solution
dotnet build SearchComparisonNet.sln --configuration Release

# Run all tests
dotnet test SearchComparisonNet.sln --configuration Release


