# NuGet Package Upgrade

## Strategy
Upgrade centrally managed packages through `Directory.Packages.props`, validate the solution, then apply only safe behavior-preserving maintainability refactoring.

## Preferences
- **Flow Mode**: Automatic
- **Commit Strategy**: After Each Task
- **Scope**: Solution (`NQueen.slnx`)
- **Package Version Policy**: Use latest stable compatible versions unless a package has commercial licensing concerns.
- **Commercial Package Policy**: Commercial packages must be pinned/fixed to the latest non-commercial version.
- **Package Management**: Apply package version changes in `Directory.Packages.props`.
- **Refactoring Scope**: Safe modernization/readability/maintainability improvements only; no performance micro-optimizations unless explicitly requested.

## Decisions
- Quick assessment: all 3 packages unified, no breaking API changes, no divergence.
- Versions applied centrally in `Directory.Packages.props`.
- Add safe C# 14 modernization tasks after the package bump (user request).

## Source Control
- **Source Branch**: main
- **Working Branch**: nuget-package-upgrade
- **Commit Strategy**: After Each Task
- **Branch Sync**: Auto (Merge)

## Package Scope
- Microsoft.Extensions.DependencyInjection
- Microsoft.Extensions.Hosting
- Microsoft.VisualStudio.DiagnosticsHub.BenchmarkDotNetDiagnosers

## Selected Versions
- **Microsoft.Extensions.DependencyInjection**: 10.0.14
- **Microsoft.Extensions.Hosting**: 10.0.14
- **Microsoft.VisualStudio.DiagnosticsHub.BenchmarkDotNetDiagnosers**: 18.12.37802.2

## Custom Instructions
<!-- Task-specific overrides: "For {taskId}: {instruction}" -->
