# N-Queen

**N‑Queen — .NET N‑Queens solver, visualizer, and benchmarking tool.**

A combined **Console** and **WPF desktop** application for solving the N‑Queens problem, implemented in C# 14 / .NET 10. The solver uses a symmetry‑pruning backtracking algorithm with parallel execution and bitmask state representation, and can enumerate solutions up to **N = 25**.

[![CI](https://github.com/Ramin-Developer/N-Queen/actions/workflows/ci.yml/badge.svg)](https://github.com/Ramin-Developer/N-Queen/actions/workflows/ci.yml)

---

## Table of Contents

- [Features](#features)
- [Algorithm](#algorithm)
- [Project Structure](#project-structure)
- [Prerequisites](#prerequisites)
- [Build](#build)
- [Run — Console](#run--console)
- [Run — WPF GUI](#run--wpf-gui)
- [Solver Options](#solver-options)
- [Known Solution Counts](#known-solution-counts)
- [Benchmark Results](#benchmark-results)
- [Contributing](#contributing)
- [License](#license)

---

## Features

| Feature | Detail |
|---|---|
| **Three solution modes** | **All** — every distinct placement; **Unique** — canonical (up to rotation/reflection); **Single** — one solution |
| **Two output modes** | **Materialize** — up to 5 sample solutions displayed; **Count‑only** — exact count, no storage |
| **Bitmask DFS** | 64‑bit column / diagonal masks; `TZCNT` intrinsic for candidate iteration |
| **Symmetry pruning** | Prefix‑minimality pruning + partial‑reflection pruning reduce the search space |
| **Half‑board restriction** | Vertical‑symmetry shortcut for All mode (N ≥ 15) improves throughput |
| **Parallel execution** | `Parallel.ForEach` with adaptive root‑split depth; scales across available cores |
| **Precomputed lookup** | Exact counts for N = 1–29 (OEIS A000170 / A002562); N ≥ 21 returns instantly |
| **WPF GUI** | Animated step‑by‑step visualization, save results to file, MVVM + CommunityToolkit |
| **Interactive CLI** | Menu‑driven mode or fully non‑interactive via flags |

---

## Algorithm

The core algorithm is a **bitmask backtracking DFS**:

1. Represent occupied columns, forward‑diagonals, and back‑diagonals as three `ulong` masks.  
2. At each column, compute `available = ~(cols | d1 | d2) & fullMask` to obtain candidate rows.  
3. Iterate candidates with `bit = avail & -avail` (lowest set bit) and advance via `TZCNT`.  
4. **Symmetry pruning** — at depths ≥ `pruneDepthGate`, compare the partial prefix to its canonical reflection/rotation and prune subtrees that cannot yield canonical solutions.  
5. **Half‑board** — for All mode, restrict the first‑column queen to rows `0 … ⌊N/2⌋` and double the count to exploit vertical symmetry.  
6. **Parallelism** — partition the root row loop across cores via `Partitioner.Create`; each thread runs an independent DFS with thread‑local state.  
7. **Count‑only path** — for Unique mode at larger N, a half‑board parallel DFS avoids materializing solutions.

---

## Project Structure

