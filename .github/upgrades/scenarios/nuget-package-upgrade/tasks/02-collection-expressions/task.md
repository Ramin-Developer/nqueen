# 02-collection-expressions: Convert legacy collection initializers to collection expressions

Replace `new List<T> { ... }`, `new T[] { ... }` and `Array.Empty<T>()` with `[...]` where the target type is explicit (e.g. `ExpectedSolutionData.cs`, `BitmaskParallelEngine.Unique.cs`, test files).

**Done when**: No remaining convertible initializers; build clean, tests pass.

## Research
- Candidates: 6 test files (List/array/object[] initializers) + BitmaskParallelEngine.Unique.cs Array.Empty.
- ExpectedSolutionData.cs is a protected data file (DO NOT EDIT, pre-commit hook) - excluded.
