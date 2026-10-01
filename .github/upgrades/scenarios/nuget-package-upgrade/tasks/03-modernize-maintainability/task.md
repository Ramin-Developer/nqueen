# 03-modernize-maintainability: Apply safe readability and maintainability refactoring

Review the solution for low-risk modernization opportunities that improve readability or maintainability without changing solver behavior or pursuing performance micro-optimizations. Keep changes aligned with the repository conventions: C# 14/.NET 10, global usings per project, partial files for large view models, and no magic strings for property names.

Limit refactoring to clear, behavior-preserving cleanup discovered during the review. Avoid broad rewrites, public API churn, or algorithmic changes unless required by validation.

**Done when**: Safe modernization changes are applied, behavior remains unchanged, and affected projects build cleanly with tests passing.

## Research Findings

- The package assessment reported `xunit.runner.console` is not referenced by any scoped project, or no supported version was found.
- Workspace search confirmed `xunit.runner.console` appears only in `Directory.Packages.props`, scenario artifacts, and historical documentation; no project has a `PackageReference` for it.
- Low-risk maintainability cleanup: remove the orphaned central `PackageVersion` entry so `Directory.Packages.props` only manages packages that projects consume.
- No solver, GUI, or test logic changes are needed; validation should confirm restore/build/tests remain clean.
