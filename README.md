# SearchComparisonNet

**SearchComparisonNet — C#/.NET app comparing search methods with benchmarks and visualizations.**

SearchComparisonNet is a compact .NET project that implements, compares, and benchmarks multiple search algorithms over representative datasets. It includes console runners and simple visualization/export helpers so you can reproduce experiments, measure performance with BenchmarkDotNet, and export results for analysis or teaching.

[![CI](https://github.com/Ramin-Developer/SearchComparisonNet/actions/workflows/ci.yml/badge.svg)](https://github.com/Ramin-Developer/SearchComparisonNet/actions/workflows/ci.yml)

---

## Table of Contents

- [Features](#features)
- [Project Structure](#project-structure)
- [Prerequisites](#prerequisites)
- [Build](#build)
- [Run](#run)
- [Benchmarks](#benchmarks)
- [Contributing](#contributing)
- [License](#license)

---

## Features

| Feature | Detail |
|---|---|
| **Multiple search algorithms** | Linear scan; binary search; indexed search; heuristic and hybrid approaches |
| **Benchmarking** | Uses BenchmarkDotNet for reproducible microbenchmarks |
| **Datasets** | Small synthetic sets and configurable real-world sample inputs |
| **Visualization** | Simple console and exportable CSV/JSON for external plotting |
| **Configurable runs** | Command-line flags for algorithm, dataset, size, and repeat counts |
| **Extensible** | Modular design to add new search strategies and metrics |

---

## Project Structure
SearchComparisonNet/
├── src/
│   ├── SearchComparisonNet.Core/     Core algorithms, interfaces, models
│   ├── SearchComparisonNet.Console/  Console runner and CLI
│   ├── SearchComparisonNet.Bench/    BenchmarkDotNet benchmarks
│   └── SearchComparisonNet.Utils/    Helpers: IO, parsing, export
├── tests/
│   ├── SearchComparisonNet.UnitTests
│   └── SearchComparisonNet.IntegrationTests
├── docs/                             Project notes and TODOs
├── .github/                          CI workflows and issue templates
├── README.md
└── LICENSE


---

## Prerequisites

| Requirement | Version |
|---|---|
| **.NET SDK** | **10.0** or later |
| **OS (GUI)** | Windows 10 / 11 (if using any Windows-specific visual helpers) |
| **OS (Console)** | Windows, Linux, or macOS |

---

## Build

Clone and build the solution:

```bash
git clone https://github.com/Ramin-Developer/SearchComparisonNet.git
cd SearchComparisonNet
dotnet build --configuration Release
