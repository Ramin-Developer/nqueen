# NuGet Package Upgrade Plan

## Overview

**Target**: Bump 3 centrally managed packages to latest stable, then apply safe C# 14 modernizations.
**Scope**: NQueen.slnx, 9 projects; no breaking API changes found.

## Tasks

### 01-upgrade-packages: Upgrade Microsoft.Extensions and BenchmarkDotNetDiagnosers packages

Update `Directory.Packages.props`: Microsoft.Extensions.DependencyInjection and Microsoft.Extensions.Hosting 10.0.13 → 10.0.14; Microsoft.VisualStudio.DiagnosticsHub.BenchmarkDotNetDiagnosers 18.7.37220.1 → 18.12.37802.2. API diffs (`apidiff/*.apidiff.md`) show no breaking changes.

**Done when**: Versions updated, solution restores and builds with no errors or warnings, all tests pass.

---

### 02-collection-expressions: Convert legacy collection initializers to collection expressions

Replace `new List<T> { ... }`, `new T[] { ... }` and `Array.Empty<T>()` with `[...]` where the target type is explicit (e.g. `ExpectedSolutionData.cs`, `BitmaskParallelEngine.Unique.cs`, test files).

**Done when**: No remaining convertible initializers; build clean, tests pass.

---

### 03-guard-clauses-and-lock: Modernize null guards and synchronization primitive

Use `ArgumentNullException.ThrowIfNull` for constructor/method null checks (MainViewModel, MainWindow, LazyPositionList, SynchronousProgress, BitmaskSolver). Switch `BitmaskSolver._sync` to `System.Threading.Lock` if it is a plain `object`.

**Done when**: Guards modernized without behavior change; build clean, tests pass.

---

### 04-primary-ctors-and-field-keyword: Apply primary constructors and C# 14 field keyword

Convert trivial constructor-assigned fields to primary constructors (e.g. `Solution`, test helpers) and replace backing fields with the `field` keyword where a property only validates/assigns (e.g. `BitmaskSolver.DelayInMillisec`). Skip WPF code-behind where unsafe.

**Done when**: Conversions applied where trivial; build clean, tests pass.

---

### 05-seal-classes: Seal non-inherited classes

Mark leaf classes `sealed` (GUI converters, dispatchers, services, leaf ViewModels, test mocks, benchmark classes) after verifying no subclasses or mocking requirements.

**Done when**: Leaf classes sealed; build clean, tests pass; CHANGELOG updated under [Unreleased].
